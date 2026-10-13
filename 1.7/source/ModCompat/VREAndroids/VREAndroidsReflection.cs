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
            if (utilsType != null)
            {
                isAndroidMethod = AccessTools.Method(utilsType, "IsAndroid", new[] { typeof(Pawn) });
            }

            if (isAndroidMethod == null)
            {
                UserLogger.Warning(
                    "VRE Androids compatibility disabled: could not find 'VREAndroids.Utils.IsAndroid(Pawn)'. " +
                    "The installed VRE Androids build is likely incompatible with this version of Cheat Menu.");
            }
        }

        public static bool IsAndroid(Pawn pawn)
        {
            return isAndroidMethod != null && (bool)isAndroidMethod.Invoke(null, new object[] { pawn });
        }
    }
}
