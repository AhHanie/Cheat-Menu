using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public sealed class SpawnPawnSelectionWindow : Window
    {
        private const string SearchControlName = "CheatMenu.SpawnPawn.SearchField";
        private const float SearchRowHeight = 34f;
        private const float RowHeight = 54f;
        private const float RowSpacing = 4f;
        private const float IconSize = 40f;
        private const float SelectButtonWidth = 86f;
        private static readonly Vector2 CachedPortraitSize = new Vector2(128f, 128f);
        private const float CachedPortraitZoom = 1.6f;
        private static readonly Dictionary<PawnKindDef, Texture2D> pawnKindPortraitCache = new Dictionary<PawnKindDef, Texture2D>();
        private static bool pawnKindPortraitCacheInitialized;

        private readonly Action<PawnKindDef> onPawnKindSelected;
        private readonly List<PawnKindDef> allPawnKinds;
        private readonly SearchableTableRenderer<PawnKindDef> tableRenderer =
            new SearchableTableRenderer<PawnKindDef>(RowHeight, RowSpacing);

        private string searchText = string.Empty;
        private bool focusSearchOnNextDraw = true;

        public SpawnPawnSelectionWindow(Action<PawnKindDef> onPawnKindSelected)
        {
            this.onPawnKindSelected = onPawnKindSelected;
            allPawnKinds = BuildPawnKindList();
            EnsurePawnKindPortraitCacheBuilt(allPawnKinds);

            doCloseX = true;
            closeOnAccept = false;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(980f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            focusSearchOnNextDraw = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "CheatMenu.SpawnPawn.SelectWindow.Title".Translate());
            Text.Font = GameFont.Small;

            Rect searchRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, SearchRowHeight);
            DrawSearchRow(searchRect);

            Rect listRect = new Rect(
                inRect.x,
                searchRect.yMax + 8f,
                inRect.width,
                inRect.yMax - (searchRect.yMax + 8f));
            DrawPawnList(listRect);
        }

        private void DrawSearchRow(Rect rect)
        {
            SearchBarWidget.DrawLabeledSearchRow(
                rect,
                "CheatMenu.Window.SearchLabel",
                "CheatMenu.SpawnPawn.SelectWindow.SearchTooltip",
                SearchControlName,
                132f,
                ref searchText,
                ref focusSearchOnNextDraw);
        }

        private void DrawPawnList(Rect outRect)
        {
            tableRenderer.Draw(
                outRect,
                allPawnKinds,
                MatchesSearch,
                DrawPawnKindRow,
                rect => Widgets.Label(rect, "CheatMenu.SpawnPawn.SelectWindow.NoMatches".Translate(searchText)));
        }

        private void DrawPawnKindRow(Rect rowRect, PawnKindDef pawnKindDef, bool drawAlt)
        {
            if (drawAlt)
            {
                Widgets.DrawAltRect(rowRect);
            }

            Widgets.DrawHighlightIfMouseover(rowRect);

            Rect iconRect = new Rect(rowRect.x + 8f, rowRect.y + ((rowRect.height - IconSize) * 0.5f), IconSize, IconSize);
            DrawPawnIcon(iconRect, pawnKindDef);

            Rect buttonRect = new Rect(rowRect.xMax - SelectButtonWidth - 8f, rowRect.y + 8f, SelectButtonWidth, rowRect.height - 16f);
            Rect infoRect = new Rect(iconRect.xMax + 10f, rowRect.y + 6f, rowRect.width - IconSize - SelectButtonWidth - 34f, rowRect.height - 12f);

            DrawPawnInfo(infoRect, pawnKindDef);
            if (Widgets.ButtonText(buttonRect, "CheatMenu.SpawnPawn.SelectWindow.SelectButton".Translate()))
            {
                SelectPawnKind(pawnKindDef);
            }

            if (Widgets.ButtonInvisible(rowRect))
            {
                SelectPawnKind(pawnKindDef);
            }
        }

        private static void DrawPawnInfo(Rect rect, PawnKindDef pawnKindDef)
        {
            string label = GetSafeLabel(pawnKindDef);
            string categoryLabel = GetCategoryLabel(pawnKindDef);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 24f), label);

            Text.Font = GameFont.Tiny;
            Widgets.Label(
                new Rect(rect.x, rect.yMax - 20f, rect.width, 20f),
                "CheatMenu.SpawnPawn.SelectWindow.InfoLine".Translate(pawnKindDef.defName, categoryLabel));
            Text.Font = GameFont.Small;
        }

        private static void DrawPawnIcon(Rect iconRect, PawnKindDef pawnKindDef)
        {
            Texture icon = GetIconTexture(pawnKindDef, out bool usedCachedPortrait);
            Color iconColor = usedCachedPortrait
                ? Color.white
                : pawnKindDef?.race?.uiIconColor ?? Color.white;

            Color previousColor = GUI.color;
            GUI.color = iconColor;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            GUI.color = previousColor;
        }

        private static Texture GetIconTexture(PawnKindDef pawnKindDef, out bool usedCachedPortrait)
        {
            Texture2D cachedPortrait;
            if (pawnKindDef != null
                && pawnKindPortraitCache.TryGetValue(pawnKindDef, out cachedPortrait)
                && cachedPortrait != null)
            {
                usedCachedPortrait = true;
                return cachedPortrait;
            }

            usedCachedPortrait = false;
            return pawnKindDef?.race?.uiIcon ?? BaseContent.BadTex;
        }

        private bool MatchesSearch(PawnKindDef pawnKindDef)
        {
            if (pawnKindDef == null)
            {
                return false;
            }

            if (searchText.NullOrEmpty())
            {
                return true;
            }

            string needle = searchText.Trim().ToLowerInvariant();
            if (needle.Length == 0)
            {
                return true;
            }

            string label = GetSafeLabel(pawnKindDef).ToLowerInvariant();
            string defName = (pawnKindDef.defName ?? string.Empty).ToLowerInvariant();
            string category = GetCategoryLabel(pawnKindDef).ToLowerInvariant();

            return label.Contains(needle) || defName.Contains(needle) || category.Contains(needle);
        }

        private void SelectPawnKind(PawnKindDef pawnKindDef)
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

        private static void EnsurePawnKindPortraitCacheBuilt(List<PawnKindDef> pawnKinds)
        {
            if (pawnKindPortraitCacheInitialized || pawnKinds == null)
            {
                return;
            }

            pawnKindPortraitCacheInitialized = true;
            for (int i = 0; i < pawnKinds.Count; i++)
            {
                PawnKindDef pawnKindDef = pawnKinds[i];
                if (pawnKindDef == null)
                {
                    continue;
                }

                if (pawnKindDef.RaceProps == null || !pawnKindDef.RaceProps.Humanlike)
                {
                    continue;
                }

                Texture2D portraitCopy = TryBuildPortraitCopy(pawnKindDef);
                if (portraitCopy != null)
                {
                    pawnKindPortraitCache[pawnKindDef] = portraitCopy;
                }
            }
        }

        private static Texture2D TryBuildPortraitCopy(PawnKindDef pawnKindDef)
        {
            Pawn generatedPawn = null;
            try
            {
                if (pawnKindDef?.RaceProps == null || !pawnKindDef.RaceProps.Humanlike)
                {
                    return null;
                }

                Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionDef);
                generatedPawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);

                Texture portraitTexture = PortraitsCache.Get(generatedPawn, CachedPortraitSize, Rot4.South, Vector3.zero, CachedPortraitZoom);
                Texture2D portraitCopy = CopyTexture(portraitTexture);
                if (portraitCopy == null)
                {
                    return null;
                }

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
            if (sourceTexture == null)
            {
                return null;
            }

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
            return pawnKindDef != null && pawnKindDef.showInDebugSpawner;
        }

        private static string GetSafeLabel(PawnKindDef pawnKindDef)
        {
            return (pawnKindDef?.label ?? pawnKindDef?.defName ?? string.Empty).CapitalizeFirst();
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
