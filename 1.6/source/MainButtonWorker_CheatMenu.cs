using RimWorld;
using Verse;

namespace Cheat_Menu
{
    public class MainButtonWorker_CheatMenu : MainButtonWorker_ToggleTab
    {
        public override bool Visible
        {
            get { return base.Visible && (!ModSettings.ShowMainButtonOnlyInDevMode || Prefs.DevMode); }
        }
    }
}
