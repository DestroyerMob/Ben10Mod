using System;
using Ben10Mod.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Projectiles;

public class GhostFreakPossesionProjectile : ModProjectile {
    public override void SetDefaults() {
        Projectile.width   = 26;
        Projectile.height  = 44;
        Projectile.aiStyle = ProjAIStyleID.Arrow;

        AIType                 = ProjectileID.Bullet;
        Projectile.friendly    = true;
        Projectile.timeLeft    = 64;
        Projectile.tileCollide = false;
            
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
    }


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player player = Main.player[Projectile.owner];
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.inPossessionMode)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            if (Projectile.owner != Main.myPlayer)
                return;

            omp.BeginPossession(target.whoAmI, player.position, shouldSync: false);

            ModPacket packet = ModContent.GetInstance<global::Ben10Mod.Ben10Mod>().GetPacket();
            packet.Write((byte)global::Ben10Mod.Ben10Mod.MessageType.RequestGhostFreakPossession);
            packet.Write(target.whoAmI);
            packet.Send();
            return;
        }

        omp.BeginPossession(target.whoAmI, player.position);
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
