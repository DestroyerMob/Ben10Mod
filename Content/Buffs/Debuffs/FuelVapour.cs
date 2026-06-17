using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class FuelVapour : ModBuff {
    public override string Texture => "Ben10Mod/Content/Interface/EmptyAlien";

    public override void SetStaticDefaults() {
        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.color = Color.Lerp(npc.color, new Color(235, 180, 70), 0.38f);

        if (Main.dedServ || !Main.rand.NextBool(5))
            return;

        Dust vapour = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.48f, npc.height * 0.48f),
            Main.rand.NextBool() ? DustID.Grass : DustID.Torch, Main.rand.NextVector2Circular(0.35f, 0.35f),
            120, new Color(215, 170, 80), Main.rand.NextFloat(0.75f, 1.05f));
        vapour.noGravity = true;
        vapour.fadeIn = 0.75f;
    }

    public override bool RightClick(int buffIndex) => false;
}
