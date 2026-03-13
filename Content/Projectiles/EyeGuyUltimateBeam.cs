using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Ben10Mod.Content.Projectiles.UltimateAttacks;

namespace Ben10Mod.Content.Projectiles
{
    public class EyeGuyUltimateBeam : ChannelBeamUltimateProjectile
    {
        protected override float MaxLength => 2600f;
        protected override float BeamThickness => 28f;
        protected override float StartOffset => 52f;
        protected override int MinEnergyToSustain => 10;
        protected override Color BeamColor => new(60, 255, 140);
        protected override int EndDustType => DustID.GreenTorch;
        protected override float LightR => 0.2f;
        protected override float LightG => 1.6f;
        protected override float LightB => 0.6f;
    }
}
