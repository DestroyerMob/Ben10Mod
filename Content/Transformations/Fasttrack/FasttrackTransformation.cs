using System;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Fasttrack;

public class FasttrackTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:Fasttrack";
    public override string TransformationName => "Fasttrack";
    public override int TransformationBuffId => ModContent.BuffType<Fasttrack_Buff>();
    protected override string BasicDescription => "A simple speedster base-form implementation with a basic projectile primary attack.";
    protected override int HeadSlot => ArmorIDs.Head.TinHelmet;
    protected override int BodySlot => ArmorIDs.Body.TinChainmail;
    protected override int LegSlot => ArmorIDs.Legs.TinGreaves;
    
    public override int    PrimaryAttack           => ModContent.ProjectileType<XLR8PunchProjectile>();
    public override int    PrimaryAttackSpeed      => 15;
    public override int    PrimaryShootSpeed       => 28;
    public override float  PrimaryAttackModifier   => 0.75f;
    
    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        
        player.moveSpeed *= 2.2f;
        player.accRunSpeed *= 1.8f;
        player.GetAttackSpeed<HeroDamage>() += 0.15f;
        player.GetCritChance<HeroDamage>() += 5f;
        player.pickSpeed *= 0.9f;
        player.jumpSpeedBoost += 1.3f;
        if (Math.Abs(player.velocity.X) > 2) {
            player.waterWalk =  true;
        }
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        base.FrameEffects(player, omp);
        player.armorEffectDrawShadow = true;
    }
}
