using Verse;

namespace Cheat_Menu
{
    public sealed class SettlementServicesModCompat : ModCompat
    {
        private const string PackageId = "sk.settlementservices";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive(PackageId) && SettlementServicesReflection.IsFullyResolved;
        }

        public override void Init()
        {
            SettlementServicesCheats.Register();
        }

        public override string GetModPackageIdentifier()
        {
            return PackageId;
        }
    }
}
