using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GhostFreakPossesionProjectile : ModProjectile {
    private const int BasePossessionDuration = 320;
    private const int HauntedPossessionBonus = 120;
    private const int FearStackPossessionBonus = 28;
    private const int MaxPossessionDuration = 600;

    public override void SetDefaults() {
        Projectile.width   = 26;
        Projectile.height  = 44;
        Projectile.aiStyle = ProjAIStyleID.Arrow;

        AIType                 = ProjectileID.Bullet;
        Projectile.friendly    = true;
        Projectile.timeLeft    = 64;
        Projectile.tileCollide = false;
            
        Projectile.DamageType = ModContent.GetInstance<HeroDamage>();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int fearStacks = state.GetGhostFreakFearStacks(Projectile.owner);
        if (fearStacks > 0) {
            modifiers.SourceDamage *= 1f + fearStacks * 0.12f;
            modifiers.ArmorPenetration += fearStacks * 2;
        }

        if (state.IsGhostFreakHauntedFor(Projectile.owner)) {
            modifiers.SourceDamage *= 1.35f;
            modifiers.ArmorPenetration += 10;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player player = Main.player[Projectile.owner];
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.inPossessionMode)
            return;

        int possessionDuration = ResolvePossessionDuration(target, Projectile.owner);
        target.GetGlobalNPC<AlienIdentityGlobalNPC>().ConsumeGhostFreakHaunt(Projectile.owner);
        if (!target.boss)
            target.AddBuff(BuffID.Confused, 180);

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            if (Projectile.owner != Main.myPlayer)
                return;

            omp.BeginPossession(target.whoAmI, player.position, possessionDuration, shouldSync: false);

            ModPacket packet = ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().GetPacket();
            packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.RequestGhostFreakPossession);
            packet.Write(target.whoAmI);
            packet.Send();
            return;
        }

        omp.BeginPossession(target.whoAmI, player.position, possessionDuration);
    }

    public static int ResolvePossessionDuration(NPC target, int owner) {
        AlienIdentityGlobalNPC state = target.GetGlobalNPC<AlienIdentityGlobalNPC>();
        int fearStacks = state.GetGhostFreakFearStacks(owner);
        int duration = BasePossessionDuration + fearStacks * FearStackPossessionBonus;
        if (state.IsGhostFreakHauntedFor(owner))
            duration += HauntedPossessionBonus;

        return Utils.Clamp(duration, BasePossessionDuration, MaxPossessionDuration);
    }

    public override bool PreDraw(ref Color lightColor) {
        lightColor.A /= 2;
        return base.PreDraw(ref lightColor);
    }

    public override void AI() {
        for (int i = 0; i < 5; i++) {
            int dustNum = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, 0, 0, 1, i % 2 == 0 ? Color.White : Color.Black, 3);
            Main.dust[dustNum].noGravity = true;
        }
    }
}
