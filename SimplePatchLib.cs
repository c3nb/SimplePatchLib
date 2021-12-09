using System;
using System.Linq;
using System.Security;
using System.Reflection.Emit;
using HarmonyLib;
using System.Reflection;
using System.Dynamic;

namespace SimplePatchLib
{
    public static class SimplePatch
    {
        public static Harmony Harmony => new Harmony(new object().GetAddress().ToString());
        public static void UnpatchAll() => Harmony.UnpatchAll(Harmony.Id);
        #region Misc
        public static void Prefix(Type targetType, string targetMethod, Delegate del) => PrefixInternal(targetType, targetMethod, del);
        public static void Postfix(Type targetType, string targetMethod, Delegate del) => PostfixInternal(targetType, targetMethod, del);
        public static void PrePostfix(Type targetType, string targetMethod, Delegate preDel, Delegate postDel) => PrePostfixInternal(targetType, targetMethod, preDel, postDel);
        
        public static void Property<T>(Type targetType, string targetProperty, T value) => Property(targetType, targetProperty, (ref T __result) =>
        {
            __result = value;
            return false;
        });
        public static void GetterPrefix<T>(Type targetType, string targetProperty, RefFunc<T, bool> patch) => PatchInternal(AccessTools.DeclaredProperty(targetType, targetProperty).GetGetMethod(true), PatchType.Prefix, patch.ToDynamicMethod(), null);
        public static void GetterPrefix<T, T2>(Type targetType, string targetProperty, RefFunc<T, T2, bool> patch) => PatchInternal(AccessTools.DeclaredProperty(targetType, targetProperty).GetGetMethod(true), PatchType.Prefix, patch.ToDynamicMethod(), null);
        public static void GetterPrefix<T>(Type targetType, string targetProperty, Action<T> patch) => PatchInternal(AccessTools.DeclaredProperty(targetType, targetProperty).GetGetMethod(true), PatchType.Prefix, patch.ToDynamicMethod(), null);
        public static void GetterPostfix<T, T2>(Type targetType, string targetProperty, Action<T, T2> patch) => PatchInternal(AccessTools.DeclaredProperty(targetType, targetProperty).GetGetMethod(true), PatchType.Postfix, null, patch.ToDynamicMethod());
        public static void GetterPostfix<T>(Type targetType, string targetProperty, Action<T> patch) => PatchInternal(AccessTools.DeclaredProperty(targetType, targetProperty).GetGetMethod(true), PatchType.Postfix, null, patch.ToDynamicMethod());
        #endregion
        #region Non-Generic
        public static void Prefix(Type targetType, string targetMethod, Func<bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix(Type targetType, string targetMethod, Action patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix(Type targetType, string targetMethod, Action patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix(Type targetType, string targetMethod, Action prePatch, Action postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        #endregion
        #region Generic
        public static void Prefix<T>(Type targetType, string targetMethod, Func<T, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T>(Type targetType, string targetMethod, Action<T> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T>(Type targetType, string targetMethod, Action<T> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T>(Type targetType, string targetMethod, Action<T> prePatch, Action<T> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T>(Type targetType, string targetMethod, RefFunc<T, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T>(Type targetType, string targetMethod, RefAction<T> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T>(Type targetType, string targetMethod, RefAction<T> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T>(Type targetType, string targetMethod, RefAction<T> prePatch, RefAction<T> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2>(Type targetType, string targetMethod, Func<T, T2, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2>(Type targetType, string targetMethod, Action<T, T2> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2>(Type targetType, string targetMethod, Action<T, T2> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2>(Type targetType, string targetMethod, Action<T, T2> prePatch, Action<T, T2> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2>(Type targetType, string targetMethod, RefFunc<T, T2, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2>(Type targetType, string targetMethod, RefAction<T, T2> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2>(Type targetType, string targetMethod, RefAction<T, T2> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2>(Type targetType, string targetMethod, RefAction<T, T2> prePatch, RefAction<T, T2> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3>(Type targetType, string targetMethod, Func<T, T2, T3, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3>(Type targetType, string targetMethod, Action<T, T2, T3> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3>(Type targetType, string targetMethod, Action<T, T2, T3> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3>(Type targetType, string targetMethod, Action<T, T2, T3> prePatch, Action<T, T2, T3> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3>(Type targetType, string targetMethod, RefFunc<T, T2, T3, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3>(Type targetType, string targetMethod, RefAction<T, T2, T3> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3>(Type targetType, string targetMethod, RefAction<T, T2, T3> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3>(Type targetType, string targetMethod, RefAction<T, T2, T3> prePatch, RefAction<T, T2, T3> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4>(Type targetType, string targetMethod, Func<T, T2, T3, T4, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4>(Type targetType, string targetMethod, Action<T, T2, T3, T4> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4>(Type targetType, string targetMethod, Action<T, T2, T3, T4> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4>(Type targetType, string targetMethod, Action<T, T2, T3, T4> prePatch, Action<T, T2, T3, T4> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, bool> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4> prePatch, RefAction<T, T2, T3, T4> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5> prePatch, Action<T, T2, T3, T4, T5> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5> prePatch, RefAction<T, T2, T3, T4, T5> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6> prePatch, Action<T, T2, T3, T4, T5, T6> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6> prePatch, RefAction<T, T2, T3, T4, T5, T6> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7> prePatch, Action<T, T2, T3, T4, T5, T6, T7> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> prePatch, Action<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);

        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Prefix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> patch) => PrefixInternal(targetType, targetMethod, patch);
        public static void Postfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> patch) => PostfixInternal(targetType, targetMethod, patch);
        public static void PrePostfix<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type targetType, string targetMethod, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> prePatch, RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> postPatch) => PrePostfixInternal(targetType, targetMethod, prePatch, postPatch);
        #endregion
        #region Internal
        static void PrefixInternal(Type targetType, string targetMethod, Delegate @delegate) => PatchInternal(targetType.GetMethod(targetMethod, AccessTools.all), PatchType.Prefix, @delegate.ToDynamicMethod(), null);
        static void PostfixInternal(Type targetType, string targetMethod, Delegate @delegate) => PatchInternal(targetType.GetMethod(targetMethod, AccessTools.all), PatchType.Postfix, null, @delegate.ToDynamicMethod());
        static void PrePostfixInternal(Type targetType, string targetMethod, Delegate preDelegate, Delegate postDelegate) => PatchInternal(targetType.GetMethod(targetMethod, AccessTools.all), PatchType.Prefix | PatchType.Postfix, preDelegate.ToDynamicMethod(), postDelegate.ToDynamicMethod());
        static void PatchInternal(MethodInfo targetMethod, PatchType patchType, MethodInfo patchMethodPrefix = null, MethodInfo patchMethodPostfix = null)
        {
            switch (patchType)
            {
                case PatchType.Prefix:
                    {
                        Harmony.Patch(targetMethod, new HarmonyMethod(patchMethodPrefix));
                        break;
                    }
                case PatchType.Postfix:
                    {
                        Harmony.Patch(targetMethod, postfix: new HarmonyMethod(patchMethodPostfix));
                        break;
                    }
                case PatchType.Prefix | PatchType.Postfix:
                    {
                        Harmony.Patch(targetMethod, new HarmonyMethod(patchMethodPrefix), new HarmonyMethod(patchMethodPostfix));
                        break;
                    }
            }
        }
        [Flags]
        enum PatchType
        {
            Prefix = 1,
            Postfix = 2
        }
        #endregion
    }
    #region RefDelegates
    public delegate void RefAction<T>(ref T result);
    public delegate void RefAction<T, T2>(T value, ref T2 result);
    public delegate void RefAction<T, T2, T3>(T value, T2 value2, ref T3 result);
    public delegate void RefAction<T, T2, T3, T4>(T value, T2 value2, T3 value3, ref T4 result);
    public delegate void RefAction<T, T2, T3, T4, T5>(T value, T2 value2, T3 value3, T4 value4, ref T5 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6>(T value, T2 value2, T3 value3, T4 value4, T5 value5, ref T6 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, ref T7 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, ref T8 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, ref T9 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, ref T10 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, ref T11 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, ref T12 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, ref T13 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, ref T14 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, ref T15 result);
    public delegate void RefAction<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15, ref T16 result);

    public delegate R RefFunc<T, R>(ref T result);
    public delegate R RefFunc<T, T2, R>(T value, ref T2 result);
    public delegate R RefFunc<T, T2, T3, R>(T value, T2 value2, ref T3 result);
    public delegate R RefFunc<T, T2, T3, T4, R>(T value, T2 value2, T3 value3, ref T4 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, R>(T value, T2 value2, T3 value3, T4 value4, ref T5 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, ref T6 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, ref T7 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, ref T8 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, ref T9 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, ref T10 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, ref T11 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, ref T12 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, ref T13 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, ref T14 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, ref T15 result);
    public delegate R RefFunc<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(T value, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10, T11 value11, T12 value12, T13 value13, T14 value14, T15 value15, ref T16 result);
    #endregion
    [SecurityCritical]
    [SecuritySafeCritical]
    public static class DynamicMethodFactory
    {
        class ILEmitter
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
}
