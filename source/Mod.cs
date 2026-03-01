using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "CheatMenu.LoadingLabel", doAsynchronously: true, null);
        }

        public void Init()
        {
            GetSettings<ModSettings>();
            CheatStatOffsetsCompInjector.Inject();
            new Harmony("sk.cheatmenu").PatchAll();
            BaseCheatsBootstrap.RegisterAll();
            ToggleCheatBoostrap.RegisterAll();
        }

        public override string SettingsCategory()
        {
            return "CheatMenu.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
