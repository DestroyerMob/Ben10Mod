using Ben10Mod.Content.Transformations.ChromaStone;
using Ben10Mod.Keybinds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class ChromaStoneGuardProjectile : ModProjectile {
    private ref float HoldTicks => ref Projectile.localAI[0];
    private ref float Released => ref Projectile.localAI[1];

    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

    public override void SetDefaults() {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 2;
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        Player owner = Main.player[Projectile.owner];
        OmnitrixPlayer omp = owner.GetModPlayer<OmnitrixPlayer>();
        ChromaStoneStatePlayer state = owner.GetModPlayer<ChromaStoneStatePlayer>();
        if (!owner.active || owner.dead || omp.currentTransformationId != ChromaStoneStatePlayer.TransformationId) {
            Release(owner, state);
            Projectile.Kill();
            return;
        }

        Projectile.timeLeft = 2;
        Projectile.Center = owner.Center;
        owner.velocity *= 0.72f;
        owner.itemTime = 2;
        owner.itemAnimation = 2;

        float holdRatio = HoldTicks / ChromaStoneStatePlayer.GuardMaxHoldTicks;
        state.RegisterGuardFrame(holdRatio);

        bool stillHolding = owner.whoAmI == Main.myPlayer &&
                            KeybindSystem.SecondaryAbility?.Current == true &&
                            HoldTicks < ChromaStoneStatePlayer.GuardMaxHoldTicks &&
                            !owner.noItems &&
                            !owner.CCed;
        if (stillHolding) {
            HoldTicks++;

            if (!Main.dedServ && Main.rand.NextBool(2)) {
                Color prismColor = ChromaStonePrismHelper.GetSpectrumColor(holdRatio * 2.2f + Projectile.identity * 0.08f, 1.06f);
                Dust dust = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(20f, 28f), DustID.GemDiamond,
                    Main.rand.NextVector2Circular(1.6f, 1.6f), 95, prismColor, Main.rand.NextFloat(0.95f, 1.24f));
                dust.noGravity = true;
            }

            return;
        }

        Release(owner, state);
        Projectile.Kill();
    }

    private void Release(Player owner, ChromaStoneStatePlayer state) {
        if (Released > 0f || owner.whoAmI != Main.myPlayer)
            return;

        Released = 1f;
        Vector2 direction = Main.MouseWorld - owner.Center;
        if (direction.LengthSquared() < 0.0001f)
            direction = new Vector2(owner.direction == 0 ? 1 : owner.direction, 0f);

        state.ReleaseGuardBurst(direction);
    }
}
