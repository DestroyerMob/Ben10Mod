using Ben10Mod.Keybinds;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.CommandLine.Help;
using System.Data.SqlTypes;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Ben10Mod.Content.Transformations.HeatBlast;
using Ben10Mod.Content.Items.Accessories;
using Terraria.DataStructures;
using Ben10Mod.Content.Transformations.DiamondHead;
using Ben10Mod.Content.Transformations.XLR8;
using Ben10Mod.Content.Transformations.BuzzShock;
using Ben10Mod.Content.Transformations.ChromaStone;
using Ben10Mod.Content.Transformations.FourArms;
using Ben10Mod.Content.Transformations.GhostFreak;
using Ben10Mod.Content.Transformations.RipJaws;
using Ben10Mod.Content.Transformations.StinkFly;
using Ben10Mod.Content.Transformations.WildVine;
using Ben10Mod.Enums;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Terraria.Audio;
using Ben10Mod.Content.Items.Accessories.Wings;
using Ben10Mod.Content.Items.Vanity.ShaderDyes;
using Ben10Mod.Content.Transformations.BigChill;
using Ben10Mod.Content.Transformations.EyeGuy;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.WorldBuilding;

namespace Ben10Mod {
    public class OmnitrixPlayer : ModPlayer {

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

        public bool               PrimaryAbilityEnabled     = false;
        public bool               PrimaryAbilityWasEnabled  = false;
        public bool               UltimateAbilityEnabled    = false;
        public bool               ultimateAbilityWasEnabled = false;
        public TransformationEnum tranUsedAbility           = TransformationEnum.None;

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

        public TransformationEnum[] transformations = {
            TransformationEnum.HeatBlast, TransformationEnum.HeatBlast, TransformationEnum.HeatBlast,
            TransformationEnum.HeatBlast, TransformationEnum.HeatBlast
        };

        public TransformationEnum currTransformation = TransformationEnum.None;

        public List<TransformationEnum> unlockedTransformation = new List<TransformationEnum>()
            { TransformationEnum.HeatBlast };

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

        public override void SaveData(TagCompound tag) {

            tag["masterControl"]      = masterControl;
            tag["currTransformation"] = (int)currTransformation;
            tag["omnitrixEnergy"]     = omnitrixEnergy;

            tag["roster"] = transformations.Select(t => (int)t).ToArray();

            tag["unlockedRoster"] = unlockedTransformation
                .Where(t => t != TransformationEnum.None)
                .Select(t => (int)t)
                .ToArray();
        }

        public override void LoadData(TagCompound tag) {

            tag.TryGet("masterControl", out masterControl);

            int currInt = -1;
            tag.TryGet("currTransformation", out currInt);
            currTransformation = (TransformationEnum)currInt;
            omnitrixEnergy     = tag.TryGet("omnitrixEnergy", out omnitrixEnergy) ? omnitrixEnergy : 0f;

            if (tag.TryGet("roster", out int[] rosterArray) && rosterArray != null) {
                for (int i = 0; i < Math.Min(rosterArray.Length, transformations.Length); i++) {
                    transformations[i] = (TransformationEnum)rosterArray[i];
                }
            }

            unlockedTransformation.Clear();
            if (tag.TryGet("unlockedRoster", out int[] unlockedArray) && unlockedArray != null) {
                foreach (int id in unlockedArray) {
                    var trans = (TransformationEnum)id;
                    if (trans != TransformationEnum.None && !unlockedTransformation.Contains(trans))
                        unlockedTransformation.Add(trans);
                }
            }

            if (!unlockedTransformation.Contains(TransformationEnum.HeatBlast))
                unlockedTransformation.Insert(0, TransformationEnum.HeatBlast);
        }


        public override void ResetEffects() {
            advancedCircuitMatrix = false;
            snowflake             = false;

            cooldownTime        = 120;
            transformationTime  = 300;
            omnitrixEnergyMax   = 0;
            omnitrixEnergyRegen = 0;

            // Transformations
            isTransformed     = false;
            onCooldown        = false;
            omnitrixEquipped  = false;
            prototypeOmnitrix = false;

            // Updating
            omnitrixUpdating = false;

            // Abilities
            PrimaryAbilityEnabled  = false;
            UltimateAbilityEnabled = false;

            // Handle dashing
            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15) {
                DashDir = DashDown;
            }
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15) {
                DashDir = DashUp;
            }
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15) {
                DashDir = DashRight;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15) {
                DashDir = DashLeft;
            }
            else {
                DashDir = -1;
            }
        }

        // Handles players abilities

        public override void PostUpdateBuffs() {
            var abilitySlot  = ModContent.GetInstance<AbilitySlot>();
            var omnitrixSlot = ModContent.GetInstance<OmnitrixSlot>();

            // Handles the detransformation effect

            if (wasTransformed != isTransformed) {
                var customSlot = ModContent.GetInstance<OmnitrixSlot>();
                if (customSlot != null) {
                    if (masterControl) {
                        TransformationHandler.Detransform(Player, 0, true, false);
                    }
                    else {
                        if (omnitrixSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>())
                            TransformationHandler.Detransform(Player,
                                advancedCircuitMatrixEquippedWhileTransformed ? cooldownTime * 2 : cooldownTime);
                        if (omnitrixSlot.FunctionalItem.type == ModContent.ItemType<RecalibratedOmnitrix>())
                            TransformationHandler.Detransform(Player, 0, addCooldown: false);
                    }

                    advancedCircuitMatrixEquippedWhileTransformed = false;
                }

                wasTransformed = isTransformed;
            }

            if (omnitrixUpdating != omnitrixWasUpdating) {
                if (omnitrixSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>()) {
                    Random random = new Random();
                    for (int i = 0; i < 25; i++) {
                        int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                            Player.height + 1, DustID.BlueTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White,
                            4);
                        Main.dust[dustNum].noGravity = true;
                    }

                    omnitrixSlot.FunctionalItem = new Item(ModContent.ItemType<RecalibratedOmnitrix>());
                }

                omnitrixWasUpdating = omnitrixUpdating;
            }

            // XLR8 Transformation

            if (currTransformation == TransformationEnum.XLR8) {
                float multiplier = 1;

                if (PrimaryAbilityEnabled) {
                    multiplier = 2;
                }

                Player.moveSpeed                           *= 2.5f * multiplier;
                Player.accRunSpeed                         *= 2.0f * multiplier;
                Player.GetAttackSpeed(DamageClass.Generic) += (multiplier / 2);
                if (Math.Abs(Player.velocity.X) > 2) {
                    Player.jumpSpeed *= 1.5f * multiplier;
                    Player.waterWalk =  true;
                }

                if (Player.velocity.X == 0 &&
                    (Player.holdDownCardinalTimer[2] > 0 || Player.holdDownCardinalTimer[3] > 0)) {
                    if (Player.holdDownCardinalTimer[0] > 0) {
                        Player.maxFallSpeed *= 2.0f;
                    }
                    else if (Player.holdDownCardinalTimer[1] > 0) {
                        Player.velocity.Y = -Player.maxFallSpeed * multiplier;
                    }
                    else {
                        Player.maxFallSpeed = 0;
                    }
                }
            }

            // Heatblast Transformation

            if (currTransformation == TransformationEnum.HeatBlast) {
                Player.fireWalk   = true;
                Player.lavaImmune = true;
            }

            // Diamondhead Transformation

            if (currTransformation == TransformationEnum.DiamondHead) {

                Player.statDefense += 20;
                Player.wingTimeMax =  0;
                Player.wingTime    =  0;

                if (PrimaryAbilityEnabled) {
                    Player.moveSpeed   /= 10;
                    Player.lifeRegen   += 15;
                    Player.statDefense *= 1.5f;
                    Player.releaseJump =  false;
                    Player.gravity     *= 2f;
                }
            }

            // Ripjaws Transformation

            if (currTransformation == TransformationEnum.RipJaws) {
                if (Player.wet || Main.raining) {
                    Player.merman                  =  true;
                    Player.breathCD                =  0;
                    Player.breath                  =  Player.breathMax;
                    Player.GetDamage<HeroDamage>() *= 2.0f;
                    Lighting.AddLight(Player.Center, new Vector3(1, 1, 1));
                    Player.maxFallSpeed *= 2;
                    Player.moveSpeed    *= 4;
                }
                else {
                    Player.breath -= 4;
                    if (Player.breath <= 1) {
                        Player.lifeRegen -= 60;
                    }
                }
                
                Player.accFlipper = true;

            }

            // Chromastone Transformation

            if (currTransformation == TransformationEnum.ChromaStone) { }

            // Buzzshock Transformation

            if (currTransformation == TransformationEnum.BuzzShock) { }

            // Fourarms Transformation

            if (currTransformation == TransformationEnum.FourArms) {
                Player.GetAttackSpeed(DamageClass.Melee)  += 0.25f;
                Player.GetCritChance(DamageClass.Generic) =  50;
                Player.noFallDmg                          =  true;
                Player.jumpSpeed                          *= 1.9f;
            }

            // Stinkfly Transformation

            if (currTransformation == TransformationEnum.StinkFly) { }

            // Ghostfreak Transformation

            if (currTransformation == TransformationEnum.GhostFreak) {
                Player.noFallDmg = true;
            }

            // Wildvine Transformation

            if (currTransformation == TransformationEnum.WildVine) { }
        }

        public override void PostUpdate() {

            var abilitySlot = ModContent.GetInstance<AbilitySlot>();

            if (!isTransformed) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            omnitrixEnergy += (omnitrixEnergyRegen / 120);

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



            // XLR8 Transformation

            if (currTransformation == TransformationEnum.XLR8) {

                if (KeybindSystem.PrimaryAbility.JustPressed &&
                    !Player.HasBuff(ModContent.BuffType<PrimaryAbilityCooldown>()) &&
                    !Player.HasBuff(ModContent.BuffType<PrimaryAbility>())) {
                    Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), 10 * 60);
                    tranUsedAbility = currTransformation;
                }

                if (KeybindSystem.UltimateAbility.JustPressed &&
                    !Player.HasBuff(ModContent.BuffType<UltimateAbilityCooldown>()) &&
                    !Player.HasBuff(ModContent.BuffType<UltimateAbility>()) && omnitrixEnergy >= 50) {
                    Player.AddBuff(ModContent.BuffType<UltimateAbility>(), 4 * 60);
                    tranUsedAbility =  currTransformation;
                    omnitrixEnergy  -= 50;
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Heatblast Transformation

            if (currTransformation == TransformationEnum.HeatBlast) {
                Random rand = new Random();
                int dustNum = Dust.NewDust(Player.position, Player.width, Player.height,
                    snowflake ? DustID.IceTorch : DustID.Flare, 0, rand.Next(-1, 2), rand.Next(-1, 2), Color.White,
                    rand.Next(3));
                Main.dust[dustNum].noGravity = true;
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (!Player.HasBuff(ModContent.BuffType<PrimaryAbilityCooldown>()) &&
                        !Player.HasBuff(ModContent.BuffType<PrimaryAbility>())) {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), 60 * 60);
                        tranUsedAbility = currTransformation;
                    }
                }

                if (PrimaryAbilityEnabled) {
                    Vector2[] points = GenerateCirclePoints(250, 7 * (16));
                    for (int i = 0; i < points.Length; i++) {
                        dustNum = Dust.NewDust(points[i] + Player.Center, 1, 1,
                            snowflake ? DustID.IceTorch : DustID.Torch, rand.Next(-1, 2), rand.Next(-1, 2));
                        Main.dust[dustNum].noGravity = true;
                    }

                    foreach (NPC npc in Main.npc) {
                        if (Player.Distance(npc.Center) <= 10 * (16) && !npc.friendly) {
                            if (!npc.HasBuff(BuffID.Frostburn2) && snowflake) {
                                npc.AddBuff(BuffID.Frostburn2, 10 * 60);
                            }
                            else if (!npc.HasBuff(BuffID.OnFire3)) {
                                npc.AddBuff(BuffID.OnFire3, 10 * 60);
                            }
                        }
                    }
                }

                Player.fireWalk   = true;
                Player.lavaImmune = true;

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<HeatBlastExtraJumpAccessory>());
            }

            // Diamondhead Transformation

            if (currTransformation == TransformationEnum.DiamondHead) {
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (!Player.HasBuff(ModContent.BuffType<PrimaryAbilityCooldown>()) &&
                        !Player.HasBuff(ModContent.BuffType<PrimaryAbility>())) {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), 60 * 60);
                        tranUsedAbility = currTransformation;
                        Player.velocity = Vector2.Zero;
                    }
                }

                if (PrimaryAbilityEnabled) {
                    Player.velocity = new Vector2(float.Clamp(Player.velocity.X, -0.5f, 0.5f),
                        Math.Max(0, Player.velocity.Y));
                    Lighting.AddLight(Player.Center, new Vector3(0.4f, 0.3f, 0.8f));
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Ripjaws Transformation

            if (currTransformation == TransformationEnum.RipJaws) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Chromastone Transformation

            if (currTransformation == TransformationEnum.ChromaStone) {
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (!Player.HasBuff(ModContent.BuffType<PrimaryAbilityCooldown>()) &&
                        !Player.HasBuff(ModContent.BuffType<PrimaryAbility>())) {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), 30 * 60);
                        tranUsedAbility = currTransformation;
                    }
                }

                if (PrimaryAbilityEnabled) {
                    Lighting.AddLight(Player.Center, Main.DiscoColor.ToVector3());
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());

            }

            if (!PrimaryAbilityEnabled) { }

            // Buzzshock Transformation

            if (currTransformation == TransformationEnum.BuzzShock) {
                if (KeybindSystem.PrimaryAbility.JustPressed && !Player.HasBuff<PrimaryAbilityCooldown>()) {
                    if (Main.myPlayer == Player.whoAmI) {
                        SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        Random random = new Random();
                        for (int i = 0; i < 50; i++) {
                            int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                                Player.height + 1, DustID.UltraBrightTorch, random.Next(-4, 5), random.Next(-4, 5), 1,
                                Color.White, 2);
                            Main.dust[dustNum].noGravity = true;
                        }

                        Player.Teleport(Main.MouseWorld, TeleportationStyleID.DebugTeleport);
                        for (int i = 0; i < 50; i++) {
                            int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1,
                                Player.height + 1, DustID.UltraBrightTorch, random.Next(-4, 5), random.Next(-4, 5), 1,
                                Color.White, 2);
                            Main.dust[dustNum].noGravity = true;
                        }

                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 60 * 6);
                    }
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Fourarms Transformation

            if (currTransformation == TransformationEnum.FourArms) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Stinkfly Transformation

            if (currTransformation == TransformationEnum.StinkFly) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<StinkFlyWings>());
            }

            // Ghostfreak Transformation

            if (currTransformation == TransformationEnum.GhostFreak) {
                Random random = new Random();

                if (KeybindSystem.PrimaryAbility.Current && !Player.HasBuff<PrimaryAbilityCooldown>() &&
                    !Player.HasBuff<PrimaryAbility>()) {
                    Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), 15 * 60);
                    tranUsedAbility = currTransformation;
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Wildvine Transformation

            if (currTransformation == TransformationEnum.WildVine) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }
            
            // Bigchill Transformation

            if (currTransformation == TransformationEnum.BigChill)
            {
                if (KeybindSystem.PrimaryAbility.Current && !Player.HasBuff<PrimaryAbilityCooldown>() &&
                    !Player.HasBuff<PrimaryAbility>()) {
                    Player.AddBuff(ModContent.BuffType<PrimaryAbility>(), 15 * 60);
                    tranUsedAbility = currTransformation;
                }
                
                if (KeybindSystem.UltimateAbility.JustPressed &&
                    !Player.HasBuff(ModContent.BuffType<UltimateAbilityCooldown>()) &&
                    !Player.HasBuff(ModContent.BuffType<UltimateAbility>())) {
                    Player.AddBuff(ModContent.BuffType<UltimateAbility>(), 10 * 60);
                    tranUsedAbility = currTransformation;
                }
                
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BigChillWings>());
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
                Player.immuneTime    = 999;
                Player.Center        = npc.Center;

                Player.velocity = npc.velocity * 0.8f;

                Player.controlJump    = false;
                Player.controlDown    = false;
                Player.controlLeft    = false;
                Player.controlRight   = false;
                Player.controlUp      = false;
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlHook    = false;

                possessionTimer--;
                if (possessionTimer <= 0) {
                    npc.SimpleStrikeNPC(Player.HeldItem.damage * 2, Player.direction, false, 0,
                        DamageClass.Magic);
                    EndPossession();
                }
            }

            if (isTransformed) {
                if (KeybindSystem.UltimateAbility.JustPressed && currTransformation.HasUltimateAttack())
                    if (!Player.HasBuff<UltimateAbilityCooldown>() && !Player.HasBuff<UltimateAbility>()) {
                        for (int i = 0; i < 50; i++) {
                            Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                                ultimateAttack ? DustID.Firework_Yellow : DustID.Firework_Blue,
                                Main.rand.NextVector2Circular(6f, 6f), Scale: Main.rand.NextFloat(1.5f, 2.5f));
                            d.noGravity = true;
                        }

                        ultimateAttack = !ultimateAttack;
                    }
            }

            if (PrimaryAbilityEnabled != PrimaryAbilityWasEnabled) {
                PrimaryAbilityWasEnabled = PrimaryAbilityEnabled;
                switch (tranUsedAbility) {
                    case TransformationEnum.HeatBlast: {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 30 * 60);
                        break;
                    }
                    case TransformationEnum.XLR8: {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 30 * 60);
                        break;
                    }
                    case TransformationEnum.DiamondHead: {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 30 * 60);
                        break;
                    }
                    case TransformationEnum.ChromaStone: {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 60 * 60);
                        break;
                    }
                    case TransformationEnum.GhostFreak: {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 30 * 60);
                        break;
                    }
                    case TransformationEnum.BigChill:
                    {
                        Player.AddBuff(ModContent.BuffType<PrimaryAbilityCooldown>(), 30 * 60);
                        break;
                    }
                }

                ChromaStoneAbsorbtion    = 0;
                PrimaryAbilityWasEnabled = PrimaryAbilityEnabled;
            }

            if (UltimateAbilityEnabled != ultimateAbilityWasEnabled) {
                ultimateAbilityWasEnabled = UltimateAbilityEnabled;
                switch (tranUsedAbility) {
                    case TransformationEnum.XLR8: {
                        Player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(), 30 * 60);
                        break;
                    }
                    case TransformationEnum.BigChill: {
                        Player.AddBuff(ModContent.BuffType<UltimateAbilityCooldown>(), 30 * 60);
                        break;
                    }
                }
            }
        }

        public override bool CanUseItem(Item item) {
            if (Player.whoAmI != Main.myPlayer) return false;
            return !((currTransformation == TransformationEnum.GhostFreak || currTransformation == TransformationEnum.BigChill) && PrimaryAbilityEnabled);
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
            if (Player.whoAmI != Main.myPlayer) return false;
            return !((currTransformation == TransformationEnum.GhostFreak || currTransformation == TransformationEnum.BigChill) && PrimaryAbilityEnabled);
        }

        public override bool CanBeHitByProjectile(Projectile proj) {
            if (Player.whoAmI != Main.myPlayer) return false;
            return !((currTransformation == TransformationEnum.GhostFreak || currTransformation == TransformationEnum.BigChill) && PrimaryAbilityEnabled);
        }

        public override void OnHurt(Player.HurtInfo info) {
            if (PrimaryAbilityEnabled && currTransformation == TransformationEnum.ChromaStone) {
                ChromaStoneAbsorbtion += Math.Max(info.Damage / 5, 0);
            }

            base.OnHurt(info);
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            if (victim is NPC npc && currTransformation == TransformationEnum.HeatBlast) {
                if (!npc.HasBuff(BuffID.Frostburn2) && snowflake) {
                    npc.AddBuff(BuffID.Frostburn2, 10 * 60);
                }
                else if (!npc.HasBuff(BuffID.OnFire3) && !snowflake) {
                    npc.AddBuff(BuffID.OnFire3, 10 * 60);
                }
            }
        }

        public static Vector2[] GenerateCirclePoints(int numberOfPoints, float radius) {
            Vector2[] circlePoints   = new Vector2[numberOfPoints];
            float     angleIncrement = 360f / numberOfPoints;

            for (int i = 0; i < numberOfPoints; i++) {
                float angle   = i * angleIncrement;
                float radians = MathF.PI / 180 * angle;

                float x = radius * MathF.Cos(radians);
                float y = radius * MathF.Sin(radians);

                circlePoints[i] = new Vector2(x, y);
            }

            return circlePoints;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (isTransformed) { }

            switch (currTransformation) {
                case TransformationEnum.HeatBlast:
                    drawInfo.colorArmorHead = Color.White;
                    drawInfo.colorArmorBody = Color.White;
                    drawInfo.colorArmorLegs = Color.White;
                    break;
                case TransformationEnum.GhostFreak when PrimaryAbilityEnabled:
                    drawInfo.colorArmorHead.A /= 2;
                    drawInfo.colorArmorBody.A /= 2;
                    drawInfo.colorArmorLegs.A /= 2;
                    break;
                case TransformationEnum.GhostFreak when inPossessionMode:
                    Player.invis = true;
                    break;
                case TransformationEnum.BigChill when PrimaryAbilityEnabled:
                    drawInfo.colorArmorHead.A /= 2;
                    drawInfo.colorArmorBody.A /= 2;
                    drawInfo.colorArmorLegs.A /= 2;
                    break;
                case TransformationEnum.Arctiguana:
                    break;
                case TransformationEnum.BuzzShock:
                    break;
                case TransformationEnum.ChromaStone when PrimaryAbilityEnabled:
                    Color overlayColor = Main.DiscoColor;
                    // drawInfo.colorArmorHead = overlayColor;
                    // drawInfo.colorArmorBody = overlayColor;
                    // drawInfo.colorArmorLegs = overlayColor;
                    break;
                case TransformationEnum.DiamondHead:
                case TransformationEnum.FourArms:
                case TransformationEnum.RipJaws:
                case TransformationEnum.StinkFly:
                case TransformationEnum.WildVine:
                case TransformationEnum.XLR8:
                case TransformationEnum.EyeGuy:
                case TransformationEnum.None:
                    break;
            }
        }

        // Set the visuals for the aliens

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
            ref bool fullBright) {


            var customSlot = ModContent.GetInstance<OmnitrixSlot>();

            if (omnitrixEquipped) {
                if (customSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>()) {
                    var costume = ModContent.GetInstance<PrototypeOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed) {
                        if (omnitrixUpdating) {
                            Player.handon =
                                EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixUpdating", EquipType.HandsOn);
                        }
                        else if (onCooldown) {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixAlt", EquipType.HandsOn);
                        }
                        else {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.HandsOn);
                        }
                    }
                }
                else if (customSlot.FunctionalItem.type == ModContent.ItemType<RecalibratedOmnitrix>()) {
                    var costume = ModContent.GetInstance<RecalibratedOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed) {
                        if (onCooldown) {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrixAlt", EquipType.HandsOn);
                        }
                        else {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrix", EquipType.HandsOn);
                        }
                    }
                }
            }

            if (!customSlot.HideVisuals) {
                if (currTransformation == TransformationEnum.HeatBlast) {
                    r          = 255;
                    g          = 255;
                    b          = 255;
                    fullBright = true;
                }
            }

        }

        public override void FrameEffects() {
            var customSlot = ModContent.GetInstance<OmnitrixSlot>();

            if (omnitrixEquipped) {
                if (customSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>()) {
                    var costume = ModContent.GetInstance<PrototypeOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed) {
                        if (omnitrixUpdating) {
                            Player.handon =
                                EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixUpdating", EquipType.HandsOn);
                        }
                        else if (onCooldown) {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "PrototypeOmnitrixAlt", EquipType.HandsOn);
                        }
                        else {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.HandsOn);
                        }
                    }
                }
                else if (customSlot.FunctionalItem.type == ModContent.ItemType<RecalibratedOmnitrix>()) {
                    var costume = ModContent.GetInstance<RecalibratedOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed) {
                        if (onCooldown) {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrixAlt", EquipType.HandsOn);
                        }
                        else {
                            Player.handon = EquipLoader.GetEquipSlot(Mod, "RecalibratedOmnitrix", EquipType.HandsOn);
                        }
                    }
                }
            }

            if (!customSlot.HideVisuals) {
                if (isTransformed) {
                    Player.wings   = -1;
                    Player.shoe    = -1;
                    Player.handoff = -1;
                    Player.handon  = -1;
                    Player.back    = -1;
                    Player.waist   = -1;
                    Player.shield  = -1;
                }

                if (currTransformation == TransformationEnum.BuzzShock) {
                    var costume = ModContent.GetInstance<BuzzShock>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                }

                if (currTransformation == TransformationEnum.ChromaStone) {
                    var costume = ModContent.GetInstance<ChromaStone>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    if (PrimaryAbilityEnabled) {
                        int shaderID = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DiscoDye>());
                        GameShaders.Armor.GetShaderFromItemId(ModContent.ItemType<DiscoDye>())?.UseColor(Main.DiscoR / 255f, Main.DiscoG / 255f, Main.DiscoB / 255f);
                        Player.cHead = shaderID;
                        Player.cBody = shaderID;
                        Player.cLegs = shaderID;
                    }
                }

                if (currTransformation == TransformationEnum.DiamondHead) {
                    var costume = ModContent.GetInstance<DiamondHead>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    Player.back = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Back);
                }

                if (currTransformation == TransformationEnum.FourArms) {
                    var costume = ModContent.GetInstance<FourArms>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                }

                if (currTransformation == TransformationEnum.GhostFreak) {
                    if (inPossessionMode) {
                        Player.head = -1;
                        Player.body = -1;
                        Player.legs = -1;
                    }
                    else {
                        var costume = ModContent.GetInstance<GhostFreak>();
                        Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                        Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                        Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    }

                }

                if (currTransformation == TransformationEnum.HeatBlast) {
                    var costume = ModContent.GetInstance<HeatBlast>();
                    if (snowflake) {
                        Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name + "Alt", EquipType.Head);
                        Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name + "Alt", EquipType.Body);
                        Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name + "Alt", EquipType.Legs);
                    }
                    else {
                        Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                        Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                        Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    }
                }

                if (currTransformation == TransformationEnum.RipJaws) {
                    var costume = ModContent.GetInstance<RipJaws>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    if (Player.wet) {
                        Player.legs = EquipLoader.GetEquipSlot(Mod, "RipJaws_alt", EquipType.Legs);
                    }
                    else {
                        Player.legs  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                        Player.waist = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Waist);
                    }
                }

                if (currTransformation == TransformationEnum.StinkFly) {
                    var costume = ModContent.GetInstance<StinkFly>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    Player.wings = EquipLoader.GetEquipSlot(Mod, ModContent.GetInstance<StinkFlyWings>().Name,
                        EquipType.Wings);
                }

                if (currTransformation == TransformationEnum.WildVine) {
                    var costume = ModContent.GetInstance<WildVine>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                }

                if (currTransformation == TransformationEnum.XLR8) {
                    var costume = ModContent.GetInstance<XLR8>();
                    if (PrimaryAbilityEnabled) {
                        Player.head = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
                    }
                    else {
                        Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    }

                    Player.body                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    Player.back                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Back);
                    Player.armorEffectDrawShadow = true;
                }

                if (currTransformation == TransformationEnum.EyeGuy) {
                    var costume = ModContent.GetInstance<EyeGuy>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                }
                
                if (currTransformation == TransformationEnum.BigChill) {
                    var costume = ModContent.GetInstance<BigChill>();
                    Player.head = ultimateForm ? EquipLoader.GetEquipSlot(Mod, "Ultimate" + costume.Name, EquipType.Head) : EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = ultimateForm ? EquipLoader.GetEquipSlot(Mod, "Ultimate" + costume.Name, EquipType.Body) : EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = ultimateForm ? EquipLoader.GetEquipSlot(Mod, "Ultimate" + costume.Name, EquipType.Legs) : EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    Player.wings = ultimateForm ? EquipLoader.GetEquipSlot(Mod, "Ultimate" + ModContent.GetInstance<BigChillWings>().Name,
                        EquipType.Wings) : EquipLoader.GetEquipSlot(Mod, ModContent.GetInstance<BigChillWings>().Name,
                        EquipType.Wings);
                }
            }
        }

        public override void PreUpdateMovement() {
            DashMovement();

            if (PrimaryAbilityEnabled && (currTransformation == TransformationEnum.GhostFreak || currTransformation == TransformationEnum.BigChill)) { // Phasing Logic
                Vector2 input                    = Vector2.Zero;
                if (Player.controlLeft) input.X  -= 1f;
                if (Player.controlRight) input.X += 1f;
                if (Player.controlUp) input.Y    -= 1f;
                if (Player.controlDown) input.Y  += 1f;

                const float speed = 14.5f;
                const float damp  = 0.82f;

                if (input != Vector2.Zero) {
                    input.Normalize();
                    Vector2 move = input * speed;
                    
                    if (input.Y < 0) move.Y -= 3f;

                    Player.position += move;
                }
                else {
                    Player.velocity *= damp;
                    Player.position += Player.velocity;
                }
                
                Player.velocity = Vector2.Zero;
            }
        }

        private void DashMovement() {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0) {
                if (currTransformation == TransformationEnum.XLR8) {
                    Vector2 newVelocity = Player.velocity;

                    switch (DashDir) {
                        // Only apply the dash velocity if our current speed in the wanted direction is less than DashVelocity
                        case DashUp when Player.velocity.Y > -DashVelocity:
                            return;
                            // float dashDirection = DashDir == DashDown ? 1 : -1.3f;
                            // newVelocity.Y = dashDirection * DashVelocity;
                            // break;
                        case DashDown when Player.velocity.Y < DashVelocity: {
                            return;
                            // float dashDirection = DashDir == DashDown ? 1 : -1.3f;
                            // newVelocity.Y = dashDirection * DashVelocity;
                            // break;
                        }
                        case DashLeft when Player.velocity.X > -DashVelocity:
                        case DashRight when Player.velocity.X < DashVelocity: {
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * DashVelocity;
                            break;
                        }
                        default:
                            return;
                    }
                    
                    DashDelay       = DashCooldown;
                    DashTimer       = DashDuration;
                    Player.velocity = newVelocity;
                }
            }

            if (DashDelay > 0)
                DashDelay--;

            if (DashTimer > 0) {
                Player.eocDash                        = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;

                Player.GiveImmuneTimeForCollisionAttack(40);
                
                DashTimer--;
            }
        }

        private bool CanUseDash() {
            return
                Player.dashType == 0
                && !Player.setSolar
                && !Player.mount.Active;
        }

        public override void OnEnterWorld() {
            ModContent.GetInstance<UISystem>().HideMyUI();
            if (!isTransformed) {
                currTransformation = TransformationEnum.None;
            }
        }

        private void EndPossession() {
            if (!inPossessionMode) return;

            inPossessionMode     = false;
            possessedTargetIndex = -1;
            
            Player.position = prePossessionPosition;
            
            Player.invis  = false;
            Player.immune = false;
            
            Player.immune     = true;
            Player.immuneTime = 60;
            
            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
            for (int i = 0; i < 30; i++) {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f),
                    Scale: 1.8f);
                d.noGravity = true;
            }
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            base.OnHitNPC(target, hit, damageDone);
            if (target.life <= 0) {
                if (Main.bloodMoon) {
                    AddTransformation(TransformationEnum.GhostFreak);
                }

                if (NPC.downedGoblins) {
                    AddTransformation(TransformationEnum.RipJaws);
                }
            }


            if (isTransformed && !ultimateAttack && omnitrixEnergyRegen == 0 && !UltimateAbilityEnabled && !ultimateAttack)
                omnitrixEnergy += Math.Max(hit.Damage / 25, 1);
        }

        public override void PreUpdate() {
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
                
                if (Main.GameUpdateCount % 60 == 0) {
                    if (npc.active && npc.whoAmI == possessedTargetIndex && npc.life > 0) {
                        int dotDamage = 35;
                        npc.life -= dotDamage;
                        if (npc.life < 1) npc.life = 1;
                        CombatText.NewText(npc.Hitbox, new Color(180, 80, 255), dotDamage, dramatic: true);
                    }
                }
            }

            if (PrimaryAbilityEnabled &&
                currTransformation is TransformationEnum.GhostFreak or TransformationEnum.BigChill) {
                Player.gravity     = 0f;
                Player.noKnockback = true;
                Player.noFallDmg   = true;
                Player.fallStart   = (int)(Player.position.Y / 16f);
            }

            if (UltimateAbilityEnabled && Main.netMode != NetmodeID.Server &&
                currTransformation == TransformationEnum.BigChill) {
                if (!Filters.Scene["Ben10Mod:Bluescale"].IsActive()) {
                    Filters.Scene.Activate("Ben10Mod:Bluescale");
                }
            }
            else if (Filters.Scene["Ben10Mod:Bluescale"].IsActive()) {
                Filters.Scene.Deactivate("Ben10Mod:Bluescale");
            }

            if (UltimateAbilityEnabled && Main.netMode != NetmodeID.Server &&
                currTransformation == TransformationEnum.XLR8) {
                if (!Filters.Scene["Ben10Mod:Grayscale"].IsActive()) {
                    Filters.Scene.Activate("Ben10Mod:Grayscale");
                    Filters.Scene["Ben10Mod:Grayscale"].GetShader().Shader.Parameters["strength"]?.SetValue(1f);
                }
            }
            else if (Filters.Scene["Ben10Mod:Grayscale"].IsActive()) {
                Filters.Scene["Ben10Mod:Grayscale"].GetShader().Shader.Parameters["strength"]?.SetValue(0f);
                Filters.Scene.Deactivate("Ben10Mod:Grayscale");
            }
        }

        public void AddTransformation(TransformationEnum transformation) {
            if (!TransformationHandler.HasTransformation(Player, transformation)) {
                unlockedTransformation.Add(transformation);
                Main.NewText(Player.name + " has unlocked " + transformation.GetName(), Color.Green);

                if (Main.netMode == NetmodeID.Server) {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)Ben10Mod.MessageType.UnlockTransformation);
                    packet.Write((byte)Player.whoAmI);
                    packet.Write((int)transformation);
                    packet.Send(toClient: Player.whoAmI);
                }

            }
        }
    }
}