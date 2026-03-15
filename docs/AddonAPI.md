# Ben10Mod Addon API Guide

This guide explains how to build a separate tModLoader addon mod that extends `Ben10Mod`.

It is written so you can build an addon from scratch using only this document and the Ben10Mod source.

Related docs:

- [README](../README.md)
- [Architecture Guide](Architecture.md)
- [Development Guide](Development.md)

The intended addon use cases are:

- add a new Omnitrix
- add a new transformation
- add a new Plumber's Badge
- add projectiles, buffs, items, and unlock content that interact with the Ben10Mod systems

## What Is Supported

Ben10Mod currently exposes three main extension points:

- `Ben10Mod.Content.Items.Accessories.Omnitrix`
- `Ben10Mod.Content.Transformations.Transformation`
- `Ben10Mod.Content.Items.Weapons.PlumbersBadge`

The important design idea is:

- transformations register themselves automatically
- Omnitrix items drive Omnitrix-specific behavior through overridable properties and hooks
- badges are generic weapon shells and let the current transformation decide attack behavior

If you are new to the internal runtime flow, read the architecture guide first and then come back here.

## High-Level Architecture

If you are building an addon, this is the mental model to use.

### Omnitrixes

An Omnitrix is an accessory accepted by the custom Omnitrix slot. The base class handles:

- energy capacity
- energy regen or drain
- transformation selection hotkeys
- transformation activation
- energy-based upkeep
- detransform behavior
- energy gained from damage
- evolution hooks
- child transformation capability hooks
- hand visual hooks

You extend this by subclassing `Omnitrix` and overriding the properties or virtual methods you want.

### Transformations

A transformation is not a `ModItem`. It is a custom `ModType` subclass that Ben10Mod registers into its transformation loader.

Each transformation provides:

- an ID
- display name
- icon
- description
- abilities list
- buff ID
- passive hooks
- attack data
- optional primary and ultimate ability hooks
- costume frame logic

To make a transformation usable in-game, you usually also create:

- a transformation buff
- a costume/equip item for the alien body
- projectiles or helper items if needed
- an unlock item or unlock event

### Plumber's Badges

The badge base class is intentionally simple. It provides:

- generic weapon setup
- damage class
- energy consumption
- attack timing reset
- interaction with the current transformation

The transformation controls what happens when the badge is used. The badge mostly controls:

- base damage
- rank identity
- recipes

## Folder Layout For An Addon

Recommended layout:

```text
ModSources/
  Ben10Mod/
  Ben10Addon/
    Ben10Addon.cs
    Ben10Addon.csproj
    build.txt
    Content/
      Items/
        Accessories/
        Weapons/
      Transformations/
        MyAlien/
      Buffs/
      Projectiles/
```

You do not need to put your addon inside the Ben10Mod folder. It should be its own mod in `ModSources`.

## Step 1: Add Ben10Mod As A Dependency

There are two separate things to set up:

1. runtime dependency
2. compile-time reference

You usually want both.

### Runtime Dependency In `build.txt`

Create `build.txt` in your addon and include Ben10Mod as a mod reference.

Example:

```ini
displayName = Ben10 Addon Example
author = YourName
version = 0.1
hideCode = false
hideResources = false
modReferences = Ben10Mod
```

This tells tModLoader that your addon depends on Ben10Mod being loaded.

### Compile-Time Reference In `.csproj`

Your addon code needs access to Ben10Mod types such as `Omnitrix`, `Transformation`, and `PlumbersBadge`.

If Ben10Mod source is present next to your addon in `ModSources`, the easiest setup is a project reference.

Example `Ben10Addon.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Import Project="..\tModLoader.targets" />

  <PropertyGroup>
    <AssemblyName>Ben10Addon</AssemblyName>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Ben10Mod\Ben10Mod.csproj" />
  </ItemGroup>
</Project>
```

If you are not building against source and only have the built mod assembly, you can instead reference the Ben10Mod DLL manually. The exact path depends on your tModLoader install, so the project-reference approach is the recommended dev workflow.

### Editor Note For Rider And Other IDEs

If your addon builds successfully but the editor still shows missing-type errors for Ben10Mod symbols, that is usually an IDE design-time restore or indexing problem rather than a real code problem.

Typical fixes:

1. run `dotnet restore YourAddon.csproj`
2. reload all projects in the editor
3. invalidate editor caches if needed
4. delete `bin` and `obj` and let the editor reindex

If command-line build works but the editor still shows red squiggles, treat that as an IDE problem until proven otherwise.

### Optional: Guard Against Missing Ben10Mod

Your addon can assume Ben10Mod is present if you set `modReferences = Ben10Mod`.

If you want a sanity check, in your `Mod` class you can still verify:

```csharp
public override void Load() {
    if (!ModLoader.TryGetMod("Ben10Mod", out _)) {
        Logger.Warn("Ben10Mod was not found. This addon requires Ben10Mod.");
    }
}
```

## Step 2: Create Your Addon Mod Root

Minimal root file:

```csharp
using Terraria.ModLoader;

namespace Ben10Addon;

public class Ben10Addon : Mod {
}
```

## Building A New Transformation

This is the most important Ben10Mod addon type.

### What A Transformation Needs

At minimum:

- a `Transformation` subclass
- a `ModBuff` that sets the active transformation ID

Usually also:

- a costume item that loads the alien equip textures
- icon texture
- head/body/legs textures
- projectiles for attacks
- an item or event that unlocks the transformation

## Transformation IDs

Use a fully qualified ID based on your own mod name.

Example:

- `Ben10Addon:ShockRock`

Do not use `Ben10Mod:...` for addon transformations.

Ben10Mod stores and syncs transformations by string ID, so the exact ID matters.

## Example Transformation Class

```csharp
using System.Collections.Generic;
using Ben10Mod;
using Ben10Mod.Content.Transformations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Addon.Content.Transformations.ShockRock;

public class ShockRockTransformation : Transformation {
    public override string FullID => "Ben10Addon:ShockRock";
    public override string TransformationName => "Shock Rock";
    public override string Description => "A conductive crystal alien built for beam and burst attacks.";
    public override string IconPath => "Ben10Addon/Content/Interface/ShockRockSelect";

    public override int TransformationBuffId => ModContent.BuffType<ShockRockBuff>();

    public override List<string> Abilities => new() {
        "Crystal bolt primary fire",
        "Charged burst secondary attack",
        "Energy shield primary ability"
    };

    public override int PrimaryAttack => ModContent.ProjectileType<Projectiles.ShockRockBolt>();
    public override int PrimaryAttackSpeed => 16;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;
    public override float PrimaryAttackModifier => 1.0f;
    public override int PrimaryEnergyCost => 0;

    public override int SecondaryAttack => ModContent.ProjectileType<Projectiles.ShockRockBurst>();
    public override int SecondaryAttackSpeed => 30;
    public override int SecondaryShootSpeed => 8;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override float SecondaryAttackModifier => 1.6f;
    public override int SecondaryEnergyCost => 5;

    public override int PrimaryAbilityDuration => 60 * 5;
    public override int PrimaryAbilityCooldown => 60 * 20;

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        base.UpdateEffects(player, omp);
        player.endurance += 0.08f;
    }

    public override void FrameEffects(Player player, OmnitrixPlayer omp) {
        var costume = ModContent.GetInstance<ShockRockCostume>();
        player.head = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Head);
        player.body = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Body);
        player.legs = EquipLoader.GetEquipSlot(Mod, costume.Name, EquipType.Legs);
    }
}
```

### Important Notes

- `Transformation` auto-registers itself with Ben10Mod. You do not need to manually add it to a list.
- `IconPath` must point to a real texture in your addon.
- `TransformationBuffId` must point at a buff that keeps the transformation active.
- `FrameEffects` is where you apply the alien costume visuals.
- `UpdateEffects` is where you apply passive stats, accessories, flight items, and so on.

## Example Transformation Buff

Your transformation buff is what tells Ben10Mod which transformation is active while the buff exists.

```csharp
using Ben10Mod;
using Terraria;
using Terraria.ModLoader;

namespace Ben10Addon.Content.Transformations.ShockRock;

public class ShockRockBuff : ModBuff {
    public override string Texture => "Ben10Mod/Content/Buffs/Transformations/EmptyTransformation";

    public override void Update(Player player, ref int buffIndex) {
        var omp = player.GetModPlayer<OmnitrixPlayer>();
        omp.currentTransformationId = "Ben10Addon:ShockRock";
        omp.isTransformed = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
```

### Why This Buff Matters

Ben10Mod transformation logic is driven by `currentTransformationId`. If the buff does not set it, the player will not be considered transformed into your alien.

## Example Costume Item

This is the alien "body" asset container. It loads head/body/legs equip textures so `FrameEffects` can use them.

```csharp
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Addon.Content.Transformations.ShockRock;

public class ShockRockCostume : ModItem {
    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
    }

    public override void SetStaticDefaults() {
        if (Main.netMode == NetmodeID.Server)
            return;

        int head = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        int body = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        int legs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

        ArmorIDs.Head.Sets.DrawHead[head] = false;
        ArmorIDs.Body.Sets.HidesTopSkin[body] = true;
        ArmorIDs.Body.Sets.HidesArms[body] = true;
        ArmorIDs.Legs.Sets.HidesBottomSkin[legs] = true;
    }
}
```

### Texture Naming Convention

If the costume item is `ShockRockCostume.cs`, the common texture names are:

- `ShockRockCostume.png`
- `ShockRockCostume_Head.png`
- `ShockRockCostume_Body.png`
- `ShockRockCostume_Legs.png`

You can use a different naming layout if your `Load()` method points to the correct texture paths.

## Unlocking A Transformation

There are two common ways to unlock a transformation.

### Recommended: Use Ben10Mod's Helper

```csharp
using Ben10Mod.Content;

TransformationHandler.AddTransformation(player, "Ben10Addon:ShockRock");
```

### Directly Through The Player

```csharp
using Ben10Mod;

player.GetModPlayer<OmnitrixPlayer>().UnlockTransformation("Ben10Addon:ShockRock");
```

### Example Unlock Item

```csharp
using Ben10Mod.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Addon.Content.Items;

public class ShockRockDNA : ModItem {
    public override void SetDefaults() {
        Item.width = 28;
        Item.height = 28;
        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = true;
        Item.maxStack = 99;
    }

    public override bool CanUseItem(Player player) {
        return !TransformationHandler.HasTransformation(player, "Ben10Addon:ShockRock");
    }

    public override bool? UseItem(Player player) {
        TransformationHandler.AddTransformation(player, "Ben10Addon:ShockRock");
        return true;
    }
}
```

## Building A New Plumber's Badge

This is the easiest addon type.

### What The Badge Actually Does

The badge itself is just the weapon shell. The current transformation decides what projectile, use speed, channel behavior, and energy cost to use.

That means a badge usually only needs:

- base damage
- rank metadata
- recipe

## Example Badge

```csharp
using Ben10Mod.Content.Items.Weapons;
using Terraria.ID;

namespace Ben10Addon.Content.Items.Weapons;

public class PlumberEliteBadge : PlumbersBadge {
    public override int BaseDamage => 34;
    public override string BadgeRankName => "Elite";
    public override int BadgeRankValue => 7;

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
            .AddIngredient(ItemID.SoulofLight, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
```

### How The Badge Uses Your Transformation

When held, the badge calls into the current transformation for:

- `ModifyPlumbersBadgeStats`
- `GetEnergyCost`
- `Shoot`

So if you want your transformation to support primary, secondary, or ultimate badge attacks, you implement those in the transformation.

### Minimum Badge Compatibility For A Transformation

A transformation should define any of the following that it supports:

- `PrimaryAttack`
- `SecondaryAttack`
- `UltimateAttack`
- attack speed overrides
- shoot speed overrides
- use style overrides
- channel flags
- energy costs

If you do nothing special beyond the built-in attack profile properties, the base `Transformation` class already knows how to feed the badge.

## Building A New Omnitrix

This is now designed to be addon-friendly.

### What The Base Omnitrix Already Handles

The base `Omnitrix` class already supports these overridable behaviors:

- `MaxOmnitrixEnergy`
- `OmnitrixEnergyRegen`
- `OmnitrixEnergyDrain`
- `EnergyPerDamageDivisor`
- `MinimumEnergyGainPerHit`
- `UseEnergyForTransformation`
- `TranformationSwapCost`
- `TimeoutDuration`
- `TransformationDuration`
- `EvolutionFeature`
- `EvolutionCost`
- `EvolutionResultItemType`
- `EvolutionAnimationDuration`
- `HideWhileUpdating`
- `HandsOnTextureKey`
- `CooldownHandsOnTextureKey`
- `UpdatingHandsOnTextureKey`

It also exposes these methods you can override:

- `GetTransformationDuration`
- `GetDetransformCooldownDuration`
- `ShouldAddDetransformCooldown`
- `HandleForcedDetransform`
- `HandleUnequip`
- `DetransformFromEnergyDepletion`
- `GetEnergyGainFromDamage`
- `ShouldStartEvolution`
- `StartEvolution`
- `GetEvolutionResultItemType`
- `CompleteEvolution`
- `GetHandsOnTextureKey`
- `ApplyHandVisuals`

### Example Omnitrix

```csharp
using Ben10Mod;
using Ben10Mod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Ben10Addon.Content.Items.Accessories;

public class TacticalOmnitrix : Omnitrix {
    public override int MaxOmnitrixEnergy => 900;
    public override int OmnitrixEnergyRegen => 5;
    public override int OmnitrixEnergyDrain => 2;
    public override int EnergyPerDamageDivisor => 20;
    public override int MinimumEnergyGainPerHit => 2;
    public override bool UseEnergyForTransformation => true;
    public override int TranformationSwapCost => 60;
    public override int TimeoutDuration => 45;
    public override int TransformationDuration => 300;

    public override string HandsOnTextureKey => "TacticalOmnitrix";
    public override string CooldownHandsOnTextureKey => "TacticalOmnitrixCooldown";
    public override string UpdatingHandsOnTextureKey => "TacticalOmnitrixUpdating";

    public override string Texture => $"Ben10Addon/Content/Items/Accessories/{Name}";

    public override void Load() {
        if (Main.netMode == NetmodeID.Server)
            return;

        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.HandsOn}", EquipType.HandsOn, name: "TacticalOmnitrix");
        EquipLoader.AddEquipTexture(Mod, $"{Texture}Cooldown_{EquipType.HandsOn}", EquipType.HandsOn, name: "TacticalOmnitrixCooldown");
        EquipLoader.AddEquipTexture(Mod, $"{Texture}Updating_{EquipType.HandsOn}", EquipType.HandsOn, name: "TacticalOmnitrixUpdating");
    }

    public override ModItem Clone(Item item) {
        var clone = (TacticalOmnitrix)base.Clone(item);
        clone.transformationNum = transformationNum;
        clone.transformationSlots = (string[])transformationSlots?.Clone();
        return clone;
    }

    public override void SaveData(TagCompound tag) {
        tag["selectedAlien"] = transformationNum;
    }

    public override void LoadData(TagCompound tag) {
        tag.TryGet("selectedAlien", out transformationNum);
    }

    public override void SetStaticDefaults() {
        dynamicTexture = ModContent.Request<Texture2D>("Ben10Addon/Content/Items/Accessories/TacticalOmnitrix").Value;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor, Vector2 origin, float scale) {
        if (player == null)
            return true;

        var omp = player.GetModPlayer<OmnitrixPlayer>();
        string texturePath = omp.omnitrixUpdating
            ? "Ben10Addon/Content/Items/Accessories/TacticalOmnitrixUpdating"
            : omp.onCooldown
                ? "Ben10Addon/Content/Items/Accessories/TacticalOmnitrixCooldown"
                : "Ben10Addon/Content/Items/Accessories/TacticalOmnitrix";

        dynamicTexture = ModContent.Request<Texture2D>(texturePath).Value;
        spriteBatch.Draw(dynamicTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
        return false;
    }

    public override int GetEnergyGainFromDamage(int damageDone) {
        return damageDone <= 0 ? 0 : Math.Max(damageDone / 18, 3);
    }

    public override void DetransformFromEnergyDepletion(Player player, OmnitrixPlayer omp) {
        TransformationHandler.Detransform(player, 20, showParticles: true, addCooldown: true);
    }
}
```

## Omnitrix Data You Usually Need To Persist

Most Omnitrix items should persist at least:

- selected transformation slot index

That is why the built-in Omnitrixes use:

- `Clone`
- `SaveData`
- `LoadData`

If your Omnitrix has custom state, store it there too.

## Choosing Timer-Based Vs Energy-Based Omnitrixes

### Timer-Based Omnitrix

Set:

- `UseEnergyForTransformation => false`

Then typically override:

- `TransformationDuration`
- `TimeoutDuration`

This gives you classic timed transformation behavior.

### Energy-Based Omnitrix

Set:

- `UseEnergyForTransformation => true`

Then typically override:

- `MaxOmnitrixEnergy`
- `OmnitrixEnergyRegen`
- `OmnitrixEnergyDrain`
- `TranformationSwapCost`
- `GetEnergyGainFromDamage`

This gives you transformations that stay active while the Omnitrix has enough energy.

## Omnitrix Evolution

Ben10Mod still supports Omnitrix item evolution, such as one Omnitrix upgrading into another item.

### Built-In Example

`PrototypeOmnitrix` evolves into `RecalibratedOmnitrix` by overriding:

- `EvolutionResultItemType`
- `ShouldStartEvolution`

### Example Evolution Override

```csharp
public override int EvolutionResultItemType => ModContent.ItemType<TacticalOmnitrixMk2>();

public override bool ShouldStartEvolution(Player player, OmnitrixPlayer omp, int defeatedNpcType) {
    return defeatedNpcType == NPCID.Golem;
}
```

If that is enough for your design, you do not need to override anything else.

### Custom Evolution Flow

If you need special effects or different timing:

- override `StartEvolution`
- override `CompleteEvolution`

## Child Transformations And Branching Forms

Transformation-to-transformation branching is now handled with child transformations.

Use this system for:

- ultimate forms that should behave as full transformations
- alternate forms with different attacks or abilities
- mixed forms that depend on both the active alien and the currently selected alien
- future DNA splice forms

The important design idea is:

- the active transformation handles the transform key while already transformed
- it can branch into one of its child transformations
- the child is a real `Transformation` with its own buff, attacks, visuals, description, and abilities
- a child can step back down into its parent and then fully detransform after a delay

### Core Transformation Properties

The main branching properties on `Transformation` are:

- `HasChildTransformation`
- `ChildTransformation`
- `ChildTransformations`
- `ParentTransformation`
- `ParentStepDownDelay`
- `StepDownToParentOnRepeatedTransform`

Use `ChildTransformation` for the simple single-child case.

Use `ChildTransformations` when you want multiple possible outcomes and will decide between them with conditions.

Set `ParentTransformation` on the child if repeated transform should step back down into the parent first.

### Simple Single-Child Example

```csharp
public class BigChillTransformation : Transformation {
    public override string FullID => "MyAddon:BigChill";
    public override string TransformationName => "Bigchill";
    public override int TransformationBuffId => ModContent.BuffType<BigChillBuff>();

    public override Transformation ChildTransformation
        => ModContent.GetInstance<UltimateBigChillTransformation>();
}

public class UltimateBigChillTransformation : Transformation {
    public override string FullID => "MyAddon:UltimateBigChill";
    public override string TransformationName => "Ultimate Bigchill";
    public override int TransformationBuffId => ModContent.BuffType<UltimateBigChillBuff>();

    public override Transformation ParentTransformation
        => ModContent.GetInstance<BigChillTransformation>();
}
```

With the default implementation:

- if Big Chill is active
- and Big Chill is also the currently selected alien
- and the Omnitrix allows evolution

then pressing the transform key again will branch into `UltimateBigChillTransformation`.

### Multiple Child Transformations

If you want more than one possible child result:

```csharp
public override IReadOnlyList<Transformation> ChildTransformations => new Transformation[] {
    ModContent.GetInstance<UltimateDiamondHeadTransformation>(),
    ModContent.GetInstance<CrystalSpeedTransformation>()
};
```

Then override `CanUseChildTransformation(...)` to decide which branch is valid.

### Registering A Child For A Base-Mod Transformation

Addons can now attach child branches to transformations they do not own by using
`TransformationBranchRegistry`.

This is the API to use if you want something like:

- `Ben10Mod:BigChill -> MyAddon:UltimateBigChill`
- `Ben10Mod:DiamondHead -> MyAddon:CrystalSpeed`

Example:

```csharp
using Ben10Mod.Content.Transformations;

public override void Load() {
    TransformationBranchRegistry.RegisterChildBranch(
        parentTransformationId: "Ben10Mod:BigChill",
        childTransformationId: "MyAddon:UltimateBigChill",
        condition: (player, omp, omnitrix, parent, child, selected) =>
            selected?.FullID == "Ben10Mod:BigChill" &&
            omnitrix.CanUseEvolutionFeature(player, omp, parent),
        energyCost: (player, omp, omnitrix, parent, child, selected) =>
            omnitrix.EvolutionCost,
        shouldDetransformOnFailure: (player, omp, omnitrix, parent, child, selected) => true
    );
}
```

The registry is global at runtime, so the base transformation does not need to know your addon child exists.

### Conditional Child Transformations

The main condition hook is:

```csharp
protected override bool CanUseChildTransformation(
    Player player,
    OmnitrixPlayer omp,
    Omnitrix omnitrix,
    Transformation childTransformation,
    string selectedTransformationId)
```

This gives you access to:

- the player
- Omnitrix runtime state
- the equipped Omnitrix
- the candidate child transformation
- the currently selected roster transformation ID

This is the hook to use for mixed forms.

Example:

```csharp
protected override bool CanUseChildTransformation(
    Player player,
    OmnitrixPlayer omp,
    Omnitrix omnitrix,
    Transformation childTransformation,
    string selectedTransformationId) {
    if (childTransformation.FullID == "MyAddon:UltimateDiamondHead")
        return selectedTransformationId == FullID &&
               omnitrix.CanUseEvolutionFeature(player, omp, this);

    if (childTransformation.FullID == "MyAddon:CrystalSpeed") {
        return selectedTransformationId == "MyAddon:XLR8" &&
               omnitrix.CanDNASplice(player, omp, this,
                   TransformationLoader.Get(selectedTransformationId),
                   childTransformation);
    }

    return false;
}
```

### Energy Cost And Failure Rules

The default child branch uses `EvolutionCost` only for the simple default child case.

If you want custom branch costs or different failure behavior, override:

- `GetChildTransformationEnergyCost(...)`
- `ShouldDetransformWhenChildTransformationFails(...)`
- `OnChildTransformationActivated(...)`

This lets you support:

- free branches
- expensive DNA splice branches
- branches that fail silently
- branches that detransform on failure

### Omnitrix Capability Hooks

The Omnitrix now exposes capability hooks for branch logic:

- `CanUseChildTransformation(...)`
- `CanDNASplice(...)`
- `CanUseEvolutionFeature(...)`

These can be used both by transformation overrides and by addon branch-registry conditions.

Typical use:

- keep transformation-specific branch rules in the transformation
- keep Omnitrix-specific permission rules in the Omnitrix

Example:

```csharp
public override bool CanDNASplice(Player player, OmnitrixPlayer omp,
    Transformation current, Transformation selectedTransformation, Transformation child) {
    return masterDNAFeatureUnlocked;
}
```

### Step-Down Behavior

If a child has `ParentTransformation` set, the default branch handling can step down first and then fully detransform after a delay.

Useful properties and hooks:

- `ParentTransformation`
- `ParentStepDownDelay`
- `StepDownToParentOnRepeatedTransform`
- `CompleteEvolutionStepDown(...)`

This is how forms like Ultimate Big Chill can:

- repeated transform to return to normal Big Chill
- play detransform visuals and sound
- wait briefly
- then fully detransform

### Recommended Pattern

When making branchable forms:

1. create the base transformation as normal
2. create each child form as its own full `Transformation`
3. give every child its own buff
4. set `ParentTransformation` on children that should step down
5. use `CanUseChildTransformation(...)` for mixed-form conditions
6. use Omnitrix capability hooks for feature gating such as evolution or DNA splicing

This is the preferred replacement for the removed `Evolved*` property pattern.

## Connecting An Omnitrix To Your Addon Transformations

You do not need to hardcode your Omnitrix to specific transformations.

The Omnitrix system already works with string IDs stored in the player's roster and unlock list. Your addon transformations can be unlocked and assigned in the same UI.

The main requirements are:

- the transformation must be registered
- the player must have unlocked it
- the transformation ID must be placed into one of the roster slots

## How The Roster/UI Works

Ben10Mod stores:

- `transformationSlots`
- `unlockedTransformations`
- `currentTransformationId`

The alien selection UI reads those lists and renders icons from the transformation loader.

This means addon transformations appear automatically in the roster UI as long as:

- the transformation class is loaded
- `GetTransformationIcon()` can load the icon
- the transformation is unlocked for the player

## Multiplayer Notes

Ben10Mod already syncs transformation unlocks by transformation ID string.

For addon safety:

- use stable transformation IDs
- never rename a transformation ID after users have saves using it
- do not reuse an old ID for a different alien

If you rename `Ben10Addon:ShockRock` to `Ben10Addon:ShockRockV2`, existing saves will still contain the old ID and the roster entry will no longer resolve.

## Common Pitfalls

### 1. Buff ID Does Not Match Transformation ID

If the buff sets the wrong `currentTransformationId`, the transformation will not work correctly.

### 2. Icon Path Is Wrong

If `IconPath` points to a missing texture, the UI will break or show missing assets.

### 3. No Costume Registered

If your `FrameEffects` code asks for equip slots that were never loaded, the alien visuals will not appear.

### 4. Unlock Item Uses The Wrong ID

Make sure your unlock code uses the exact same transformation ID as `FullID`.

### 5. Addon Uses `Ben10Mod:` IDs

Do not do this for your own addon content. Use your addon mod's namespace and ID prefix.

### 6. Your Badge "Does Nothing"

That usually means your current transformation does not provide an attack profile or `Shoot` logic for the currently selected attack mode.

### 7. Save-Breaking Renames

Avoid renaming:

- transformation IDs
- item internal class names when texture naming depends on `Name`
- hands-on texture keys if existing content relies on them

### 8. Editor Shows Errors But Build Works

This is especially common with project references in Rider.

If:

- `dotnet build` works
- tModLoader can build the mod
- but the editor still says Ben10Mod types are missing

then your project setup is probably fine and the editor needs a restore/reload cycle.

## Recommended Addon Workflow

If you want the smoothest experience, build in this order:

1. Create the addon mod root.
2. Add `modReferences = Ben10Mod`.
3. Add the project reference to `Ben10Mod.csproj`.
4. Build one new transformation first.
5. Add an unlock item and confirm it appears in the Ben10Mod roster UI.
6. Add a custom badge if you want a new progression weapon.
7. Add a custom Omnitrix last, once the transformation works.

This makes debugging much easier because transformations are the foundation for the other two systems.

## Minimal "First Addon" Checklist

If you want the absolute minimum viable addon:

1. Create a new mod folder in `ModSources`.
2. Add `modReferences = Ben10Mod` to `build.txt`.
3. Add a project reference to `../Ben10Mod/Ben10Mod.csproj`.
4. Create one `Transformation` subclass.
5. Create one matching `ModBuff`.
6. Create one icon texture.
7. Create one unlock item that calls `TransformationHandler.AddTransformation`.
8. Launch and confirm the alien appears in the Ben10Mod roster UI.

## Suggested Asset Checklist

For a fully featured transformation:

- icon texture for UI
- costume base texture
- head texture
- body texture
- legs texture
- projectile textures
- optional buff textures

For a fully featured Omnitrix:

- inventory texture
- cooldown inventory texture
- updating inventory texture
- hands-on texture
- hands-on cooldown texture
- hands-on updating texture

For a badge:

- item texture

## Final Advice

Keep the addon small at first.

The fastest successful path is:

- one transformation
- one unlock item
- one simple projectile

Once that works, add:

- a badge
- a custom Omnitrix
- more advanced abilities

If you want to mirror Ben10Mod's style, use the base systems as intended:

- let transformations own combat behavior
- let badges stay generic
- let Omnitrixes own Omnitrix-specific resource and lifecycle rules
