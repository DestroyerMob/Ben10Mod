using Ben10Mod.Keybinds;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Accessories;
using Terraria.Audio;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Items.Weapons;
using Ben10Mod.Common.CustomVisuals;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Events;

namespace Ben10Mod {
    public class OmnitrixPlayer : ModPlayer {
        public bool masterControl = false;

        public bool omnitrixEquipped = false;
        public bool isTransformed = false;
        public bool wasTransformed = false;
        public bool onCooldown = false;
        public bool altAttack = false;
        public bool ultimateAttack = false;
        public int transformationAttackSerial = 0;
        public int transformationAttackDamage = 0;

        public int cooldownTime = 120;
        public int transformationTime = 300;

        public bool PrimaryAbilityEnabled = false;
        public bool PrimaryAbilityWasEnabled = false;
        public bool UltimateAbilityEnabled = false;
        public bool UltimateAbilityWasEnabled = false;
        public string tranUsedAbilityId = "";

        public int DashDir = -1;
        public const int DashDown = 0;
        public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;
        public int DashVelocity = 15;
        public int DashDelay = 0;
        public int DashTimer = 0;
        public const int DashCooldown = 15;
        public const int DashDuration = 15;

        public string[] transformationSlots = { "Ben10Mod:HeatBlast", "", "", "", "" };
        public string currentTransformationId = "";
        public List<string> unlockedTransformations = new() { "Ben10Mod:HeatBlast" };

        public bool showingUI = false;

        public bool omnitrixUpdating = false;
        public bool omnitrixWasUpdating = false;
        public float omnitrixEnergy = 0f;
        public float omnitrixEnergyMax = 0f;
        public float omnitrixEnergyRegen = 0f;
        public float transformationDurationMultiplier = 1f;
        public float cooldownDurationMultiplier = 1f;
        public float activeTransformationDurationMultiplier = 1f;
        public float activeCooldownDurationMultiplier = 1f;
        public int pendingEvolutionStepDownTime = 0;
        public string pendingEvolutionStepDownTransformationId = "";
        public Omnitrix equippedOmnitrix = null;

        public bool inPossessionMode = false;
        public Vector2 prePossessionPosition = Vector2.Zero;
        public int possessedTargetIndex = -1;
        public int possessionTimer = 0;
        private const int PossessionDuration = 360;

        public bool snowflake = false;
        public bool advancedCircuitMatrix = false;
        public bool advancedCircuitMatrixEquippedWhileTransformed = false;

        private readonly HashSet<int> participatedEvents = new();
        private readonly HashSet<int> activeEvents = new();

        private const int EventBloodMoon = -1;
        private const int EventSolarEclipse = -2;
        private const int EventSlimeRain = -3;
        private const int EventPumpkinMoon = -4;
        private const int EventFrostMoon = -5;
        private const int EventOldOnesArmy = -6;

        public Transformation CurrentTransformation
            => TransformationLoader.Get(currentTransformationId);

        public bool IsTransformed => !string.IsNullOrEmpty(currentTransformationId);

        public Omnitrix GetActiveOmnitrix() {
            if (equippedOmnitrix != null)
                return equippedOmnitrix;

            var omnitrixSlot = ModContent.GetInstance<OmnitrixSlot>();
            return omnitrixSlot?.FunctionalItem?.ModItem as Omnitrix;
        }

        public override void SaveData(TagCompound tag) {
            tag["masterControl"] = masterControl;
            tag["currentTransformationId"] = currentTransformationId;
            tag["omnitrixEnergy"] = omnitrixEnergy;

            tag["transformationRoster"] = transformationSlots;
            tag["unlockedTransformationRoster"] = unlockedTransformations.ToArray();
        }

        public override void LoadData(TagCompound tag) {
            tag.TryGet("masterControl", out masterControl);
            omnitrixEnergy = tag.TryGet("omnitrixEnergy", out omnitrixEnergy) ? omnitrixEnergy : 0f;

            tag.TryGet("currentTransformationId", out currentTransformationId);
            int[] oldUnlockedRoster;
            int[] oldTransformationRoster;
            int oldCurrentTransformation;

            if (tag.TryGet("transformationRoster", out string[] rosterArray))
                transformationSlots = rosterArray;
            else if (tag.TryGet("transformationRoster", out oldTransformationRoster)) {
                transformationSlots = new string[oldTransformationRoster.Length];
                for (int i = 0; i < oldTransformationRoster.Length; i++)
                    transformationSlots[i] = MapOldTransformationId((TransformationEnumOld)oldTransformationRoster[i]);
            }
            else if (tag.TryGet("roster", out oldTransformationRoster)) {
                transformationSlots = new string[oldTransformationRoster.Length];
                for (int i = 0; i < oldTransformationRoster.Length; i++)
                    transformationSlots[i] = MapOldTransformationId((TransformationEnumOld)oldTransformationRoster[i]);
            }

            if (string.IsNullOrEmpty(currentTransformationId) &&
                tag.TryGet("currentTransformation", out oldCurrentTransformation)) {
                currentTransformationId = MapOldTransformationId((TransformationEnumOld)oldCurrentTransformation);
            }
            else if (string.IsNullOrEmpty(currentTransformationId) &&
                     tag.TryGet("currTransformation", out oldCurrentTransformation)) {
                currentTransformationId = MapOldTransformationId((TransformationEnumOld)oldCurrentTransformation);
            }

            unlockedTransformations.Clear();
            if (tag.TryGet("unlockedTransformationRoster", out string[] unlockedArray))
                unlockedTransformations.AddRange(unlockedArray);

            if (tag.TryGet("unlockedRoster", out oldUnlockedRoster)) {
                for (int i = 0; i < oldUnlockedRoster.Length; i++) {
                    string migratedId = MapOldTransformationId((TransformationEnumOld)oldUnlockedRoster[i]);
                    if (!string.IsNullOrEmpty(migratedId) && !unlockedTransformations.Contains(migratedId))
                        unlockedTransformations.Insert(Math.Min(i, unlockedTransformations.Count), migratedId);
                }
            }

            if (!unlockedTransformations.Contains("Ben10Mod:HeatBlast"))
                unlockedTransformations.Insert(0, "Ben10Mod:HeatBlast");
        }

        private static string MapOldTransformationId(TransformationEnumOld transformation) {
            return transformation switch {
                TransformationEnumOld.Arctiguana => "Ben10Mod:Arctiguana",
                TransformationEnumOld.BigChill => "Ben10Mod:BigChill",
                TransformationEnumOld.BuzzShock => "Ben10Mod:BuzzShock",
                TransformationEnumOld.ChromaStone => "Ben10Mod:ChromaStone",
                TransformationEnumOld.DiamondHead => "Ben10Mod:DiamondHead",
                TransformationEnumOld.EyeGuy => "Ben10Mod:EyeGuy",
                TransformationEnumOld.FourArms => "Ben10Mod:FourArms",
                TransformationEnumOld.GhostFreak => "Ben10Mod:GhostFreak",
                TransformationEnumOld.HeatBlast => "Ben10Mod:HeatBlast",
                TransformationEnumOld.WildVine => "Ben10Mod:WildVine",
                TransformationEnumOld.RipJaws => "Ben10Mod:RipJaws",
                TransformationEnumOld.XLR8 => "Ben10Mod:XLR8",
                TransformationEnumOld.StinkFly => "Ben10Mod:StinkFly",
                _ => string.Empty
            };
        }

        public override void ResetEffects() {
            var trans = CurrentTransformation;

            advancedCircuitMatrix = false;
            snowflake = false;

            omnitrixEnergyMax = 0;
            omnitrixEnergyRegen = 0;
            transformationDurationMultiplier = 1f;
            cooldownDurationMultiplier = 1f;

            isTransformed = false;
            onCooldown = false;
            omnitrixEquipped = false;
            equippedOmnitrix = null;

            omnitrixUpdating = false;

            PrimaryAbilityEnabled = false;
            UltimateAbilityEnabled = false;

            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15)
                DashDir = DashDown;
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15)
                DashDir = DashUp;
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
                DashDir = DashRight;
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
                DashDir = DashLeft;
            else
                DashDir = -1;

            trans?.ResetEffects(Player, this);
        }

        public override void PostUpdateBuffs() {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();
            var omnitrixSlot = ModContent.GetInstance<OmnitrixSlot>();
            var trans = CurrentTransformation;

            if (wasTransformed && !isTransformed) {
                var customSlot = ModContent.GetInstance<OmnitrixSlot>();
                if (customSlot != null) {
                    var activeOmnitrix = GetActiveOmnitrix();
                    if (masterControl) {
                        TransformationHandler.Detransform(Player, 0, true, false);
                    }
                    else {
                        if (activeOmnitrix != null)
                            activeOmnitrix.HandleForcedDetransform(Player, this);
                        else if (!string.IsNullOrEmpty(currentTransformationId))
                            TransformationHandler.Detransform(Player, 0, showParticles: true, addCooldown: false);
                    }
                }
            }
            wasTransformed = isTransformed;

            // Play the update effect once when the Omnitrix enters its updating state, and complete
            // the item replacement when the updating buff falls off.
            if (omnitrixUpdating != omnitrixWasUpdating) {
                var activeOmnitrix = GetActiveOmnitrix();
                if (omnitrixUpdating) {
                    Random random = new Random();
                    for (int i = 0; i < 25; i++) {
                        int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                            Player.height + 1, DustID.BlueTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White,
                            4);
                        Main.dust[dustNum].noGravity = true;
                    }
                }
                else if (omnitrixWasUpdating && activeOmnitrix != null) {
                    activeOmnitrix.CompleteEvolution(Player, this, omnitrixSlot.FunctionalItem);
                }

                omnitrixWasUpdating = omnitrixUpdating;
            }

            if (trans != null)
                trans.UpdateEffects(Player, this);

            trans?.PostUpdateBuffs(Player, this);
        }

        public override void PostUpdate() {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();

            if (!isTransformed) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
                pendingEvolutionStepDownTime = 0;
                pendingEvolutionStepDownTransformationId = "";
            }

            omnitrixEnergy += (omnitrixEnergyRegen / 120f);
            if (omnitrixEnergy > omnitrixEnergyMax) omnitrixEnergy = omnitrixEnergyMax;

            if (KeybindSystem.OpenTransformationScreen.JustPressed && omnitrixEquipped) {
                if (!showingUI) {
                    ModContent.GetInstance<UISystem>().ShowMyUI();
                    showingUI = true;
                }
                else {
                    ModContent.GetInstance<UISystem>().HideMyUI();
                    showingUI = false;
                }
            }

            if (Main.mouseRight && Main.mouseRightRelease && Player.HeldItem.ModItem is PlumbersBadge)
                altAttack = !altAttack;

            var trans = CurrentTransformation;
            if (trans != null)
                trans.PostUpdate(Player, this);

            if (pendingEvolutionStepDownTime > 0) {
                bool sameTransformation = currentTransformationId == pendingEvolutionStepDownTransformationId;
                if (!isTransformed || !sameTransformation) {
                    pendingEvolutionStepDownTime = 0;
                    pendingEvolutionStepDownTransformationId = "";
                }
                else if (--pendingEvolutionStepDownTime <= 0) {
                    pendingEvolutionStepDownTime = 0;
                    pendingEvolutionStepDownTransformationId = "";
                    trans?.CompleteEvolutionStepDown(Player, this);
                }
            }

            if (inPossessionMode) {
                if (possessedTargetIndex < 0 || possessedTargetIndex >= Main.maxNPCs) {
                    EndPossession();
                    return;
                }

                NPC npc = Main.npc[possessedTargetIndex];
                if (npc == null || !npc.active || npc.whoAmI != possessedTargetIndex) {
                    EndPossession();
                    return;
                }

                Player.immuneNoBlink = true;
                Player.immuneTime = 999;
                Player.Center = npc.Center;
                Player.velocity = npc.velocity * 0.8f;

                Player.controlJump = false;
                Player.controlDown = false;
                Player.controlLeft = false;
                Player.controlRight = false;
                Player.controlUp = false;
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlHook = false;

                possessionTimer--;
                if (possessionTimer <= 0) {
                    npc.SimpleStrikeNPC(Player.HeldItem.damage * 2, Player.direction, false, 0, DamageClass.Magic);
                    EndPossession();
                }
            }

            // Drop out of ultimate attack mode immediately when the Omnitrix can no longer sustain it.
            if (isTransformed && ultimateAttack &&
                omnitrixEnergy < (CurrentTransformation?.GetUltimateAbilityCost(this) ?? 50)) {
                for (int i = 0; i < 50; i++) {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Firework_Yellow,
                        Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }

                ultimateAttack = false;
            }

            if (isTransformed) {
                if (KeybindSystem.PrimaryAbility.JustPressed &&
                    CurrentTransformation?.HasPrimaryAbilityForState(this) == true)
                    ActivatePrimaryAbility();

                if (KeybindSystem.UltimateAbility.JustPressed && CurrentTransformation != null)
                    ActivateUltimateAbility();
            }

            if (PrimaryAbilityWasEnabled && !PrimaryAbilityEnabled) {
                var abilityTransformation = TransformationLoader.Get(tranUsedAbilityId);
                if (abilityTransformation != null)
                    Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(),
                        abilityTransformation.GetPrimaryAbilityCooldown(this));
            }
            PrimaryAbilityWasEnabled = PrimaryAbilityEnabled;

            if (UltimateAbilityWasEnabled && !UltimateAbilityEnabled) {
                var abilityTransformation = TransformationLoader.Get(tranUsedAbilityId);
                if (abilityTransformation != null)
                    Player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(),
                        abilityTransformation.GetUltimateAbilityCooldown(this));
            }
            UltimateAbilityWasEnabled = UltimateAbilityEnabled;

            UpdateEventTransformationUnlocks();
        }

        public bool ActivateUltimateAbility() {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (trans.TryActivateUltimateAbility(Player, this))
                return true;

            if (trans.HasUltimateAbilityForState(this) && !Player.HasBuff<UltimateAbilityCooldown>() &&
                !Player.HasBuff<UltimateAbility>()) {
                int ultimateAbilityCost = trans.GetUltimateAbilityCost(this);
                if (omnitrixEnergy >= ultimateAbilityCost) {
                    Player.AddBuff(ModContent.BuffType<UltimateAbility>(), trans.GetUltimateAbilityDuration(this));
                    tranUsedAbilityId = currentTransformationId;
                    omnitrixEnergy -= ultimateAbilityCost;
                    return true;
                }
            }
            else {
                int ultimateAttackCost = trans.GetUltimateAbilityCost(this);
                if (omnitrixEnergy >= ultimateAttackCost && !ultimateAttack &&
                    !Player.HasBuff<UltimateAbilityCooldown>()) {
                    for (int i = 0; i < 50; i++) {
                        Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.Firework_Blue,
                            Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                        d.noGravity = true;
                    }

                    ultimateAttack = true;
                    return true;
                }

                if (ultimateAttack) {
                    for (int i = 0; i < 50; i++) {
                        Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.Firework_Yellow,
                            Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                        d.noGravity = true;
                    }

                    ultimateAttack = false;
                    return true;
                }
            }

            return false;
        }

        public bool ActivatePrimaryAbility() {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (!trans.HasPrimaryAbilityForState(this))
                return false;

            if (trans.TryActivatePrimaryAbility(Player, this))
                return true;

            if (!Player.HasBuff<PrimaryAbilityCooldown>() && !Player.HasBuff<PrimaryAbility>()) {
                Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), trans.GetPrimaryAbilityDuration(this));
                tranUsedAbilityId = currentTransformationId;
                return true;
            }

            return false;
        }

        public override bool CanUseItem(Item item) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanUseItem(Player, this, item) ?? true;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanBeHitByNPC(Player, this, npc, ref cooldownSlot) ?? true;
        }

        public override bool CanBeHitByProjectile(Projectile proj) {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return trans?.CanBeHitByProjectile(Player, this, proj) ?? true;
        }

        public override bool FreeDodge(Player.HurtInfo info) {
            var trans = CurrentTransformation;
            return trans?.FreeDodge(Player, this, info) ?? false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHurt(Player, this, ref modifiers);
        }

        public override void OnHurt(Player.HurtInfo info) {
            CurrentTransformation?.OnHurt(Player, this, info);
            base.OnHurt(info);
        }

        public override void PostHurt(Player.HurtInfo info) {
            CurrentTransformation?.PostHurt(Player, this, info);
            base.PostHurt(info);
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            CurrentTransformation?.OnHitAnything(Player, this, victim, x, y);
        }

        public override bool? CanHitNPCWithItem(Item item, NPC target) {
            var trans = CurrentTransformation;
            return trans?.CanHitNPCWithItem(Player, this, item, target);
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHitNPCWithItem(Player, this, item, target, ref modifiers);
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPCWithItem(Player, this, item, target, hit, damageDone);
            base.OnHitNPCWithItem(item, target, hit, damageDone);
        }

        public override bool? CanHitNPCWithProj(Projectile proj, NPC target) {
            var trans = CurrentTransformation;
            return trans?.CanHitNPCWithProjectile(Player, this, proj, target);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
            var trans = CurrentTransformation;
            trans?.ModifyHitNPCWithProjectile(Player, this, proj, target, ref modifiers);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPCWithProjectile(Player, this, proj, target, hit, damageDone);
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            var trans = CurrentTransformation;
            if (trans == null) return;

            trans.ModifyDrawInfo(Player, this, ref drawInfo);
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
            ref bool fullBright) {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();
            GetActiveOmnitrix()?.ApplyHandVisuals(Player, this, customSlot.HideVisuals);

            var trans = CurrentTransformation;
            if (trans != null)
                trans.DrawEffects(ref drawInfo);
        }

        public override void FrameEffects() {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();
            GetActiveOmnitrix()?.ApplyHandVisuals(Player, this, customSlot.HideVisuals);

            if (!customSlot.HideVisuals && isTransformed) {
                Player.wings = -1;
                Player.shoe = -1;
                Player.handoff = -1;
                Player.handon = -1;
                Player.back = -1;
                Player.waist = -1;
                Player.shield = -1;
            }

            CurrentTransformation?.FrameEffects(Player, this);
        }

        public override void PreUpdateMovement() {
            DashMovement();

            var trans = CurrentTransformation;
            if (trans != null)
                trans.PreUpdateMovement(Player, this);
        }

        private void DashMovement() {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0) {
                var trans = CurrentTransformation;
                if (trans?.FullID == "Ben10Mod:XLR8") {
                    Vector2 newVelocity = Player.velocity;

                    switch (DashDir) {
                        case DashLeft when Player.velocity.X > -DashVelocity:
                        case DashRight when Player.velocity.X < DashVelocity:
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * DashVelocity;
                            break;
                        default:
                            return;
                    }

                    DashDelay = DashCooldown;
                    DashTimer = DashDuration;
                    Player.velocity = newVelocity;
                }
            }

            if (DashDelay > 0) DashDelay--;
            if (DashTimer > 0) {
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;
                Player.GiveImmuneTimeForCollisionAttack(40);
                DashTimer--;
            }
        }

        private bool CanUseDash() {
            return Player.dashType == 0 && !Player.setSolar && !Player.mount.Active;
        }

        public override void OnEnterWorld() {
            ModContent.GetInstance<UISystem>().HideMyUI();
            if (!isTransformed)
                currentTransformationId = "";

            CurrentTransformation?.OnEnterWorld(Player, this);
        }

        private void EndPossession() {
            if (!inPossessionMode) return;

            inPossessionMode = false;
            possessedTargetIndex = -1;

            Player.position = prePossessionPosition;
            Player.invis = false;
            Player.immune = true;
            Player.immuneTime = 60;

            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f),
                    Scale: 1.8f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            var trans = CurrentTransformation;
            trans?.OnHitNPC(Player, this, target, hit, damageDone);

            base.OnHitNPC(target, hit, damageDone);

            var activeOmnitrix = GetActiveOmnitrix();
            if (isTransformed && !ultimateAttack && !UltimateAbilityEnabled && activeOmnitrix != null)
                omnitrixEnergy += activeOmnitrix.GetEnergyGainFromDamage(hit.Damage);
        }

        public override void PreUpdate() {
            var trans = CurrentTransformation;
            trans?.PreUpdate(Player, this);

            if (inPossessionMode) {
                if (possessedTargetIndex < 0 || possessedTargetIndex >= Main.maxNPCs) {
                    EndPossession();
                    return;
                }

                NPC npc = Main.npc[possessedTargetIndex];
                if (npc == null || !npc.active || npc.whoAmI != possessedTargetIndex || npc.life <= 0) {
                    EndPossession();
                    return;
                }

                if (Main.GameUpdateCount % 60 == 0 && npc.active && npc.life > 0) {
                    int dotDamage = 35;
                    npc.life -= dotDamage;
                    if (npc.life < 1) npc.life = 1;
                    CombatText.NewText(npc.Hitbox, new Color(180, 80, 255), dotDamage, dramatic: true);
                }
            }

            if (PrimaryAbilityEnabled && trans != null &&
                (trans.TransformationName == "Ghostfreak" || trans.TransformationName == "Bigchill")) {
                Player.gravity = 0f;
                Player.noKnockback = true;
                Player.noFallDmg = true;
                Player.fallStart = (int)(Player.position.Y / 16f);
            }

            ScreenShaderController.UpdateForLocalPlayer(Player);
        }

        public void RecordEventParticipation(NPC npc) {
            if (npc == null || !npc.active || npc.friendly || npc.townNPC || npc.CountsAsACritter)
                return;

            foreach (int eventId in GetActiveTrackedEvents()) {
                if (DoesNpcCountForEventParticipation(eventId, npc))
                    participatedEvents.Add(eventId);
            }
        }

        public bool UnlockTransformation(string transformationId, bool sync = true, bool showEffects = true) {
            if (TransformationHandler.HasTransformation(Player, transformationId))
                return false;

            unlockedTransformations.Add(transformationId);

            if (showEffects && Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer) {
                string name = TransformationLoader.Get(transformationId)?.TransformationName ?? "Unknown";
                Main.NewText($"{name} has been unlocked!", Color.LimeGreen);
                CombatText.NewText(Player.getRect(), Color.LimeGreen, $"{name}!", dramatic: true);
            }

            if (sync && Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.UnlockTransformation);
                packet.Write((byte)Player.whoAmI);
                packet.Write(transformationId);
                packet.Send(toClient: Player.whoAmI);
            }

            return true;
        }

        private void UpdateEventTransformationUnlocks() {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            HashSet<int> currentlyActiveEvents = new(GetActiveTrackedEvents());

            foreach (int eventId in currentlyActiveEvents)
                activeEvents.Add(eventId);

            List<int> completedEvents = new();
            foreach (int eventId in activeEvents) {
                if (!currentlyActiveEvents.Contains(eventId))
                    completedEvents.Add(eventId);
            }

            foreach (int eventId in completedEvents) {
                if (participatedEvents.Contains(eventId) && DidEventComplete(eventId)) {
                    string transformationId = GetTransformationIdForCompletedEvent(eventId);
                    if (!string.IsNullOrEmpty(transformationId))
                        UnlockTransformation(transformationId);
                }

                participatedEvents.Remove(eventId);
                activeEvents.Remove(eventId);
            }
        }

        private static IEnumerable<int> GetActiveTrackedEvents() {
            if (Main.bloodMoon)
                yield return EventBloodMoon;

            if (Main.eclipse)
                yield return EventSolarEclipse;

            if (Main.slimeRain)
                yield return EventSlimeRain;

            if (Main.pumpkinMoon)
                yield return EventPumpkinMoon;

            if (Main.snowMoon)
                yield return EventFrostMoon;

            if (Main.invasionType == InvasionID.GoblinArmy && Main.invasionSize > 0)
                yield return InvasionID.GoblinArmy;

            if (Main.invasionType == InvasionID.SnowLegion && Main.invasionSize > 0)
                yield return InvasionID.SnowLegion;

            if (Main.invasionType == InvasionID.PirateInvasion && Main.invasionSize > 0)
                yield return InvasionID.PirateInvasion;

            if (Main.invasionType == InvasionID.MartianMadness && Main.invasionSize > 0)
                yield return InvasionID.MartianMadness;

            if (DD2Event.Ongoing)
                yield return EventOldOnesArmy;
        }

        private static bool DoesNpcCountForEventParticipation(int eventId, NPC npc) {
            if (eventId == InvasionID.GoblinArmy)
                return IsGoblinArmyNpc(npc);

            return true;
        }

        private static bool DidEventComplete(int eventId) {
            switch (eventId) {
                case EventBloodMoon:
                case EventSolarEclipse:
                case EventSlimeRain:
                case EventPumpkinMoon:
                case EventFrostMoon:
                    return true;
                case InvasionID.GoblinArmy:
                    return NPC.downedGoblins;
                case InvasionID.SnowLegion:
                case InvasionID.PirateInvasion:
                case InvasionID.MartianMadness:
                case EventOldOnesArmy:
                    return true;
                default:
                    return false;
            }
        }

        private static string GetTransformationIdForCompletedEvent(int eventId) {
            switch (eventId) {
                case EventBloodMoon:
                    return "Ben10Mod:GhostFreak";
                case EventSolarEclipse:
                    return string.Empty;
                case EventSlimeRain:
                    return string.Empty;
                case EventPumpkinMoon:
                    return string.Empty;
                case EventFrostMoon:
                    return string.Empty;
                case InvasionID.GoblinArmy:
                    return "Ben10Mod:RipJaws";
                case InvasionID.SnowLegion:
                    return string.Empty;
                case InvasionID.PirateInvasion:
                    return string.Empty;
                case InvasionID.MartianMadness:
                    return string.Empty;
                case EventOldOnesArmy:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private static bool IsGoblinArmyNpc(NPC npc) {
            return npc.type == NPCID.GoblinPeon ||
                   npc.type == NPCID.GoblinThief ||
                   npc.type == NPCID.GoblinWarrior ||
                   npc.type == NPCID.GoblinSorcerer ||
                   npc.type == NPCID.GoblinArcher ||
                   npc.type == NPCID.GoblinSummoner;
        }
    }
}
