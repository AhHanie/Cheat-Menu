using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Applies the patch classes of an assembly one at a time, so a single failing patch
    /// (for example because another mod removed the call an infix patch wraps) does not
    /// stop the remaining patches, and logs which other mods patch the failed method.
    /// </summary>
    public static class HarmonyPatchInstaller
    {
        public static void PatchAll(Harmony harmony, Assembly assembly)
        {
            Type[] types = AccessTools.GetTypesFromAssembly(assembly);
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (!type.HasHarmonyAttribute())
                {
                    continue;
                }

                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    UserLogger.Exception(ex, BuildFailureReport(harmony.Id, type));
                }
            }
        }

        private static string BuildFailureReport(string ownHarmonyId, Type patchClass)
        {
            StringBuilder report = new StringBuilder();
            report.Append("Harmony patch class '").Append(patchClass.Name).Append("' failed to apply, so the feature it backs will not work.");

            // This runs inside a catch block, so it must not throw or the remaining patches would be skipped too.
            try
            {
                MethodBase original = GetOriginalMethod(patchClass);
                if (original == null)
                {
                    return report.ToString();
                }

                report.Append(" Target method: ").Append(original.FullDescription()).Append('.');

                Patches patches = Harmony.GetPatchInfo(original);
                if (patches == null)
                {
                    return report.ToString();
                }

                int startLength = report.Length;
                report.Append(" Other patches on this method (transpilers are the usual cause):");
                int conflictStart = report.Length;
                AppendPatches(report, "transpiler", patches.Transpilers, ownHarmonyId);
                AppendPatches(report, "prefix", patches.Prefixes, ownHarmonyId);
                AppendPatches(report, "postfix", patches.Postfixes, ownHarmonyId);
                AppendPatches(report, "finalizer", patches.Finalizers, ownHarmonyId);
                AppendPatches(report, "inner prefix", patches.InnerPrefixes, ownHarmonyId);
                AppendPatches(report, "inner postfix", patches.InnerPostfixes, ownHarmonyId);
                AppendPatches(report, "inner finalizer", patches.InnerFinalizers, ownHarmonyId);

                if (report.Length == conflictStart)
                {
                    report.Length = startLength;
                    report.Append(" No other mod patches this method, so another mod may be altering it indirectly.");
                }
            }
            catch (Exception ex)
            {
                report.Append(" Could not determine conflicting mods: ").Append(ex.Message);
            }

            return report.ToString();
        }

        private static MethodBase GetOriginalMethod(Type patchClass)
        {
            HarmonyMethod target = HarmonyMethodExtensions.GetMergedFromType(patchClass);
            if (target.declaringType == null || target.methodName == null)
            {
                return null;
            }

            return AccessTools.DeclaredMethod(target.declaringType, target.methodName, target.argumentTypes);
        }

        private static void AppendPatches(StringBuilder report, string kind, IEnumerable<Patch> patches, string ownHarmonyId)
        {
            foreach (Patch patch in patches)
            {
                if (patch.owner == ownHarmonyId)
                {
                    continue;
                }

                report.Append("\n  - ").Append(kind).Append(" from ").Append(GetModLabel(patch)).Append(" (harmony id '").Append(patch.owner).Append("')");
            }
        }

        private static string GetModLabel(Patch patch)
        {
            // Patch methods of other mods can be dynamic or unresolvable; one bad entry must not hide the rest.
            try
            {
                MethodInfo patchMethod = patch.PatchMethod;
                string method = patchMethod.DeclaringType.FullName + "." + patchMethod.Name;

                List<ModContentPack> mods = LoadedModManager.RunningModsListForReading;
                for (int i = 0; i < mods.Count; i++)
                {
                    if (mods[i].assemblies.loadedAssemblies.Contains(patchMethod.DeclaringType.Assembly))
                    {
                        return mods[i].Name + " [" + mods[i].PackageId + "] via " + method;
                    }
                }

                return "unknown mod via " + method;
            }
            catch (Exception)
            {
                return "unknown mod";
            }
        }
    }
}
