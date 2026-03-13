using System;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.ChromaStone;

public class ChromaStoneTransformation : Transformation {
    public override string FullID => "Ben10Mod:ChromaStone";
    public override string TransformationName => "Chromastone";
    public override string IconPath => "Ben10Mod/Content/Interface/ChromaStoneSelect";
    public override int TransformationBuffId => ModContent.BuffType<ChromaStone_Buff>();
    public override int PrimaryAttack => ModContent.ProjectileType<ChromaStoneProjectile>();

    public override int PrimaryAbilityDuration => 60 * 60;
    public override int PrimaryAbilityCooldown => 30 * 60;
    
    private int _chromastoneAbsobtion = 0;

    public override void OnHurt(Player player, OmnitrixPlayer omp, Player.HurtInfo info) {
        if (omp.PrimaryAbilityEnabled)
            _chromastoneAbsobtion += Math.Max(info.Damage / 5, 0);
    }

    public override void PostUpdate(Player player, OmnitrixPlayer omp) {
        if (!omp.PrimaryAbilityEnabled) {
            _chromastoneAbsobtion = 0;
        }
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<ChromaStone>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}