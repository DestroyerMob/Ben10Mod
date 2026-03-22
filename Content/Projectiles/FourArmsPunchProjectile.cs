using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class FourArmsPunchProjectile : PunchProjectile {
    protected override Color Background => Color.Black;
    protected override Color Foreground => Color.Red;
}
