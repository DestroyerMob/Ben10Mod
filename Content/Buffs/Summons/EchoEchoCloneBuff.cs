using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Summons;

public class EchoEchoCloneBuff : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<EchoEchoCloneProjectile>()] > 0 &&
            player.GetModPlayer<OmnitrixPlayer>().currentTransformationId == "Ben10Mod:EchoEcho") {
            player.buffTime[buffIndex] = 18000;
        }
        else {
            player.DelBuff(buffIndex);
            buffIndex--;
        }
    }
}
