using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.Humungousaur;

public class HumungousaurTransformation : Transformation {
    public override string FullID => "Ben10Mod:Humungousaur";
    public override string TransformationName => "Humungousaur";
    public override string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
    public override int TransformationBuffId => ModContent.BuffType<Humungousaur_Buff>();
    public override Transformation ChildTransformation => ModContent.GetInstance<UltimateHumungousaurTransformation>();

    public override string Description =>
        "A towering Vaxasaurian bruiser that can grow stronger mid-battle and smash enemies apart with raw force.";

    public override List<string> Abilities => new() {
        "Close-range power punch",
        "Forward shockwave slam",
        "Growth surge that boosts strength and toughness",
        "Ultimate evolution"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<HumungousaurPunchProjectile>();
    public override int PrimaryAttackSpeed => 34;
    public override int PrimaryShootSpeed => 10;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryAttack => ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>();
    public override int SecondaryAttackSpeed => 34;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 18 * 60;
    public override int PrimaryAbilityCooldown => 50 * 60;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        player.statDefense += 8;
        player.GetDamage(DamageClass.Generic) += 0.12f;
        player.GetKnockback(DamageClass.Generic) += 0.25f;

        if (!omp.PrimaryAbilityEnabled)
            return;

        player.statDefense += 14;
        player.GetDamage(DamageClass.Generic) += 0.2f;
        player.GetKnockback(DamageClass.Generic) += 0.5f;
        player.moveSpeed *= 0.9f;
    }

    public override string GetDisplayName(OmnitrixPlayer omp) {
        return omp.PrimaryAbilityEnabled ? "Humungousaur (Grown)" : base.GetDisplayName(omp);
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        float growthScale = omp.PrimaryAbilityEnabled ? 1.45f : 1f;

        if (omp.altAttack) {
            Vector2 shockwaveVelocity = new(player.direction * (omp.PrimaryAbilityEnabled ? 9f : 7.5f), 0f);
            Projectile.NewProjectile(source, player.Bottom + new Vector2(player.direction * 12f, -10f), shockwaveVelocity,
                ModContent.ProjectileType<HumungousaurShockwavePlayerProjectile>(), (int)(damage * 1.05f * growthScale),
                knockback, player.whoAmI);
            return false;
        }

        Vector2 punchVelocity = velocity.SafeNormalize(new Vector2(player.direction, 0f)) * 10f;
        Projectile.NewProjectile(source, player.Center + punchVelocity * 2f, punchVelocity,
            ModContent.ProjectileType<HumungousaurPunchProjectile>(), (int)(damage * growthScale), knockback,
            player.whoAmI, growthScale);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        player.head = ArmorIDs.Head.MoltenHelmet;
        player.body = ArmorIDs.Body.MoltenBreastplate;
        player.legs = ArmorIDs.Legs.MoltenGreaves;
    }

    public override void DrawEffects(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.PrimaryAbilityEnabled)
            return;

        if (Main.rand.NextBool(3)) {
            Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch, Scale: 1.2f);
            dust.velocity *= 0.2f;
            dust.noGravity = true;
        }
    }
}
