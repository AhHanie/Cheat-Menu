using System;
using System.Collections.Generic;
using Verse;

namespace Cheat_Menu
{
    public abstract class ModCompat
    {
        private static readonly List<string> registeredCompatModNames = new List<string>();

        public abstract bool IsEnabled();

        public abstract void Init();

        public abstract string GetModPackageIdentifier();

        public static IReadOnlyList<string> GetRegisteredCompatModNames()
        {
            return registeredCompatModNames;
        }

        public static void RegisterCompatMod(ModCompat compat)
        {
            string packageId = compat.GetModPackageIdentifier();
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }

            ModMetaData meta = ModLister.GetActiveModWithIdentifier(packageId);
            if (meta == null)
            {
                return;
            }

            string displayName = meta.Name;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return;
            }

            if (!registeredCompatModNames.Contains(displayName))
            {
                registeredCompatModNames.Add(displayName);
            }
        }
    }
}
