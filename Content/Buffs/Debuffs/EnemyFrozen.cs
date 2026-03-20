using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Buffs.Debuffs;

public class EnemyFrozen : ModBuff {
    public override void Update(NPC npc, ref int buffIndex) {
        npc.velocity = Vector2.Zero;
        npc.color = new Color(0.65f, 0.85f, 1.2f);
    }

    public override bool RightClick(int buffIndex) => false;
}
