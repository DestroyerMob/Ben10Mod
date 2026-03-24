using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations;

public abstract class SimpleRangedTransformationBase : Transformation {
    protected abstract string BasicDescription { get; }
    protected virtual int HeadSlot => -1;
    protected virtual int BodySlot => -1;
    protected virtual int LegSlot => -1;

    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override string Description => BasicDescription;
    public override List<string> Abilities => new() { "Basic ranged projectile attack" };
    public override string PrimaryAttackName => "Hero Bolt";
    public override int PrimaryAttack => ProjectileID.WoodenArrowFriendly;
    public override int PrimaryAttackSpeed => 24;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 0.9f;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.GetDamage<HeroDamage>() += 0.04f;
        player.statDefense += 4;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        if (HeadSlot >= 0)
            player.head = HeadSlot;
        if (BodySlot >= 0)
            player.body = BodySlot;
        if (LegSlot >= 0)
            player.legs = LegSlot;
    }
}
