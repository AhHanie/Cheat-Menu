using Verse;

namespace Cheat_Menu
{
    public sealed class VREAndroidsModCompat : ModCompat
    {
        public override bool IsEnabled()
        {
            return ModsConfig.IsActive(VREAndroidsToggleCheats.PackageId);
        }

        public override void Init()
        {
            VREAndroidsReflection.EnsureInitialized();
            VREAndroidsToggleCheats.Register();
        }

        public override string GetModPackageIdentifier()
        {
            return VREAndroidsToggleCheats.PackageId;
        }
    }
}
