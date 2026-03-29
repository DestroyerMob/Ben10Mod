using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.HeatBlast {
    public class HeatBlastTransformation : Transformation {
        private const float AuraRodDamageMultiplier = 0.1f;
        private const int AuraRodEnergyCost = 80;
        private const float SolarHaloDamageMultiplier = 0.42f;
        private const int SolarHaloEnergyCost = 30;
        private const int SolarHaloSustainCost = 1;
        private const int SolarHaloSustainInterval = 9;
        private const int SuperheatDuration = 9 * 60;
        private const int SuperheatCooldown = 28 * 60;
        private const int SuperheatCost = 24;
        private const int SuperheatBaseDamage = 16;

        private const int PotisPrimaryAttackSpeed = 11;
        private const float PotisPrimaryShootSpeed = 18f;
        private const float PotisPrimaryDamageMultiplier = 0.82f;
        private const int PotisPrimaryEnergyCost = 1;
        private const int PotisPrimaryArmorPenetration = 4;
        private const int PotisMeteorAttackSpeed = 24;
        private const float PotisMeteorShootSpeed = 15f;
        private const float PotisMeteorDamageMultiplier = 0.42f;
        private const int PotisMeteorEnergyCost = 10;
        private const int PotisMeteorCount = 3;
        private const float PotisSunspotDamageMultiplier = 0.82f;
        private const int PotisSunspotAttackSpeed = 28;
        private const int PotisSunspotEnergyCost = 44;
        private const int PotisSunspotMaxCount = 2;
        private const float PotisCoronaDamageMultiplier = 0.56f;
        private const int PotisCoronaAttackSpeed = 16;
        private const int PotisCoronaEnergyCost = 26;
        private const int PotisCoronaSustainCost = 2;
        private const int PotisCoronaSustainInterval = 8;
        private const int PotisCataclysmDuration = 8 * 60;
        private const int PotisCataclysmCooldown = 36 * 60;
        private const int PotisCataclysmCost = 36;
        private const int PotisCataclysmPulseBaseDamage = 28;
        private const int PotisCataclysmPulseInterval = 24;
        private const float PotisCataclysmPulseRadius = 118f;
        private const int PotisUltimateAttackSpeed = 8;
        private const float PotisUltimateDamageMultiplier = 1.1f;
        private const int PotisUltimateEnergyCost = 16;
        private const int PotisUltimateSustainCost = 2;
        private const int PotisUltimateSustainInterval = 6;
        private const int PotisUltimateCooldown = 78 * 60;

        public override string FullID => "Ben10Mod:HeatBlast";
        public override string TransformationName => "Heatblast";
        public override bool IsStarterTransformation(OmnitrixPlayer omp) => true;

        public override string Description =>
            "A fiery Pyronite from the blazing star Pyros. A living inferno of plasma wrapped in molten rock that can flood the battlefield with bombs, halos, and superheated fire.";

        public override string IconPath => "Ben10Mod/Content/Interface/HeatBlastSelect";
        public override int TransformationBuffId => ModContent.BuffType<HeatBlast_Buff>();
        public override bool HasPrimaryAbility => false;
        public override int PrimaryAbilityCooldown => 0;
        public override int PrimaryAbilityAttack => ModContent.ProjectileType<HeatBlastAuraRodProjectile>();
        public override int PrimaryAbilityAttackSpeed => 24;
        public override int PrimaryAbilityAttackShootSpeed => 0;
        public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
        public override float PrimaryAbilityAttackModifier => AuraRodDamageMultiplier;
        public override int PrimaryAbilityAttackEnergyCost => AuraRodEnergyCost;
        public override bool PrimaryAbilityAttackSingleUse => false;
        public override int SecondaryAbilityAttack => ModContent.ProjectileType<HeatBlastSolarHaloProjectile>();
        public override int SecondaryAbilityAttackSpeed => 18;
        public override int SecondaryAbilityAttackShootSpeed => 0;
        public override int SecondaryAbilityAttackUseStyle => ItemUseStyleID.Shoot;
        public override float SecondaryAbilityAttackModifier => SolarHaloDamageMultiplier;
        public override int SecondaryAbilityAttackEnergyCost => SolarHaloEnergyCost;
        public override int SecondaryAbilityAttackSustainEnergyCost => SolarHaloSustainCost;
        public override int SecondaryAbilityAttackSustainInterval => SolarHaloSustainInterval;
        public override bool SecondaryAbilityAttackChannel => true;
        public override bool SecondaryAbilityAttackSingleUse => false;
        public override string TertiaryAbilityName => "Superheat";
        public override int TertiaryAbilityDuration => SuperheatDuration;
        public override int TertiaryAbilityCooldown => SuperheatCooldown;
        public override int TertiaryAbilityCost => SuperheatCost;

        public override List<string> Abilities => new() {
            "Flamethrower blast",
            "Flame bombs",
            "Flame-boosted jump",
            "Fire & lava immunity",
            "Flame aura rod sentry",
            "Solar halo of orbiting imp fireballs",
            "Superheated flame aura that scorches everything around Heatblast",
            "Growing fireball that hits harder the longer it builds"
        };

        public override string PrimaryAttackName => "Flame Jet";
        public override string SecondaryAttackName => "Fire Bomb";
        public override string PrimaryAbilityAttackName => "Flare Rod";
        public override string SecondaryAbilityAttackName => "Solar Halo";
        public override string UltimateAttackName => "Fireball";

        public override int PrimaryAttack => ProjectileID.Flames;
        public override int PrimaryAttackSpeed => 6;
        public override int PrimaryShootSpeed => 3;
        public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
        public override float PrimaryAttackModifier => 0.3f;

        public override int SecondaryAttack => ModContent.ProjectileType<HeatBlastBomb>();
        public override int SecondaryAttackSpeed => 40;
        public override int SecondaryShootSpeed => 10;
        public override float SecondaryAttackModifier => 1.5f;
        public override int SecondaryUseStyle => ItemUseStyleID.Swing;

        public override int UltimateAttack => ModContent.ProjectileType<HeatBlastUltimateProjectile>();
        public override int UltimateAbilityCooldown => 60 * 60;
        public override int UltimateAttackSpeed => 6;
        public override int UltimateShootSpeed => 0;
        public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
        public override bool UltimateChannel => true;
        public override int UltimateEnergyCost => 10;
        public override int UltimateAttackSustainEnergyCost => UltimateEnergyCost;
        public override int UltimateAttackSustainInterval => UltimateAttackSpeed;

        public override int GetMoveSetIndex(OmnitrixPlayer omp) => HasPotisAltiare(omp?.Player) ? 1 : 0;

        public override string GetDescription(OmnitrixPlayer omp) {
            if (!HasPotisAltiare(omp?.Player))
                return Description;

            return $"{Description} Potis Altiare rewires Heatblast into a solar artillery platform with lances, meteor rain, orbiting corona fire, and repeated nova pulses.";
        }

        public override List<string> GetAbilities(OmnitrixPlayer omp) {
            if (!HasPotisAltiare(omp?.Player))
                return base.GetAbilities(omp);

            return new List<string> {
                "Prominence lances with heavy piercing pressure",
                "Molten starfall that drags meteors out of the sky",
                "Sunspot Forge sentries that fire their own solar volleys",
                "Corona Maelstrom that channels an orbiting barrage around Heatblast",
                "Cataclysm Core that turns Superheat into repeated nova pulses",
                "Stronger movement, damage, and tempo while Cataclysm Core is active",
                "Fire and lava immunity",
                "Pyroclasm Supernova charged ultimate with impact starfall"
            };
        }

        public override string GetAbilitySelectionDisplayName(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
            if (selection == OmnitrixPlayer.AttackSelection.TertiaryAbility && HasPotisAltiare(omp?.Player))
                return "Cataclysm Core";

            return base.GetAbilitySelectionDisplayName(selection, omp);
        }

        public override int GetTertiaryAbilityDuration(OmnitrixPlayer omp) {
            return HasPotisAltiare(omp?.Player) ? PotisCataclysmDuration : base.GetTertiaryAbilityDuration(omp);
        }

        public override int GetTertiaryAbilityCooldown(OmnitrixPlayer omp) {
            if (!HasPotisAltiare(omp?.Player))
                return base.GetTertiaryAbilityCooldown(omp);

            return ApplyAbilityCooldownMultiplier(PotisCataclysmCooldown, omp.tertiaryAbilityCooldownMultiplier);
        }

        public override int GetTertiaryAbilityCost(OmnitrixPlayer omp) {
            return HasPotisAltiare(omp?.Player) ? PotisCataclysmCost : base.GetTertiaryAbilityCost(omp);
        }

        public override int GetUltimateAbilityCooldown(OmnitrixPlayer omp) {
            if (!HasPotisAltiare(omp?.Player))
                return base.GetUltimateAbilityCooldown(omp);

            return ApplyAbilityCooldownMultiplier(PotisUltimateCooldown, omp.ultimateAbilityCooldownMultiplier);
        }

        protected override IReadOnlyList<TransformationAttackProfile> GetPrimaryAttackProfiles() {
            return CreateMoveSetProfiles(
                CreatePrimaryAttackProfile(),
                new TransformationAttackProfile {
                    DisplayName = "Prominence Lance",
                    ProjectileType = ModContent.ProjectileType<HeatBlastPotisLanceProjectile>(),
                    DamageMultiplier = PotisPrimaryDamageMultiplier,
                    UseTime = PotisPrimaryAttackSpeed,
                    ShootSpeed = PotisPrimaryShootSpeed,
                    UseStyle = ItemUseStyleID.Shoot,
                    Channel = false,
                    NoMelee = true,
                    ArmorPenetration = PotisPrimaryArmorPenetration,
                    EnergyCost = PotisPrimaryEnergyCost
                }
            );
        }

        protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAttackProfiles() {
            return CreateMoveSetProfiles(
                CreateSecondaryAttackProfile(),
                new TransformationAttackProfile {
                    DisplayName = "Molten Starfall",
                    ProjectileType = ModContent.ProjectileType<HeatBlastPotisMeteorProjectile>(),
                    DamageMultiplier = PotisMeteorDamageMultiplier,
                    UseTime = PotisMeteorAttackSpeed,
                    ShootSpeed = PotisMeteorShootSpeed,
                    UseStyle = ItemUseStyleID.Shoot,
                    Channel = false,
                    NoMelee = true,
                    ArmorPenetration = SecondaryArmorPenetration + 6,
                    EnergyCost = PotisMeteorEnergyCost
                }
            );
        }

        protected override IReadOnlyList<TransformationAttackProfile> GetPrimaryAbilityAttackProfiles() {
            return CreateMoveSetProfiles(
                CreatePrimaryAbilityAttackProfile(),
                new TransformationAttackProfile {
                    DisplayName = "Sunspot Forge",
                    ProjectileType = ModContent.ProjectileType<HeatBlastPotisSunspotProjectile>(),
                    DamageMultiplier = PotisSunspotDamageMultiplier,
                    UseTime = PotisSunspotAttackSpeed,
                    ShootSpeed = 0f,
                    UseStyle = ItemUseStyleID.HoldUp,
                    Channel = false,
                    NoMelee = true,
                    ArmorPenetration = PrimaryAbilityAttackArmorPenetration + 4,
                    EnergyCost = PotisSunspotEnergyCost
                }
            );
        }

        protected override IReadOnlyList<TransformationAttackProfile> GetSecondaryAbilityAttackProfiles() {
            return CreateMoveSetProfiles(
                CreateSecondaryAbilityAttackProfile(),
                new TransformationAttackProfile {
                    DisplayName = "Corona Maelstrom",
                    ProjectileType = ModContent.ProjectileType<HeatBlastPotisCoronaProjectile>(),
                    DamageMultiplier = PotisCoronaDamageMultiplier,
                    UseTime = PotisCoronaAttackSpeed,
                    ShootSpeed = 0f,
                    UseStyle = ItemUseStyleID.Shoot,
                    Channel = true,
                    NoMelee = true,
                    ArmorPenetration = SecondaryAbilityAttackArmorPenetration + 4,
                    EnergyCost = PotisCoronaEnergyCost,
                    SustainEnergyCost = PotisCoronaSustainCost,
                    SustainInterval = PotisCoronaSustainInterval
                }
            );
        }

        protected override IReadOnlyList<TransformationAttackProfile> GetUltimateAttackProfiles() {
            return CreateMoveSetProfiles(
                CreateUltimateAttackProfile(),
                new TransformationAttackProfile {
                    DisplayName = "Pyroclasm Supernova",
                    ProjectileType = ModContent.ProjectileType<HeatBlastPotisUltimateProjectile>(),
                    DamageMultiplier = PotisUltimateDamageMultiplier,
                    UseTime = PotisUltimateAttackSpeed,
                    ShootSpeed = 0f,
                    UseStyle = ItemUseStyleID.HoldUp,
                    Channel = true,
                    NoMelee = true,
                    ArmorPenetration = UltimateArmorPenetration + 8,
                    EnergyCost = PotisUltimateEnergyCost,
                    SustainEnergyCost = PotisUltimateSustainCost,
                    SustainInterval = PotisUltimateSustainInterval
                }
            );
        }

        public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
            player.GetDamage<HeroDamage>() += 0.1f;
            player.GetAttackSpeed<HeroDamage>() += 0.08f;
            player.fireWalk = true;
            player.lavaImmune = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Burning] = true;

            bool potis = HasPotisAltiare(player);
            if (potis) {
                player.GetDamage<HeroDamage>() += 0.06f;
                player.GetAttackSpeed<HeroDamage>() += 0.04f;
                player.moveSpeed += 0.04f;
                player.statDefense += 4;

                if (omp.IsTertiaryAbilityActive) {
                    player.GetDamage<HeroDamage>() += 0.12f;
                    player.GetAttackSpeed<HeroDamage>() += 0.1f;
                    player.moveSpeed += 0.18f;
                    player.maxRunSpeed += 0.9f;
                    player.runAcceleration += 0.12f;
                    player.endurance += 0.05f;
                    player.noFallDmg = true;
                }
            }

            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            abilitySlot.FunctionalItem = new Item(ModContent.ItemType<HeatBlastExtraJumpAccessory>());

            int dustType = omp.snowflake ? DustID.IceTorch : DustID.Flare;
            Color dustColor = potis
                ? (omp.snowflake ? new Color(185, 235, 255) : new Color(255, 165, 72))
                : Color.White;
            int dustNum = Dust.NewDust(player.position, player.width, player.height, dustType,
                0f, Main.rand.NextFloat(-1f, 1f), 100, dustColor, potis ? Main.rand.NextFloat(1.5f, 2.35f) : Main.rand.NextFloat(1.2f, 2.8f));
            Main.dust[dustNum].noGravity = true;

            if (potis && omp.IsTertiaryAbilityActive) {
                int glowDustType = omp.snowflake ? DustID.SnowflakeIce : DustID.InfernoFork;
                Vector2 dustVelocity = new Vector2(Main.rand.NextFloat(-0.55f, 0.55f), Main.rand.NextFloat(-2.8f, -1.2f));
                Dust glowDust = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(player.width * 0.45f, player.height * 0.65f),
                    glowDustType, dustVelocity, 96, dustColor, Main.rand.NextFloat(1.1f, 1.65f));
                glowDust.noGravity = true;
                Lighting.AddLight(player.Center, omp.snowflake ? new Vector3(0.42f, 0.74f, 1.04f) : new Vector3(1.2f, 0.5f, 0.08f));
            }
        }

        public override void OnDetransform(Player player, OmnitrixPlayer omp) {
            KillOwnedProjectiles(player,
                ModContent.ProjectileType<HeatBlastAuraRodProjectile>(),
                ModContent.ProjectileType<HeatBlastSolarHaloProjectile>(),
                ModContent.ProjectileType<HeatBlastSuperheatAuraProjectile>(),
                ModContent.ProjectileType<HeatBlastPotisSunspotProjectile>(),
                ModContent.ProjectileType<HeatBlastPotisCoronaProjectile>(),
                ModContent.ProjectileType<HeatBlastPotisUltimateProjectile>(),
                ModContent.ProjectileType<HeatBlastPotisSolarBurstProjectile>());
        }

        public override void OnHitNPC(Player player, OmnitrixPlayer omp, NPC target, NPC.HitInfo hit, int damageDone) {
            if (target.life <= 0)
                return;

            if (omp.snowflake && !target.HasBuff(BuffID.Frostburn2))
                target.AddBuff(BuffID.Frostburn2, 10 * 60);
            else if (!omp.snowflake && !target.HasBuff(BuffID.OnFire3))
                target.AddBuff(BuffID.OnFire3, 10 * 60);
        }

        public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int damage, float knockback) {
            Vector2 direction = velocity.SafeNormalize(new Vector2(player.direction, 0f));
            Vector2 spawnPosition = player.MountedCenter + direction * 16f;

            if (omp.ultimateAttack) {
                TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.Ultimate, omp);
                if (profile == null || profile.ProjectileType <= 0 || HasActiveOwnedProjectile(player, profile.ProjectileType))
                    return false;

                Projectile.NewProjectile(source, player.Center + new Vector2(0f, -24f), Vector2.Zero, profile.ProjectileType,
                    GetScaledDamage(damage, profile), knockback + 1f, player.whoAmI);
                return false;
            }

            if (omp.IsSecondaryAbilityAttackLoaded) {
                TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.SecondaryAbility, omp);
                if (profile == null || profile.ProjectileType <= 0)
                    return false;

                Projectile.NewProjectile(source, player.Center, Vector2.Zero, profile.ProjectileType,
                    GetScaledDamage(damage, profile), knockback + 0.5f, player.whoAmI);
                return false;
            }

            if (omp.IsPrimaryAbilityAttackLoaded) {
                TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.PrimaryAbility, omp);
                if (profile == null || profile.ProjectileType <= 0)
                    return false;

                if (profile.ProjectileType == ModContent.ProjectileType<HeatBlastAuraRodProjectile>())
                    return TrySpawnAuraRod(player, omp, source, profile, damage, knockback);

                if (profile.ProjectileType == ModContent.ProjectileType<HeatBlastPotisSunspotProjectile>())
                    return TrySpawnPotisSunspot(player, omp, source, profile, damage, knockback);

                return false;
            }

            if (omp.altAttack) {
                TransformationAttackProfile profile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.Secondary, omp);
                if (profile == null || profile.ProjectileType <= 0)
                    return false;

                if (profile.ProjectileType == ModContent.ProjectileType<HeatBlastPotisMeteorProjectile>())
                    return SpawnPotisMeteorBarrage(player, omp, source, profile, damage, knockback);

                Projectile.NewProjectile(source, spawnPosition, direction * GetProfileShootSpeed(profile, SecondaryShootSpeed),
                    profile.ProjectileType, GetScaledDamage(damage, profile), knockback, player.whoAmI);
                return false;
            }

            TransformationAttackProfile primaryProfile = GetRawAttackProfile(OmnitrixPlayer.AttackSelection.Primary, omp);
            if (primaryProfile == null || primaryProfile.ProjectileType <= 0)
                return false;

            if (primaryProfile.ProjectileType == ProjectileID.Flames) {
                Projectile.NewProjectile(source, spawnPosition, direction * PrimaryShootSpeed, primaryProfile.ProjectileType,
                    GetScaledDamage(damage, primaryProfile), knockback, player.whoAmI);
                return false;
            }

            Projectile.NewProjectile(source, spawnPosition, direction * GetProfileShootSpeed(primaryProfile, PrimaryShootSpeed),
                primaryProfile.ProjectileType, GetScaledDamage(damage, primaryProfile), knockback, player.whoAmI,
                omp.IsTertiaryAbilityActive ? 1f : 0f, omp.snowflake ? 1f : 0f);
            return false;
        }

        public override bool CanStartCurrentAttack(Player player, OmnitrixPlayer omp) {
            if (!base.CanStartCurrentAttack(player, omp))
                return false;

            TransformationAttackProfile profile = GetSelectedAttackProfile(omp);
            if (profile?.ProjectileType == ModContent.ProjectileType<HeatBlastSolarHaloProjectile>() ||
                profile?.ProjectileType == ModContent.ProjectileType<HeatBlastPotisCoronaProjectile>() ||
                profile?.ProjectileType == ModContent.ProjectileType<HeatBlastUltimateProjectile>() ||
                profile?.ProjectileType == ModContent.ProjectileType<HeatBlastPotisUltimateProjectile>())
                return !HasActiveOwnedProjectile(player, profile.ProjectileType);

            return true;
        }

        public override void PostUpdate(Player player, OmnitrixPlayer omp) {
            if (!omp.IsTertiaryAbilityActive || player.whoAmI != Main.myPlayer)
                return;

            if (HasPotisAltiare(player)) {
                KillOwnedProjectiles(player, ModContent.ProjectileType<HeatBlastSuperheatAuraProjectile>());
                if (Main.GameUpdateCount % PotisCataclysmPulseInterval == player.whoAmI % PotisCataclysmPulseInterval) {
                    int burstDamage = Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(PotisCataclysmPulseBaseDamage)));
                    SpawnPotisSolarBurst(player, burstDamage, PotisCataclysmPulseRadius, 1.1f, 20);
                }

                return;
            }

            int projectileType = ModContent.ProjectileType<HeatBlastSuperheatAuraProjectile>();
            int auraDamage = Math.Max(1, (int)Math.Round(player.GetDamage<HeroDamage>().ApplyTo(SuperheatBaseDamage)));
            int existingAura = FindOwnedProjectile(player.whoAmI, projectileType);

            if (existingAura >= 0) {
                Projectile aura = Main.projectile[existingAura];
                aura.damage = auraDamage;
                aura.originalDamage = auraDamage;
                aura.Center = player.Center;
                aura.timeLeft = 2;
                aura.netUpdate = true;
                return;
            }

            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, projectileType,
                auraDamage, 0.7f, player.whoAmI);
        }

        public override void UpdateActiveAbilityVisuals(Player player, OmnitrixPlayer omp) {
        }

        public override void FrameEffects(Player player, OmnitrixPlayer omp) {
            var costume = ModContent.GetInstance<HeatBlast>();
            player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
            player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
            player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
        }

        public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => new[] {
            new TransformationPaletteChannel(
                "flames",
                "Flames",
                new Color(255, 255, 255),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlast_Head",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastFlameMask_Head"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlast_Body",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastFlameMask_Body"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlast_Legs",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastFlameMask_Legs")
            ),
            new TransformationPaletteChannel(
                "rock",
                "Rock",
                new Color(255, 255, 255),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlast_Head",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastRockMask_Head"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlast_Body",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastRockMask_Body"),
                new TransformationPaletteOverlay(
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlast_Legs",
                    "Ben10Mod/Content/Transformations/HeatBlast/HeatBlastRockMask_Legs")
            )
        };

        private static bool TrySpawnAuraRod(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
            TransformationAttackProfile profile, int damage, float knockback) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            int rodType = profile.ProjectileType;
            CullOldestOwnedProjectile(player, rodType, Math.Max(1, player.maxTurrets));

            Vector2 spawnPosition = Main.MouseWorld;
            int projectileIndex = Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, rodType,
                GetScaledDamage(damage, profile), 0f, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.transformationAttackSerial++;
                Main.projectile[projectileIndex].originalDamage = GetScaledDamage(damage, profile);
                Main.projectile[projectileIndex].localAI[1] = omp.transformationAttackSerial;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            for (int i = 0; i < 18; i++) {
                Dust dust = Dust.NewDustPerfect(spawnPosition + Main.rand.NextVector2Circular(18f, 18f), DustID.Flare,
                    Main.rand.NextVector2Circular(2f, 2f), 100, new Color(255, 145, 50), Main.rand.NextFloat(1.2f, 1.7f));
                dust.noGravity = true;
            }

            return false;
        }

        private static bool TrySpawnPotisSunspot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
            TransformationAttackProfile profile, int damage, float knockback) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            int maxSunspots = Math.Max(1, Math.Min(PotisSunspotMaxCount, player.maxTurrets));
            CullOldestOwnedProjectile(player, profile.ProjectileType, maxSunspots);

            Vector2 spawnPosition = Main.MouseWorld;
            int projectileIndex = Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, profile.ProjectileType,
                GetScaledDamage(damage, profile), knockback + 0.3f, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                omp.transformationAttackSerial++;
                Main.projectile[projectileIndex].localAI[1] = omp.transformationAttackSerial;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            for (int i = 0; i < 20; i++) {
                Dust dust = Dust.NewDustPerfect(spawnPosition + Main.rand.NextVector2Circular(22f, 22f),
                    omp.snowflake ? DustID.IceTorch : DustID.InfernoFork,
                    Main.rand.NextVector2Circular(2.4f, 2.4f), 100,
                    omp.snowflake ? new Color(180, 235, 255) : new Color(255, 165, 72),
                    Main.rand.NextFloat(1.2f, 1.7f));
                dust.noGravity = true;
            }

            return false;
        }

        private static bool SpawnPotisMeteorBarrage(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
            TransformationAttackProfile profile, int damage, float knockback) {
            if (Main.netMode == NetmodeID.Server ||
                (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer))
                return false;

            Vector2 targetPosition = Main.MouseWorld;
            int meteorDamage = GetScaledDamage(damage, profile);
            float shootSpeed = GetProfileShootSpeed(profile, PotisMeteorShootSpeed);

            for (int i = 0; i < PotisMeteorCount; i++) {
                float t = PotisMeteorCount == 1 ? 0.5f : i / (float)(PotisMeteorCount - 1);
                float horizontalOffset = MathHelper.Lerp(-88f, 88f, t) + Main.rand.NextFloat(-20f, 20f);
                Vector2 impactPosition = targetPosition + new Vector2(horizontalOffset * 0.2f, Main.rand.NextFloat(-20f, 20f));
                Vector2 spawnPosition = targetPosition + new Vector2(horizontalOffset, -440f - 42f * i);
                Vector2 launchVelocity = (impactPosition - spawnPosition).SafeNormalize(Vector2.UnitY) * shootSpeed;
                int projectileIndex = Projectile.NewProjectile(source, spawnPosition, launchVelocity, profile.ProjectileType,
                    meteorDamage, knockback + 0.9f, player.whoAmI, 0f, omp.snowflake ? 1f : 0f);
                if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                    Main.projectile[projectileIndex].netUpdate = true;
            }

            for (int i = 0; i < 12; i++) {
                Dust dust = Dust.NewDustPerfect(targetPosition + Main.rand.NextVector2Circular(18f, 18f),
                    omp.snowflake ? DustID.SnowflakeIce : DustID.Flare,
                    Main.rand.NextVector2Circular(1.8f, 1.8f), 95,
                    omp.snowflake ? new Color(190, 235, 255) : new Color(255, 170, 88),
                    Main.rand.NextFloat(1.15f, 1.5f));
                dust.noGravity = true;
            }

            return false;
        }

        private static void SpawnPotisSolarBurst(Player player, int damage, float radius, float knockback, int timeLeft) {
            int projectileType = ModContent.ProjectileType<HeatBlastPotisSolarBurstProjectile>();
            int projectileIndex = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
                projectileType, damage, knockback, player.whoAmI, radius);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles) {
                Main.projectile[projectileIndex].timeLeft = timeLeft;
                Main.projectile[projectileIndex].netUpdate = true;
            }
        }

        private static bool HasPotisAltiare(Player player) {
            return player?.GetModPlayer<PotisAltiarePlayer>()?.potisAltiareEquipped == true;
        }

        private static float GetProfileShootSpeed(TransformationAttackProfile profile, float fallbackSpeed) {
            return profile?.ShootSpeed >= 0f ? profile.ShootSpeed : fallbackSpeed;
        }

        private static int GetScaledDamage(int damage, TransformationAttackProfile profile) {
            return Math.Max(1, (int)Math.Round(damage * (profile?.DamageMultiplier ?? 1f)));
        }

        private static void KillOwnedProjectiles(Player player, params int[] projectileTypes) {
            if (projectileTypes == null || projectileTypes.Length == 0)
                return;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != player.whoAmI)
                    continue;

                for (int j = 0; j < projectileTypes.Length; j++) {
                    if (projectile.type != projectileTypes[j])
                        continue;

                    projectile.Kill();
                    break;
                }
            }
        }

        private static bool HasActiveOwnedProjectile(Player player, int projectileType) {
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == player.whoAmI && projectile.type == projectileType)
                    return true;
            }

            return false;
        }

        private static void CullOldestOwnedProjectile(Player player, int projectileType, int maxCount) {
            int activeCount = 0;
            int oldestIndex = -1;
            float oldestSpawnOrder = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != player.whoAmI || projectile.type != projectileType)
                    continue;

                activeCount++;
                float spawnOrder = projectile.localAI[1] <= 0f ? projectile.identity : projectile.localAI[1];
                if (spawnOrder < oldestSpawnOrder) {
                    oldestSpawnOrder = spawnOrder;
                    oldestIndex = i;
                }
            }

            if (activeCount >= maxCount && oldestIndex != -1)
                Main.projectile[oldestIndex].Kill();
        }

        private static int FindOwnedProjectile(int owner, int projectileType) {
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile projectile = Main.projectile[i];
                if (projectile.active && projectile.owner == owner && projectile.type == projectileType)
                    return i;
            }

            return -1;
        }
    }
}
