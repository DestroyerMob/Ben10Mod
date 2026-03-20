using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
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
        public bool SingleUse { get; init; }
    }

    public abstract class Transformation : ModType {
        public virtual string FullID => $"{Mod.Name}:{TransformationName}";

        public virtual int PrimaryAttack => -1;
        public virtual int SecondaryAttack => -1;
        public virtual int PrimaryAbilityAttack => -1;
        public virtual int SecondaryAbilityAttack => -1;
        public virtual int TertiaryAbilityAttack => -1;
        public virtual int UltimateAttack => -1;
        public virtual float PrimaryAttackModifier => 1f;
        public virtual float SecondaryAttackModifier => 1f;
        public virtual float PrimaryAbilityAttackModifier => 1f;
        public virtual float SecondaryAbilityAttackModifier => 1f;
        public virtual float TertiaryAbilityAttackModifier => 1f;
        public virtual float UltimateAttackModifier => 1f;
        public virtual int PrimaryAttackSpeed => -1;
        public virtual int PrimaryShootSpeed => -1;
        public virtual int SecondaryAttackSpeed => -1;
        public virtual int SecondaryShootSpeed => -1;
        public virtual int PrimaryAbilityAttackSpeed => -1;
        public virtual int PrimaryAbilityAttackShootSpeed => -1;
        public virtual int SecondaryAbilityAttackSpeed => -1;
        public virtual int SecondaryAbilityAttackShootSpeed => -1;
        public virtual int TertiaryAbilityAttackSpeed => -1;
        public virtual int TertiaryAbilityAttackShootSpeed => -1;
        public virtual int UltimateAttackSpeed => -1;
        public virtual int UltimateShootSpeed => -1;
        public virtual int PrimaryUseStyle => ItemUseStyleID.Swing;
        public virtual int SecondaryUseStyle => ItemUseStyleID.Swing;
        public virtual int PrimaryAbilityAttackUseStyle => ItemUseStyleID.Swing;
        public virtual int SecondaryAbilityAttackUseStyle => ItemUseStyleID.Swing;
        public virtual int TertiaryAbilityAttackUseStyle => ItemUseStyleID.Swing;
        public virtual int UltimateUseStyle => ItemUseStyleID.Swing;
        public virtual bool PrimaryChannel => false;
        public virtual bool SecondaryChannel => false;
        public virtual bool PrimaryAbilityAttackChannel => false;
        public virtual bool SecondaryAbilityAttackChannel => false;
        public virtual bool TertiaryAbilityAttackChannel => false;
        public virtual bool UltimateChannel => false;
        public virtual bool PrimaryNoMelee => true;
        public virtual bool SecondaryNoMelee => true;
        public virtual bool PrimaryAbilityAttackNoMelee => true;
        public virtual bool SecondaryAbilityAttackNoMelee => true;
        public virtual bool TertiaryAbilityAttackNoMelee => true;
        public virtual bool UltimateNoMelee => true;
        public virtual int PrimaryArmorPenetration => 0;
        public virtual int SecondaryArmorPenetration => 0;
        public virtual int PrimaryAbilityAttackArmorPenetration => 0;
        public virtual int SecondaryAbilityAttackArmorPenetration => 0;
        public virtual int TertiaryAbilityAttackArmorPenetration => 0;
        public virtual int UltimateArmorPenetration => 0;
        public virtual int PrimaryEnergyCost => 0;
        public virtual int SecondaryEnergyCost => 0;
        public virtual int PrimaryAbilityAttackEnergyCost => 0;
        public virtual int SecondaryAbilityAttackEnergyCost => 0;
        public virtual int TertiaryAbilityAttackEnergyCost => 0;
        public virtual int UltimateEnergyCost => 0;
        public virtual bool HasPrimaryAttack => PrimaryAttack > 0;
        public virtual bool HasSecondaryAttack => SecondaryAttack > 0;
        public virtual bool HasPrimaryAbilityAttack => PrimaryAbilityAttack > 0;
        public virtual bool HasSecondaryAbilityAttack => SecondaryAbilityAttack > 0;
        public virtual bool HasTertiaryAbilityAttack => TertiaryAbilityAttack > 0;
        public virtual bool HasUltimateAttack => UltimateAttack > 0;
        public virtual bool HasPrimaryAbility => PrimaryAbilityDuration > 0;
        public virtual bool HasSecondaryAbility => SecondaryAbilityDuration > 0;
        public virtual bool HasTertiaryAbility => TertiaryAbilityDuration > 0;
        public virtual bool PrimaryAbilityAttackSingleUse => false;
        public virtual bool SecondaryAbilityAttackSingleUse => false;
        public virtual bool TertiaryAbilityAttackSingleUse => false;
        public virtual bool HasUltimateAbility => false;
        public virtual int PrimaryAbilityCost => 0;
        public virtual int SecondaryAbilityCost => 0;
        public virtual int TertiaryAbilityCost => 0;
        public virtual int UltimateAbilityCost => 50;
        public virtual int UltimateAbilityDuration => 30;
        public virtual int UltimateAbilityCooldown => 30;
        public virtual int PrimaryAbilityDuration => 0;
        public virtual int PrimaryAbilityCooldown => 0;
        public virtual int SecondaryAbilityDuration => 0;
        public virtual int SecondaryAbilityCooldown => 0;
        public virtual int TertiaryAbilityDuration => 0;
        public virtual int TertiaryAbilityCooldown => 0;
        public virtual int TransformationBuffId => -1;
        public virtual string TransformationName => "None";
        public virtual string IconPath => "Ben10Mod/Content/Interface/EmptyAlien";
        public virtual string Description => "A mysterious alien from the Omnitrix database.";
        public virtual List<string> Abilities => new List<string> { "Unknown abilities" };
        public virtual string PrimaryAttackDisplayName => ResolveProjectileDisplayName(PrimaryAttack, "Primary Attack");
        public virtual string SecondaryAttackDisplayName => ResolveProjectileDisplayName(SecondaryAttack, "Secondary Attack");
        public virtual string PrimaryAbilityAttackDisplayName => ResolveProjectileDisplayName(PrimaryAbilityAttack, "Primary Ability");
        public virtual string SecondaryAbilityAttackDisplayName => ResolveProjectileDisplayName(SecondaryAbilityAttack, "Secondary Ability");
        public virtual string TertiaryAbilityAttackDisplayName => ResolveProjectileDisplayName(TertiaryAbilityAttack, "Tertiary Ability");
        public virtual string UltimateAttackDisplayName => ResolveProjectileDisplayName(UltimateAttack, "Ultimate Attack");
        public virtual bool HasChildTransformation => ChildTransformation != null || ChildTransformations.Count > 0;
        public virtual Transformation ChildTransformation => null;
        public virtual IReadOnlyList<Transformation> ChildTransformations => System.Array.Empty<Transformation>();
        public virtual Transformation ParentTransformation => null;
        public virtual int ParentStepDownDelay => 45;
        public virtual bool StepDownToParentOnRepeatedTransform => ParentTransformation != null;

        public virtual Asset<Texture2D> GetTransformationIcon()
            => ModContent.Request<Texture2D>(IconPath);

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

        public virtual bool TryActivatePrimaryAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual bool TryActivateSecondaryAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual bool TryActivateTertiaryAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual bool TryActivateUltimateAbility(Player player, OmnitrixPlayer omp) => false;
        public virtual bool TryHandleTransformKeyWhileActive(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            string selectedTransformationId) {
            if (omp.pendingEvolutionStepDownTime > 0 &&
                omp.pendingEvolutionStepDownTransformationId == FullID)
                return true;

            if (TryActivateChildTransformation(player, omp, omnitrix, selectedTransformationId))
                return true;

            if (TryStepDownToParentTransformation(player, omp, omnitrix, selectedTransformationId))
                return true;
            return false;
        }

        public virtual string GetDisplayName(OmnitrixPlayer omp) => TransformationName;
        public virtual string GetDescription(OmnitrixPlayer omp) => Description;
        public virtual List<string> GetAbilities(OmnitrixPlayer omp) => Abilities;
        public virtual bool HasPrimaryAbilityActionForState(OmnitrixPlayer omp)
            => HasPrimaryAbilityForState(omp) || HasPrimaryAbilityAttackForState(omp);
        public virtual bool HasSecondaryAbilityActionForState(OmnitrixPlayer omp)
            => HasSecondaryAbilityForState(omp) || HasSecondaryAbilityAttackForState(omp);
        public virtual bool HasTertiaryAbilityActionForState(OmnitrixPlayer omp)
            => HasTertiaryAbilityForState(omp) || HasTertiaryAbilityAttackForState(omp);
        public virtual bool HasPrimaryAbilityForState(OmnitrixPlayer omp) => HasPrimaryAbility;
        public virtual bool HasSecondaryAbilityForState(OmnitrixPlayer omp) => HasSecondaryAbility;
        public virtual bool HasTertiaryAbilityForState(OmnitrixPlayer omp) => HasTertiaryAbility;
        public virtual bool HasPrimaryAbilityAttackForState(OmnitrixPlayer omp) => HasPrimaryAbilityAttack;
        public virtual bool HasSecondaryAbilityAttackForState(OmnitrixPlayer omp) => HasSecondaryAbilityAttack;
        public virtual bool HasTertiaryAbilityAttackForState(OmnitrixPlayer omp) => HasTertiaryAbilityAttack;
        public virtual bool HasUltimateAbilityForState(OmnitrixPlayer omp) => HasUltimateAbility;
        public virtual int GetPrimaryAbilityCost(OmnitrixPlayer omp) => PrimaryAbilityCost;
        public virtual int GetSecondaryAbilityCost(OmnitrixPlayer omp) => SecondaryAbilityCost;
        public virtual int GetTertiaryAbilityCost(OmnitrixPlayer omp) => TertiaryAbilityCost;
        public virtual int GetPrimaryAbilityDuration(OmnitrixPlayer omp) => PrimaryAbilityDuration;
        public virtual int GetPrimaryAbilityCooldown(OmnitrixPlayer omp) {
            return ApplyAbilityCooldownMultiplier(PrimaryAbilityCooldown, omp.primaryAbilityCooldownMultiplier);
        }
        public virtual int GetSecondaryAbilityDuration(OmnitrixPlayer omp) => SecondaryAbilityDuration;
        public virtual int GetSecondaryAbilityCooldown(OmnitrixPlayer omp) {
            return ApplyAbilityCooldownMultiplier(SecondaryAbilityCooldown, omp.secondaryAbilityCooldownMultiplier);
        }
        public virtual int GetTertiaryAbilityDuration(OmnitrixPlayer omp) => TertiaryAbilityDuration;
        public virtual int GetTertiaryAbilityCooldown(OmnitrixPlayer omp) {
            return ApplyAbilityCooldownMultiplier(TertiaryAbilityCooldown, omp.tertiaryAbilityCooldownMultiplier);
        }
        public virtual int GetUltimateAbilityCost(OmnitrixPlayer omp) => UltimateAbilityCost;
        public virtual int GetUltimateAbilityDuration(OmnitrixPlayer omp) => UltimateAbilityDuration;
        public virtual int GetUltimateAbilityCooldown(OmnitrixPlayer omp) {
            return ApplyAbilityCooldownMultiplier(UltimateAbilityCooldown, omp.ultimateAbilityCooldownMultiplier);
        }
        public virtual int GetPrimaryAbilityAttackProjectileType(OmnitrixPlayer omp) => PrimaryAbilityAttack;
        public virtual int GetSecondaryAbilityAttackProjectileType(OmnitrixPlayer omp) => SecondaryAbilityAttack;
        public virtual int GetTertiaryAbilityAttackProjectileType(OmnitrixPlayer omp) => TertiaryAbilityAttack;
        public virtual int GetUltimateAttackProjectileType(OmnitrixPlayer omp) => UltimateAttack;
        public virtual OmnitrixPlayer.AttackSelection ResolveAttackSelection(OmnitrixPlayer.AttackSelection selection,
            OmnitrixPlayer omp) {
            return selection switch {
                OmnitrixPlayer.AttackSelection.Ultimate
                    when GetUltimateAttackProjectileType(omp) > 0 => OmnitrixPlayer.AttackSelection.Ultimate,
                OmnitrixPlayer.AttackSelection.TertiaryAbility
                    when GetTertiaryAbilityAttackProjectileType(omp) > 0 => OmnitrixPlayer.AttackSelection.TertiaryAbility,
                OmnitrixPlayer.AttackSelection.SecondaryAbility
                    when GetSecondaryAbilityAttackProjectileType(omp) > 0 => OmnitrixPlayer.AttackSelection.SecondaryAbility,
                OmnitrixPlayer.AttackSelection.PrimaryAbility
                    when GetPrimaryAbilityAttackProjectileType(omp) > 0 => OmnitrixPlayer.AttackSelection.PrimaryAbility,
                OmnitrixPlayer.AttackSelection.Secondary
                    when SecondaryAttack > 0 => OmnitrixPlayer.AttackSelection.Secondary,
                _ when PrimaryAttack > 0 => OmnitrixPlayer.AttackSelection.Primary,
                _ => selection
            };
        }
        public virtual string GetAttackSelectionLabel(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
            return ResolveAttackSelection(selection, omp) switch {
                OmnitrixPlayer.AttackSelection.Secondary => "Secondary",
                OmnitrixPlayer.AttackSelection.PrimaryAbility => "Primary Ability",
                OmnitrixPlayer.AttackSelection.SecondaryAbility => "Secondary Ability",
                OmnitrixPlayer.AttackSelection.TertiaryAbility => "Tertiary Ability",
                OmnitrixPlayer.AttackSelection.Ultimate => "Ultimate",
                _ => "Primary"
            };
        }
        public virtual string GetAttackSelectionDisplayName(OmnitrixPlayer.AttackSelection selection, OmnitrixPlayer omp) {
            return ResolveAttackSelection(selection, omp) switch {
                OmnitrixPlayer.AttackSelection.Secondary => SecondaryAttackDisplayName,
                OmnitrixPlayer.AttackSelection.PrimaryAbility => PrimaryAbilityAttackDisplayName,
                OmnitrixPlayer.AttackSelection.SecondaryAbility => SecondaryAbilityAttackDisplayName,
                OmnitrixPlayer.AttackSelection.TertiaryAbility => TertiaryAbilityAttackDisplayName,
                OmnitrixPlayer.AttackSelection.Ultimate => UltimateAttackDisplayName,
                _ => PrimaryAttackDisplayName
            };
        }

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
        public virtual bool CanAffordCurrentAttack(OmnitrixPlayer omp) {
            int energyCost = GetEnergyCost(omp);
            return energyCost <= 0 || omp.omnitrixEnergy >= energyCost;
        }
        public virtual bool TryConsumeCurrentAttackCost(OmnitrixPlayer omp) {
            int energyCost = GetEnergyCost(omp);
            if (energyCost <= 0)
                return true;

            if (omp.omnitrixEnergy < energyCost)
                return false;

            omp.omnitrixEnergy -= energyCost;
            return true;
        }
        public virtual void FrameEffects(Player player, OmnitrixPlayer omp) { }

        protected virtual bool IsIntangibleWhilePrimaryAbilityActive(OmnitrixPlayer omp) {
            return omp.IsPrimaryAbilityActive &&
                   (TransformationName == "Ghostfreak" || TransformationName == "Bigchill");
        }

        protected static int ApplyAbilityCooldownMultiplier(int baseCooldown, float multiplier) {
            if (baseCooldown <= 0)
                return 0;

            float safeMultiplier = Math.Max(0f, multiplier);
            return Math.Max(1, (int)Math.Round(baseCooldown * safeMultiplier));
        }

        protected static string ResolveProjectileDisplayName(int projectileType, string fallback) {
            if (projectileType <= 0)
                return fallback;

            string displayName = Lang.GetProjectileName(projectileType).Value;
            return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName;
        }

        protected virtual IEnumerable<Transformation> EnumerateChildTransformations() {
            if (ChildTransformation != null)
                yield return ChildTransformation;

            foreach (Transformation transformation in ChildTransformations) {
                if (transformation != null &&
                    (ChildTransformation == null || transformation.FullID != ChildTransformation.FullID))
                    yield return transformation;
            }
        }

        protected virtual bool TryActivateChildTransformation(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            string selectedTransformationId) {
            Transformation selectedTransformation = TransformationLoader.Get(selectedTransformationId);

            foreach (Transformation childTransformation in EnumerateChildTransformations()) {
                if (!CanUseChildTransformation(player, omp, omnitrix, childTransformation, selectedTransformationId))
                    continue;

                int energyCost = GetChildTransformationEnergyCost(player, omp, omnitrix, childTransformation,
                    selectedTransformationId);
                if (energyCost > 0 && omp.omnitrixEnergy < energyCost) {
                    if (ShouldDetransformWhenChildTransformationFails(player, omp, omnitrix, childTransformation,
                            selectedTransformationId))
                        TransformationHandler.Detransform(player, 0, addCooldown: false);
                    return true;
                }

                if (energyCost > 0)
                    omp.omnitrixEnergy -= energyCost;

                int branchDuration = omnitrix.GetBranchTransformationDuration(omp);
                TransformInto(player, omp, childTransformation, branchDuration);
                OnChildTransformationActivated(player, omp, omnitrix, childTransformation, selectedTransformationId);
                return true;
            }

            foreach (RegisteredTransformationBranch registeredBranch in
                     TransformationBranchRegistry.GetChildBranches(FullID)) {
                Transformation childTransformation = registeredBranch.ResolveChild();
                if (childTransformation == null || !registeredBranch.CanUse(player, omp, omnitrix, selectedTransformation))
                    continue;

                int energyCost = registeredBranch.ResolveEnergyCost(player, omp, omnitrix, selectedTransformation);
                if (energyCost > 0 && omp.omnitrixEnergy < energyCost) {
                    if (registeredBranch.ShouldDetransform(player, omp, omnitrix, selectedTransformation))
                        TransformationHandler.Detransform(player, 0, addCooldown: false);
                    return true;
                }

                if (energyCost > 0)
                    omp.omnitrixEnergy -= energyCost;

                int branchDuration = omnitrix.GetBranchTransformationDuration(omp);
                TransformInto(player, omp, childTransformation, branchDuration);
                OnChildTransformationActivated(player, omp, omnitrix, childTransformation, selectedTransformationId);
                return true;
            }

            return false;
        }

        protected virtual bool CanUseChildTransformation(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            Transformation childTransformation, string selectedTransformationId) {
            Transformation selectedTransformation = TransformationLoader.Get(selectedTransformationId);
            return ChildTransformation != null &&
                   childTransformation.FullID == ChildTransformation.FullID &&
                   MatchesSelfOrAncestorSelection(selectedTransformationId) &&
                   omnitrix.CanUseEvolutionFeature(player, omp, this) &&
                   omnitrix.CanUseChildTransformation(player, omp, this, childTransformation, selectedTransformation);
        }

        protected virtual int GetChildTransformationEnergyCost(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            Transformation childTransformation, string selectedTransformationId) {
            return ChildTransformation != null &&
                   childTransformation.FullID == ChildTransformation.FullID &&
                   MatchesSelfOrAncestorSelection(selectedTransformationId)
                ? omnitrix.EvolutionCost
                : 0;
        }

        protected virtual bool ShouldDetransformWhenChildTransformationFails(Player player, OmnitrixPlayer omp,
            Omnitrix omnitrix, Transformation childTransformation, string selectedTransformationId) {
            return ChildTransformation != null &&
                   childTransformation.FullID == ChildTransformation.FullID &&
                   MatchesSelfOrAncestorSelection(selectedTransformationId);
        }

        protected virtual void OnChildTransformationActivated(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            Transformation childTransformation, string selectedTransformationId) { }

        protected virtual bool TryStepDownToParentTransformation(Player player, OmnitrixPlayer omp, Omnitrix omnitrix,
            string selectedTransformationId) {
            if (!StepDownToParentOnRepeatedTransform || ParentTransformation == null)
                return false;

            if (!MatchesAncestorSelection(selectedTransformationId))
                return false;

            int branchDuration = omnitrix.GetBranchTransformationDuration(omp);
            TransformInto(player, omp, ParentTransformation, branchDuration, playTransformEffects: false);
            omp.pendingEvolutionStepDownTime = ParentStepDownDelay;
            omp.pendingEvolutionStepDownTransformationId = ParentTransformation.FullID;
            TransformationHandler.PlayDetransformEffects(player);
            return true;
        }

        protected virtual bool MatchesSelfOrAncestorSelection(string selectedTransformationId) {
            if (string.IsNullOrEmpty(selectedTransformationId))
                return false;

            if (selectedTransformationId == FullID)
                return true;

            return MatchesAncestorSelection(selectedTransformationId);
        }

        protected virtual bool MatchesAncestorSelection(string selectedTransformationId) {
            if (string.IsNullOrEmpty(selectedTransformationId))
                return false;

            Transformation ancestor = ParentTransformation;
            while (ancestor != null) {
                if (ancestor.FullID == selectedTransformationId)
                    return true;

                ancestor = ancestor.ParentTransformation;
            }

            return false;
        }

        protected virtual void TransformInto(Player player, OmnitrixPlayer omp, Transformation targetTransformation,
            int seconds, bool playTransformEffects = true) {
            omp.pendingEvolutionStepDownTime = 0;
            omp.pendingEvolutionStepDownTransformationId = "";
            TransformationHandler.Detransform(player, 0, showParticles: false, addCooldown: false, playSound: false);
            TransformationHandler.Transform(player, targetTransformation.FullID, seconds, showParticles: playTransformEffects,
                playSound: playTransformEffects);
        }

        public virtual void CompleteEvolutionStepDown(Player player, OmnitrixPlayer omp) {
            TransformationHandler.Detransform(player, 0, showParticles: false, addCooldown: false, playSound: false);
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

        protected virtual TransformationAttackProfile CreatePrimaryAbilityAttackProfile() {
            return new TransformationAttackProfile {
                ProjectileType = PrimaryAbilityAttack,
                DamageMultiplier = PrimaryAbilityAttackModifier,
                UseTime = PrimaryAbilityAttackSpeed,
                ShootSpeed = PrimaryAbilityAttackShootSpeed,
                UseStyle = PrimaryAbilityAttackUseStyle,
                Channel = PrimaryAbilityAttackChannel,
                NoMelee = PrimaryAbilityAttackNoMelee,
                ArmorPenetration = PrimaryAbilityAttackArmorPenetration,
                EnergyCost = PrimaryAbilityAttackEnergyCost,
                SingleUse = PrimaryAbilityAttackSingleUse
            };
        }

        protected virtual TransformationAttackProfile CreateSecondaryAbilityAttackProfile() {
            return new TransformationAttackProfile {
                ProjectileType = SecondaryAbilityAttack,
                DamageMultiplier = SecondaryAbilityAttackModifier,
                UseTime = SecondaryAbilityAttackSpeed,
                ShootSpeed = SecondaryAbilityAttackShootSpeed,
                UseStyle = SecondaryAbilityAttackUseStyle,
                Channel = SecondaryAbilityAttackChannel,
                NoMelee = SecondaryAbilityAttackNoMelee,
                ArmorPenetration = SecondaryAbilityAttackArmorPenetration,
                EnergyCost = SecondaryAbilityAttackEnergyCost,
                SingleUse = SecondaryAbilityAttackSingleUse
            };
        }

        protected virtual TransformationAttackProfile CreateTertiaryAbilityAttackProfile() {
            return new TransformationAttackProfile {
                ProjectileType = TertiaryAbilityAttack,
                DamageMultiplier = TertiaryAbilityAttackModifier,
                UseTime = TertiaryAbilityAttackSpeed,
                ShootSpeed = TertiaryAbilityAttackShootSpeed,
                UseStyle = TertiaryAbilityAttackUseStyle,
                Channel = TertiaryAbilityAttackChannel,
                NoMelee = TertiaryAbilityAttackNoMelee,
                ArmorPenetration = TertiaryAbilityAttackArmorPenetration,
                EnergyCost = TertiaryAbilityAttackEnergyCost,
                SingleUse = TertiaryAbilityAttackSingleUse
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
            return omp.setAttack switch {
                OmnitrixPlayer.AttackSelection.Ultimate
                    when GetUltimateAttackProjectileType(omp) > 0 => CreateUltimateAttackProfile(),
                OmnitrixPlayer.AttackSelection.TertiaryAbility
                    when GetTertiaryAbilityAttackProjectileType(omp) > 0 => CreateTertiaryAbilityAttackProfile(),
                OmnitrixPlayer.AttackSelection.SecondaryAbility
                    when GetSecondaryAbilityAttackProjectileType(omp) > 0 => CreateSecondaryAbilityAttackProfile(),
                OmnitrixPlayer.AttackSelection.PrimaryAbility
                    when GetPrimaryAbilityAttackProjectileType(omp) > 0 => CreatePrimaryAbilityAttackProfile(),
                OmnitrixPlayer.AttackSelection.Secondary
                    when SecondaryAttack > 0 => CreateSecondaryAttackProfile(),
                _
                    when PrimaryAttack > 0 => CreatePrimaryAttackProfile(),
                _ => null
            };
        }

        protected sealed override void Register() {
            TransformationLoader.Register(this);
            Mod.Logger.Info($"[Ben10Mod] Registered transformation: {FullID}");
        }

        public sealed override void SetupContent() => SetStaticDefaults();
    }
}
