using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod;

public class NpcEffects : GlobalNPC {
    public override bool InstancePerEntity => true;
    
    public bool IsPossessed(NPC npc) {
        foreach (Player p in Main.ActivePlayers) {
            var omp = p.GetModPlayer<OmnitrixPlayer>();
            if (omp.inPossessionMode && omp.possessedTarget.whoAmI == npc.whoAmI) return true;
        }
        return false;
    }

    public override bool PreAI(NPC npc) {
        var omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        if (omp.UltimateAbilityEnabled && omp.currTransformation == TransformationEnum.XLR8) {
            npc.velocity = Vector2.Zero;
        }
        return base.PreAI(npc);
    }

    public override void AI(NPC npc) {
        if (IsPossessed(npc)) {
            npc.velocity *= 0.68f;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (IsPossessed(npc)) {
            drawColor = new Color(170, 100, 255, 190);
        }
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (IsPossessed(npc)) {
            if (Main.rand.NextBool(3)) {
                Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f),
                    DustID.Ghost, new Vector2(0, -1.2f), 100, new Color(160, 210, 255), 1.5f).noGravity = true;
            }
        }
    }
}