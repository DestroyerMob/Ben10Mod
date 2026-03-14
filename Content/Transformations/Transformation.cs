using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content.Items.Accessories;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations {
    public class TransformationAttackProfile {
        public int ProjectileType { get; init; } = -1;
        public float DamageMultiplier { get; init; } = 1f;
        public int UseTime { get; init; } = -1;
        public float ShootSpeed { get; init; } = -1f;
        public int UseStyle { get; init; } = ItemUseStyleID.Swing;
        public bool Channel { get; init; }
        public bool NoMelee { get; init; }
        public int ArmorPenetration { get; init; }
        public int EnergyCost { get; init; }
    }

    public abstract class Transformation : ModType {
        public virtual string FullID => $"{Mod.Name}:{TransformationName}";

        public virtual int PrimaryAttack => -1;
        public virtual int SecondaryAttack => -1;
        public virtual int UltimateAttack => -1;
        public virtual float PrimaryAttackModifier => 1f;
        public virtual float SecondaryAttackModifier => 1f;
        public virtual float UltimateAttackModifier => 1f;
        public virtual int PrimaryAttackSpeed => -1;
        public virtual int PrimaryShootSpeed => -1;
        public virtual int SecondaryAttackSpeed => -1;
        public virtual int SecondaryShootSpeed => -1;
        public virtual int UltimateAttackSpeed => -1;
        public virtual int UltimateShootSpeed => -1;
        public virtual int PrimaryUseStyle => ItemUseStyleID.Swing;
        public virtual int SecondaryUseStyle => ItemUseStyleID.Swing;
        public virtual int UltimateUseStyle => ItemUseStyleID.Swing;
        public virtual bool PrimaryChannel => false;
        public virtual bool SecondaryChannel => false;
        public virtual bool UltimateChannel => false;
        public virtual bool PrimaryNoMelee => true;
        public virtual bool SecondaryNoMelee => true;
        public virtual bool UltimateNoMelee => true;
        public virtual int PrimaryArmorPenetration => 0;
        public virtual int SecondaryArmorPenetration => 0;
        public virtual int UltimateArmorPenetration => 0;
        public virtual int PrimaryEnergyCost => 0;
        public virtual int SecondaryEnergyCost => 0;
        public virtual int UltimateEnergyCost => 0;
        public virtual bool HasPrimaryAttack => PrimaryAttack > 0;
        public virtual bool HasSecondaryAttack => SecondaryAttack > 0;
        public virtual bool HasUltimateAttack => UltimateAttack > 0;
        public virtual bool HasUltimateAbility => false;
        public virtual int UltimateAbilityCost => 50;
        public virtual int UltimateAbilityDuration => 30;
        public virtual int UltimateAbilityCooldown => 30;
        public virtual int PrimaryAbilityDuration => 30;
        public virtual int PrimaryAbilityCooldown => 30;
        public virtual int TransformationBuffId => -1;
        public virtual string TransformationName => "None";
        public virtual string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
        public virtual string Description => "A mysterious alien from the Omnitrix database.";
        public virtual List<string> Abilities => new List<string> { "Unknown abilities" };

        public virtual Asset<Texture2D> GetTransformationIcon()
            => ModContent.Request<Texture2D>(IconPath);

        // Lifecycle
        public virtual void ResetEffects(Player player, OmnitrixPlayer omp) { }
        public virtual void OnEnterWorld(Player player, OmnitrixPlayer omp) { }
        public virtual void OnTransform(Player player, OmnitrixPlayer omp) { }
        public virtual void OnDetransform(Player player, OmnitrixPlayer omp) { }
        public virtual void PreUpdate(Player player, OmnitrixPlayer omp) { }
        public virtual void PostUpdateBuffs(Player player, OmnitrixPlayer omp) { }

        public virtual void UpdateEffects(Player player, OmnitrixPlayer omp) {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
        }
        public virtual void PostUpdate(Player player, OmnitrixPlayer omp) { }
        public virtual void PreUpdateMovement(Player player, OmnitrixPlayer omp) { }

        // Combat and damage hooks
        public virtual bool? CanUseItem(Player player, OmnitrixPlayer omp, Item item) {
            return IsIntangibleWhilePrimaryAbilityActive(omp) ? false : null;
        }

        public virtual bool? CanBeHitByNPC(Player player, OmnitrixPlayer omp, NPC npc, ref int cooldownSlot) {
            return IsIntangibleWhilePrimaryAbilityActive(omp) ? false : null;
        }

        public virtual bool? CanBeHitByProjectile(Player player, OmnitrixPlayer omp, Projectile projectile) {
            return IsIntangibleWhilePrimaryAbilityActive(omp) ? false : null;
        }

        public virtual void ModifyHurt(Player player, OmnitrixPlayer omp, ref Player.HurtModifiers modifiers) { }
        public virtual bool FreeDodge(Player player, OmnitrixPlayer omp, Player.HurtInfo info) => false;
        public virtual void ModifyDrawInfo(Player player, OmnitrixPlayer omp, ref PlayerDrawSet drawInfo) { }
        public virtual void DrawEffects(ref PlayerDrawSet drawInfo) { }
        public virtual void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) { }
        public virtual void PostHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) { }
        public virtual bool? CanHitNPCWithItem(Player player, OmnitrixPlayer omp, Item item, NPC target) => null;
        public virtual void ModifyHitNPCWithItem(Player player, OmnitrixPlayer omp, Item item, NPC target,
            ref NPC.HitModifiers modifiers) { }
        public virtual void OnHitNPCWithItem(Player player, OmnitrixPlayer omp, Item item, NPC target,
            NPC.HitInfo hit, int damageDone) { }
        public virtual bool? CanHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile,
            NPC target) => null;
        public virtual void ModifyHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile,
            NPC target, ref NPC.HitModifiers modifiers) { }
        public virtual void OnHitNPCWithProjectile(Player player, OmnitrixPlayer omp, Projectile projectile,
            NPC target, NPC.HitInfo hit, int damageDone) { }
        public virtual void OnHitNPC(Player player, OmnitrixPlayer omp, NPC target, NPC.HitInfo hit, int damageDone) { }
        public virtual void OnHitAnything(Player player, OmnitrixPlayer omp, Entity victim, float x, float y) { }

        // Ability and weapon hooks
        public virtual bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) {
            var profile = GetSelectedAttackProfile(omp);
            if (profile == null)
                return;

            if (profile.UseTime > 0)
                item.useTime = item.useAnimation = profile.UseTime;

            if (profile.ShootSpeed >= 0f)
                item.shootSpeed = profile.ShootSpeed;

            item.useStyle = profile.UseStyle;
            item.channel = profile.Channel;
            item.noMelee = profile.NoMelee;
            item.ArmorPenetration = profile.ArmorPenetration;
        }

        public virtual bool Shoot(Player player, OmnitrixPlayer omp,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity,
            int damage, float knockback) {
            var profile = GetSelectedAttackProfile(omp);
            if (profile == null || profile.ProjectileType <= 0)
                return false;

            int finalDamage = (int)(damage * profile.DamageMultiplier);
            Projectile.NewProjectile(source, position, velocity, profile.ProjectileType, finalDamage, knockback,
                player.whoAmI);
            
            return false;
        }
        

        public virtual int GetEnergyCost(OmnitrixPlayer omp) {
            return GetSelectedAttackProfile(omp)?.EnergyCost ?? 0;
        }
        public virtual void FrameEffects(Player player, OmnitrixPlayer omp) { }

        protected virtual bool IsIntangibleWhilePrimaryAbilityActive(OmnitrixPlayer omp) {
            return omp.PrimaryAbilityEnabled &&
                   (TransformationName == "Ghostfreak" || TransformationName == "Bigchill");
        }

        protected virtual TransformationAttackProfile CreatePrimaryAttackProfile() {
            return new TransformationAttackProfile {
                ProjectileType = PrimaryAttack,
                DamageMultiplier = PrimaryAttackModifier,
                UseTime = PrimaryAttackSpeed,
                ShootSpeed = PrimaryShootSpeed,
                UseStyle = PrimaryUseStyle,
                Channel = PrimaryChannel,
                NoMelee = PrimaryNoMelee,
                ArmorPenetration = PrimaryArmorPenetration,
                EnergyCost = PrimaryEnergyCost
            };
        }

        protected virtual TransformationAttackProfile CreateSecondaryAttackProfile() {
            return new TransformationAttackProfile {
                ProjectileType = SecondaryAttack,
                DamageMultiplier = SecondaryAttackModifier,
                UseTime = SecondaryAttackSpeed,
                ShootSpeed = SecondaryShootSpeed,
                UseStyle = SecondaryUseStyle,
                Channel = SecondaryChannel,
                NoMelee = SecondaryNoMelee,
                ArmorPenetration = SecondaryArmorPenetration,
                EnergyCost = SecondaryEnergyCost
            };
        }

        protected virtual TransformationAttackProfile CreateUltimateAttackProfile() {
            return new TransformationAttackProfile {
                ProjectileType = UltimateAttack,
                DamageMultiplier = UltimateAttackModifier,
                UseTime = UltimateAttackSpeed,
                ShootSpeed = UltimateShootSpeed,
                UseStyle = UltimateUseStyle,
                Channel = UltimateChannel,
                NoMelee = UltimateNoMelee,
                ArmorPenetration = UltimateArmorPenetration,
                EnergyCost = UltimateEnergyCost
            };
        }

        protected virtual TransformationAttackProfile GetSelectedAttackProfile(OmnitrixPlayer omp) {
            if (HasUltimateAttack && omp.ultimateAttack)
                return CreateUltimateAttackProfile();

            if (HasSecondaryAttack && omp.altAttack)
                return CreateSecondaryAttackProfile();

            if (HasPrimaryAttack)
                return CreatePrimaryAttackProfile();

            return null;
        }

        protected sealed override void Register() {
            TransformationLoader.Register(this); // ← This line makes EVERY alien auto-register
            Mod.Logger.Info($"[Ben10Mod] Registered transformation: {FullID}");
        }

        public sealed override void SetupContent() => SetStaticDefaults();
    }
}
