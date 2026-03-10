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
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Ben10Mod
{
    public class OmnitrixPlayer : ModPlayer
    {
        public bool masterControl = false;

        public bool omnitrixEquipped  = false;
        public bool prototypeOmnitrix = false;
        public bool isTransformed     = false;
        public bool wasTransformed    = false;
        public bool onCooldown        = false;
        public bool altAttack         = false;
        public bool ultimateAttack    = false;
        public bool ultimateForm      = false;

        public int cooldownTime       = 120;
        public int transformationTime = 300;

        public bool PrimaryAbilityEnabled     = false;
        public bool PrimaryAbilityWasEnabled  = false;
        public bool UltimateAbilityEnabled    = false;
        public bool UltimateAbilityWasEnabled = false;
        public string tranUsedAbilityId       = ""; // changed to string for modularity

        public int ChromaStoneAbsorbtion = 0;

        // Dashing
        public       int DashDir      = -1;
        public const int DashDown     = 0;
        public const int DashUp       = 1;
        public const int DashRight    = 2;
        public const int DashLeft     = 3;
        public       int DashVelocity = 15;
        public       int DashDelay    = 0;
        public       int DashTimer    = 0;
        public const int DashCooldown = 15;
        public const int DashDuration = 15;

        public string[]     transformationSlots     = { "Ben10Mod:HeatBlast", "", "", "", "" };
        public string       currentTransformationId = "";
        public List<string> unlockedTransformations = new() { "Ben10Mod:HeatBlast" };

        public bool showingUI = false;

        public bool  omnitrixUpdating    = false;
        public bool  omnitrixWasUpdating = false;
        public float omnitrixEnergy      = 0f;
        public float omnitrixEnergyMax   = 0f;
        public float omnitrixEnergyRegen = 0f;

        public        bool    inPossessionMode      = false;
        public        Vector2 prePossessionPosition = Vector2.Zero;
        public        int     possessedTargetIndex  = -1;
        public        int     possessionTimer       = 0;
        private const int     PossessionDuration    = 360;

        public bool snowflake                                     = false;
        public bool advancedCircuitMatrix                         = false;
        public bool advancedCircuitMatrixEquippedWhileTransformed = false;

        // Helper properties (used everywhere now)
        public Transformation CurrentTransformation 
            => TransformationLoader.Get(currentTransformationId);

        public bool IsTransformed => !string.IsNullOrEmpty(currentTransformationId);

        public override void SaveData(TagCompound tag)
        {
            tag["masterControl"]           = masterControl;
            tag["currentTransformationId"] = currentTransformationId;
            tag["omnitrixEnergy"]          = omnitrixEnergy;

            tag["roster"]         = transformationSlots;
            tag["unlockedRoster"] = unlockedTransformations.ToArray();
        }

        public override void LoadData(TagCompound tag)
        {
            tag.TryGet("masterControl", out masterControl);
            omnitrixEnergy = tag.TryGet("omnitrixEnergy", out omnitrixEnergy) ? omnitrixEnergy : 0f;

            tag.TryGet("currentTransformationId", out currentTransformationId);

            if (tag.TryGet("roster", out string[] rosterArray))
                transformationSlots = rosterArray;

            unlockedTransformations.Clear();
            if (tag.TryGet("unlockedRoster", out string[] unlockedArray))
                unlockedTransformations.AddRange(unlockedArray);

            if (!unlockedTransformations.Contains("Ben10Mod:HeatBlast"))
                unlockedTransformations.Insert(0, "Ben10Mod:HeatBlast");
        }

        public override void ResetEffects()
        {
            advancedCircuitMatrix = false;
            snowflake             = false;

            omnitrixEnergyMax   = 0;
            omnitrixEnergyRegen = 0;

            isTransformed     = false;
            onCooldown        = false;
            omnitrixEquipped  = false;
            prototypeOmnitrix = false;

            omnitrixUpdating = false;

            PrimaryAbilityEnabled  = false;
            UltimateAbilityEnabled = false;

            // Handle dashing
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
        }

        public override void PostUpdateBuffs()
        {
            var abilitySlot  = ModContent.GetInstance<AbilitySlot>();
            var omnitrixSlot = ModContent.GetInstance<OmnitrixSlot>();

            // Detransform handling
            if (wasTransformed != isTransformed)
            {
                var customSlot = ModContent.GetInstance<OmnitrixSlot>();
                if (customSlot != null)
                {
                    if (masterControl)
                    {
                        TransformationHandler.Detransform(Player, 0, true, false);
                    }
                    else
                    {
                        if (omnitrixSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>())
                            TransformationHandler.Detransform(Player, ModContent.GetInstance<PrototypeOmnitrix>().TimeoutDuration);
                        if (omnitrixSlot.FunctionalItem.type == ModContent.ItemType<RecalibratedOmnitrix>())
                            TransformationHandler.Detransform(Player, 0, addCooldown: false);
                    }
                }

                wasTransformed = isTransformed;
            }

            // Prototype → Recalibrated update effect
            if (omnitrixUpdating != omnitrixWasUpdating)
            {
                if (omnitrixSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>())
                {
                    Random random = new Random();
                    for (int i = 0; i < 25; i++)
                    {
                        int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                            Player.height + 1, DustID.BlueTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 4);
                        Main.dust[dustNum].noGravity = true;
                    }

                    omnitrixSlot.FunctionalItem = new Item(ModContent.ItemType<RecalibratedOmnitrix>());
                }

                omnitrixWasUpdating = omnitrixUpdating;
            }

            // Let the current alien handle ALL passive effects
            var trans = CurrentTransformation;
            if (trans != null)
                trans.UpdateEffects(Player, this);
        }

        public override void PostUpdate()
        {
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();

            if (!isTransformed)
            {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            omnitrixEnergy += (omnitrixEnergyRegen / 120f);
            if (omnitrixEnergy > omnitrixEnergyMax) omnitrixEnergy = omnitrixEnergyMax;

            if (KeybindSystem.OpenTransformationScreen.JustPressed && omnitrixEquipped)
            {
                if (!showingUI)
                {
                    ModContent.GetInstance<UISystem>().ShowMyUI();
                    showingUI = true;
                }
                else
                {
                    ModContent.GetInstance<UISystem>().HideMyUI();
                    showingUI = false;
                }
            }

            if (Main.mouseRight && Main.mouseRightRelease && Player.HeldItem.ModItem is PlumbersBadge)
                altAttack = !altAttack;

            // Let the current alien handle PostUpdate logic (wings, dust, teleport, etc.)
            var trans = CurrentTransformation;
            if (trans != null)
                trans.PostUpdate(Player, this);

            // Possession mode
            if (inPossessionMode)
            {
                if (possessedTargetIndex < 0 || possessedTargetIndex >= Main.maxNPCs)
                {
                    EndPossession();
                    return;
                }

                NPC npc = Main.npc[possessedTargetIndex];
                if (npc == null || !npc.active || npc.whoAmI != possessedTargetIndex)
                {
                    EndPossession();
                    return;
                }

                Player.immuneNoBlink = true;
                Player.immuneTime    = 999;
                Player.Center        = npc.Center;
                Player.velocity      = npc.velocity * 0.8f;

                Player.controlJump    = false;
                Player.controlDown    = false;
                Player.controlLeft    = false;
                Player.controlRight   = false;
                Player.controlUp      = false;
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlHook    = false;

                possessionTimer--;
                if (possessionTimer <= 0)
                {
                    npc.SimpleStrikeNPC(Player.HeldItem.damage * 2, Player.direction, false, 0, DamageClass.Magic);
                    EndPossession();
                }
            }

            // Ultimate energy check
            if (isTransformed && ultimateAttack && omnitrixEnergy < (CurrentTransformation?.UltimateAbilityCost ?? 50))
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Firework_Yellow,
                        Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                    d.noGravity = true;
                }
                ultimateAttack = false;
            }

            // Ability keybinds
            if (isTransformed)
            {
                if (KeybindSystem.PrimaryAbility.JustPressed && CurrentTransformation != null)
                    ActivatePrimaryAbility();

                if (KeybindSystem.UltimateAbility.JustPressed && CurrentTransformation != null)
                    ActivateUltimateAbility();
            }

            // Cooldown buff application
            if (PrimaryAbilityEnabled != PrimaryAbilityWasEnabled)
            {
                if (CurrentTransformation != null)
                    Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), CurrentTransformation.PrimaryAbilityCooldown);

                PrimaryAbilityWasEnabled = PrimaryAbilityEnabled;
                ChromaStoneAbsorbtion = 0;
            }

            if (UltimateAbilityEnabled != UltimateAbilityWasEnabled)
            {
                if (CurrentTransformation != null)
                    Player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(), CurrentTransformation.UltimateAbilityCooldown);

                UltimateAbilityWasEnabled = UltimateAbilityEnabled;
            }
        }

        public bool ActivateUltimateAbility()
        {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (trans.HasUltimateAbility && !Player.HasBuff<UltimateAbilityCooldown>() && !Player.HasBuff<UltimateAbility>())
            {
                if (omnitrixEnergy >= trans.UltimateAbilityCost)
                {
                    Player.AddBuff(ModContent.BuffType<UltimateAbility>(), trans.UltimateAbilityDuration);
                    tranUsedAbilityId = currentTransformationId;
                    omnitrixEnergy -= trans.UltimateAbilityCost;
                    return true;
                }
            }
            else
            {
                if (omnitrixEnergy >= trans.UltimateAbilityCost && !ultimateAttack && !Player.HasBuff<UltimateAbilityCooldown>())
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.Firework_Blue,
                            Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                        d.noGravity = true;
                    }
                    ultimateAttack = true;
                    return true;
                }
                if (ultimateAttack)
                {
                    for (int i = 0; i < 50; i++)
                    {
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

        public bool ActivatePrimaryAbility()
        {
            var trans = CurrentTransformation;
            if (trans == null) return false;

            if (!Player.HasBuff<PrimaryAbilityCooldown>() && !Player.HasBuff<PrimaryAbility>())
            {
                Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), trans.PrimaryAbilityDuration);
                tranUsedAbilityId = currentTransformationId;
                return true;
            }
            return false;
        }

        public override bool CanUseItem(Item item)
        {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return !(trans != null && (trans.TransformationName == "Ghostfreak" || trans.TransformationName == "Bigchill") && PrimaryAbilityEnabled);
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return !(trans != null && (trans.TransformationName == "Ghostfreak" || trans.TransformationName == "Bigchill") && PrimaryAbilityEnabled);
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (Player.whoAmI != Main.myPlayer) return false;
            var trans = CurrentTransformation;
            return !(trans != null && (trans.TransformationName == "Ghostfreak" || trans.TransformationName == "Bigchill") && PrimaryAbilityEnabled);
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (PrimaryAbilityEnabled && CurrentTransformation?.TransformationName == "Chromastone")
                ChromaStoneAbsorbtion += Math.Max(info.Damage / 5, 0);

            base.OnHurt(info);
        }

        public override void OnHitAnything(float x, float y, Entity victim)
        {
            if (victim is NPC npc && CurrentTransformation?.TransformationName == "Heatblast")
            {
                if (!npc.HasBuff(BuffID.Frostburn2) && snowflake)
                    npc.AddBuff(BuffID.Frostburn2, 10 * 60);
                else if (!npc.HasBuff(BuffID.OnFire3) && !snowflake)
                    npc.AddBuff(BuffID.OnFire3, 10 * 60);
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            var trans = CurrentTransformation;
            if (trans == null) return;

            trans.ModifyDrawInfo(ref drawInfo);
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();

            if (omnitrixEquipped)
            {
                if (customSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>())
                {
                    var costume = ModContent.GetInstance<PrototypeOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed)
                    {
                        if (omnitrixUpdating)
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixUpdating", EquipType.HandsOn);
                        else if (onCooldown)
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixAlt", EquipType.HandsOn);
                        else
                            Player.handon = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.HandsOn);
                    }
                }
                else if (customSlot.FunctionalItem.type == ModContent.ItemType<RecalibratedOmnitrix>())
                {
                    var costume = ModContent.GetInstance<RecalibratedOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed)
                    {
                        if (onCooldown)
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrixAlt", EquipType.HandsOn);
                        else
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrix", EquipType.HandsOn);
                    }
                }
            }

            var trans = CurrentTransformation;
            if (trans != null)
                trans.DrawEffects(ref drawInfo);
        }

        public override void FrameEffects()
        {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();

            if (omnitrixEquipped)
            {
                // Omnitrix hand visuals (unchanged)
                if (customSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>())
                {
                    var costume = ModContent.GetInstance<PrototypeOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed)
                    {
                        if (omnitrixUpdating)
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixUpdating", EquipType.HandsOn);
                        else if (onCooldown)
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixAlt", EquipType.HandsOn);
                        else
                            Player.handon = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.HandsOn);
                    }
                }
                else if (customSlot.FunctionalItem.type == ModContent.ItemType<RecalibratedOmnitrix>())
                {
                    var costume = ModContent.GetInstance<RecalibratedOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed)
                    {
                        if (onCooldown)
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrixAlt", EquipType.HandsOn);
                        else
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrix", EquipType.HandsOn);
                    }
                }
            }

            if (!customSlot.HideVisuals && isTransformed)
            {
                Player.wings   = -1;
                Player.shoe    = -1;
                Player.handoff = -1;
                Player.handon  = -1;
                Player.back    = -1;
                Player.waist   = -1;
                Player.shield  = -1;
            }

            // All alien costume logic is now handled inside each Transformation class via FrameEffects hook if needed
            var trans = CurrentTransformation;
            if (trans != null)
            {
                // Future: trans.FrameEffects(Player); – you can add this hook later if you want per-alien FrameEffects
            }
        }

        public override void PreUpdateMovement()
        {
            DashMovement();

            var trans = CurrentTransformation;
            if (trans != null)
                trans.PreUpdateMovement(Player, this);
        }

        private void DashMovement()
        {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                var trans = CurrentTransformation;
                if (trans?.TransformationName == "XLR8")
                {
                    Vector2 newVelocity = Player.velocity;

                    switch (DashDir)
                    {
                        case DashLeft when Player.velocity.X > -DashVelocity:
                        case DashRight when Player.velocity.X < DashVelocity:
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * DashVelocity;
                            break;
                        default:
                            return;
                    }

                    DashDelay       = DashCooldown;
                    DashTimer       = DashDuration;
                    Player.velocity = newVelocity;
                }
            }

            if (DashDelay > 0) DashDelay--;
            if (DashTimer > 0)
            {
                Player.eocDash                        = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;
                Player.GiveImmuneTimeForCollisionAttack(40);
                DashTimer--;
            }
        }

        private bool CanUseDash()
        {
            return Player.dashType == 0 && !Player.setSolar && !Player.mount.Active;
        }

        public override void OnEnterWorld()
        {
            ModContent.GetInstance<UISystem>().HideMyUI();
            if (!isTransformed)
                currentTransformationId = "";
        }

        private void EndPossession()
        {
            if (!inPossessionMode) return;

            inPossessionMode     = false;
            possessedTargetIndex = -1;

            Player.position = prePossessionPosition;
            Player.invis    = false;
            Player.immune   = true;
            Player.immuneTime = 60;

            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f), Scale: 1.8f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            if (target.life <= 0)
            {
                if (Main.bloodMoon)
                    TransformationHandler.AddTransformation(Player, "Ben10Mod:GhostFreak");

                if (NPC.downedGoblins)
                    TransformationHandler.AddTransformation(Player, "Ben10Mod:RipJaws");
            }

            if (isTransformed && !ultimateAttack && omnitrixEnergyRegen == 0 && !UltimateAbilityEnabled)
                omnitrixEnergy += Math.Max(hit.Damage / 25, 1);
        }

        public override void PreUpdate()
        {
            if (inPossessionMode)
            {
                if (possessedTargetIndex < 0 || possessedTargetIndex >= Main.maxNPCs)
                {
                    EndPossession();
                    return;
                }

                NPC npc = Main.npc[possessedTargetIndex];
                if (npc == null || !npc.active || npc.whoAmI != possessedTargetIndex || npc.life <= 0)
                {
                    EndPossession();
                    return;
                }

                if (Main.GameUpdateCount % 60 == 0 && npc.active && npc.life > 0)
                {
                    int dotDamage = 35;
                    npc.life -= dotDamage;
                    if (npc.life < 1) npc.life = 1;
                    CombatText.NewText(npc.Hitbox, new Color(180, 80, 255), dotDamage, dramatic: true);
                }
            }

            var trans = CurrentTransformation;
            if (PrimaryAbilityEnabled && trans != null && (trans.TransformationName == "Ghostfreak" || trans.TransformationName == "Bigchill"))
            {
                Player.gravity     = 0f;
                Player.noKnockback = true;
                Player.noFallDmg   = true;
                Player.fallStart   = (int)(Player.position.Y / 16f);
            }

            // Shader effects (kept here because they are global)
            if (UltimateAbilityEnabled && Main.netMode != NetmodeID.Server)
            {
                if (trans?.TransformationName == "Bigchill")
                {
                    if (!Filters.Scene["Ben10Mod:Bluescale"].IsActive())
                        Filters.Scene.Activate("Ben10Mod:Bluescale");
                }
                else if (Filters.Scene["Ben10Mod:Bluescale"].IsActive())
                    Filters.Scene.Deactivate("Ben10Mod:Bluescale");

                if (trans?.TransformationName == "XLR8")
                {
                    if (!Filters.Scene["Ben10Mod:Grayscale"].IsActive())
                    {
                        Filters.Scene.Activate("Ben10Mod:Grayscale");
                        Filters.Scene["Ben10Mod:Grayscale"].GetShader().Shader.Parameters["strength"]?.SetValue(1f);
                    }
                }
                else if (Filters.Scene["Ben10Mod:Grayscale"].IsActive())
                {
                    Filters.Scene["Ben10Mod:Grayscale"].GetShader().Shader.Parameters["strength"]?.SetValue(0f);
                    Filters.Scene.Deactivate("Ben10Mod:Grayscale");
                }
            }
        }

        public void AddTransformation(string transformationId)
        {
            if (TransformationHandler.HasTransformation(Player, transformationId)) return;

            unlockedTransformations.Add(transformationId);
            var name = TransformationLoader.Get(transformationId)?.TransformationName ?? "Unknown";
            Main.NewText(Player.name + " has unlocked " + name, Color.Green);

            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)Ben10Mod.MessageType.UnlockTransformation);
                packet.Write((byte)Player.whoAmI);
                packet.Write(transformationId);
                packet.Send(toClient: Player.whoAmI);
            }
        }
    }
}