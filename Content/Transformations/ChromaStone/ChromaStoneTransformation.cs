using System;
using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Vanity.ShaderDyes;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStoneTransformation : Transformation {
    public override string FullID               => "Ben10Mod:ChromaStone";
    public override string TransformationName   => "Chromastone";
    public override string IconPath             => "Ben10Mod/Content/Interface/ChromaStoneSelect";
    public override int    TransformationBuffId => ModContent.BuffType<ChromaStone_Buff>();

    public override string Description =>
        "A living crystal conduit that absorbs punishment, turns it into extra firepower, and shines brighter as the pressure rises.";

    public override List<string> Abilities => new() {
        "Crystal bolts powered by stored energy",
        "Absorbs punishment while his guard is up",
        "Stored energy boosts damage and toughness",
        "Chromatic glow that flares brighter as charge rises"
    };

    public override string PrimaryAttackName  => "Crystal Bolt";
    public override string PrimaryAbilityName => "Crystal Guard";
    public override int    PrimaryAttack      => ModContent.ProjectileType<ChromaStoneProjectile>();

    public override int PrimaryAbilityDuration => 60 * 60;
    public override int PrimaryAbilityCooldown => 30 * 60;
    public override int PrimaryAttackSpeed     => 20;

    private int _chromastoneAbsobtion = 0;

    public override void ResetEffects(Player player, OmnitrixPlayer omp) {
        int storedCharge = Math.Min(_chromastoneAbsobtion, 80);

        player.GetDamage<HeroDamage>()           += 0.1f + storedCharge / 500f;
        player.statDefense                       += 8 + storedCharge / 12;
        player.endurance                         += 0.04f;
        player.GetArmorPenetration<HeroDamage>() += 6 + storedCharge / 20;

        if (omp.PrimaryAbilityEnabled)
            player.endurance += 0.08f;
    }

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity,
        int damage, float knockback) {
        damage += _chromastoneAbsobtion;
        return base.Shoot(player, omp, source, position, velocity, damage, knockback);
    }

    public override void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        if (omp.PrimaryAbilityEnabled)
            _chromastoneAbsobtion += Math.Max(info.Damage / 5, 0);
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<ChromaStone>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);

        if (omp.PrimaryAbilityEnabled) {
            GameShaders.Armor.GetShaderFromItemId(ModContent.ItemType<DiscoDye>()).UseColor(Main.DiscoColor);
            player.cHead = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DiscoDye>());
            player.cBody = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DiscoDye>());
            player.cLegs = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DiscoDye>());
        }
    }

    public override IReadOnlyList<TransformationPaletteChannel> PaletteChannels => [
        new TransformationPaletteChannel(
            "crystal",
            "Crystal",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStone_Head",
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStoneCrystalMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStone_Body",
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStoneCrystalMask_Body")),
        new TransformationPaletteChannel(
            "base",
            "Base",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStone_Head",
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStoneBaseMask_Head"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStone_Body",
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStoneBaseMask_Body"),
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStone_Legs",
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStoneBaseMask_Legs")),
        new TransformationPaletteChannel(
            "eye",
            "Eye",
            Color.White,
            new TransformationPaletteOverlay(
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStone_Head",
                "Ben10Mod/Content/Transformations/ChromaStone/ChromaStoneEyeMask_Head"))
    ];
}
