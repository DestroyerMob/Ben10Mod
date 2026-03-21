using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Buffs.Summons;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.BuzzShock;

public class BuzzShockTransformation : Transformation {
    public override string FullID => "Ben10Mod:BuzzShock";
    public override string TransformationName => "Buzzshock";
    public override string IconPath => "Ben10Mod/Content/Interface/BuzzShockSelect";
    public override int TransformationBuffId => ModContent.BuffType<BuzzShock_Buff>();

    public override string Description =>
        "A living bolt of Nosedeenian energy that zaps enemies, teleports in a flash, and can summon electric support.";

    public override List<string> Abilities => new() {
        "Lightning bolts",
        "Teleport burst",
        "Summon electrical minion",
        "Homing lightning barrage"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<BuzzShockProjectile>();
    public override int PrimaryAttackSpeed => 20;
    public override int PrimaryShootSpeed => 25;

    public override bool HasPrimaryAbility => true;
    public override int PrimaryAbilityDuration => 1;
    public override int PrimaryAbilityCooldown => 10 * 60;

    public override int UltimateAttack => ModContent.ProjectileType<BuzzShockUltimateProjectile>();
    public override int UltimateAttackSpeed => 20;
    public override int UltimateShootSpeed => 25;
    public override int UltimateEnergyCost => 25;
    public override float UltimateAttackModifier => 2.5f;

    public override int SecondaryAttack => -1;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetDamage<HeroDamage>() += 0.08f;
        player.GetAttackSpeed<HeroDamage>() += 0.12f;
        player.moveSpeed += 0.1f;
        player.maxRunSpeed += 0.8f;
        player.maxMinions += 1;
        player.noFallDmg = true;
        Lighting.AddLight(player.Center, new Vector3(0.2f, 0.45f, 0.7f));
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled || Main.myPlayer != player.whoAmI)
            return;

        SoundEngine.PlaySound(SoundID.Item8, player.position);
        for (int i = 0; i < 50; i++) {
            int dustNum = Dust.NewDust(player.position - Vector2.One, player.width + 1, player.height + 1,
                DustID.UltraBrightTorch, Main.rand.Next(-4, 5), Main.rand.Next(-4, 5), 1, Color.White, 2f);
            Main.dust[dustNum].noGravity = true;
        }

        player.Teleport(Main.MouseWorld, TeleportationStyleID.DebugTeleport);

        for (int i = 0; i < 50; i++) {
            int dustNum = Dust.NewDust(player.position - Vector2.One, player.width + 1, player.height + 1,
                DustID.UltraBrightTorch, Main.rand.Next(-4, 5), Main.rand.Next(-4, 5), 1, Color.White, 2f);
            Main.dust[dustNum].noGravity = true;
        }
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        if (omp.altAttack && !omp.ultimateAttack) {
            SoundEngine.PlaySound(SoundID.AbigailSummon, player.position);
            player.AddBuff(ModContent.BuffType<BuzzShockMinionBuff>(), 2);
            player.SpawnMinionOnCursor(source, player.whoAmI, ModContent.ProjectileType<BuzzShockMinionProjectile>(),
                damage, knockback);
            return false;
        }

        int projectileType = omp.ultimateAttack
            ? ModContent.ProjectileType<BuzzShockUltimateProjectile>()
            : ModContent.ProjectileType<BuzzShockProjectile>();
        int finalDamage = (int)(damage * (omp.ultimateAttack ? UltimateAttackModifier : 1f));

        SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap, player.position);

        if (omp.ultimateAttack) {
            for (int i = 0; i < 5; i++) {
                Projectile.NewProjectile(source, position, velocity.RotatedBy(i * 2.5f), projectileType, finalDamage,
                    knockback, player.whoAmI);
            }

            return false;
        }

        Projectile.NewProjectile(source, position, velocity, projectileType, finalDamage, knockback, player.whoAmI);
        return false;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<BuzzShock>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}
