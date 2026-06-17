using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class WildVineTethered : ModBuff {
    public override string Texture => "Ben10Mod/Content/Interface/EmptyAlien";

    public override void SetStaticDefaults() {
        BuffID.Sets.LongerExpertDebuff[Type] = false;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.velocity *= 0.94f;
        npc.color = Color.Lerp(npc.color, new Color(95, 190, 85), 0.42f);
    }

    public override bool RightClick(int buffIndex) => false;
}
