using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Accessories;

public class HeatBlastExtraJumpAccessory : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(20, 26);
        Item.SetShopValues(ItemRarityColor.Green2, 005000);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetJumpState<HeatBlastExtraJump>().Enable();
    }
}

public class HeatBlastExtraJump : ExtraJump {
    public override Position GetDefaultPosition()                 => new After(BlizzardInABottle);
    public override float    GetDurationMultiplier(Player player) => 2.25f;

    public override void UpdateHorizontalSpeeds(Player player) {
        player.runAcceleration *= 1.75f;
        player.maxRunSpeed     *= 2f;
    }

    public override void ShowVisuals(Player player) {
        for (int i = 0; i < 6; i++) {
            int dustNum = Dust.NewDust(new Vector2(player.position.X, player.position.Y + player.height), player.width,
                0, DustID.Torch, 1F, 1F, Scale: 2f);
            Main.dust[dustNum].noGravity = true;
        }
    }
}