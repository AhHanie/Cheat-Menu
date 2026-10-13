using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Verse;

namespace Cheat_Menu
{
    public static class Infusion2Reflection
    {
        private const string InfusionDefTypeName = "Infusion.InfusionDef";
        private const string CompInfusionTypeName = "Infusion.CompInfusion";
        private const string CompInfusionExtensionsTypeName = "Infusion.CompInfusionExtensions";

        private static bool initialized;

        private static Type infusionDefType;
        private static Type compInfusionType;
        private static Type compInfusionExtensionsType;
        private static MethodInfo activeForUseMethod;
        private static MethodInfo addInfusionMethod;
        private static MethodInfo removeInfusionMethod;
        private static MethodInfo setInfusionsMethod;
        private static PropertyInfo infusionsProperty;
        private static MethodInfo thingWithCompsTryGetCompGenericMethodDefinition;
        private static MethodInfo enumerableEmptyMethodDefinition;

        public static List<Def> GetActiveInfusionDefs()
        {
            EnsureInitialized();

            List<Def> result = new List<Def>();
            IEnumerable infusionDefs = GetAllDefsForType(infusionDefType);
            foreach (object defObj in infusionDefs)
            {
                Def def = defObj as Def;

                if ((bool)activeForUseMethod.Invoke(null, new object[] { def }))
                {
                    result.Add(def);
                }
            }

            return result
                .OrderBy(def => def.defName)
                .ToList();
        }

        private static IEnumerable GetAllDefsForType(Type defType)
        {
            Type defDatabaseType = typeof(DefDatabase<>).MakeGenericType(defType);
            PropertyInfo allDefsProperty = AccessTools.Property(defDatabaseType, "AllDefsListForReading");
            return allDefsProperty.GetValue(null, null) as IEnumerable;
        }

        public static object GetCompInfusion(Thing thing)
        {
            if (!(thing is ThingWithComps thingWithComps))
            {
                return null;
            }

            MethodInfo tryGetCompMethod = thingWithCompsTryGetCompGenericMethodDefinition.MakeGenericMethod(compInfusionType);
            return tryGetCompMethod.Invoke(thingWithComps, null);
        }

        public static List<Def> GetInfusions(object compInfusion)
        {
            IEnumerable infusions = infusionsProperty.GetValue(compInfusion, null) as IEnumerable;

            List<Def> result = new List<Def>();
            foreach (object infusion in infusions)
            {
                result.Add(infusion as Def);
            }

            return result;
        }

        public static bool AddInfusion(object compInfusion, Def infusionDef)
        {
            addInfusionMethod.Invoke(null, new object[] { compInfusion, infusionDef });
            return true;
        }

        public static bool RemoveInfusion(object compInfusion, Def infusionDef)
        {
            removeInfusionMethod.Invoke(null, new object[] { compInfusion, infusionDef });
            return true;
        }

        public static bool RemoveAllInfusions(object compInfusion)
        {
            MethodInfo enumerableEmptyMethod = enumerableEmptyMethodDefinition.MakeGenericMethod(infusionDefType);
            object emptyInfusions = enumerableEmptyMethod.Invoke(null, null);
            setInfusionsMethod.Invoke(compInfusion, new object[] { emptyInfusions, false });
            return true;
        }

        public static string GetInfusionLabel(Def infusionDef)
        {
            return infusionDef.LabelCap;
        }

        public static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            infusionDefType = AccessTools.TypeByName(InfusionDefTypeName);
            compInfusionType = AccessTools.TypeByName(CompInfusionTypeName);
            compInfusionExtensionsType = AccessTools.TypeByName(CompInfusionExtensionsTypeName);

            if (infusionDefType == null || compInfusionType == null || compInfusionExtensionsType == null)
            {
                return;
            }

            activeForUseMethod = AccessTools.Method(infusionDefType, "ActiveForUse", new[] { infusionDefType });
            addInfusionMethod = AccessTools.Method(compInfusionExtensionsType, "AddInfusion", new[] { compInfusionType, infusionDefType });
            removeInfusionMethod = AccessTools.Method(compInfusionExtensionsType, "RemoveInfusion", new[] { compInfusionType, infusionDefType });
            setInfusionsMethod = AccessTools.Method(compInfusionType, "SetInfusions", new[] { typeof(IEnumerable<>).MakeGenericType(infusionDefType), typeof(bool) });
            infusionsProperty = AccessTools.Property(compInfusionType, "Infusions");

            thingWithCompsTryGetCompGenericMethodDefinition = typeof(ThingWithComps)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(
                    method =>
                        method.Name == "GetComp" &&
                        method.IsGenericMethodDefinition &&
                        method.GetParameters().Length == 0);

            enumerableEmptyMethodDefinition = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(
                    method =>
                        method.Name == "Empty" &&
                        method.IsGenericMethodDefinition &&
                        method.GetParameters().Length == 0);
        }
    }
}
