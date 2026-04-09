using System;
using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Materials;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Weapons;

public abstract class PlumberBlasterBase : ModItem {
    private float sustainedRandomBoltSpread;
    private ulong lastBlasterShotTick;

    protected abstract int TextureItemID { get; }
    protected abstract int BaseDamage { get; }
    protected abstract int UseTime { get; }
    protected abstract int ItemValue { get; }
    protected abstract int ItemRarity { get; }

    protected virtual int UseAnimationTicks => UseTime;
    protected virtual int ReuseDelay => 0;
    protected virtual int CritChance => 0;
    protected virtual int BoltCount => 1;
    protected virtual float BoltSpread => 0f;
    protected virtual float RandomBoltSpread => 0f;
    protected virtual float SustainedRandomBoltSpreadPerShot => 0f;
    protected virtual float MaxSustainedRandomBoltSpread => 0f;
    protected virtual int SustainedSpreadResetTicks => 16;
    protected virtual float BoltDamageMultiplier => 1f;
    protected virtual float LateralSpacing => 6f;
    protected virtual float ShootSpeed => 13.5f;
    protected virtual float KnockBack => 2f;
    protected virtual float HoldoutOffsetX => -4f;
    protected virtual float SoundPitch => -0.08f;
    protected virtual float SoundVolume => 0.68f;
    protected virtual bool StrongBolts => true;
    protected virtual string CombatStyleText => "Fires energized plumber bolts.";

    public override string Texture => $"Terraria/Images/Item_{TextureItemID}";

    public override void SetDefaults() {
        Item.width = 46;
        Item.height = 24;
        Item.damage = BaseDamage;
        Item.DamageType = ModContent.GetInstance<HeroDamage>();
        Item.useTime = UseTime;
        Item.useAnimation = UseAnimationTicks;
        Item.reuseDelay = ReuseDelay;
        Item.crit = CritChance;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = KnockBack;
        Item.value = ItemValue;
        Item.rare = ItemRarity;
        Item.UseSound = SoundID.Item91 with { Pitch = SoundPitch, Volume = SoundVolume };
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<PlumberBlasterBoltProjectile>();
        Item.shootSpeed = ShootSpeed;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        tooltips.Add(new TooltipLine(Mod, "BlasterStyle", CombatStyleText));
    }

    public override Vector2? HoldoutOffset() {
        return new Vector2(HoldoutOffsetX, 0f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback) {
        ResetSustainedSpreadIfNeeded();

        Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction == 0 ? 1 : player.direction, 0f));
        Vector2 lateral = direction.RotatedBy(MathHelper.PiOver2);
        Vector2 muzzleOffset = direction * (BoltCount >= 4 ? 22f : BoltCount >= 2 ? 20f : 18f);
        float centerOffset = (BoltCount - 1) * 0.5f;
        int boltDamage = Math.Max(1, (int)Math.Round(damage * BoltDamageMultiplier));
        float currentRandomSpread = RandomBoltSpread + sustainedRandomBoltSpread;

        for (int i = 0; i < BoltCount; i++) {
            float boltIndex = i - centerOffset;
            Vector2 spawnPosition = position + muzzleOffset + lateral * boltIndex * LateralSpacing;
            float shotAngle = boltIndex * BoltSpread;
            if (currentRandomSpread > 0f)
                shotAngle += Main.rand.NextFloat(-currentRandomSpread, currentRandomSpread);

            Vector2 shotVelocity = velocity.RotatedBy(shotAngle) * Main.rand.NextFloat(0.985f, 1.02f);
            int projectileIndex = Projectile.NewProjectile(source, spawnPosition, shotVelocity, type, boltDamage, knockback,
                player.whoAmI, StrongBolts ? 1f : 0f);

            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                Main.projectile[projectileIndex].netUpdate = true;
        }

        if (SustainedRandomBoltSpreadPerShot > 0f && MaxSustainedRandomBoltSpread > 0f)
            sustainedRandomBoltSpread = Math.Min(MaxSustainedRandomBoltSpread,
                sustainedRandomBoltSpread + SustainedRandomBoltSpreadPerShot);

        lastBlasterShotTick = Main.GameUpdateCount;
        return false;
    }

    private void ResetSustainedSpreadIfNeeded() {
        if (lastBlasterShotTick == 0 || sustainedRandomBoltSpread <= 0f)
            return;

        ulong currentTick = Main.GameUpdateCount;
        if (currentTick <= lastBlasterShotTick + (ulong)Math.Max(1, SustainedSpreadResetTicks))
            return;

        sustainedRandomBoltSpread = 0f;
    }
}

public class PlumberDeputyBlaster : PlumberBlasterBase {
    protected override int TextureItemID => ItemID.FlintlockPistol;
    protected override int BaseDamage => 22;
    protected override int UseTime => 22;
    protected override int ItemValue => Item.buyPrice(silver: 90);
    protected override int ItemRarity => ItemRarityID.Green;
    protected override float ShootSpeed => 13f;
    protected override float SoundPitch => -0.16f;
    protected override float SoundVolume => 0.6f;
    protected override bool StrongBolts => false;
    protected override string CombatStyleText => "Sidearm: a steady semi-auto energy pistol.";

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.FlintlockPistol)
            .AddIngredient(ItemID.DemoniteBar, 12)
            .AddIngredient(ItemID.ShadowScale, 6)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.FlintlockPistol)
            .AddIngredient(ItemID.CrimtaneBar, 12)
            .AddIngredient(ItemID.TissueSample, 6)
            .AddIngredient(ItemID.FallenStar, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

public class PlumberSeniorDeputyBlaster : PlumberBlasterBase {
    protected override int TextureItemID => ItemID.Boomstick;
    protected override int BaseDamage => 34;
    protected override int UseTime => 30;
    protected override int ItemValue => Item.buyPrice(gold: 1, silver: 80);
    protected override int ItemRarity => ItemRarityID.Orange;
    protected override int BoltCount => 5;
    protected override float BoltSpread => 0.16f;
    protected override float RandomBoltSpread => 0.11f;
    protected override float BoltDamageMultiplier => 0.23f;
    protected override float LateralSpacing => 2f;
    protected override float ShootSpeed => 11.25f;
    protected override float KnockBack => 3.25f;
    protected override float SoundPitch => -0.22f;
    protected override float SoundVolume => 0.72f;
    protected override bool StrongBolts => false;
    protected override string CombatStyleText => "Scattergun: blasts a short-range cone of weak plasma pellets.";

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Handgun)
            .AddIngredient(ItemID.MeteoriteBar, 16)
            .AddIngredient(ItemID.Bone, 30)
            .AddIngredient(ItemID.FallenStar, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

public class PlumberAgentBlaster : PlumberBlasterBase {
    protected override int TextureItemID => ItemID.ClockworkAssaultRifle;
    protected override int BaseDamage => 26;
    protected override int UseTime => 6;
    protected override int UseAnimationTicks => 18;
    protected override int ReuseDelay => 16;
    protected override int ItemValue => Item.buyPrice(gold: 3, silver: 80);
    protected override int ItemRarity => ItemRarityID.LightRed;
    protected override float ShootSpeed => 14.5f;
    protected override float SoundPitch => -0.08f;
    protected override string CombatStyleText => "Burst carbine: fires crisp three-shot plasma bursts.";

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ClockworkAssaultRifle)
            .AddIngredient(ItemID.CobaltBar, 14)
            .AddIngredient(ItemID.SoulofLight, 6)
            .AddIngredient(ItemID.SoulofNight, 6)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.ClockworkAssaultRifle)
            .AddIngredient(ItemID.PalladiumBar, 14)
            .AddIngredient(ItemID.SoulofLight, 6)
            .AddIngredient(ItemID.SoulofNight, 6)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

public class PlumberSeniorAgentBlaster : PlumberBlasterBase {
    protected override int TextureItemID => ItemID.Megashark;
    protected override int BaseDamage => 34;
    protected override int UseTime => 7;
    protected override int ItemValue => Item.buyPrice(gold: 6, silver: 50);
    protected override int ItemRarity => ItemRarityID.Pink;
    protected override float ShootSpeed => 16.25f;
    protected override float SoundPitch => -0.02f;
    protected override bool StrongBolts => true;
    protected override string CombatStyleText => "Pulse rifle: rapid full-auto fire with heavier energized bolts.";

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.Megashark)
            .AddIngredient(ItemID.HallowedBar, 14)
            .AddIngredient(ItemID.SoulofMight, 5)
            .AddIngredient(ItemID.SoulofSight, 5)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class PlumberProctorBlaster : PlumberBlasterBase {
    protected override int TextureItemID => ItemID.SniperRifle;
    protected override int BaseDamage => 82;
    protected override int UseTime => 34;
    protected override int ItemValue => Item.buyPrice(gold: 9, silver: 50);
    protected override int ItemRarity => ItemRarityID.Lime;
    protected override int CritChance => 14;
    protected override float ShootSpeed => 21.5f;
    protected override float KnockBack => 6f;
    protected override float SoundPitch => -0.24f;
    protected override float SoundVolume => 0.86f;
    protected override bool StrongBolts => true;
    protected override string CombatStyleText => "Longshot: a precise high-impact plasma marksman rifle.";

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IllegalGunParts)
            .AddIngredient(ItemID.ChlorophyteBar, 18)
            .AddIngredient(ItemID.Ectoplasm, 10)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class PlumberMagisterBlaster : PlumberBlasterBase {
    protected override int TextureItemID => ItemID.VortexBeater;
    protected override int BaseDamage => 40;
    protected override int UseTime => 6;
    protected override int ItemValue => Item.buyPrice(gold: 14);
    protected override int ItemRarity => ItemRarityID.Cyan;
    protected override float RandomBoltSpread => 0.01f;
    protected override float SustainedRandomBoltSpreadPerShot => 0.0065f;
    protected override float MaxSustainedRandomBoltSpread => 0.085f;
    protected override int SustainedSpreadResetTicks => 14;
    protected override float ShootSpeed => 19.5f;
    protected override float KnockBack => 3f;
    protected override float SoundPitch => 0.02f;
    protected override float SoundVolume => 0.82f;
    protected override bool StrongBolts => true;
    protected override string CombatStyleText => "Support gun: a heavy automatic blaster that blooms wider the longer you hold the trigger.";

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 10)
            .AddIngredient(ItemID.FragmentSolar, 8)
            .AddIngredient(ItemID.FragmentVortex, 8)
            .AddIngredient(ItemID.FragmentNebula, 8)
            .AddIngredient(ItemID.FragmentStardust, 8)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}
