using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations {
    public abstract class Transformation : ModType {
        public virtual string FullID => $"{Mod.Name}:{TransformationName}";

        public virtual int PrimaryAttack => -1;
        public virtual int SecondaryAttack => -1;
        public virtual int UltimateAttack => -1;
        public virtual float PrimaryAttackModifier => 1f;
        public virtual float SecondaryAttackModifier => 1f;
        public virtual float UltimateAttackModifier => 1f;
        public virtual int PrimaryAttackSpeed => 0;
        public virtual int PrimaryShootSpeed => 0;
        public virtual int SecondaryAttackSpeed => 0;
        public virtual int SecondaryShootSpeed => 0;
        public virtual int UltimateAttackSpeed => 0;
        public virtual int UltimateShootSpeed => 0;
        public virtual bool HasPrimaryAttack => false;
        public virtual bool HasSecondaryAttack => false;
        public virtual bool HasUltimateAttack => false;
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

        public virtual void OnTransform(Player player, OmnitrixPlayer omp) { }
        public virtual void OnDetransform(Player player, OmnitrixPlayer omp) { }
        public virtual void UpdateEffects(Player player, OmnitrixPlayer omp) { }
        public virtual void PostUpdate(Player player, OmnitrixPlayer omp) { }
        public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo) { }
        public virtual void DrawEffects(ref PlayerDrawSet drawInfo) { }
        public virtual void PreUpdateMovement(Player player, OmnitrixPlayer omp) { }
        public virtual bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) => false;

        public virtual void ModifyPlumbersBadgeStats(Item item, OmnitrixPlayer omp) { }

        public virtual bool Shoot(Player player, OmnitrixPlayer omp,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity,
            int damage, float knockback) {

            int projType = PrimaryAttack;
            int finalDamage = (int)(damage * 0.3f);

            if (HasUltimateAttack && omp.ultimateAttack) {
                projType = UltimateAttack;
            } else if (HasSecondaryAttack && omp.altAttack) {
                projType = SecondaryAttack;
                finalDamage = (int)(damage * 1.5f);
            }
            
            Projectile.NewProjectile(source, position, velocity, projType, finalDamage, knockback, player.whoAmI);
            
            return false;
        }
        

        public virtual int GetEnergyCost(OmnitrixPlayer omp) => 0;
        public virtual void FrameEffects(Player player, OmnitrixPlayer omp) { }

        protected sealed override void Register() {
            TransformationLoader.Register(this); // ← This line makes EVERY alien auto-register
            Mod.Logger.Info($"[Ben10Mod] Registered transformation: {FullID}");
        }

        public sealed override void SetupContent() => SetStaticDefaults();
    }
}