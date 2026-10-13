using Verse;

namespace Cheat_Menu
{
    public sealed class Infusion2ModCompat : ModCompat
    {
        private const string PackageId = "sk.infusion";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive(PackageId);
        }

        public override void Init()
        {
            Infusion2Reflection.EnsureInitialized();
            Infusion2Cheats.Register();
        }

        public override string GetModPackageIdentifier()
        {
            return PackageId;
        }
    }
}
