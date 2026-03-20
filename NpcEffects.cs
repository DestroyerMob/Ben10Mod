using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using Ben10Mod.Content.Transformations.HeatBlast;
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
            if (omp.inPossessionMode && omp.possessedTargetIndex == npc.whoAmI) return true;
        }
        return false;
    }

    private static bool IsFrozenByXLR8Ultimate() {
        foreach (Player player in Main.ActivePlayers) {
            var omp = player.GetModPlayer<OmnitrixPlayer>();
            if (omp.IsUltimateAbilityActive && omp.currentTransformationId == "Ben10Mod:XLR8")
                return true;
        }

        return false;
    }

    private static bool ShouldHardFreeze(NPC npc) {
        if (!npc.active || npc.friendly)
            return false;

        return npc.HasBuff(ModContent.BuffType<EnemyFrozen>()) || IsFrozenByXLR8Ultimate();
    }

    public override bool PreAI(NPC npc) {
        var omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        if (ShouldHardFreeze(npc)) {
            npc.velocity = Vector2.Zero;
            npc.position = npc.oldPosition;
            return false;
        }

        if (omp.IsUltimateAbilityActive && omp.CurrentTransformation == ModContent.GetInstance<HeatBlastTransformation>()) {
            npc.velocity = Vector2.Zero;
        }
        return base.PreAI(npc);
    }

    public override void AI(NPC npc) {
        var omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        
        if (IsPossessed(npc)) {
            npc.velocity *= 0.68f;
        }

        // if (omp.currTransformation == TransformationEnum.BigChill && omp.UltimateAbilityEnabled && npc.active && !npc.friendly) {
        //     npc.AddBuff(ModContent.BuffType<EnemySlow>(), 120);
        // }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (ShouldHardFreeze(npc)) {
            drawColor = Color.Lerp(drawColor, new Color(150, 210, 255), 0.75f);
        }

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
