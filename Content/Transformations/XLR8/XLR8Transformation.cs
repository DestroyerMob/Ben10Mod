using System;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.XLR8;

public class XLR8Transformation : Transformation {
    public override string FullID => "Ben10Mod:XLR8";
    public override string TransformationName => "XLR8";
    public override string IconPath => "Ben10Mod/Content/Interface/XLR8Select";
    public override int TransformationBuffId => ModContent.BuffType<XLR8_Buff>();
    public override int PrimaryAbilityDuration => 10 * 60;
    public override int PrimaryAttack => ModContent.ProjectileType<FistProjectile>();
    public override int PrimaryAttackSpeed => 3;
    public override int PrimaryShootSpeed => 30;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.moveSpeed *= omp.PrimaryAbilityEnabled ? 5f : 2.5f;
        player.accRunSpeed *= omp.PrimaryAbilityEnabled ? 4f : 2f;
        if (Math.Abs(player.velocity.X) > 2) {
            Player.jumpSpeed *= omp.PrimaryAbilityEnabled ? 3.0f : 1.5f;
            player.waterWalk =  true;
        }
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {

        player.armorEffectDrawShadowEOCShield = true;
        
        var costume = ModContent.GetInstance<XLR8>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        if (omp.PrimaryAbilityEnabled)
            player.head = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        player.waist = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Waist);
    }
}