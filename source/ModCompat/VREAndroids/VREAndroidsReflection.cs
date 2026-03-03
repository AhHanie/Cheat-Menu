using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Cheat_Menu
{
    public static class VREAndroidsReflection
    {
        private const string UtilsTypeName = "VREAndroids.Utils";

        private static bool initialized;
        private static MethodInfo isAndroidMethod;

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            Type utilsType = AccessTools.TypeByName(UtilsTypeName);
            isAndroidMethod = AccessTools.Method(utilsType, "IsAndroid", new[] { typeof(Pawn) });
        }

        public static bool IsAndroid(Pawn pawn)
        {
            return (bool)isAndroidMethod.Invoke(null, new object[] { pawn });
        }
    }
}
