using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Cheat_Menu
{
    public static class CheatRegistry
    {
        private static readonly object syncRoot = new object();
        private static readonly Dictionary<string, CheatDefinition> cheatsById =
            new Dictionary<string, CheatDefinition>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Other mods should call this at startup (for example in a static constructor)
        /// to expose cheats inside this framework window.
        /// Example:
        /// CheatRegistry.Register("MyMod.SpawnSilver", "MyMod.Cheat.SpawnSilver.Label", "MyMod.Cheat.SpawnSilver.Desc", builder => builder.AddAction(...));
        /// </summary>
        public static bool Register(CheatDefinition cheat, bool replaceExisting = false)
        {
            if (cheat == null)
            {
                throw new ArgumentNullException(nameof(cheat));
            }

            lock (syncRoot)
            {
                CheatDefinition existingCheat;
                if (cheatsById.TryGetValue(cheat.Id, out existingCheat) && !replaceExisting)
                {
                    UserLogger.Warning("Duplicate cheat id '" + cheat.Id + "' ignored.");
                    return false;
                }

                cheatsById[cheat.Id] = cheat;
                return true;
            }
        }

        public static CheatDefinition Register(
            string id,
            string labelKey,
            string descriptionKey,
            Action<CheatBuilder> configure,
            bool replaceExisting = false)
        {
            CheatBuilder builder = CheatBuilder.Create(id, labelKey, descriptionKey);
            configure?.Invoke(builder);
            CheatDefinition cheat = builder.Build();
            Register(cheat, replaceExisting);
            return cheat;
        }

        public static bool TryGet(string id, out CheatDefinition cheat)
        {
            if (id.NullOrEmpty())
            {
                cheat = null;
                return false;
            }

            lock (syncRoot)
            {
                return cheatsById.TryGetValue(id, out cheat);
            }
        }

        public static bool Unregister(string id)
        {
            if (id.NullOrEmpty())
            {
                return false;
            }

            lock (syncRoot)
            {
                return cheatsById.Remove(id);
            }
        }

        public static IReadOnlyList<CheatDefinition> GetAllCheats()
        {
            lock (syncRoot)
            {
                return cheatsById.Values
                    .OrderBy(cheat => cheat.GetCategoryOrDefault())
                    .ThenBy(cheat => cheat.GetLabel())
                    .ToList();
            }
        }
    }
}
