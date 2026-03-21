using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.WildVine;

public class WildVineTransformation : Transformation {
    public override string FullID => "Ben10Mod:WildVine";
    public override string TransformationName => "Wildvine";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<WildVine_Buff>();

    public override string Description =>
        "A flexible Florauna that lashes out with living vines and uses a plant-like grapple to control space.";

    public override List<string> Abilities => new() {
        "Latch and pull vine",
        "Plant grapple"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<WildVineProjectile>();
    public override int PrimaryAttackSpeed => 32;
    public override int PrimaryShootSpeed => 10;

    public override int SecondaryAttack => ModContent.ProjectileType<WildVineGrapple>();
    public override int SecondaryAttackSpeed => 32;
    public override int SecondaryShootSpeed => 10;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.1f;
        player.moveSpeed += 0.08f;
        player.runAcceleration += 0.06f;
        player.jumpSpeedBoost += 1.8f;
        player.noFallDmg = true;
        player.lifeRegen += player.velocity.Y == 0f ? 3 : 1;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<WildVine>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}
