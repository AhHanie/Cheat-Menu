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

        private static bool initialized;
        private static bool resolved;

        private static Type debugApiType;

        private static MethodInfo rotateTemplateSpawnMethod;
        private static MethodInfo getTemplatesMethod;
        private static MethodInfo spawnTemplateAtMethod;
        private static MethodInfo testLandingSearchMethod;
        private static MethodInfo forceGravshipRaidAtMethod;
        private static MethodInfo forceEnemyGravshipDepartureMethod;
        private static MethodInfo startPrefabCaptureWithTerrainMethod;

        public static bool IsFullyResolved
        {
            get
            {
                EnsureInitialized();
                return resolved;
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
            if (spawnTemplateAtMethod == null)
            {
                LogResolutionFailure();
                return;
            }

            resolved = true;
        }

        private static void LogResolutionFailure()
        {
            UserLogger.Warning(
                "Gravship Raids compatibility disabled: GravshipRaidDebugApi does not expose the expected members. " +
                "The installed Gravship Raids build is likely incompatible with this version of Cheat Menu.");
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

        public static void ForceEnemyGravshipDeparture(Map map)
        {
            Invoke(forceEnemyGravshipDepartureMethod, new object[] { map });
        }

        public static void StartPrefabCaptureWithTerrain()
        {
            Invoke(startPrefabCaptureWithTerrainMethod, null);
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
