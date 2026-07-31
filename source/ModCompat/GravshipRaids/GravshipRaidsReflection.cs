using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace Cheat_Menu
{
    public static class GravshipRaidsReflection
    {
        private const string DebugApiTypeName = "Gravship_Raids.GravshipRaidDebugApi";
        private const string TemplateUtilityTypeName = "Gravship_Raids.GravshipRaidTemplateUtility";
        private const string ShuttleTemplateUtilityTypeName = "Gravship_Raids.ShuttleRaidTemplateUtility";

        private static bool initialized;
        private static bool resolved;
        private static bool shuttleResolved;

        private static Type debugApiType;

        private static MethodInfo rotateTemplateSpawnMethod;
        private static MethodInfo getTemplatesMethod;
        private static MethodInfo spawnTemplateAtMethod;
        private static MethodInfo testLandingSearchMethod;
        private static MethodInfo forceGravshipRaidAtMethod;
        private static MethodInfo forceGravshipRaidAtTemplateMethod;
        private static MethodInfo forceEnemyGravshipDepartureMethod;
        private static MethodInfo startPrefabCaptureWithTerrainMethod;
        private static MethodInfo isValidTemplateMethod;
        private static FieldInfo templateDisabledField;

        private static MethodInfo rotateShuttleTemplateSpawnMethod;
        private static MethodInfo getShuttleTemplatesMethod;
        private static MethodInfo spawnShuttleTemplateAtMethod;
        private static MethodInfo forceShuttleRaidAtMethod;
        private static MethodInfo forceShuttleRaidAtTemplateMethod;
        private static MethodInfo forceEnemyShuttleDepartureMethod;
        private static MethodInfo isValidShuttleTemplateMethod;
        private static FieldInfo shuttleTemplateDisabledField;

        public static bool IsFullyResolved
        {
            get
            {
                EnsureInitialized();
                return resolved;
            }
        }

        public static bool IsShuttleCheatsResolved
        {
            get
            {
                EnsureInitialized();
                return shuttleResolved;
            }
        }

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            debugApiType = AccessTools.TypeByName(DebugApiTypeName);
            if (debugApiType == null)
            {
                UserLogger.Warning(
                    "Gravship Raids compatibility disabled: could not find type '" + DebugApiTypeName +
                    "'. The installed Gravship Raids build is likely incompatible with this version of Cheat Menu.");
                return;
            }

            rotateTemplateSpawnMethod = AccessTools.Method(debugApiType, "RotateTemplateSpawn", Type.EmptyTypes);
            getTemplatesMethod = AccessTools.Method(debugApiType, "GetTemplates", Type.EmptyTypes);
            testLandingSearchMethod = AccessTools.Method(debugApiType, "TestLandingSearch", new[] { typeof(Map) });
            forceGravshipRaidAtMethod = AccessTools.Method(debugApiType, "ForceGravshipRaidAt", new[] { typeof(Map), typeof(IntVec3) });
            forceEnemyGravshipDepartureMethod = AccessTools.Method(debugApiType, "ForceEnemyGravshipDeparture", new[] { typeof(Map) });
            startPrefabCaptureWithTerrainMethod = AccessTools.Method(debugApiType, "StartPrefabCaptureWithTerrain", Type.EmptyTypes);

            if (rotateTemplateSpawnMethod == null
                || getTemplatesMethod == null
                || testLandingSearchMethod == null
                || forceGravshipRaidAtMethod == null
                || forceEnemyGravshipDepartureMethod == null
                || startPrefabCaptureWithTerrainMethod == null)
            {
                LogResolutionFailure();
                return;
            }

            Type templatesReturnType = getTemplatesMethod.ReturnType;
            Type templateDefType = templatesReturnType.IsGenericType && templatesReturnType.GetGenericArguments().Length == 1
                ? templatesReturnType.GetGenericArguments()[0]
                : null;

            if (templateDefType == null || !typeof(Def).IsAssignableFrom(templateDefType))
            {
                LogResolutionFailure();
                return;
            }

            spawnTemplateAtMethod = AccessTools.Method(debugApiType, "SpawnTemplateAt", new[] { templateDefType, typeof(Map), typeof(IntVec3) });
            forceGravshipRaidAtTemplateMethod = AccessTools.Method(debugApiType, "ForceGravshipRaidAt", new[] { templateDefType, typeof(Map), typeof(IntVec3) });
            if (spawnTemplateAtMethod == null || forceGravshipRaidAtTemplateMethod == null)
            {
                LogResolutionFailure();
                return;
            }

            Type templateUtilityType = AccessTools.TypeByName(TemplateUtilityTypeName);
            if (templateUtilityType == null)
            {
                LogResolutionFailure();
                return;
            }

            isValidTemplateMethod = AccessTools.Method(templateUtilityType, "IsValidTemplate", new[] { templateDefType });
            templateDisabledField = AccessTools.Field(templateDefType, "disabled");
            if (isValidTemplateMethod == null || templateDisabledField == null)
            {
                LogResolutionFailure();
                return;
            }

            resolved = true;

            ResolveShuttleCapability();
        }

        private static void ResolveShuttleCapability()
        {
            rotateShuttleTemplateSpawnMethod = AccessTools.Method(debugApiType, "RotateShuttleTemplateSpawn", Type.EmptyTypes);
            getShuttleTemplatesMethod = AccessTools.Method(debugApiType, "GetShuttleTemplates", Type.EmptyTypes);
            forceShuttleRaidAtMethod = AccessTools.Method(debugApiType, "ForceShuttleRaidAt", new[] { typeof(Map), typeof(IntVec3) });
            forceEnemyShuttleDepartureMethod = AccessTools.Method(debugApiType, "ForceEnemyShuttleDeparture", new[] { typeof(Map) });

            if (rotateShuttleTemplateSpawnMethod == null
                || getShuttleTemplatesMethod == null
                || forceShuttleRaidAtMethod == null
                || forceEnemyShuttleDepartureMethod == null)
            {
                LogShuttleResolutionFailure();
                return;
            }

            Type shuttleTemplatesReturnType = getShuttleTemplatesMethod.ReturnType;
            Type shuttleTemplateDefType = shuttleTemplatesReturnType.IsGenericType && shuttleTemplatesReturnType.GetGenericArguments().Length == 1
                ? shuttleTemplatesReturnType.GetGenericArguments()[0]
                : null;

            if (shuttleTemplateDefType == null || !typeof(Def).IsAssignableFrom(shuttleTemplateDefType))
            {
                LogShuttleResolutionFailure();
                return;
            }

            spawnShuttleTemplateAtMethod = AccessTools.Method(debugApiType, "SpawnShuttleTemplateAt", new[] { shuttleTemplateDefType, typeof(Map), typeof(IntVec3) });
            forceShuttleRaidAtTemplateMethod = AccessTools.Method(debugApiType, "ForceShuttleRaidAt", new[] { shuttleTemplateDefType, typeof(Map), typeof(IntVec3) });
            if (spawnShuttleTemplateAtMethod == null || forceShuttleRaidAtTemplateMethod == null)
            {
                LogShuttleResolutionFailure();
                return;
            }

            Type shuttleTemplateUtilityType = AccessTools.TypeByName(ShuttleTemplateUtilityTypeName);
            if (shuttleTemplateUtilityType == null)
            {
                LogShuttleResolutionFailure();
                return;
            }

            isValidShuttleTemplateMethod = AccessTools.Method(shuttleTemplateUtilityType, "IsValidTemplate", new[] { shuttleTemplateDefType });
            shuttleTemplateDisabledField = AccessTools.Field(shuttleTemplateDefType, "disabled");
            if (isValidShuttleTemplateMethod == null || shuttleTemplateDisabledField == null)
            {
                LogShuttleResolutionFailure();
                return;
            }

            shuttleResolved = true;
        }

        private static void LogResolutionFailure()
        {
            UserLogger.Warning(
                "Gravship Raids compatibility disabled: GravshipRaidDebugApi does not expose the expected members. " +
                "The installed Gravship Raids build is likely incompatible with this version of Cheat Menu.");
        }

        private static void LogShuttleResolutionFailure()
        {
            UserLogger.Warning(
                "Gravship Raids shuttle raid cheats disabled: the installed Gravship Raids build does not expose the expected shuttle debug API members. " +
                "Existing gravship compatibility remains available; update Gravship Raids for shuttle raid cheats.");
        }

        public static void RotateTemplateSpawn()
        {
            Invoke(rotateTemplateSpawnMethod, null);
        }

        public static List<Def> GetTemplates()
        {
            object result = Invoke(getTemplatesMethod, null);
            List<Def> templates = new List<Def>();
            if (result is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item is Def def)
                    {
                        templates.Add(def);
                    }
                }
            }

            return templates;
        }

        public static void SpawnTemplateAt(Def template, Map map, IntVec3 cell)
        {
            Invoke(spawnTemplateAtMethod, new object[] { template, map, cell });
        }

        public static void TestLandingSearch(Map map)
        {
            Invoke(testLandingSearchMethod, new object[] { map });
        }

        public static void ForceGravshipRaidAt(Map map, IntVec3 cell)
        {
            Invoke(forceGravshipRaidAtMethod, new object[] { map, cell });
        }

        public static void ForceGravshipRaidAt(Def template, Map map, IntVec3 cell)
        {
            Invoke(forceGravshipRaidAtTemplateMethod, new object[] { template, map, cell });
        }

        public static bool IsValidTemplate(Def template)
        {
            return template != null && Invoke(isValidTemplateMethod, new object[] { template }) is bool valid && valid;
        }

        public static bool IsTemplateDisabled(Def template)
        {
            return template != null && templateDisabledField.GetValue(template) is bool disabled && disabled;
        }

        public static void ForceEnemyGravshipDeparture(Map map)
        {
            Invoke(forceEnemyGravshipDepartureMethod, new object[] { map });
        }

        public static void StartPrefabCaptureWithTerrain()
        {
            Invoke(startPrefabCaptureWithTerrainMethod, null);
        }

        public static void RotateShuttleTemplateSpawn()
        {
            Invoke(rotateShuttleTemplateSpawnMethod, null);
        }

        public static List<Def> GetShuttleTemplates()
        {
            object result = Invoke(getShuttleTemplatesMethod, null);
            List<Def> templates = new List<Def>();
            if (result is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item is Def def)
                    {
                        templates.Add(def);
                    }
                }
            }

            return templates;
        }

        public static void SpawnShuttleTemplateAt(Def template, Map map, IntVec3 cell)
        {
            Invoke(spawnShuttleTemplateAtMethod, new object[] { template, map, cell });
        }

        public static void ForceShuttleRaidAt(Map map, IntVec3 cell)
        {
            Invoke(forceShuttleRaidAtMethod, new object[] { map, cell });
        }

        public static void ForceShuttleRaidAt(Def template, Map map, IntVec3 cell)
        {
            Invoke(forceShuttleRaidAtTemplateMethod, new object[] { template, map, cell });
        }

        public static bool IsValidShuttleTemplate(Def template)
        {
            return template != null && Invoke(isValidShuttleTemplateMethod, new object[] { template }) is bool valid && valid;
        }

        public static bool IsShuttleTemplateDisabled(Def template)
        {
            return template != null && shuttleTemplateDisabledField.GetValue(template) is bool disabled && disabled;
        }

        public static void ForceEnemyShuttleDeparture(Map map)
        {
            Invoke(forceEnemyShuttleDepartureMethod, new object[] { map });
        }

        private static object Invoke(MethodInfo method, object[] args)
        {
            try
            {
                return method.Invoke(null, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }
    }
}
