using Ben10Mod.Content.Transformations.XLR8;
using Ben10Mod.Keybinds;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Ben10Mod.Content.Transformations.HeatBlast;
using Ben10Mod.Content.Items.Accessories;
using Terraria.DataStructures;
using Ben10Mod.Content.Transformations.DiamondHead;
using Terraria.GameInput;
using Ben10Mod.Content.Buffs.Abilities.XLR8;
using Ben10Mod.Content.Buffs.Abilities.HeatBlast;
using Ben10Mod.Content.Projectiles;
using Ben10Mod.Content.Buffs.Abilities.DiamondHead;
using Ben10Mod.Content.Transformations.BuzzShock;
using Ben10Mod.Content.Transformations.ChromaStone;
using Ben10Mod.Content.Transformations.FourArms;
using Ben10Mod.Content.Transformations.GhostFreak;
using Ben10Mod.Content.Transformations.RipJaws;
using Ben10Mod.Content.Transformations.StinkFly;
using Ben10Mod.Content.Transformations.WildVine;
using Ben10Mod.Content.Buffs.Abilities.ChromaStone;
using Ben10Mod.Enums;
using Ben10Mod.Content.Interface;
using Ben10Mod.Content;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Weapons;
using Terraria.Audio;
using Ben10Mod.Content.Buffs.Abilities.BuzzShock;
using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Items.Accessories.Wings;
using Microsoft.Xna.Framework.Input;
using Terraria.Graphics.Effects;

namespace Ben10Mod
{
    public class OmnitrixPlayer : ModPlayer {

        public bool masterControl = false;
        
        public bool omnitrixEquipped = false;
        public bool isTransformed    = false;
        public bool wasTransformed   = false;
        public bool onCooldown       = false;
        public bool altAttack        = false;

        public bool advancedCircuitMatrix                         = false;
        public bool advancedCircuitMatrixEquippedWhileTransformed = false;
        
        public int cooldownTime       = 120;
        public int transformationTime = 300;

        public bool XLR8PrimaryAbilityEnabled           = false;
        public bool XLR8PrimaryAbilityWasEnabled        = false;
        public bool HeatBlastPrimaryAbilityEnabled      = false;
        public bool HeatBlastPrimaryAbilityWasEnabled   = false;
        public bool DiamondHeadPrimaryAbilityEnabled    = false;
        public bool DiamondHeadPrimaryAbilityWasEnabled = false;
        public bool ChromaStonePrimaryAbilityEnabled    = false;
        public bool ChromaStonePrimaryAbilityWasEnabled = false;
        public bool BuzzShockPrimaryAbilityEnabled      = false;
        public bool BuzzShockPrimaryAbilityWasEnabled   = false;

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

        public bool isPerformingHeatBlastDoubleJump;

        public TransformationEnum[] transformations = { TransformationEnum.HeatBlast, TransformationEnum.HeatBlast, TransformationEnum.HeatBlast, TransformationEnum.HeatBlast, TransformationEnum.HeatBlast };
        public TransformationEnum currTransformation = TransformationEnum.None;

        public List<TransformationEnum> unlockedTransformation = new List<TransformationEnum>() {TransformationEnum.HeatBlast};
            
        // Rainbow effect
        public Color[] colours = { Color.White, Color.LightPink, Color.Pink, Color.OrangeRed, Color.LightBlue, Color.Cyan, Color.LightGreen, Color.YellowGreen, Color.LightYellow, Color.Yellow };
        public float colourAmount = 0.00f;
        public int thisColour = 0;
        public int nextColour = 1;
        
        public        bool    inPossessionMode      = false;
        public        Vector2 prePossessionPosition = Vector2.Zero; // Save player's position before possession
        public        NPC     possessedTarget       = null;
        public        int     possessionTimer       = 0;
        private const int     PossessionDuration    = 360; // 6 seconds

        public Color GetChromaStoneOverlayColor()
        {
            return Color.Lerp(colours[thisColour], colours[nextColour], colourAmount);
        }

        public override void SaveData(TagCompound tag) {
            tag["masterControl"] = masterControl;
            int[] temp = new int[transformations.Length];
            for (int i = 0; i < temp.Length; i++) {
                temp[i] = (int)transformations[i];
            }
            tag["roster"] = temp;
            int[] tempList = new int[unlockedTransformation.Count];
            for (int i = 0; i < tempList.Length; i++) {
                tempList[i] = (int)unlockedTransformation[i];
            }
            tag["unlockedRoster"] = tempList;
            tag["currTransformation"] = (int)currTransformation;
        }

        public override void LoadData(TagCompound tag) {
            int[] temp = null;
            if (tag.TryGet("roster", out temp)) {
                if (temp != null) {
                    for (int i = 0; i < temp.Length; i++) {
                        transformations[i] = (TransformationEnum)temp[i];
                    }
                }
            }
            int[] tempList = null;
            if (tag.TryGet("unlockedRoster", out tempList)) {
                if (tempList != null) {
                    for (int i = 0; i < tempList.Length; i++) {
                        if (!TransformationHandler.HasTransformation(Player, (TransformationEnum)tempList[i])) {
                            unlockedTransformation.Add((TransformationEnum)tempList[i]);
                        }
                    }
                }
            }
            int tempInt = -1;
            tag.TryGet("currTransformation", out tempInt);
            currTransformation = (TransformationEnum)tempInt;
            tag.TryGet("masterControl", out masterControl);
        }


        public override void ResetEffects() {

            advancedCircuitMatrix = false;
            
            cooldownTime       = 120;
            transformationTime = 300;

            // Transformations
            isTransformed = false;
            onCooldown = false;
            omnitrixEquipped = false;

            // Abilities
            XLR8PrimaryAbilityEnabled = false;
            HeatBlastPrimaryAbilityEnabled = false;
            DiamondHeadPrimaryAbilityEnabled = false;
            ChromaStonePrimaryAbilityEnabled = false;
            BuzzShockPrimaryAbilityEnabled = false;

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
            
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();

            // Handles the detransformation effect

            if (wasTransformed != isTransformed) {
                var customSlot = ModContent.GetInstance<OmnitrixSlot>();
                if (customSlot != null) {
                    if (masterControl) {
                        TransformationHandler.Detransform(Player, 0, true, false);
                    } else {
                        TransformationHandler.Detransform(Player, advancedCircuitMatrixEquippedWhileTransformed ? cooldownTime * 2 : cooldownTime);
                    }

                    advancedCircuitMatrixEquippedWhileTransformed = false;
                }
                wasTransformed = false;
            }
            
            // XLR8 Transformation

            if (currTransformation == TransformationEnum.XLR8) {
                float multiplier = 1;

                if (XLR8PrimaryAbilityEnabled) {
                    multiplier = 2;
                }
                
                Player.moveSpeed                           *= 2.5f * multiplier;
                Player.accRunSpeed                         *= 2.0f * multiplier;
                Player.GetAttackSpeed(DamageClass.Generic) += (multiplier / 2);
                if (Math.Abs(Player.velocity.X) > 2) {
                    Player.jumpSpeed *= 1.5f * multiplier;
                    Player.waterWalk =  true;
                }

                if (Player.velocity.X == 0 && (Player.holdDownCardinalTimer[2] > 0 || Player.holdDownCardinalTimer[3] > 0)) {
                    if (Player.holdDownCardinalTimer[0] > 0) {
                        Player.maxFallSpeed *= 2.0f;
                    } else if (Player.holdDownCardinalTimer[1] > 0) {
                        Player.velocity.Y = -Player.maxFallSpeed * multiplier;
                    } else {
                        Player.maxFallSpeed = 0;
                    }
                }
            }

            // Heatblast Transformation

            if (currTransformation == TransformationEnum.HeatBlast) {
                Player.fireWalk = true;
                Player.lavaImmune = true;
            }

            // Diamondhead Transformation

            if (currTransformation == TransformationEnum.DiamondHead) {

                Player.statDefense += 25;

                if (DiamondHeadPrimaryAbilityEnabled) {
                    Player.moveSpeed   /= 10;
                    Player.lifeRegen   += 15;
                    Player.statDefense *= 2;
                    Player.releaseJump =  false;
                    Player.gravity     *= 2f;
                }
            }

            // Ripjaws Transformation

            if (currTransformation == TransformationEnum.RipJaws) {
                if (Player.wet) {
                    Player.merman = true;
                    Player.breathCD = 0;
                    Player.breath = Player.breathMax;
                    Player.GetDamage<HeroDamage>() *= 2.0f;
                }

                Player.accFlipper = true;
                
            }

            // Chromastone Transformation

            if (currTransformation == TransformationEnum.ChromaStone) {
            }

            // Buzzshock Transformation

            if (currTransformation == TransformationEnum.BuzzShock) { }

            // Fourarms Transformation

            if (currTransformation == TransformationEnum.FourArms) {
                
                Player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
                Player.GetCritChance(DamageClass.Generic) = 50;
                Player.noFallDmg = true;
                Player.jumpSpeed *= 1.9f;
            }

            // Stinkfly Transformation

            if (currTransformation == TransformationEnum.StinkFly) {
            }
            
            // Ghostfreak Transformation

            if (currTransformation == TransformationEnum.GhostFreak) {
                Player.noFallDmg = true;
            }

            // Wildvine Transformation

            if (currTransformation == TransformationEnum.WildVine) {
            }
        }
        
        public override void PostUpdate() {
            
            var abilitySlot = ModContent.GetInstance<AbilitySlot>();

            if (!isTransformed) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }
            
            // XLR8 Transformation

            if (currTransformation == TransformationEnum.XLR8) {

                if (KeybindSystem.PrimaryAbility.JustPressed && !Player.HasBuff(ModContent.BuffType<XLR8_Primary_Cooldown_Buff>()) && !Player.HasBuff(ModContent.BuffType<XLR8_Primary_Buff>())) {
                    Player.AddBuff(ModContent.BuffType<XLR8_Primary_Buff>(), 10 * 60);
                }

                if (XLR8PrimaryAbilityEnabled != XLR8PrimaryAbilityWasEnabled) {
                    XLR8PrimaryAbilityWasEnabled = false;
                    Player.AddBuff(ModContent.BuffType<XLR8_Primary_Cooldown_Buff>(), 10 * 60);
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Heatblast Transformation

            if (currTransformation == TransformationEnum.HeatBlast) {
                Random rand = new Random();
                int dustNum = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Flare, 0, rand.Next(-1, 2), rand.Next(-1, 2), Color.White, rand.Next(3));
                Main.dust[dustNum].noGravity = true;
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (!Player.HasBuff(ModContent.BuffType<HeatBlast_Primary_Cooldown_Buff>()) && !Player.HasBuff(ModContent.BuffType<HeatBlast_Primary_Buff>())) {
                        Player.AddBuff(ModContent.BuffType<HeatBlast_Primary_Buff>(), 60 * 60);
                    }
                }
                if (HeatBlastPrimaryAbilityEnabled) {
                    Vector2[] points = GenerateCirclePoints(250, 7 * (16));
                    for (int i = 0; i < points.Length; i++) {
                        dustNum = Dust.NewDust(points[i] + Player.Center, 1, 1, DustID.Torch, rand.Next(-1, 2), rand.Next(-1, 2));
                        Main.dust[dustNum].noGravity = true;
                    }
                    foreach (NPC npc in Main.npc) {
                        if (Player.Distance(npc.Center) <= 10 * (16) && !npc.friendly) {
                            if (!npc.HasBuff(BuffID.OnFire3)) {
                                npc.AddBuff(BuffID.OnFire3, 10 * 60);
                            }
                        }
                    }
                }

                Player.fireWalk = true;
                Player.lavaImmune = true;

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<HeatBlastWings>());
                
                if (!Filters.Scene["Ben10Mod:HeatDistort"].IsActive())
                    Filters.Scene.Activate("Ben10Mod:HeatDistort", Player.Center);

                Filters.Scene["Ben10Mod:HeatDistort"]
                    .GetShader()
                    .UseTargetPosition(Player.Center)
                    .UseColor(1f, 0.45f, 0.1f)
                    .UseIntensity(0.35f);
            } else {
                if (Filters.Scene["Ben10Mod:HeatDistort"].IsActive())
                    Filters.Scene["Ben10Mod:HeatDistort"].Deactivate();
            }

            if (HeatBlastPrimaryAbilityEnabled != HeatBlastPrimaryAbilityWasEnabled) {
                HeatBlastPrimaryAbilityWasEnabled = false;
                Player.AddBuff(ModContent.BuffType<HeatBlast_Primary_Cooldown_Buff>(), 10 * 60);
            }

            // Diamondhead Transformation

            if (currTransformation == TransformationEnum.DiamondHead) {
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (!Player.HasBuff(ModContent.BuffType<DiamondHead_Primary_Cooldown_Buff>()) && !Player.HasBuff(ModContent.BuffType<DiamondHead_Primary_Buff>())) {
                        Player.AddBuff(ModContent.BuffType<DiamondHead_Primary_Buff>(), 60 * 60);
                        Player.velocity = Vector2.Zero;
                    }
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            if (DiamondHeadPrimaryAbilityEnabled != DiamondHeadPrimaryAbilityWasEnabled) {
                DiamondHeadPrimaryAbilityWasEnabled = false;
                // Player.AddBuff(ModContent.BuffType<DiamondHead_Primary_Cooldown_Buff>(), 25 * 60);
            }

            if (DiamondHeadPrimaryAbilityEnabled) {
                Lighting.AddLight(Player.Center, new Vector3(0.4f, 0.3f, 0.8f));
            }

            // Ripjaws Transformation

            if (currTransformation == TransformationEnum.RipJaws) {

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Chromastone Transformation

            if (currTransformation == TransformationEnum.ChromaStone) {
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (!Player.HasBuff(ModContent.BuffType<ChromaStone_Primary_Cooldown_Buff>()) && !Player.HasBuff(ModContent.BuffType<ChromaStone_Primary_Buff>())) {
                        Player.AddBuff(ModContent.BuffType<ChromaStone_Primary_Buff>(), 30 * 60);
                    }
                }

                if (ChromaStonePrimaryAbilityEnabled)
                {
                    Lighting.AddLight(Player.Center, GetChromaStoneOverlayColor().ToVector3());
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());

            } else {
            }

            if (ChromaStonePrimaryAbilityEnabled == false) {
                ChromaStoneAbsorbtion = 0;
            }

            // Buzzshock Transformation

            if (currTransformation == TransformationEnum.BuzzShock) {
                if (KeybindSystem.PrimaryAbility.JustPressed) {
                    if (Main.myPlayer == Player.whoAmI) {
                        SoundEngine.PlaySound(SoundID.Item8, Player.position);
                        Random random = new Random();
                        for (int i = 0; i < 50; i++) {
                            int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1, Player.height + 1, DustID.UltraBrightTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 2);
                            Main.dust[dustNum].noGravity = true;
                        }
                        Player.Teleport(Main.MouseWorld, TeleportationStyleID.DebugTeleport);
                        for (int i = 0; i < 50; i++) {
                            int dustNum = Dust.NewDust(Player.position - new Vector2(1, 1), Player.width + 1, Player.height + 1, DustID.UltraBrightTorch, random.Next(-4, 5), random.Next(-4, 5), 1, Color.White, 2);
                            Main.dust[dustNum].noGravity = true;
                        }

                        Player.AddBuff(BuffID.ChaosState, 60 * 6);
                    }
                }

                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            if (BuzzShockPrimaryAbilityEnabled != BuzzShockPrimaryAbilityWasEnabled) {
                BuzzShockPrimaryAbilityWasEnabled = false;
                Player.AddBuff(ModContent.BuffType<BuzzShock_Primary_Cooldown_Buff>(), 10 * 60);
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
                
                if (KeybindSystem.PrimaryAbility.Current) { // Phasing Logic
                    
                    Vector2 move                    = Vector2.Zero;
                    if (Player.controlLeft)  move.X -= 1f;
                    if (Player.controlRight) move.X += 1f;
                    if (Player.controlUp)    move.Y -= 1f;
                    if (Player.controlDown)  move.Y += 1f;
                    
                    if (move == Vector2.Zero)
                        return;

                    float phaseSpeed = 6f;

                    move              = Vector2.Normalize(move);
                    Player.velocity   = Vector2.Zero;
                    Player.gravity    = 0f;
                    Player.fallStart  = (int)(Player.position.Y / 16f);
                    Player.velocity.Y = move.Y;
                    
                    Player.position += move * phaseSpeed;
                }
                
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }

            // Wildvine Transformation

            if (currTransformation == TransformationEnum.WildVine) {
                abilitySlot.FunctionalItem = new Item(ModContent.ItemType<BlankAccessory>());
            }
            
            if (inPossessionMode)
            {
                if (possessedTarget == null || !possessedTarget.active || currTransformation != TransformationEnum.GhostFreak )
                {
                    EndPossession(); // Safety: end if target dies/missing
                    return;
                }

                // Sync player to possessed NPC's center (camera follows player naturally)
                Player.immuneNoBlink = true;
                Player.immuneTime    = 999;
                Player.Center        = possessedTarget.Center;

                // Optional: Sync some velocity for "riding" feel (but keep player locked)
                Player.velocity = possessedTarget.velocity * 0.8f; // Slight lag for smoothness

                // Lock controls to prevent input interference
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
                    possessedTarget.SimpleStrikeNPC(Player.HeldItem.damage, Player.direction, false, 0, DamageClass.Magic);
                    EndPossession();
                }
            }
        }

        public override bool CanUseItem(Item item) {
            return !(currTransformation == TransformationEnum.GhostFreak && KeybindSystem.PrimaryAbility.Current);
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) {
            return !(currTransformation == TransformationEnum.GhostFreak && KeybindSystem.PrimaryAbility.Current);
        }

        public override bool CanBeHitByProjectile(Projectile proj) {
            return !(currTransformation == TransformationEnum.GhostFreak && KeybindSystem.PrimaryAbility.Current);
        }

        public override void OnHurt(Player.HurtInfo info) {
            if (ChromaStonePrimaryAbilityEnabled) {
                ChromaStoneAbsorbtion += Math.Max(info.Damage / 5, 0);
            }

            base.OnHurt(info);
        }

        public override void OnHitAnything(float x, float y, Entity victim) {
            if (!Main.npc[victim.whoAmI].HasBuff(BuffID.OnFire3) && currTransformation == TransformationEnum.HeatBlast) {
                Main.npc[victim.whoAmI].AddBuff(BuffID.OnFire3, 3 * 60);
            }
        }

        public static Vector2[] GenerateCirclePoints(int numberOfPoints, float radius) {
            Vector2[] circlePoints = new Vector2[numberOfPoints];
            float angleIncrement = 360f / numberOfPoints;

            for (int i = 0; i < numberOfPoints; i++) {
                float angle = i * angleIncrement;
                float radians = MathF.PI / 180 * angle;

                float x = radius * MathF.Cos(radians);
                float y = radius * MathF.Sin(radians);

                circlePoints[i] = new Vector2(x, y);
            }

            return circlePoints;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (isTransformed) {
            }
            switch (currTransformation) {
                case TransformationEnum.HeatBlast:
                    drawInfo.colorArmorHead = Color.White;
                    drawInfo.colorArmorBody = Color.White;
                    drawInfo.colorArmorLegs = Color.White;
                    break;
                case TransformationEnum.GhostFreak when KeybindSystem.PrimaryAbility.Current:
                    drawInfo.colorArmorHead.A /= 2;
                    drawInfo.colorArmorBody.A /= 2;
                    drawInfo.colorArmorLegs.A /= 2;
                    break;
                case TransformationEnum.GhostFreak when inPossessionMode:
                    Player.invis              = true;
                    break;
                case TransformationEnum.Arctiguana:
                    break;
                case TransformationEnum.BuzzShock:
                    break;
                case TransformationEnum.ChromaStone when ChromaStonePrimaryAbilityEnabled:
                    Color overlayColor = GetChromaStoneOverlayColor();
                    drawInfo.colorArmorHead = overlayColor;
                    drawInfo.colorArmorBody = overlayColor;
                    drawInfo.colorArmorLegs = overlayColor;
                    break;
                case TransformationEnum.DiamondHead:
                    break;
                case TransformationEnum.FourArms:
                case TransformationEnum.RipJaws:
                case TransformationEnum.StinkFly:
                case TransformationEnum.WildVine:
                case TransformationEnum.XLR8:
                case TransformationEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Set the visuals for the aliens

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {

            var customSlot = ModContent.GetInstance<OmnitrixSlot>();

            if (omnitrixEquipped) {
                if (customSlot.FunctionalItem.type == ModContent.ItemType<PrototypeOmnitrix>()) {
                    var costume = ModContent.GetInstance<PrototypeOmnitrix>();
                    if (!customSlot.HideVisuals && !isTransformed) {
                        if (onCooldown) {
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
                    r = 255;
                    g = 255;
                    b = 255;
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
                        if (onCooldown) {
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
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                }
                if (currTransformation == TransformationEnum.RipJaws) {
                    var costume = ModContent.GetInstance<RipJaws>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    if (Player.wet) {
                        Player.legs = EquipLoader.GetEquipSlot(Mod, "RipJaws_alt", EquipType.Legs);
                    } else {
                        Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                        Player.waist = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Waist);
                    }
                }
                if (currTransformation == TransformationEnum.StinkFly) {
                    var costume = ModContent.GetInstance<StinkFly>();
                    Player.head  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    Player.wings = EquipLoader.GetEquipSlot(Mod, ModContent.GetInstance<StinkFlyWings>().Name, EquipType.Wings);
                }
                if (currTransformation == TransformationEnum.WildVine) {
                    var costume = ModContent.GetInstance<WildVine>();
                    Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                }
                if (currTransformation == TransformationEnum.XLR8) {
                    var costume = ModContent.GetInstance<XLR8>();
                    if (XLR8PrimaryAbilityEnabled) {
                        Player.head = EquipLoader.GetEquipSlot(Mod, "XLR8_alt", EquipType.Head);
                    } else {
                        Player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
                    }
                    Player.body                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
                    Player.legs                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
                    Player.back                  = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Back);
                    Player.armorEffectDrawShadow = true;
                }
            }
        }

        public override void PreUpdateMovement() {
            DashMovement();
        }

        private void DashMovement() {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0) {
                if (currTransformation == TransformationEnum.XLR8) {
                    Vector2 newVelocity = Player.velocity;

                    switch (DashDir) {
                        // Only apply the dash velocity if our current speed in the wanted direction is less than DashVelocity
                        case DashUp when Player.velocity.Y > -DashVelocity:
                            return;
                        case DashDown when Player.velocity.Y < DashVelocity: {
                                return;
                                // Y-velocity is set here
                                // If the direction requested was DashUp, then we adjust the velocity to make the dash appear "faster" due to gravity being immediately in effect
                                // This adjustment is roughly 1.3x the intended dash velocity
                                float dashDirection = DashDir == DashDown ? 1 : -1.3f;
                                newVelocity.Y = dashDirection * DashVelocity;
                                break;
                            }
                        case DashLeft when Player.velocity.X > -DashVelocity:
                        case DashRight when Player.velocity.X < DashVelocity: {
                                // X-velocity is set here
                                float dashDirection = DashDir == DashRight ? 1 : -1;
                                newVelocity.X = dashDirection * DashVelocity;
                                break;
                            }
                        default:
                            return; // not moving fast enough, so don't start our dash
                    }

                    // start our dash
                    DashDelay = DashCooldown;
                    DashTimer = DashDuration;
                    Player.velocity = newVelocity;
                    // Here you'd be able to set an effect that happens when the dash first activates
                    // Some examples include:  the larger smoke effect from the Master Ninja Gear and Tabi
                }
            }

            if (DashDelay > 0)
                DashDelay--;

            if (DashTimer > 0) { // dash is active
                                 // This is where we set the afterimage effect.  You can replace these two lines with whatever you want to happen during the dash
                                 // Some examples include:  spawning dust where the player is, adding buffs, making the player immune, etc.
                                 // Here we take advantage of "player.eocDash" and "player.armorEffectDrawShadowEOCShield" to get the Shield of Cthulhu's afterimage effect
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;

                Player.GiveImmuneTimeForCollisionAttack(40);

                // count down frames remaining
                DashTimer--;
            }
        }

        private bool CanUseDash() {
            return Player.dashType == 0 // player doesn't have Tabi or EoCShield equipped (give priority to those dashes)
                && !Player.setSolar // player isn't wearing solar armor
                && !Player.mount.Active; // player isn't mounted, since dashes on a mount look weird
        }

        public override void OnEnterWorld() {
            ModContent.GetInstance<UISystem>().HideMyUI();
            if (!isTransformed) {
                currTransformation = TransformationEnum.None;
            }
        }
        
        private void EndPossession()
        {
            if (!inPossessionMode) return;

            inPossessionMode = false;
            possessedTarget  = null;

            // Snap player back to pre-possession position
            Player.position = prePossessionPosition;

            // Re-enable visuals and remove immunity
            Player.invis  = false;
            Player.immune = false;

            // Optional: Short invincibility after return
            Player.immune     = true;
            Player.immuneTime = 60;

            // Visual/sound cue for return
            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.3f, Volume = 0.8f }, Player.Center);
            for (int i = 0; i < 30; i++)
            {
                Dust d = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(6f, 6f), Scale: 1.8f);
                d.noGravity = true;
            }
        }


        // This is where we unlock transformations
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            base.OnHitNPC(target, hit, damageDone);
            if (target.life <= 0) {
                if (Main.bloodMoon)
                {
                    addTransformation(TransformationEnum.GhostFreak);
                }
                if (NPC.downedGoblins)
                {
                    addTransformation(TransformationEnum.RipJaws);
                }
            }
        }

        public override void PreUpdate() {

            if (colourAmount >= 1.0f) {
                thisColour++;
                nextColour++;
                colourAmount = 0.0f;
            }

            if (thisColour >= colours.Length) {
                thisColour = 0;
            }
            if (nextColour >= colours.Length) {
                nextColour = 0;
            }

            colourAmount += 0.1f;
        }

        public void addTransformation(TransformationEnum transformation) { 
            if (!TransformationHandler.HasTransformation(Player, transformation)) {
                unlockedTransformation.Add(transformation);
                Main.NewText(Player.name + " has unlocked " + transformation.GetName(), Color.Green);
            }
        }
    }
}
