using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.NPCs;

public class BuzzShockTagGlobalNPC : GlobalNPC {
    public override bool InstancePerEntity => false;

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
        if (!npc.HasBuff(ModContent.BuffType<BuzzShockTagBuff>()))
            return;

        if (projectile.type != ModContent.ProjectileType<BuzzShockMinionProjectile>())
            return;

        modifiers.FlatBonusDamage += 8f;
    }
}
