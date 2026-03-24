using Ben10Mod.Content.Buffs.Transformations;
using Ben10Mod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations.AlienX;

public class AlienXTransformation : SimpleRangedTransformationBase {
    public override string FullID => "Ben10Mod:AlienX";
    public override string TransformationName => "Alien X";
    public override int TransformationBuffId => ModContent.BuffType<AlienX_Buff>();
    protected override string BasicDescription => "A Celestialsapien that bends gravity into a compact singularity, dragging enemies into a crushing point of space.";
    public override string PrimaryAttackName => "Singularity";
    public override int PrimaryAttack => ModContent.ProjectileType<AlienXBlackHoleProjectile>();
    public override int PrimaryAttackSpeed => 28;
    public override int PrimaryShootSpeed => 8;
    public override float PrimaryAttackModifier => 1.15f;
    protected override int HeadSlot => ArmorIDs.Head.PlatinumHelmet;
    protected override int BodySlot => ArmorIDs.Body.PlatinumChainmail;
    protected override int LegSlot => ArmorIDs.Legs.PlatinumGreaves;

    public override bool Shoot(Player player, OmnitrixPlayer omp, EntitySource_ItemUse_WithAmmo source, Vector2 position,
        Vector2 velocity, int damage, float knockback) {
        Vector2 spawnPosition = player.whoAmI == Main.myPlayer ? Main.MouseWorld : position;
        Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, ModContent.ProjectileType<AlienXBlackHoleProjectile>(),
            damage, knockback, player.whoAmI);
        return false;
    }
}
