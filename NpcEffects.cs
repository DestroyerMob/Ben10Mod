using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Debuffs;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod;

public class NpcEffects : GlobalNPC {
    public override bool InstancePerEntity => true;

    private const int ElectrocutedStationaryDamagePerSecond = 4;
    private const int ElectrocutedMovingDamagePerSecond = 16;
    private const int EnergyOverloadedDamagePerSecond = 60;
    
    private static bool IsPossessed(NPC npc) {
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

    private static bool IsElectrocuted(NPC npc) {
        return npc.HasBuff(ModContent.BuffType<EnemyElectrocuted>());
    }

    private static bool IsEnergyOverloaded(NPC npc) {
        return npc.HasBuff(ModContent.BuffType<EnergyOverloaded>());
    }

    public override bool PreAI(NPC npc) {
        if (ShouldHardFreeze(npc)) {
            npc.velocity = Vector2.Zero;
            npc.position = npc.oldPosition;
            return false;
        }

        return base.PreAI(npc);
    }

    public override void AI(NPC npc) {
        if (IsPossessed(npc))
            npc.velocity *= 0.68f;
    }

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        int totalDamagePerSecond = 0;
        bool hasDamagingDebuff = IsElectrocuted(npc) || IsEnergyOverloaded(npc);

        if (hasDamagingDebuff && npc.lifeRegen > 0)
            npc.lifeRegen = 0;

        if (IsElectrocuted(npc)) {
            int damagePerSecond = Math.Abs(npc.velocity.X) > 0.01f
                ? ElectrocutedMovingDamagePerSecond
                : ElectrocutedStationaryDamagePerSecond;

            npc.lifeRegen -= damagePerSecond * 2;
            totalDamagePerSecond += damagePerSecond;
        }

        if (IsEnergyOverloaded(npc)) {
            npc.lifeRegen -= EnergyOverloadedDamagePerSecond * 2;
            totalDamagePerSecond += EnergyOverloadedDamagePerSecond;
        }

        if (totalDamagePerSecond > 0) {
            if (damage < totalDamagePerSecond)
                damage = totalDamagePerSecond;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (ShouldHardFreeze(npc)) {
            drawColor = Color.Lerp(drawColor, new Color(150, 210, 255), 0.75f);
        }

        if (IsElectrocuted(npc)) {
            drawColor = Color.Lerp(drawColor, new Color(150, 230, 255), 0.45f);
        }

        if (IsEnergyOverloaded(npc)) {
            drawColor = Color.Lerp(drawColor, new Color(110, 255, 120), 0.7f);
            Lighting.AddLight(npc.Center, 0.14f, 0.5f, 0.18f);
        }

        if (IsPossessed(npc)) {
            drawColor = new Color(170, 100, 255, 190);
        }
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (IsElectrocuted(npc) && Main.rand.NextBool(4)) {
            Vector2 position = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.45f, npc.height * 0.45f);
            Vector2 velocity = Main.rand.NextVector2Circular(0.35f, 0.35f);
            Dust spark = Dust.NewDustPerfect(position, DustID.Electric, velocity, 110, new Color(175, 235, 255), 1.05f);
            spark.noGravity = true;
        }

        if (IsEnergyOverloaded(npc)) {
            for (int i = 0; i < 2; i++) {
                Vector2 position = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.52f, npc.height * 0.52f);
                Vector2 velocity = Main.rand.NextVector2Circular(0.65f, 0.65f);
                Dust spark = Dust.NewDustPerfect(position, DustID.GreenTorch, velocity, 85,
                    new Color(125, 255, 135), Main.rand.NextFloat(1.18f, 1.45f));
                spark.noGravity = true;
            }

            if (Main.rand.NextBool(2)) {
                float ringRotation = Main.GlobalTimeWrappedHourly * 2.2f + npc.whoAmI * 0.31f;
                for (int i = 0; i < 4; i++) {
                    float angle = ringRotation + MathHelper.TwoPi * i / 4f;
                    Vector2 unit = angle.ToRotationVector2();
                    Vector2 offset = new Vector2(unit.X * npc.width * 0.42f, unit.Y * npc.height * 0.42f);
                    Dust ringDust = Dust.NewDustPerfect(npc.Center + offset, DustID.GreenTorch, unit * 0.25f, 80,
                        new Color(170, 255, 120), 1.28f);
                    ringDust.noGravity = true;
                }
            }
        }

        if (IsPossessed(npc)) {
            if (Main.rand.NextBool(3)) {
                Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f),
                    DustID.Ghost, new Vector2(0, -1.2f), 100, new Color(160, 210, 255), 1.5f).noGravity = true;
            }
        }
    }
}
