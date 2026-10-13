using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class ToggleCheatMetadata
    {
        private Texture2D cachedIcon;
        private bool attemptedIconLoad;

        public ToggleCheatMetadata(
            string labelKey,
            string descriptionKey = null,
            string categoryKey = null,
            string iconPath = null)
        {
            LabelKey = labelKey;
            DescriptionKey = descriptionKey;
            CategoryKey = categoryKey;
            IconPath = iconPath;
        }

        public string Key { get; internal set; }

        public string LabelKey { get; }

        public string DescriptionKey { get; }

        public string CategoryKey { get; }

        public string IconPath { get; }

        public string GetLabel()
        {
            return LabelKey.NullOrEmpty() ? Key : LabelKey.Translate().ToString();
        }

        public string GetDescription()
        {
            return DescriptionKey.NullOrEmpty() ? string.Empty : DescriptionKey.Translate().ToString();
        }

        public string GetCategoryOrDefault()
        {
            return CategoryKey.NullOrEmpty()
                ? "CheatMenu.Category.Uncategorized".Translate().ToString()
                : CategoryKey.Translate().ToString();
        }

        public Texture2D GetIcon()
        {
            if (attemptedIconLoad || IconPath.NullOrEmpty())
            {
                return cachedIcon;
            }

            attemptedIconLoad = true;
            cachedIcon = ContentFinder<Texture2D>.Get(IconPath, false);
            return cachedIcon;
        }
    }
}
