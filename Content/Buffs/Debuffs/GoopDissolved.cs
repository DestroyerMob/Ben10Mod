using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class GoopDissolved : ModBuff {
    public override string Texture => "Ben10Mod/Content/Interface/EmptyAlien";

    public override void SetStaticDefaults() {
        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.velocity.X *= 0.92f;
        npc.color = Color.Lerp(npc.color, new Color(90, 215, 115), 0.45f);
    }

    public override bool RightClick(int buffIndex) => false;
}
