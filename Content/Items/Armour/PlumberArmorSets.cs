using System;
using System.Collections.Generic;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Items.Armour;

internal static class PlumberArmorTextures {
    public const string Helmet = "Ben10Mod/Content/Items/Armour/PlumbersHelmet";
    public const string GlassHelmet = "Ben10Mod/Content/Items/Armour/PlumbersGlassHelmet";
    public const string Shirt = "Ben10Mod/Content/Items/Armour/PlumbersShirt";
    public const string Pants = "Ben10Mod/Content/Items/Armour/PlumbersPants";
}

internal static class PlumberArmorPalette {
    public static readonly Color Neutral = new(220, 225, 232);
    public static readonly Color Vanguard = new(120, 174, 224);
    public static readonly Color Scout = new(240, 198, 104);
    public static readonly Color Assault = new(215, 96, 96);
    public static readonly Color Overclock = new(255, 148, 72);
    public static readonly Color Bulwark = new(246, 229, 155);
    public static readonly Color Relay = new(118, 238, 163);
    public static readonly Color Siege = new(98, 132, 218);
    public static readonly Color Magistrata = new(247, 156, 230);

    public static Color ResolveSharedEarlySetColor(Player player) {
        if (player.armor[0].type == ModContent.ItemType<PlumbersGlassHelmet>()) {
            return Scout;
        }

        if (player.armor[0].type == ModContent.ItemType<PlumbersHelmet>()) {
            return Vanguard;
        }

        return Neutral;
    }

    public static Color Blend(Color baseColor, Color tint, float amount = 0.58f) {
        Color blended = Color.Lerp(baseColor, tint, amount);
        blended.A = baseColor.A;
        return blended;
    }

    public static bool DrawInventory(ModItem item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Vector2 origin, float scale, Color tint) {
        spriteBatch.Draw(TextureAssets.Item[item.Type].Value, position, frame, Blend(drawColor, tint), 0f, origin, scale,
            SpriteEffects.None, 0f);
        return false;
    }

    public static bool DrawWorld(ModItem item, SpriteBatch spriteBatch, Color alphaColor, ref float rotation, ref float scale,
        Color tint) {
        Main.GetItemDrawFrame(item.Item.type, out Texture2D itemTexture, out Rectangle itemFrame);
        Vector2 drawOrigin = itemFrame.Size() / 2f;
        Vector2 drawPosition = item.Item.Bottom - Main.screenPosition - new Vector2(0f, drawOrigin.Y);
        spriteBatch.Draw(itemTexture, drawPosition, itemFrame, Blend(alphaColor, tint), rotation, drawOrigin, scale,
            SpriteEffects.None, 0f);
        return false;
    }
}

public class HeroPlumberArmorPlayer : ModPlayer {
    private static DamageClass HeroClass => ModContent.GetInstance<HeroDamage>();

    private const int BulwarkPulseCooldownTime = 15 * 60;
    private const int RelayChargeDuration = 5 * 60;
    private const int SiegeLockDuration = 2 * 60;
    private const int SiegeRequiredLocks = 3;
    private const int MagistrataStackCooldownTime = 24;

    public bool bulwarkSet;
    public bool relaySet;
    public bool siegeSet;
    public bool magistrataSet;

    private int bulwarkPulseCooldown;
    private int relayChargeTime;
    private int siegeLockedTargetIndex = -1;
    private int siegeLockStacks;
    private int siegeLockTime;
    private int magistrataCommandStacks;
    private int magistrataStackCooldown;
    private OmnitrixPlayer.AttackSelection lastAttackSelection = OmnitrixPlayer.AttackSelection.Primary;
    private bool lastPrimaryAbilityActive;
    private bool lastSecondaryAbilityActive;
    private bool lastTertiaryAbilityActive;
    private bool lastUltimateAbilityActive;

    public override void ResetEffects() {
        bulwarkSet = false;
        relaySet = false;
        siegeSet = false;
        magistrataSet = false;
    }

    public override void UpdateDead() {
        bulwarkPulseCooldown = 0;
        relayChargeTime = 0;
        siegeLockedTargetIndex = -1;
        siegeLockStacks = 0;
        siegeLockTime = 0;
        magistrataCommandStacks = 0;
        magistrataStackCooldown = 0;
        CacheAbilityState(Player.GetModPlayer<OmnitrixPlayer>());
    }

    public override void PostUpdate() {
        if (!bulwarkSet)
            bulwarkPulseCooldown = 0;
        if (!relaySet)
            relayChargeTime = 0;
        if (!siegeSet) {
            siegeLockedTargetIndex = -1;
            siegeLockStacks = 0;
            siegeLockTime = 0;
        }
        if (!magistrataSet) {
            magistrataCommandStacks = 0;
            magistrataStackCooldown = 0;
        }

        if (bulwarkPulseCooldown > 0)
            bulwarkPulseCooldown--;
        if (relayChargeTime > 0)
            relayChargeTime--;
        if (siegeLockTime > 0)
            siegeLockTime--;
        if (magistrataStackCooldown > 0)
            magistrataStackCooldown--;

        if (siegeLockTime <= 0) {
            siegeLockedTargetIndex = -1;
            siegeLockStacks = 0;
        }

        var omp = Player.GetModPlayer<OmnitrixPlayer>();
        bool abilityActivated = HasAbilityActivation(omp);
        bool attackSelectionChanged = omp.setAttack != lastAttackSelection;

        if (!omp.IsTransformed) {
            relayChargeTime = 0;
            siegeLockedTargetIndex = -1;
            siegeLockStacks = 0;
            siegeLockTime = 0;
            magistrataCommandStacks = 0;
            magistrataStackCooldown = 0;
            CacheAbilityState(omp);
            return;
        }

        if (relaySet && (abilityActivated || (attackSelectionChanged && omp.HasLoadedBadgeAttack)))
            PrimeRelayBurst();

        if (magistrataSet && magistrataStackCooldown <= 0 && (abilityActivated || attackSelectionChanged))
            GainMagistrataCommandStack();

        CacheAbilityState(omp);
    }

    public override void PostHurt(Player.HurtInfo info) {
        if (bulwarkSet && Player.GetModPlayer<OmnitrixPlayer>().IsTransformed && bulwarkPulseCooldown <= 0 && info.Damage > 0) {
            bulwarkPulseCooldown = BulwarkPulseCooldownTime;
            TriggerBulwarkPulse(Math.Max(80, info.Damage * 2));
        }

        base.PostHurt(info);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
        if (item.CountsAsClass(HeroClass))
            HandleHeroHit(target, damageDone);

        base.OnHitNPCWithItem(item, target, hit, damageDone);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
        if (proj.CountsAsClass(HeroClass))
            HandleHeroHit(target, damageDone);

        base.OnHitNPCWithProj(proj, target, hit, damageDone);
    }

    private void HandleHeroHit(NPC target, int damageDone) {
        if (relaySet && relayChargeTime > 0)
            TriggerRelayBurst(target, damageDone);

        if (siegeSet)
            HandleSiegeHit(target, damageDone);

        if (magistrataSet && magistrataCommandStacks >= 3)
            TriggerMagistrataVerdict(target, damageDone);
    }

    private void PrimeRelayBurst() {
        bool wasUncharged = relayChargeTime <= 0;
        relayChargeTime = RelayChargeDuration;

        if (wasUncharged && Player.whoAmI == Main.myPlayer)
            CombatText.NewText(Player.getRect(), PlumberArmorPalette.Relay, "Relay Charged");
    }

    private void HandleSiegeHit(NPC target, int damageDone) {
        if (target.whoAmI == siegeLockedTargetIndex && siegeLockTime > 0) {
            siegeLockStacks = Math.Min(SiegeRequiredLocks, siegeLockStacks + 1);
        }
        else {
            siegeLockedTargetIndex = target.whoAmI;
            siegeLockStacks = 1;
        }

        siegeLockTime = SiegeLockDuration;

        if (siegeLockStacks >= SiegeRequiredLocks) {
            TriggerSiegeBurst(target, damageDone);
            siegeLockedTargetIndex = -1;
            siegeLockStacks = 0;
            siegeLockTime = 0;
        }
    }

    private void GainMagistrataCommandStack() {
        magistrataStackCooldown = MagistrataStackCooldownTime;
        if (magistrataCommandStacks < 3)
            magistrataCommandStacks++;

        if (magistrataCommandStacks >= 3 && Player.whoAmI == Main.myPlayer)
            CombatText.NewText(Player.getRect(), PlumberArmorPalette.Magistrata, "Verdict Ready");
    }

    private void TriggerBulwarkPulse(int damage) {
        float radius = 220f;
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            foreach (NPC npc in Main.ActiveNPCs) {
                if (!npc.CanBeChasedBy() || Vector2.Distance(npc.Center, Player.Center) > radius)
                    continue;

                int direction = npc.Center.X >= Player.Center.X ? 1 : -1;
                npc.SimpleStrikeNPC(damage, direction, false, 6f, HeroClass);
            }
        }

        SpawnDustBurst(Player.Center, radius, DustID.HallowedTorch, PlumberArmorPalette.Bulwark);

        if (Player.whoAmI == Main.myPlayer)
            CombatText.NewText(Player.getRect(), PlumberArmorPalette.Bulwark, "Bulwark Pulse");
    }

    private void TriggerRelayBurst(NPC primaryTarget, int damageDone) {
        relayChargeTime = 0;
        RestoreOmnitrixEnergy(12f);

        List<NPC> chainedTargets = FindNearestTargets(primaryTarget.Center, 320f, 3, primaryTarget.whoAmI);
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            int damage = Math.Max(1, (int)Math.Round(damageDone * 0.45f));
            foreach (NPC npc in chainedTargets) {
                int direction = npc.Center.X >= primaryTarget.Center.X ? 1 : -1;
                npc.SimpleStrikeNPC(damage, direction, false, 0f, HeroClass);
            }
        }

        foreach (NPC npc in chainedTargets)
            SpawnDustLink(primaryTarget.Center, npc.Center, DustID.Electric, PlumberArmorPalette.Relay);
    }

    private void TriggerSiegeBurst(NPC primaryTarget, int damageDone) {
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            int primaryDamage = Math.Max(1, (int)Math.Round(damageDone * 0.5f));
            int splashDamage = Math.Max(1, (int)Math.Round(damageDone * 0.35f));

            primaryTarget.SimpleStrikeNPC(primaryDamage, Player.direction, false, 5f, HeroClass);

            foreach (NPC npc in FindNearestTargets(primaryTarget.Center, 180f, 4, primaryTarget.whoAmI)) {
                int direction = npc.Center.X >= primaryTarget.Center.X ? 1 : -1;
                npc.SimpleStrikeNPC(splashDamage, direction, false, 4f, HeroClass);
            }
        }

        SpawnDustBurst(primaryTarget.Center, 96f, DustID.BlueTorch, PlumberArmorPalette.Siege);
    }

    private void TriggerMagistrataVerdict(NPC primaryTarget, int damageDone) {
        magistrataCommandStacks = 0;
        magistrataStackCooldown = MagistrataStackCooldownTime;
        RestoreOmnitrixEnergy(20f);

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            int primaryDamage = Math.Max(1, (int)Math.Round(damageDone * 0.7f));
            int echoDamage = Math.Max(1, (int)Math.Round(damageDone * 0.45f));

            primaryTarget.SimpleStrikeNPC(primaryDamage, Player.direction, false, 0f, HeroClass);

            foreach (NPC npc in FindNearestTargets(primaryTarget.Center, 260f, 2, primaryTarget.whoAmI)) {
                int direction = npc.Center.X >= primaryTarget.Center.X ? 1 : -1;
                npc.SimpleStrikeNPC(echoDamage, direction, false, 0f, HeroClass);
            }
        }

        SpawnDustBurst(primaryTarget.Center, 112f, DustID.PinkTorch, PlumberArmorPalette.Magistrata);
    }

    private void RestoreOmnitrixEnergy(float amount) {
        if (Player.whoAmI != Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
            return;

        var omp = Player.GetModPlayer<OmnitrixPlayer>();
        omp.omnitrixEnergy = Math.Min(omp.omnitrixEnergyMax, omp.omnitrixEnergy + amount);
    }

    private static void SpawnDustBurst(Vector2 center, float radius, int dustType, Color color) {
        for (int i = 0; i < 18; i++) {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.2f, 4.6f);
            Vector2 position = center + velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(8f, radius * 0.25f);
            Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 120, color, 1.2f);
            dust.noGravity = true;
        }
    }

    private static void SpawnDustLink(Vector2 start, Vector2 end, int dustType, Color color) {
        int points = 8;
        for (int i = 0; i <= points; i++) {
            Vector2 position = Vector2.Lerp(start, end, i / (float)points);
            Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 120, color, 1.05f);
            dust.noGravity = true;
        }
    }

    private static List<NPC> FindNearestTargets(Vector2 origin, float maxDistance, int count, int excludedNpc) {
        List<NPC> results = new();
        HashSet<int> usedIds = new();
        if (excludedNpc >= 0)
            usedIds.Add(excludedNpc);

        while (results.Count < count) {
            NPC closest = null;
            float closestDistance = maxDistance;

            foreach (NPC npc in Main.ActiveNPCs) {
                if (!npc.CanBeChasedBy() || usedIds.Contains(npc.whoAmI))
                    continue;

                float distance = Vector2.Distance(origin, npc.Center);
                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closest = npc;
            }

            if (closest == null)
                break;

            usedIds.Add(closest.whoAmI);
            results.Add(closest);
        }

        return results;
    }

    private bool HasAbilityActivation(OmnitrixPlayer omp) {
        return (!lastPrimaryAbilityActive && omp.IsPrimaryAbilityActive)
               || (!lastSecondaryAbilityActive && omp.IsSecondaryAbilityActive)
               || (!lastTertiaryAbilityActive && omp.IsTertiaryAbilityActive)
               || (!lastUltimateAbilityActive && omp.IsUltimateAbilityActive);
    }

    private void CacheAbilityState(OmnitrixPlayer omp) {
        lastAttackSelection = omp.setAttack;
        lastPrimaryAbilityActive = omp.IsPrimaryAbilityActive;
        lastSecondaryAbilityActive = omp.IsSecondaryAbilityActive;
        lastTertiaryAbilityActive = omp.IsTertiaryAbilityActive;
        lastUltimateAbilityActive = omp.IsUltimateAbilityActive;
    }
}

public abstract class PlumberArmorPiece : ModItem {
    protected abstract string ArmorTexture { get; }
    protected abstract int ArmorValue { get; }
    protected abstract int ArmorRarity { get; }
    protected abstract int ArmorDefense { get; }
    protected abstract Color ArmorTint { get; }
    protected virtual int ArmorWidth => 18;
    protected virtual int ArmorHeight => 14;
    protected virtual string EquipBonusText => "";

    public override string Texture => ArmorTexture;

    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.width = ArmorWidth;
        Item.height = ArmorHeight;
        Item.value = ArmorValue;
        Item.rare = ArmorRarity;
        Item.defense = ArmorDefense;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) {
        if (!string.IsNullOrWhiteSpace(EquipBonusText)) {
            tooltips.Add(new TooltipLine(Mod, "EquipBonus", EquipBonusText));
        }
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
        ref Color glowMaskColor) {
        color = PlumberArmorPalette.Blend(color, ArmorTint);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Color itemColor, Vector2 origin, float scale) {
        return PlumberArmorPalette.DrawInventory(this, spriteBatch, position, frame, drawColor, origin, scale, ArmorTint);
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation,
        ref float scale, int whoAmI) {
        return PlumberArmorPalette.DrawWorld(this, spriteBatch, alphaColor, ref rotation, ref scale, ArmorTint);
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumbersHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(silver: 90);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 3;
    protected override Color ArmorTint => PlumberArmorPalette.Vanguard;
    protected override string EquipBonusText => "+2 defense while transformed and +4 hero armor penetration";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedDefenseBonus += 2;
        player.GetArmorPenetration<HeroDamage>() += 4;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumbersShirt>()
            && legs.type == ModContent.ItemType<PlumbersPants>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = "While transformed: +8 defense and +4% endurance. Also grants +0.6 hero knockback";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.isTransformed) {
            omp.transformedDefenseBonus += 8;
            omp.transformedEnduranceBonus += 0.04f;
        }

        player.GetKnockback<HeroDamage>() += 0.6f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 15)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumbersGlassHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.GlassHelmet;
    protected override int ArmorValue => Item.buyPrice(silver: 95);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 2;
    protected override Color ArmorTint => PlumberArmorPalette.Scout;
    protected override string EquipBonusText => "+6 hero crit and +4% hero attack speed";

    public override void UpdateEquip(Player player) {
        player.GetCritChance<HeroDamage>() += 6f;
        player.GetAttackSpeed<HeroDamage>() += 0.04f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumbersShirt>()
            && legs.type == ModContent.ItemType<PlumbersPants>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus = "While transformed: +12% movement speed and improved jump height. Also grants +10 hero crit";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.isTransformed) {
            omp.transformedMoveSpeedBonus += 0.12f;
            omp.transformedJumpSpeedBonus += 1.6f;
        }

        player.GetCritChance<HeroDamage>() += 10f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 5)
            .AddIngredient(ItemID.Glass, 10)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 5)
            .AddIngredient(ItemID.Glass, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumbersShirt : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(silver: 110);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 4;
    protected override Color ArmorTint => PlumberArmorPalette.Neutral;
    protected override string EquipBonusText => "+4% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.04f;
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
        ref Color glowMaskColor) {
        color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.ResolveSharedEarlySetColor(drawPlayer));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 25)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 25)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumbersPants : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(silver: 100);
    protected override int ArmorRarity => ItemRarityID.White;
    protected override int ArmorDefense => 3;
    protected override Color ArmorTint => PlumberArmorPalette.Neutral;
    protected override string EquipBonusText => "+5% movement speed while transformed";

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.05f;
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask,
        ref Color glowMaskColor) {
        color = PlumberArmorPalette.Blend(color, PlumberArmorPalette.ResolveSharedEarlySetColor(drawPlayer));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 20)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 20)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberAssaultHelmet : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 1);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 6;
    protected override Color ArmorTint => PlumberArmorPalette.Assault;
    protected override string EquipBonusText => "+5% hero damage and +6 hero crit";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.05f;
        player.GetCritChance<HeroDamage>() += 6f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberAssaultHarness>()
            && legs.type == ModContent.ItemType<PlumberAssaultGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +10% hero damage, +12 hero armor penetration, and +10% hero attack speed while moving quickly";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        player.GetDamage<HeroDamage>() += 0.10f;
        player.GetArmorPenetration<HeroDamage>() += 12;

        if (Math.Abs(player.velocity.X) >= 3f || Math.Abs(player.velocity.Y) > 0.1f) {
            player.GetAttackSpeed<HeroDamage>() += 0.10f;
        }
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 12)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CrimtaneBar, 12)
            .AddIngredient(ItemID.MeteoriteBar, 8)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberAssaultHarness : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 7;
    protected override Color ArmorTint => PlumberArmorPalette.Assault;
    protected override string EquipBonusText => "+6% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 20)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CrimtaneBar, 20)
            .AddIngredient(ItemID.MeteoriteBar, 12)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberAssaultGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 1, silver: 10);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 5;
    protected override Color ArmorTint => PlumberArmorPalette.Assault;
    protected override string EquipBonusText => "+6% movement speed while transformed";

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.DemoniteBar, 16)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CrimtaneBar, 16)
            .AddIngredient(ItemID.MeteoriteBar, 10)
            .AddTile(TileID.Anvils)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberOverclockHelm : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 2);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 7;
    protected override Color ArmorTint => PlumberArmorPalette.Overclock;
    protected override string EquipBonusText => "+5% hero attack speed and +15 Omnitrix energy";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetAttackSpeed<HeroDamage>() += 0.05f;
        omp.omnitrixEnergyMaxBonus += 15;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberOverclockPlate>()
            && legs.type == ModContent.ItemType<PlumberOverclockGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +35 Omnitrix energy, +1 energy regen, 12% shorter primary/secondary/tertiary cooldowns, and +8% hero attack speed";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        if (!omp.isTransformed) {
            return;
        }

        omp.omnitrixEnergyMaxBonus += 35;
        omp.omnitrixEnergyRegenBonus += 1;
        omp.primaryAbilityCooldownMultiplier *= 0.88f;
        omp.secondaryAbilityCooldownMultiplier *= 0.88f;
        omp.tertiaryAbilityCooldownMultiplier *= 0.88f;
        player.GetAttackSpeed<HeroDamage>() += 0.08f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 16)
            .AddTile(TileID.Hellforge)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberOverclockPlate : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 2, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 8;
    protected override Color ArmorTint => PlumberArmorPalette.Overclock;
    protected override string EquipBonusText => "+6% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.06f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 24)
            .AddTile(TileID.Hellforge)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberOverclockGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 2, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Orange;
    protected override int ArmorDefense => 6;
    protected override Color ArmorTint => PlumberArmorPalette.Overclock;
    protected override string EquipBonusText => "+6% movement speed and acceleration while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.06f;
        omp.transformedRunAccelerationBonus += 0.05f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 20)
            .AddTile(TileID.Hellforge)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberBulwarkHelm : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 4);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 10;
    protected override Color ArmorTint => PlumberArmorPalette.Bulwark;
    protected override string EquipBonusText => "+3 defense while transformed and +4 hero armor penetration";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedDefenseBonus += 3;
        player.GetArmorPenetration<HeroDamage>() += 4;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberBulwarkMail>()
            && legs.type == ModContent.ItemType<PlumberBulwarkGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +6 defense and +3% endurance. Every 15 seconds, the next hit you take unleashes a Bulwark Pulse that blasts nearby enemies";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        var hvap = player.GetModPlayer<HeroPlumberArmorPlayer>();
        hvap.bulwarkSet = true;
        if (!omp.isTransformed) {
            return;
        }

        omp.transformedDefenseBonus += 6;
        omp.transformedEnduranceBonus += 0.03f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ItemID.SoulofFright, 5)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberBulwarkMail : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 4, silver: 60);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 12;
    protected override Color ArmorTint => PlumberArmorPalette.Bulwark;
    protected override string EquipBonusText => "+4% hero damage and +2 defense while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetDamage<HeroDamage>() += 0.04f;
        omp.transformedDefenseBonus += 2;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 20)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberBulwarkGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 4, silver: 20);
    protected override int ArmorRarity => ItemRarityID.Pink;
    protected override int ArmorDefense => 8;
    protected override Color ArmorTint => PlumberArmorPalette.Bulwark;
    protected override string EquipBonusText => "+5% movement speed while transformed";

    public override void UpdateEquip(Player player) {
        player.GetModPlayer<OmnitrixPlayer>().transformedMoveSpeedBonus += 0.05f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 16)
            .AddIngredient(ItemID.SoulofMight, 6)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberRelayVisor : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 6);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 11;
    protected override Color ArmorTint => PlumberArmorPalette.Relay;
    protected override string EquipBonusText => "+5% hero damage and +15 Omnitrix energy";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetDamage<HeroDamage>() += 0.05f;
        omp.omnitrixEnergyMaxBonus += 15;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberRelayCoat>()
            && legs.type == ModContent.ItemType<PlumberRelayLeggings>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +25 Omnitrix energy and 15% longer transformations. Activating an ability or loading a special attack charges your next Hero hit to chain lightning and restore energy";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        var hvap = player.GetModPlayer<HeroPlumberArmorPlayer>();
        hvap.relaySet = true;
        if (!omp.isTransformed) {
            return;
        }

        omp.omnitrixEnergyMaxBonus += 25;
        omp.transformationDurationMultiplier *= 1.15f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 14)
            .AddIngredient(ItemID.Wire, 20)
            .AddIngredient(ItemID.SoulofSight, 4)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberRelayCoat : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 6, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 13;
    protected override Color ArmorTint => PlumberArmorPalette.Relay;
    protected override string EquipBonusText => "+6 hero crit and +20 Omnitrix energy";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        player.GetCritChance<HeroDamage>() += 6f;
        omp.omnitrixEnergyMaxBonus += 20;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 24)
            .AddIngredient(ItemID.Wire, 35)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberRelayLeggings : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 6);
    protected override int ArmorRarity => ItemRarityID.Lime;
    protected override int ArmorDefense => 9;
    protected override Color ArmorTint => PlumberArmorPalette.Relay;
    protected override string EquipBonusText => "+7% movement speed and improved jump height while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.07f;
        omp.transformedJumpSpeedBonus += 1.2f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ChlorophyteBar, 18)
            .AddIngredient(ItemID.Wire, 25)
            .AddIngredient(ItemID.SoulofSight, 6)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberSiegeMask : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 8);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 13;
    protected override Color ArmorTint => PlumberArmorPalette.Siege;
    protected override string EquipBonusText => "+5% hero damage and +8 hero armor penetration";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.05f;
        player.GetArmorPenetration<HeroDamage>() += 8;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberSiegeCuirass>()
            && legs.type == ModContent.ItemType<PlumberSiegeBoots>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +10 hero crit. Repeated Hero hits lock onto a target; at 3 locks, a Siege Burst detonates around them";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        var hvap = player.GetModPlayer<HeroPlumberArmorPlayer>();
        hvap.siegeSet = true;
        if (!omp.isTransformed) {
            return;
        }

        player.GetCritChance<HeroDamage>() += 10f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 14)
            .AddIngredient(ItemID.Ectoplasm, 5)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberSiegeCuirass : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 8, silver: 40);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 15;
    protected override Color ArmorTint => PlumberArmorPalette.Siege;
    protected override string EquipBonusText => "+8% hero damage";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.08f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 24)
            .AddIngredient(ItemID.Ectoplasm, 8)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberSiegeBoots : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 8);
    protected override int ArmorRarity => ItemRarityID.Yellow;
    protected override int ArmorDefense => 11;
    protected override Color ArmorTint => PlumberArmorPalette.Siege;
    protected override string EquipBonusText => "+4% hero attack speed";

    public override void UpdateEquip(Player player) {
        player.GetAttackSpeed<HeroDamage>() += 0.04f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.ShroomiteBar, 18)
            .AddIngredient(ItemID.Ectoplasm, 6)
            .AddTile(TileID.Autohammer)
            .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class PlumberMagistrataHelm : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Helmet;
    protected override int ArmorValue => Item.buyPrice(gold: 12);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 16;
    protected override Color ArmorTint => PlumberArmorPalette.Magistrata;
    protected override string EquipBonusText => "+7% hero damage and +8 hero crit";

    public override void UpdateEquip(Player player) {
        player.GetDamage<HeroDamage>() += 0.07f;
        player.GetCritChance<HeroDamage>() += 8f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) {
        return body.type == ModContent.ItemType<PlumberMagistrataCoat>()
            && legs.type == ModContent.ItemType<PlumberMagistrataGreaves>();
    }

    public override void UpdateArmorSet(Player player) {
        player.setBonus =
            "While transformed: +40 Omnitrix energy and 10% shorter primary and ultimate cooldowns. Ability activations and attack swaps build Command stacks; at 3 stacks, your next Hero hit delivers a Verdict strike and restores energy";

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        var hvap = player.GetModPlayer<HeroPlumberArmorPlayer>();
        hvap.magistrataSet = true;
        if (!omp.isTransformed) {
            return;
        }

        omp.omnitrixEnergyMaxBonus += 40;
        omp.primaryAbilityCooldownMultiplier *= 0.90f;
        omp.ultimateAbilityCooldownMultiplier *= 0.90f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 12)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 8)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class PlumberMagistrataCoat : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Shirt;
    protected override int ArmorValue => Item.buyPrice(gold: 12, silver: 60);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 18;
    protected override Color ArmorTint => PlumberArmorPalette.Magistrata;
    protected override string EquipBonusText => "+6% hero attack speed and +10 hero armor penetration";

    public override void UpdateEquip(Player player) {
        player.GetAttackSpeed<HeroDamage>() += 0.06f;
        player.GetArmorPenetration<HeroDamage>() += 10;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 18)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 12)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}

[AutoloadEquip(EquipType.Legs)]
public class PlumberMagistrataGreaves : PlumberArmorPiece {
    protected override string ArmorTexture => PlumberArmorTextures.Pants;
    protected override int ArmorValue => Item.buyPrice(gold: 12);
    protected override int ArmorRarity => ItemRarityID.Red;
    protected override int ArmorDefense => 14;
    protected override Color ArmorTint => PlumberArmorPalette.Magistrata;
    protected override string EquipBonusText => "+8% movement speed and improved jump height while transformed";

    public override void UpdateEquip(Player player) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.transformedMoveSpeedBonus += 0.08f;
        omp.transformedJumpSpeedBonus += 1.8f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar, 14)
            .AddIngredient(ModContent.ItemType<HeroFragment>(), 10)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}
