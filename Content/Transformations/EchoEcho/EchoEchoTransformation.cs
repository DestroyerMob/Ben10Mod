using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.EchoEcho;

public class EchoEchoTransformation : Transformation {
    public override string FullID => "Ben10Mod:EchoEcho";
    public override string TransformationName => "Echo Echo";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<EchoEcho_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateEchoEchoTransformation>();

    public override string Description =>
        "A living sonic resonator that can split into combat duplicates and fire concentrated sound bursts from its mouth.";

    public override List<string> Abilities => new() {
        "Sonic mouth blasts",
        "Duplicate summon",
        "Resonance acceleration",
        "Ultimate evolution"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<EchoEchoSonicBlastProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 14;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<EchoEchoCloneProjectile>();
    public override int SecondaryAttackSpeed => 26;
    public override int SecondaryShootSpeed => 0;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 0.8f;
    

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.moveSpeed += 0.08f;
        player.maxMinions += 1;

        if (omp.PrimaryAbilityEnabled)
            player.GetAttackSpeed<HeroDamage>() += 0.18f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (!omp.altAttack)
            return base.Shoot(player, omp, source, position, velocity, damage, knockback);

        player.AddBuff(ModContent.BuffType<EchoEchoCloneBuff>(), 2);
        player.SpawnMinionOnCursor(source, player.whoAmI, ModContent.ProjectileType<EchoEchoCloneProjectile>(),
            (int)(damage * SecondaryAttackModifier), knockback);

        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.CopperHelmet;
        player.body = ArmorIDs.Body.CopperChainmail;
        player.legs = ArmorIDs.Legs.CopperGreaves;
    }
}
