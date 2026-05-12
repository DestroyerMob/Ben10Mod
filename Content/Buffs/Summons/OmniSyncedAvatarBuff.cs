using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Summons;

public class OmniSyncedAvatarBuff : ModBuff {
    public override string Texture => "Ben10Mod/Content/Interface/EmptyAlien";

    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<OmniSyncedAvatarProjectile>()] > 0) {
            player.buffTime[buffIndex] = 18000;
            return;
        }

        player.DelBuff(buffIndex);
        buffIndex--;
    }
}
