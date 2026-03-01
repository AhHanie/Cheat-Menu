using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace Cheat_Menu
{
    public class CheatsStatOffsetsEditorWindow : Window
    {
        private const float RowHeight = 32f;
        private const float BottomButtonHeight = 35f;
        private const float Gap = 6f;
        private const float RemoveButtonWidth = 30f;
        private const float AddButtonWidth = 32f;
        private const float TitleIconSize = 32f;
        private const float LabelWidth = 360f;
        private const float ValueMin = -9999f;
        private const float ValueMax = 9999f;

        private readonly ThingWithComps thing;
        private readonly CompCheatStatOffsets comp;
        private readonly Dictionary<StatDef, float> workingStatOffsets;
        private readonly List<StatDef> statDefs;
        private readonly Dictionary<StatDef, string> buffers = new Dictionary<StatDef, string>();

        private Vector2 scrollPos;
        private float viewHeight;

        public CheatsStatOffsetsEditorWindow(ThingWithComps thing, CompCheatStatOffsets comp)
        {
            this.thing = thing;
            this.comp = comp;
            workingStatOffsets = comp.StatOffsets.ToDictionary(entry => entry.Key, entry => entry.Value);
            statDefs = workingStatOffsets.Keys.ToList();
            statDefs.Sort(CompareStatDefs);

            doCloseX = true;
            closeOnCancel = true;
            absorbInputAroundWindow = true;
            forcePause = true;
            draggable = true;
        }

        public override Vector2 InitialSize => new Vector2(860f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            string title = "CheatMenu.Cheats.EditStatOffsets.Window.EditingStatsFor".Translate(thing.LabelCap);
            float titleTextWidth = inRect.width - TitleIconSize - Gap;
            float titleTextHeight = Text.CalcHeight(title, titleTextWidth);
            float titleHeight = Mathf.Max(TitleIconSize, titleTextHeight);
            float titleTop = inRect.y;
            Rect iconRect = new Rect(inRect.x, titleTop + ((titleHeight - TitleIconSize) * 0.5f), TitleIconSize, TitleIconSize);
            Rect titleRect = new Rect(iconRect.xMax + Gap, titleTop + ((titleHeight - titleTextHeight) * 0.5f), titleTextWidth, titleTextHeight);

            Widgets.ThingIcon(iconRect, thing);
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;

            Rect bottomButtonsRect = new Rect(inRect.x, inRect.yMax - BottomButtonHeight, inRect.width, BottomButtonHeight);
            float contentTop = titleRect.yMax + Gap;
            Rect scrollOuter = new Rect(inRect.x, contentTop, inRect.width, bottomButtonsRect.yMin - contentTop - Gap);
            Rect scrollView = new Rect(0f, 0f, scrollOuter.width - 16f, viewHeight);

            StatDef statToRemove = null;
            float innerY = 0f;

            Widgets.BeginScrollView(scrollOuter, ref scrollPos, scrollView);

            if (statDefs.Count == 0)
            {
                Rect noStatsRect = new Rect(0f, innerY + 4f, scrollView.width, 24f);
                Widgets.Label(noStatsRect, "CheatMenu.Cheats.EditStatOffsets.Window.NoStats".Translate());
                innerY += 30f;
            }

            for (int i = 0; i < statDefs.Count; i++)
            {
                StatDef statDef = statDefs[i];
                Rect rowRect = new Rect(0f, innerY, scrollView.width, RowHeight);
                DrawStatRow(rowRect, statDef, ref statToRemove);
                innerY += RowHeight + 2f;
            }

            Rect addRowRect = new Rect(0f, innerY, scrollView.width, RowHeight);
            DrawAddRow(addRowRect);
            innerY += RowHeight;

            Widgets.EndScrollView();

            viewHeight = innerY + 4f;

            if (statToRemove != null)
            {
                workingStatOffsets.Remove(statToRemove);
                statDefs.Remove(statToRemove);
                buffers.Remove(statToRemove);
            }

            DrawBottomButtons(bottomButtonsRect);
        }

        private void DrawStatRow(Rect rowRect, StatDef statDef, ref StatDef statToRemove)
        {
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }

            Rect labelRect = new Rect(rowRect.x, rowRect.y + 4f, LabelWidth, rowRect.height - 8f);
            Rect valueRect = new Rect(labelRect.xMax + Gap, rowRect.y + 4f, 120f, rowRect.height - 8f);
            Rect removeRect = new Rect(valueRect.xMax + Gap, rowRect.y + 4f, RemoveButtonWidth, rowRect.height - 8f);

            Widgets.Label(labelRect, statDef.LabelCap);

            float currentValue = workingStatOffsets[statDef];
            if (!buffers.TryGetValue(statDef, out string buffer))
            {
                buffer = currentValue.ToString(CultureInfo.InvariantCulture);
                buffers[statDef] = buffer;
            }

            string editedBuffer = Widgets.TextField(valueRect, buffers[statDef]);
            buffers[statDef] = editedBuffer;

            if (TryParseOffset(editedBuffer, out float editedValue))
            {
                workingStatOffsets[statDef] = Mathf.Clamp(editedValue, ValueMin, ValueMax);
            }

            if (Widgets.ButtonImage(removeRect, TexButton.Delete))
            {
                statToRemove = statDef;
            }
        }

        private void DrawAddRow(Rect rowRect)
        {
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }

            Rect plusRect = new Rect(rowRect.x, rowRect.y + 4f, AddButtonWidth, rowRect.height - 8f);
            Rect labelRect = new Rect(plusRect.xMax + Gap, rowRect.y + 4f, rowRect.width - plusRect.width - Gap, rowRect.height - 8f);

            if (Widgets.ButtonText(plusRect, "+"))
            {
                Find.WindowStack.Add(new CheatsStatOffsetsStatDefSelectionWindow(workingStatOffsets.Keys, OnStatDefSelected));
            }

            Widgets.Label(labelRect, "CheatMenu.Cheats.EditStatOffsets.Window.AddNewStat".Translate());
        }

        private void DrawBottomButtons(Rect rect)
        {
            float half = (rect.width - Gap) / 2f;

            Rect doneRect = new Rect(rect.x, rect.y, half, rect.height);
            Rect clearAllRect = new Rect(doneRect.xMax + Gap, rect.y, half, rect.height);

            if (Widgets.ButtonText(doneRect, "CheatMenu.Cheats.EditStatOffsets.Window.Done".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(clearAllRect, "CheatMenu.Cheats.EditStatOffsets.Window.ClearAll".Translate()))
            {
                workingStatOffsets.Clear();
                statDefs.Clear();
                buffers.Clear();
            }
        }

        private void OnStatDefSelected(StatDef statDef)
        {
            workingStatOffsets[statDef] = 0f;
            if (!statDefs.Contains(statDef))
            {
                statDefs.Add(statDef);
                statDefs.Sort(CompareStatDefs);
            }

            buffers[statDef] = "0";
        }

        public override void PreClose()
        {
            base.PreClose();
            ApplyWorkingOffsetsToComp();
        }

        private void ApplyWorkingOffsetsToComp()
        {
            comp.ClearOffsets();

            foreach (KeyValuePair<StatDef, float> entry in workingStatOffsets)
            {
                comp.SetOffset(entry.Key, entry.Value);
            }
        }

        private static int CompareStatDefs(StatDef left, StatDef right)
        {
            int labelCompare = string.Compare(left.LabelCap, right.LabelCap);
            if (labelCompare != 0)
            {
                return labelCompare;
            }

            return string.Compare(left.defName, right.defName);
        }

        private static bool TryParseOffset(string rawValue, out float value)
        {
            string normalized = rawValue?.Trim();
            if (string.IsNullOrEmpty(normalized))
            {
                value = 0f;
                return false;
            }

            normalized = normalized.Replace(',', '.');
            return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
