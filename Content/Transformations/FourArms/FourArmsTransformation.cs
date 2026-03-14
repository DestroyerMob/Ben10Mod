using System.Collections.Generic;
using Ben10Mod.Content.Buffs.Abilities;
using Ben10Mod.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.FourArms;

public class FourArmsTransformation : Transformation {
    public override string FullID => "Ben10Mod:FourArms";
    public override string TransformationName => "Fourarms";
    public override string IconPath => "Ben10Mod/Content/Interface/FourArmsSelect";
    public override int TransformationBuffId => ModContent.BuffType<FourArms_Buff>();

    public override string Description =>
        "A powerhouse Tetramand with brutal punches, earth-shaking claps, and heavy melee-focused mobility.";

    public override List<string> Abilities => new() {
        "Rapid punches",
        "Shockwave clap",
        "Melee speed boost",
        "High leap and fall resistance"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<FistProjectile>();
    public override int PrimaryAttackSpeed => 18;
    public override int PrimaryShootSpeed => 25;

    public override int SecondaryAttack => ModContent.ProjectileType<FourArmsClap>();
    public override int SecondaryAttackSpeed => 18;
    public override int SecondaryShootSpeed => 25;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);

        player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
        player.GetCritChance(DamageClass.Generic) = 50f;
        player.noFallDmg = true;
        Player.jumpSpeed *= 1.9f;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<FourArms>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}
