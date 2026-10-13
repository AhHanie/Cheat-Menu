using Verse;

namespace Cheat_Menu
{
    public sealed class GravshipRaidsModCompat : ModCompat
    {
        private const string PackageId = "sk.gravshipraids";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive(PackageId) && GravshipRaidsReflection.IsFullyResolved;
        }

        public override void Init()
        {
            GravshipRaidsCheats.Register();
        }

        public override string GetModPackageIdentifier()
        {
            return PackageId;
        }
    }
}
