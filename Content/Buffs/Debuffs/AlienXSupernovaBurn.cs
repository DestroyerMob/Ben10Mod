using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class AlienXSupernovaBurn : ModBuff {
    public const int DamagePerSecond = 100;
    public const int LifeRegenPenalty = DamagePerSecond * 2;
    public const int CombatTextDamage = DamagePerSecond / 2;

    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = false;
        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        if (Main.dedServ || Main.rand.NextBool(3))
            return;

        Vector2 dustPosition = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.45f);
        Vector2 dustVelocity = new(Main.rand.NextFloat(-0.55f, 0.55f), Main.rand.NextFloat(-2.9f, -1.15f));
        int dustType = Main.rand.NextBool(4)
            ? DustID.WhiteTorch
            : Main.rand.NextBool(2)
                ? DustID.GoldFlame
                : DustID.Flare;
        Color dustColor = Color.Lerp(new Color(255, 178, 96), new Color(255, 246, 225), Main.rand.NextFloat());

        Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 100, dustColor, Main.rand.NextFloat(0.95f, 1.45f));
        dust.noGravity = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
