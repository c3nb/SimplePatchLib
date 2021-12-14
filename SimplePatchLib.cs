using System;
using System.Linq;
using System.Security;
using System.Reflection.Emit;
using HarmonyLib;
using System.Reflection;
using System.Dynamic;
#if DEBUG
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
#endif

namespace SimplePatchLib
{
    public static partial class SimplePatch
    {
        public static Harmony Harmony => new Harmony(new object().GetAddress().ToString());
        public static void UnpatchAll() => Harmony.UnpatchAll(Harmony.Id);
        public static void Patch(Type targetType, string targetMethod, MethodInfo prefix = null, MethodInfo postfix = null) => Patch(targetType.GetMethod(targetMethod, AccessTools.all), prefix, postfix);
        public static void Patch(MethodInfo orig, MethodInfo prefix = null, MethodInfo postfix = null)
        {
            HarmonyMethod pre = null, post = null;
            if (prefix != null)
                pre = new HarmonyMethod(prefix);
            if (postfix != null)
                post = new HarmonyMethod(postfix);
            Harmony.Patch(orig, pre, post);
        }
    }
    [SecurityCritical]
    [SecuritySafeCritical]
    public static class DynamicMethodFactory
    {
        static DynamicMethodFactory()
        {
            string asmName = "DynamicMethodFactory";
            asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"{asmName}{new object().GetAddress()}"), AssemblyBuilderAccess.Run);
            moduleBuilder = asmBuilder.DefineDynamicModule("Module");
        }
        public static unsafe IntPtr GetAddress(this object obj)
        {
            TypedReference tr = __makeref(obj);
            return **(IntPtr**)&tr;
        }
        static readonly AssemblyBuilder asmBuilder;
        static readonly ModuleBuilder moduleBuilder;
        public static int TypeCount = 0;
        public static MethodInfo ToDynamicMethod<T>(this T del) where T : Delegate
        {
            Type delType = del.GetType();
            MethodInfo method = delType.GetMethod("Invoke");
            MethodInfo emitMethod = del.Method;
            TypeBuilder typeBuilder = moduleBuilder.DefineType($"Type{TypeCount++}", TypeAttributes.Public | TypeAttributes.Class);
            string methodName = "Method";
            ParameterInfo[] parameters = emitMethod.GetParameters();
            Type[] parameterTypes = parameters.Select(x => x.ParameterType).ToArray();
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, emitMethod.ReturnType, parameterTypes);
            for (int i = 0; i < parameters.Length; i++)
                methodBuilder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            FieldBuilder fieldBuilder = typeBuilder.DefineField("patch", delType, FieldAttributes.Public | FieldAttributes.Static);
            ILGenerator il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, fieldBuilder);
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i == 0)
                    il.Emit(OpCodes.Ldarg_0);
                else if (i == 1)
                    il.Emit(OpCodes.Ldarg_1);
                else if (i == 2)
                    il.Emit(OpCodes.Ldarg_2);
                else if (i == 3)
                    il.Emit(OpCodes.Ldarg_3);
                else
                    il.Emit(OpCodes.Ldarg, i);
            }
            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);
            Type type = typeBuilder.CreateType();
            MethodInfo defined = type.GetMethod(methodName, AccessTools.all);
            type.GetField("patch").SetValue(null, del);
            return defined;
        } 
    }
    public class ILEmitter
    {
        public static implicit operator ILEmitter(ILGenerator il) => new ILEmitter(il);
        public static implicit operator ILGenerator(ILEmitter ie) => ie.il;
        public ILEmitter(ILGenerator il) => this.il = il;
        public ILGenerator Generator => il;
        ILGenerator il;
        public ILEmitter ret() { il.Emit(OpCodes.Ret); return this; }
        public ILEmitter cast(Type type) { il.Emit(OpCodes.Castclass, type); return this; }
        public ILEmitter box(Type type) { il.Emit(OpCodes.Box, type); return this; }
        public ILEmitter unbox_any(Type type) { il.Emit(OpCodes.Unbox_Any, type); return this; }
        public ILEmitter unbox(Type type) { il.Emit(OpCodes.Unbox, type); return this; }
        public ILEmitter call(MethodInfo method) { il.Emit(OpCodes.Call, method); return this; }
        public ILEmitter callvirt(MethodInfo method) { il.Emit(OpCodes.Callvirt, method); return this; }
        public ILEmitter ldnull() { il.Emit(OpCodes.Ldnull); return this; }
        public ILEmitter bne_un(Label target) { il.Emit(OpCodes.Bne_Un, target); return this; }
        public ILEmitter beq(Label target) { il.Emit(OpCodes.Beq, target); return this; }
        public ILEmitter ldc_i4_0() { il.Emit(OpCodes.Ldc_I4_0); return this; }
        public ILEmitter ldc_i4_1() { il.Emit(OpCodes.Ldc_I4_1); return this; }
        public ILEmitter ldc_i4(int c) { il.Emit(OpCodes.Ldc_I4, c); return this; }
        public ILEmitter ldc_r4(float c) { il.Emit(OpCodes.Ldc_R4, c); return this; }
        public ILEmitter ldc_r8(double c) { il.Emit(OpCodes.Ldc_R8, c); return this; }
        public ILEmitter ldarg0() { il.Emit(OpCodes.Ldarg_0); return this; }
        public ILEmitter ldarg1() { il.Emit(OpCodes.Ldarg_1); return this; }
        public ILEmitter ldarg2() { il.Emit(OpCodes.Ldarg_2); return this; }
        public ILEmitter ldarga(int idx) { il.Emit(OpCodes.Ldarga, idx); return this; }
        public ILEmitter ldarga_s(int idx) { il.Emit(OpCodes.Ldarga_S, idx); return this; }
        public ILEmitter ldarg(int idx) { il.Emit(OpCodes.Ldarg, idx); return this; }
        public ILEmitter ldarg_s(int idx) { il.Emit(OpCodes.Ldarg_S, idx); return this; }
        public ILEmitter ldstr(string str) { il.Emit(OpCodes.Ldstr, str); return this; }
        public ILEmitter ifclass_ldind_ref(Type type) { if (!type.IsValueType) il.Emit(OpCodes.Ldind_Ref); return this; }
        public ILEmitter ldloc0() { il.Emit(OpCodes.Ldloc_0); return this; }
        public ILEmitter ldloc1() { il.Emit(OpCodes.Ldloc_1); return this; }
        public ILEmitter ldloc2() { il.Emit(OpCodes.Ldloc_2); return this; }
        public ILEmitter ldloca_s(int idx) { il.Emit(OpCodes.Ldloca_S, idx); return this; }
        public ILEmitter ldloca_s(LocalBuilder local) { il.Emit(OpCodes.Ldloca_S, local); return this; }
        public ILEmitter ldloc_s(int idx) { il.Emit(OpCodes.Ldloc_S, idx); return this; }
        public ILEmitter ldloc_s(LocalBuilder local) { il.Emit(OpCodes.Ldloc_S, local); return this; }
        public ILEmitter ldloca(int idx) { il.Emit(OpCodes.Ldloca, idx); return this; }
        public ILEmitter ldloca(LocalBuilder local) { il.Emit(OpCodes.Ldloca, local); return this; }
        public ILEmitter ldloc(int idx) { il.Emit(OpCodes.Ldloc, idx); return this; }
        public ILEmitter ldloc(LocalBuilder local) { il.Emit(OpCodes.Ldloc, local); return this; }
        public ILEmitter initobj(Type type) { il.Emit(OpCodes.Initobj, type); return this; }
        public ILEmitter newobj(ConstructorInfo ctor) { il.Emit(OpCodes.Newobj, ctor); return this; }
        public ILEmitter Throw() { il.Emit(OpCodes.Throw); return this; }
        public ILEmitter throw_new(Type type) { var exp = type.GetConstructor(Type.EmptyTypes); newobj(exp).Throw(); return this; }
        public ILEmitter stelem_ref() { il.Emit(OpCodes.Stelem_Ref); return this; }
        public ILEmitter ldelem_ref() { il.Emit(OpCodes.Ldelem_Ref); return this; }
        public ILEmitter ldlen() { il.Emit(OpCodes.Ldlen); return this; }
        public ILEmitter stloc(int idx) { il.Emit(OpCodes.Stloc, idx); return this; }
        public ILEmitter stloc_s(int idx) { il.Emit(OpCodes.Stloc_S, idx); return this; }
        public ILEmitter stloc(LocalBuilder local) { il.Emit(OpCodes.Stloc, local); return this; }
        public ILEmitter stloc_s(LocalBuilder local) { il.Emit(OpCodes.Stloc_S, local); return this; }
        public ILEmitter stloc0() { il.Emit(OpCodes.Stloc_0); return this; }
        public ILEmitter stloc1() { il.Emit(OpCodes.Stloc_1); return this; }
        public ILEmitter mark(Label label) { il.MarkLabel(label); return this; }
        public ILEmitter ldfld(FieldInfo field) { il.Emit(OpCodes.Ldfld, field); return this; }
        public ILEmitter ldsfld(FieldInfo field) { il.Emit(OpCodes.Ldsfld, field); return this; }
        public ILEmitter lodfld(FieldInfo field) { if (field.IsStatic) ldsfld(field); else ldfld(field); return this; }
        public ILEmitter ifvaluetype_box(Type type) { if (type.IsValueType) il.Emit(OpCodes.Box, type); return this; }
        public ILEmitter stfld(FieldInfo field) { il.Emit(OpCodes.Stfld, field); return this; }
        public ILEmitter stsfld(FieldInfo field) { il.Emit(OpCodes.Stsfld, field); return this; }
        public ILEmitter setfld(FieldInfo field) { if (field.IsStatic) stsfld(field); else stfld(field); return this; }
        public ILEmitter unboxorcast(Type type) { if (type.IsValueType) unbox(type); else cast(type); return this; }
        public ILEmitter callorvirt(MethodInfo method) { if (method.IsVirtual || !method.IsStatic) il.Emit(OpCodes.Callvirt, method); else il.Emit(OpCodes.Call, method); return this; }
        public ILEmitter stind_ref() { il.Emit(OpCodes.Stind_Ref); return this; }
        public ILEmitter ldind_ref() { il.Emit(OpCodes.Ldind_Ref); return this; }
        public LocalBuilder declocal(Type type) { return il.DeclareLocal(type); }
        public Label deflabel() { return il.DefineLabel(); }
        public ILEmitter ifclass_ldarg_else_ldarga(int idx, Type type) { if (type.IsValueType) ldarga(idx); else ldarg(idx); return this; }
        public ILEmitter ifclass_ldloc_else_ldloca(int idx, Type type) { if (type.IsValueType) ldloca(idx); else ldloc(idx); return this; }
        public ILEmitter perform(Action<ILEmitter, MemberInfo> action, MemberInfo member) { action(this, member); return this; }
        public ILEmitter ifbyref_ldloca_else_ldloc(LocalBuilder local, Type type) { if (type.IsByRef) ldloca(local); else ldloc(local); return this; }
    }
    public class ReflectObject : DynamicObject
    {
        object instance;
        Type type;
        public ReflectObject(object instance)
        {
            this.instance = instance;
            type = instance.GetType();
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            FieldInfo fi;
            PropertyInfo pi;
            if ((fi = type.GetField(name, AccessTools.all)) != null)
            {
                if (fi.IsStatic)
                    result = fi.GetValue(null);
                else
                    result = fi.GetValue(instance);
                return true;
            }
            else if ((pi = type.GetProperty(name, AccessTools.all)) != null)
            {
                MethodInfo getMethod;
                if ((getMethod = pi.GetGetMethod()) != null)
                {
                    if (getMethod.IsStatic)
                        result = pi.GetValue(null);
                    else
                        result = pi.GetValue(instance);
                    return true;
                }
            }
            result = null;
            return false;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            string name = binder.Name;
            FieldInfo fi;
            PropertyInfo pi;
            if ((fi = type.GetField(name, AccessTools.all)) != null)
            {
                if (fi.IsStatic)
                    fi.SetValue(null, value);
                else
                    fi.SetValue(instance, value);
                return true;
            }
            else if ((pi = type.GetProperty(name, AccessTools.all)) != null)
            {
                MethodInfo setMethod;
                if ((setMethod = pi.GetSetMethod()) != null)
                {
                    if (setMethod.IsStatic)
                        pi.SetValue(value, null);
                    else
                        pi.SetValue(value, instance);
                    return true;
                }
            }
            return false;
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            string name = binder.Name;
            MethodInfo method;
            if ((method = type.GetMethod(name, AccessTools.all)) != null)
            {
                if (method.IsStatic)
                    result = method.Invoke(null, args);
                else
                    result = method.Invoke(instance, args);
                return true;
            }
            result = null;
            return false;
        }
    }
#if DEBUG
public static class DynamicPInvokeFactory
    {
        static DynamicPInvokeFactory()
        {
            Location = Assembly.GetExecutingAssembly().Location;
            FileInfo fi = new FileInfo(Location);
            Location = Location.Replace('\\' + fi.Name, "");
        }
        public static readonly string Location;
        public static T GetPInvokeMethod<T>(string libNameWithExtension) where T : Delegate => (T)Marshal.GetDelegateForFunctionPointer(DynDll.GetFunction(DynDll.OpenLibrary(Path.Combine(Location, libNameWithExtension)), typeof(T).Name), typeof(T));
    }
    [SuppressUnmanagedCodeSecurity]
    public static class DynDll
    {
        /// <summary>
        /// Allows you to remap library paths / names and specify loading flags. Useful for cross-platform compatibility. Applies only to DynDll.
        /// </summary>
        public static Dictionary<string, List<DynDllMapping>> Mappings = new Dictionary<string, List<DynDllMapping>>();

#region kernel32 imports

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hLibModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

#endregion

#region dl imports

        [DllImport("dl", EntryPoint = "dlopen", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr dl_dlopen(string filename, int flags);
        [DllImport("dl", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool dl_dlclose(IntPtr handle);
        [DllImport("dl", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr dl_dlsym(IntPtr handle, string symbol);
        [DllImport("dl", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr dl_dlerror();

#endregion

#region libdl.so.2 imports

        [DllImport("libdl.so.2", EntryPoint = "dlopen", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr dl2_dlopen(string filename, int flags);
        [DllImport("libdl.so.2", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool dl2_dlclose(IntPtr handle);
        [DllImport("libdl.so.2", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr dl2_dlsym(IntPtr handle, string symbol);
        [DllImport("libdl.so.2", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr dl2_dlerror();

#endregion

#region dl wrappers

        private static int dlVersion = 1;

        private static IntPtr dlopen(string filename, int flags)
        {
            while (true)
            {
                try
                {
                    switch (dlVersion)
                    {
                        case 1:
                            return dl2_dlopen(filename, flags);

                        case 0:
                        default:
                            return dl_dlopen(filename, flags);
                    }
                }
                catch (DllNotFoundException) when (dlVersion > 0)
                {
                    dlVersion--;
                }
            }
        }

        private static bool dlclose(IntPtr handle)
        {
            while (true)
            {
                try
                {
                    switch (dlVersion)
                    {
                        case 1:
                            return dl2_dlclose(handle);

                        case 0:
                        default:
                            return dl_dlclose(handle);
                    }
                }
                catch (DllNotFoundException) when (dlVersion > 0)
                {
                    dlVersion--;
                }
            }
        }

        private static IntPtr dlsym(IntPtr handle, string symbol)
        {
            while (true)
            {
                try
                {
                    switch (dlVersion)
                    {
                        case 1:
                            return dl2_dlsym(handle, symbol);

                        case 0:
                        default:
                            return dl_dlsym(handle, symbol);
                    }
                }
                catch (DllNotFoundException) when (dlVersion > 0)
                {
                    dlVersion--;
                }
            }
        }

        private static IntPtr dlerror()
        {
            while (true)
            {
                try
                {
                    switch (dlVersion)
                    {
                        case 1:
                            return dl2_dlerror();

                        case 0:
                        default:
                            return dl_dlerror();
                    }
                }
                catch (DllNotFoundException) when (dlVersion > 0)
                {
                    dlVersion--;
                }
            }
        }

#endregion

        static DynDll()
        {
            // Run a dummy dlerror to resolve it so that it won't interfere with the first call
            if (!PlatformHelper.Is(Platform.Windows))
                dlerror();
        }

        private static bool CheckError(out Exception exception)
        {
            if (PlatformHelper.Is(Platform.Windows))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 0)
                {
                    exception = new Win32Exception(errorCode);
                    return false;
                }
            }
            else
            {
                IntPtr errorCode = dlerror();
                if (errorCode != IntPtr.Zero)
                {
                    exception = new Win32Exception(Marshal.PtrToStringAnsi(errorCode));
                    return false;
                }
            }

            exception = null;
            return true;
        }

        /// <summary>
        /// Open a given library and get its handle.
        /// </summary>
        /// <param name="name">The library name.</param>
        /// <param name="skipMapping">Whether to skip using the mapping or not.</param>
        /// <param name="flags">Any optional platform-specific flags.</param>
        /// <returns>The library handle.</returns>
        public static IntPtr OpenLibrary(string name, bool skipMapping = false, int? flags = null)
        {
            if (!InternalTryOpenLibrary(name, out var libraryPtr, skipMapping, flags))
                throw new DllNotFoundException($"Unable to load library '{name}'");

            if (!CheckError(out var exception))
                throw exception;

            return libraryPtr;
        }

        /// <summary>
        /// Try to open a given library and get its handle.
        /// </summary>
        /// <param name="name">The library name.</param>
		/// <param name="libraryPtr">The library handle, or null if it failed loading.</param>
        /// <param name="skipMapping">Whether to skip using the mapping or not.</param>
        /// <param name="flags">Any optional platform-specific flags.</param>
        /// <returns>True if the handle was obtained, false otherwise.</returns>
        public static bool TryOpenLibrary(string name, out IntPtr libraryPtr, bool skipMapping = false, int? flags = null)
        {
            return InternalTryOpenLibrary(name, out libraryPtr, skipMapping, flags) || CheckError(out _);
        }

        private static bool InternalTryOpenLibrary(string name, out IntPtr libraryPtr, bool skipMapping, int? flags)
        {
            if (name != null && !skipMapping && Mappings.TryGetValue(name, out List<DynDllMapping> mappingList))
            {
                foreach (var mapping in mappingList)
                {
                    if (InternalTryOpenLibrary(mapping.LibraryName, out libraryPtr, true, mapping.Flags))
                        return true;
                }

                libraryPtr = IntPtr.Zero;
                return true;
            }

            if (PlatformHelper.Is(Platform.Windows))
            {
                libraryPtr = name == null
                    ? GetModuleHandle(name)
                    : LoadLibrary(name);
            }
            else
            {
                int _flags = flags ?? (DlopenFlags.RTLD_NOW | DlopenFlags.RTLD_GLOBAL); // Default should match LoadLibrary.

                libraryPtr = dlopen(name, _flags);

                if (libraryPtr == IntPtr.Zero && File.Exists(name))
                    libraryPtr = dlopen(Path.GetFullPath(name), _flags);
            }

            return libraryPtr != IntPtr.Zero;
        }

        /// <summary>
        /// Release a library handle obtained via OpenLibrary. Don't release the result of OpenLibrary(null)!
        /// </summary>
        /// <param name="lib">The library handle.</param>
        public static bool CloseLibrary(IntPtr lib)
        {
            if (PlatformHelper.Is(Platform.Windows))
                CloseLibrary(lib);
            else
                dlclose(lib);

            return CheckError(out _);
        }

        /// <summary>
        /// Get a function pointer for a function in the given library.
        /// </summary>
        /// <param name="libraryPtr">The library handle.</param>
        /// <param name="name">The function name.</param>
        /// <returns>The function pointer.</returns>
        public static IntPtr GetFunction(this IntPtr libraryPtr, string name)
        {
            if (!InternalTryGetFunction(libraryPtr, name, out var functionPtr))
                throw new MissingMethodException($"Unable to load function '{name}'");

            if (!CheckError(out var exception))
                throw exception;

            return functionPtr;
        }

        /// <summary>
        /// Get a function pointer for a function in the given library.
        /// </summary>
        /// <param name="libraryPtr">The library handle.</param>
        /// <param name="name">The function name.</param>
        /// <param name="functionPtr">The function pointer, or null if it wasn't found.</param>
        /// <returns>True if the function pointer was obtained, false otherwise.</returns>
        public static bool TryGetFunction(this IntPtr libraryPtr, string name, out IntPtr functionPtr)
        {
            return InternalTryGetFunction(libraryPtr, name, out functionPtr) || CheckError(out _);
        }

        private static bool InternalTryGetFunction(IntPtr libraryPtr, string name, out IntPtr functionPtr)
        {
            if (libraryPtr == IntPtr.Zero)
                throw new ArgumentNullException(nameof(libraryPtr));

            functionPtr = PlatformHelper.Is(Platform.Windows)
                ? GetProcAddress(libraryPtr, name)
                : dlsym(libraryPtr, name);

            return functionPtr != IntPtr.Zero;
        }

        /// <summary>
        /// Extension method wrapping Marshal.GetDelegateForFunctionPointer
        /// </summary>
        public static T AsDelegate<T>(this IntPtr s) where T : class
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Fill all static delegate fields with the DynDllImport attribute.
        /// Call this early on in the static constructor.
        /// </summary>
        /// <param name="type">The type containing the DynDllImport delegate fields.</param>
        /// <param name="mappings">Any optional mappings similar to the static mappings.</param>
        public static void ResolveDynDllImports(this Type type, Dictionary<string, List<DynDllMapping>> mappings = null)
            => InternalResolveDynDllImports(type, null, mappings);

        /// <summary>
        /// Fill all instance delegate fields with the DynDllImport attribute.
        /// Call this early on in the constructor.
        /// </summary>
        /// <param name="instance">An instance of a type containing the DynDllImport delegate fields.</param>
        /// <param name="mappings">Any optional mappings similar to the static mappings.</param>
        public static void ResolveDynDllImports(object instance, Dictionary<string, List<DynDllMapping>> mappings = null)
            => InternalResolveDynDllImports(instance.GetType(), instance, mappings);

        private static void InternalResolveDynDllImports(Type type, object instance, Dictionary<string, List<DynDllMapping>> mappings)
        {
            BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.NonPublic;
            if (instance == null)
                fieldFlags |= BindingFlags.Static;
            else
                fieldFlags |= BindingFlags.Instance;

            foreach (FieldInfo field in type.GetFields(fieldFlags))
            {
                bool found = true;

                foreach (DynDllImportAttribute attrib in field.GetCustomAttributes(typeof(DynDllImportAttribute), true))
                {
                    found = false;

                    IntPtr libraryPtr = IntPtr.Zero;

                    if (mappings != null && mappings.TryGetValue(attrib.LibraryName, out List<DynDllMapping> mappingList))
                    {
                        bool mappingFound = false;

                        foreach (var mapping in mappingList)
                        {
                            if (TryOpenLibrary(mapping.LibraryName, out libraryPtr, true, mapping.Flags))
                            {
                                mappingFound = true;
                                break;
                            }
                        }

                        if (!mappingFound)
                            continue;
                    }
                    else
                    {
                        if (!TryOpenLibrary(attrib.LibraryName, out libraryPtr))
                            continue;
                    }


                    foreach (string entryPoint in attrib.EntryPoints.Concat(new[] { field.Name, field.FieldType.Name }))
                    {
                        if (!libraryPtr.TryGetFunction(entryPoint, out IntPtr functionPtr))
                            continue;

#pragma warning disable CS0618 // Type or member is obsolete
                        field.SetValue(instance, Marshal.GetDelegateForFunctionPointer(functionPtr, field.FieldType));
#pragma warning restore CS0618 // Type or member is obsolete

                        found = true;
                        break;
                    }

                    if (found)
                        break;
                }

                if (!found)
                    throw new EntryPointNotFoundException($"No matching entry point found for {field.Name} in {field.DeclaringType.FullName}");
            }
        }

        public static class DlopenFlags
        {
            public const int RTLD_LAZY = 0x0001;
            public const int RTLD_NOW = 0x0002;
            public const int RTLD_LOCAL = 0x0000;
            public const int RTLD_GLOBAL = 0x0100;
        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DynDllImportAttribute : Attribute
    {
        /// <summary>
        /// The library or library alias to use.
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// A list of possible entrypoints that the function can be resolved to. Implicitly includes the field name and delegate name.
        /// </summary>
        public string[] EntryPoints { get; set; }

        /// <param name="libraryName">The library or library alias to use.</param>
        /// <param name="entryPoints">A list of possible entrypoints that the function can be resolved to. Implicitly includes the field name and delegate name.</param>
        public DynDllImportAttribute(string libraryName, params string[] entryPoints)
        {
            LibraryName = libraryName;
            EntryPoints = entryPoints;
        }
    }
    public sealed class DynDllMapping
    {
        /// <summary>
        /// The name as which the library will be resolved as. Useful to remap libraries or to provide full paths.
        /// </summary>
        public string LibraryName { get; set; }

        /// <summary>
        /// Platform-dependent loading flags.
        /// </summary>
        public int? Flags { get; set; }

        /// <param name="libraryName">The name as which the library will be resolved as. Useful to remap libraries or to provide full paths.</param>
        /// <param name="flags">Platform-dependent loading flags.</param>
		public DynDllMapping(string libraryName, int? flags = null)
        {
            LibraryName = libraryName ?? throw new ArgumentNullException(nameof(libraryName));
            Flags = flags;
        }

        public static implicit operator DynDllMapping(string libraryName)
        {
            return new DynDllMapping(libraryName);
        }
    }
    static class PlatformHelper
    {
        private static void DeterminePlatform()
        {
            _current = Platform.Unknown;

#if NETSTANDARD
            // RuntimeInformation.IsOSPlatform is lying: https://github.com/dotnet/corefx/issues/3032
            // Determine the platform based on the path.
            string windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\", StringComparison.Ordinal) && Directory.Exists(windir)) {
                _current = Platform.Windows;

            } else if (File.Exists("/proc/sys/kernel/ostype")) {
                string osType = File.ReadAllText("/proc/sys/kernel/ostype");
                if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase)) {
                    _current = Platform.Linux;
                } else {
                    _current = Platform.Unix;
                }

            } else if (File.Exists("/System/Library/CoreServices/SystemVersion.plist")) {
                _current = Platform.MacOS;
            }

#else
            // For old Mono, get from a private property to accurately get the platform.
            // static extern PlatformID Platform
            PropertyInfo p_Platform = typeof(Environment).GetProperty("Platform", BindingFlags.NonPublic | BindingFlags.Static);
            string platID;
            if (p_Platform != null)
            {
                platID = p_Platform.GetValue(null, new object[0]).ToString();
            }
            else
            {
                // For .NET and newer Mono, use the usual value.
                platID = Environment.OSVersion.Platform.ToString();
            }
            platID = platID.ToLower(CultureInfo.InvariantCulture);

            if (platID.Contains("win"))
            {
                _current = Platform.Windows;
            }
            else if (platID.Contains("mac") || platID.Contains("osx"))
            {
                _current = Platform.MacOS;
            }
            else if (platID.Contains("lin") || platID.Contains("unix"))
            {
                _current = Platform.Linux;
            }
#endif

            if (Is(Platform.Linux) &&
                Directory.Exists("/data") && File.Exists("/system/build.prop")
            )
            {
                _current = Platform.Android;

            }
            else if (Is(Platform.Unix) &&
              Directory.Exists("/Applications") && Directory.Exists("/System") &&
              Directory.Exists("/User") && !Directory.Exists("/Users")
          )
            {
                _current = Platform.iOS;

            }
            else if (Is(Platform.Windows) &&
              CheckWine()
          )
            {
                // Sorry, Wine devs, but you might want to look at DetourRuntimeNETPlatform.
                _current |= Platform.Wine;
            }

            // Is64BitOperatingSystem has been added in .NET Framework 4.0
            MethodInfo m_get_Is64BitOperatingSystem = typeof(Environment).GetProperty("Is64BitOperatingSystem")?.GetGetMethod();
            if (m_get_Is64BitOperatingSystem != null)
                _current |= (((bool)m_get_Is64BitOperatingSystem.Invoke(null, new object[0])) ? Platform.Bits64 : 0);
            else
                _current |= (IntPtr.Size >= 8 ? Platform.Bits64 : 0);

#if NETSTANDARD
            // Detect ARM based on RuntimeInformation.
            if (RuntimeInformation.ProcessArchitecture.HasFlag(Architecture.Arm) ||
                RuntimeInformation.OSArchitecture.HasFlag(Architecture.Arm))
                _current |= Platform.ARM;
#else
            if ((Is(Platform.Unix) || Is(Platform.Unknown)) && Type.GetType("Mono.Runtime") != null)
            {
                /* I'd love to use RuntimeInformation, but it returns X64 up until...
                 * https://github.com/mono/mono/commit/396559769d0e4ca72837e44bcf837b7c91596414
                 * ... and that commit still hasn't reached Mono 5.16 on Debian, dated
                 * tarball Mon Nov 26 17:21:35 UTC 2018
                 * There's also the possibility to [DllImport("libc.so.6")]
                 * -ade
                 */
                try
                {
                    string arch;
                    using (Process uname = Process.Start(new ProcessStartInfo("uname", "-m")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }))
                    {
                        arch = uname.StandardOutput.ReadLine().Trim();
                    }

                    if (arch.StartsWith("aarch", StringComparison.Ordinal) || arch.StartsWith("arm", StringComparison.Ordinal))
                        _current |= Platform.ARM;
                }
                catch (Exception)
                {
                    // Starting a process can fail for various reasons. One of them being...
                    /* System.MissingMethodException: Method 'MonoIO.CreatePipe' not found.
                     * at System.Diagnostics.Process.StartWithCreateProcess (System.Diagnostics.ProcessStartInfo startInfo) <0x414ceb20 + 0x0061f> in <filename unknown>:0 
                     */
                }

            }
            else
            {
                typeof(object).Module.GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine);
                if (machine == (ImageFileMachine)0x01C4 /* ARM, .NET Framework 4.5 */)
                    _current |= Platform.ARM;
            }
#endif
        }

        private static Platform _current = Platform.Unknown;

        private static bool _currentLocked = false;

        public static Platform Current
        {
            get
            {
                if (!_currentLocked)
                {
                    if (_current == Platform.Unknown)
                    {
                        DeterminePlatform();
                    }

                    _currentLocked = true;
                }

                return _current;
            }
            set
            {
                if (_currentLocked)
                    throw new InvalidOperationException("Cannot set the value of PlatformHelper.Current once it has been accessed.");

                _current = value;
            }
        }


        private static string _librarySuffix;

        public static string LibrarySuffix
        {
            get
            {
                if (_librarySuffix == null)
                {
                    _librarySuffix =
                        Is(Platform.MacOS) ? "dylib" :
                        Is(Platform.Unix) ? "so" :
                        "dll";
                }

                return _librarySuffix;
            }
        }

        public static bool Is(Platform platform)
            => (Current & platform) == platform;
        private static bool CheckWine()
        {
            // wine_get_version can be missing because of course it can.
            // General purpose env var.
            string env = Environment.GetEnvironmentVariable("MONOMOD_WINE");
            if (env == "1")
                return true;
            if (env == "0")
                return false;

            // The "Dalamud" plugin loader for FFXIV uses Harmony, coreclr and wine. What a nice combo!
            // At least they went ahead and provide an environment variable for everyone to check.
            // See https://github.com/goatcorp/FFXIVQuickLauncher/blob/8685db4a0e8ec53235fb08cd88aded7c7061d9fb/src/XIVLauncher/Settings/EnvironmentSettings.cs
            env = Environment.GetEnvironmentVariable("XL_WINEONLINUX")?.ToLower(CultureInfo.InvariantCulture);
            if (env == "true")
                return true;
            if (env == "false")
                return false;

            IntPtr ntdll = GetModuleHandle("ntdll.dll");
            if (ntdll != IntPtr.Zero && GetProcAddress(ntdll, "wine_get_version") != IntPtr.Zero)
                return true;

            return false;
        }
        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    }
    [Flags]
    public enum Platform : int
    {
        /// <summary>
        /// Bit applied to all OSes (Unknown, Windows, MacOS, ...). 
        /// </summary>
        OS = 1 << 0,

        /// <summary>
        /// On demand 64-bit platform bit.
        /// </summary>
        Bits64 = 1 << 1,

        /// <summary>
        /// Applied to all NT and NT-oid platforms (Windows).
        /// </summary>
        NT = 1 << 2,
        /// <summary>
        /// Applied to all Unix and Unix-oid platforms (macOS, Linux, ...).
        /// </summary>
        Unix = 1 << 3,

        /// <summary>
        /// On demand ARM platform bit.
        /// </summary>
        ARM = 1 << 16,

        /// <summary>
        /// On demand Wine bit. DON'T RELY ON THIS.
        /// </summary>
        Wine = 1 << 17,

        /// <summary>
        /// Unknown OS.
        /// </summary>
        Unknown = OS | (1 << 4),
        /// <summary>
        /// Windows, using the NT kernel.
        /// </summary>
        Windows = OS | NT | (1 << 5),
        /// <summary>
        /// macOS, using the Darwin kernel.
        /// </summary>
        MacOS = OS | Unix | (1 << 6),
        /// <summary>
        /// Linux.
        /// </summary>
        Linux = OS | Unix | (1 << 7),
        /// <summary>
        /// Android, using the Linux kernel.
        /// </summary>
        Android = Linux | (1 << 8),
        /// <summary>
        /// iOS, sharing components with macOS.
        /// </summary>
        iOS = MacOS | (1 << 9),
    }
#endif
}
