using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class SpawnPawnSelectionWindow : SearchableSelectionWindow<PawnKindDef>
    {
        private const string SearchControlNameConst = "CheatMenu.SpawnPawn.SearchField";
        private static readonly Vector2 CachedPortraitSize = new Vector2(128f, 128f);
        private const float CachedPortraitZoom = 1.6f;
        private const int PortraitCacheBuildsPerFrame = 1;
        private static Dictionary<PawnKindDef, Texture2D> pawnKindPortraitCache = new Dictionary<PawnKindDef, Texture2D>();
        private static List<PawnKindDef> pawnKindPortraitBuildQueue;
        private static int pawnKindPortraitBuildIndex;
        private static bool pawnKindPortraitCacheBuildStarted;
        private static bool pawnKindPortraitCacheInitialized;

        private readonly Action<PawnKindDef> onPawnKindSelected;
        private readonly List<PawnKindDef> allPawnKinds;

        public SpawnPawnSelectionWindow(Action<PawnKindDef> onPawnKindSelected)
            : base(new Vector2(980f, 700f))
        {
            this.onPawnKindSelected = onPawnKindSelected;
            allPawnKinds = BuildPawnKindList();
            StartPawnKindPortraitCacheBuild(allPawnKinds);
        }

        protected override bool UseIconColumn => true;

        protected override float IconSize => 40f;

        protected override float SelectButtonWidth => 86f;

        protected override string TitleKey => "CheatMenu.SpawnPawn.SelectWindow.Title";

        protected override string SearchTooltipKey => "CheatMenu.SpawnPawn.SelectWindow.SearchTooltip";

        protected override string SearchControlName => SearchControlNameConst;

        protected override string NoMatchesKey => "CheatMenu.SpawnPawn.SelectWindow.NoMatches";

        protected override string SelectButtonKey => "CheatMenu.SpawnPawn.SelectWindow.SelectButton";

        protected override IReadOnlyList<PawnKindDef> Options => allPawnKinds;

        protected override void BeforeDrawWindowContents()
        {
            ProcessPawnKindPortraitCacheBuildBatch();
        }

        protected override void DrawItemInfo(Rect rect, PawnKindDef pawnKindDef)
        {
            string categoryLabel = GetCategoryLabel(pawnKindDef);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), pawnKindDef.LabelCap);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.SpawnPawn.SelectWindow.InfoLine".Translate(pawnKindDef.defName, categoryLabel));
            Text.Font = GameFont.Small;
        }

        protected override void DrawItemIcon(Rect iconRect, PawnKindDef pawnKindDef)
        {
            Texture icon = GetIconTexture(pawnKindDef, out bool usedCachedPortrait);
            Color iconColor = usedCachedPortrait
                ? Color.white
                : pawnKindDef.race.uiIconColor;

            Color previousColor = GUI.color;
            GUI.color = iconColor;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private static Texture GetIconTexture(PawnKindDef pawnKindDef, out bool usedCachedPortrait)
        {
            Texture2D cachedPortrait;
            if (pawnKindPortraitCache.TryGetValue(pawnKindDef, out cachedPortrait))
            {
                usedCachedPortrait = true;
                return cachedPortrait;
            }

            usedCachedPortrait = false;
            return pawnKindDef.race.uiIcon;
        }

        protected override bool MatchesSearch(PawnKindDef pawnKindDef, string needle)
        {
            if (needle.Length == 0)
            {
                return true;
            }

            string label = pawnKindDef.label.ToLowerInvariant();
            string defName = pawnKindDef.defName.ToLowerInvariant();
            string category = GetCategoryLabel(pawnKindDef).ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle) || category.Contains(needle);
        }

        protected override void OnItemSelected(PawnKindDef pawnKindDef)
        {
            Close();
            onPawnKindSelected?.Invoke(pawnKindDef);
        }

        private static List<PawnKindDef> BuildPawnKindList()
        {
            return DefDatabase<PawnKindDef>.AllDefsListForReading
                .Where(IsSelectablePawnKind)
                .OrderBy(GetCategorySortOrder)
                .ThenBy((PawnKindDef pawnKindDef) => pawnKindDef.defName)
                .ToList();
        }

        private static void StartPawnKindPortraitCacheBuild(List<PawnKindDef> pawnKinds)
        {
            if (pawnKindPortraitCacheInitialized || pawnKindPortraitCacheBuildStarted)
            {
                return;
            }

            pawnKindPortraitBuildQueue = pawnKinds
                .Where(pawnKindDef => pawnKindDef.RaceProps.Humanlike)
                .ToList();
            pawnKindPortraitBuildIndex = 0;
            pawnKindPortraitCacheBuildStarted = true;

            if (pawnKindPortraitBuildQueue.Count == 0)
            {
                pawnKindPortraitCacheInitialized = true;
                pawnKindPortraitCacheBuildStarted = false;
                pawnKindPortraitBuildQueue = null;
            }
        }

        private static void ProcessPawnKindPortraitCacheBuildBatch()
        {
            if (!pawnKindPortraitCacheBuildStarted || pawnKindPortraitCacheInitialized || pawnKindPortraitBuildQueue == null)
            {
                return;
            }

            int builtThisFrame = 0;
            while (builtThisFrame < PortraitCacheBuildsPerFrame && pawnKindPortraitBuildIndex < pawnKindPortraitBuildQueue.Count)
            {
                PawnKindDef pawnKindDef = pawnKindPortraitBuildQueue[pawnKindPortraitBuildIndex];
                pawnKindPortraitBuildIndex++;

                Texture2D portraitCopy = TryBuildPortraitCopy(pawnKindDef);
                if (portraitCopy != null)
                {
                    pawnKindPortraitCache[pawnKindDef] = portraitCopy;
                }

                builtThisFrame++;
            }

            if (pawnKindPortraitBuildIndex >= pawnKindPortraitBuildQueue.Count)
            {
                pawnKindPortraitCacheInitialized = true;
                pawnKindPortraitCacheBuildStarted = false;
                pawnKindPortraitBuildQueue = null;
                pawnKindPortraitBuildIndex = 0;
            }
        }

        private static Texture2D TryBuildPortraitCopy(PawnKindDef pawnKindDef)
        {
            Pawn generatedPawn = null;
            try
            {
                Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionDef);
                generatedPawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
                Texture portraitTexture = PortraitsCache.Get(generatedPawn, CachedPortraitSize, Rot4.South, Vector3.zero, CachedPortraitZoom);
                Texture2D portraitCopy = CopyTexture(portraitTexture);
                portraitCopy.name = "CheatMenu.SpawnPawnPortrait." + pawnKindDef.defName;
                portraitCopy.hideFlags = HideFlags.HideAndDontSave;
                return portraitCopy;
            }
            catch (Exception ex)
            {
                UserLogger.Exception(ex, "Failed to build cached portrait for PawnKindDef '" + pawnKindDef.defName + "'.");
                return null;
            }
            finally
            {
                if (generatedPawn != null && !generatedPawn.Destroyed)
                {
                    generatedPawn.Destroy();
                }
            }
        }

        private static Texture2D CopyTexture(Texture sourceTexture)
        {
            int width = Mathf.Max(1, sourceTexture.width);
            int height = Mathf.Max(1, sourceTexture.height);
            RenderTexture tempRenderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousActiveRenderTexture = RenderTexture.active;

            try
            {
                Graphics.Blit(sourceTexture, tempRenderTexture);
                RenderTexture.active = tempRenderTexture;

                Texture2D copy = new Texture2D(width, height, TextureFormat.ARGB32, false);
                copy.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                copy.Apply();
                return copy;
            }
            finally
            {
                RenderTexture.active = previousActiveRenderTexture;
                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }
        }

        private static bool IsSelectablePawnKind(PawnKindDef pawnKindDef)
        {
            return pawnKindDef.showInDebugSpawner;
        }

        private static int GetCategorySortOrder(PawnKindDef pawnKindDef)
        {
            string categoryKey = SpawningCheats.GetPawnKindCategoryKey(pawnKindDef);
            if (categoryKey == "CheatMenu.SpawnPawn.Category.Humanlike")
            {
                return 0;
            }

            if (categoryKey == "CheatMenu.SpawnPawn.Category.Animal")
            {
                return 1;
            }

            if (categoryKey == "CheatMenu.SpawnPawn.Category.Mechanoid")
            {
                return 2;
            }

            if (categoryKey == "CheatMenu.SpawnPawn.Category.Insect")
            {
                return 3;
            }

            return 4;
        }

        private static string GetCategoryLabel(PawnKindDef pawnKindDef)
        {
            return SpawningCheats.GetPawnKindCategoryKey(pawnKindDef).Translate().ToString();
        }
    }
}
