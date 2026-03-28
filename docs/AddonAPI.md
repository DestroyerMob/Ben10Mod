# Ben10Mod Addon API Guide

This guide explains how to build a separate tModLoader addon mod that extends `Ben10Mod`.

Related docs:

- [README](../README.md)
- [Architecture Guide](Architecture.md)
- [Development Guide](Development.md)

## Supported Extension Points

Ben10Mod currently exposes three main extension surfaces:

- `Ben10Mod.Content.Items.Accessories.Omnitrix`
- `Ben10Mod.Content.Transformations.Transformation`
- `Ben10Mod.Content.Items.Weapons.PlumbersBadge`

The important design rule is:

- transformations own alien-specific behavior
- Omnitrix items own energy, timing, and branch behavior
- badges stay generic and ask the current transformation what to do

## Mental Model

If you are building an addon, this is the model to use:

- your transformation is a `Transformation` subclass, not a `ModItem`
- your transformation registers itself automatically
- your transformation becomes active through a `ModBuff` that sets `currentTransformationId`
- your badge attacks are defined on the transformation
- your Omnitrix item is just another `Omnitrix` subclass accepted by the shared slot

## Step 1: Add Ben10Mod As A Dependency

You usually want both:

- a runtime dependency
- a compile-time project reference

### `build.txt`

Example:

```ini
displayName = Ben10 Addon Example
author = YourName
version = 0.1
hideCode = false
hideResources = false
modReferences = Ben10Mod
```

### `.csproj`

Example:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Import Project="..\\tModLoader.targets" />

  <PropertyGroup>
    <AssemblyName>Ben10Addon</AssemblyName>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\\Ben10Mod\\Ben10Mod.csproj" />
  </ItemGroup>
</Project>
```

## Step 2: Create Your Addon Root

Minimal root file:

```csharp
using Terraria.ModLoader;

namespace Ben10Addon;

public class Ben10Addon : Mod {
}
```

## Building A Transformation

This is the most important addon type.

### What A Transformation Needs

At minimum:

- a `Transformation` subclass
- a `ModBuff` that sets the active transformation ID

Usually also:

- icon texture
- costume or equip content
- projectiles
- unlock item or unlock progression hook

### Transformation IDs

Use your own mod-prefixed string ID.

Example:

- `Ben10Addon:ShockRock`

Do not use `Ben10Mod:...` for addon content.

## Current Action-Slot Model

The public action slots are:

- primary
- secondary
- tertiary
- ultimate

Each slot can be one of two things:

- a timed ability
- a badge attack loader

That means `F`, `G`, `H`, and `U` are not just buff toggles.

### Timed Ability Slots

Use properties such as:

- `PrimaryAbilityName`
- `PrimaryAbilityDuration`
- `PrimaryAbilityCooldown`
- `PrimaryAbilityCost`
- `SecondaryAbilityName`
- `SecondaryAbilityDuration`
- `TertiaryAbilityName`
- `TertiaryAbilityDuration`
- `UltimateAbilityName`
- `UltimateAbilityDuration`

And optionally:

- `TryActivatePrimaryAbility(...)`
- `TryActivateSecondaryAbility(...)`
- `TryActivateTertiaryAbility(...)`
- `TryActivateUltimateAbility(...)`

### Badge Attack Slots

Use properties such as:

- `PrimaryAttack`
- `SecondaryAttack`
- `PrimaryAbilityAttack`
- `SecondaryAbilityAttack`
- `TertiaryAbilityAttack`
- `UltimateAttack`

And the matching timing, style, armor penetration, and cost properties.

### Important Rule

For a given slot, prefer one mode per state.

If you define both a timed ability and an ability-attack on the same slot, the timed ability wins by default unless you override the activation hook and choose differently yourself.

## Attack Naming

The current-attack HUD no longer depends on projectile names.

Use:

- `PrimaryAttackName`
- `SecondaryAttackName`
- `PrimaryAbilityAttackName`
- `SecondaryAbilityAttackName`
- `TertiaryAbilityAttackName`
- `UltimateAttackName`

If a name changes by moveset, use the attack profile `DisplayName` for that state-specific version.

## Timed Ability Naming

The active-ability HUD reads timed ability names from the transformation API.

Use:

- `PrimaryAbilityName`
- `SecondaryAbilityName`
- `TertiaryAbilityName`
- `UltimateAbilityName`

If you do not set one, the HUD falls back to the slot label.

## Moveset Profiles

Ben10Mod now supports moveset-indexed attack profiles.

Use this when one transformation should swap attack packages by state, for example:

- powered versus unpowered
- suit versus unbound
- normal versus ultimate stance

Current pattern:

1. override `GetMoveSetIndex(OmnitrixPlayer omp)`
2. return one or more `TransformationAttackProfile` entries from `GetPrimaryAttackProfiles()` or the equivalent slot method

Each `TransformationAttackProfile` can define:

- `DisplayName`
- `ProjectileType`
- `DamageMultiplier`
- `UseTime`
- `ShootSpeed`
- `UseStyle`
- `Channel`
- `NoMelee`
- `ArmorPenetration`
- `EnergyCost`
- `SustainEnergyCost`
- `SustainInterval`
- `SingleUse`

If you do not need custom movesets, the base implementation already wraps the normal `PrimaryAttack`, `SecondaryAttack`, and related properties into a one-entry profile list for you.

## Attack Costs

All badge attack profiles support optional energy costs.

Current upfront cost properties:

- `PrimaryEnergyCost`
- `SecondaryEnergyCost`
- `PrimaryAbilityAttackEnergyCost`
- `SecondaryAbilityAttackEnergyCost`
- `TertiaryAbilityAttackEnergyCost`
- `UltimateEnergyCost`

Current sustain cost properties:

- `PrimaryAttackSustainEnergyCost`
- `SecondaryAttackSustainEnergyCost`
- `PrimaryAbilityAttackSustainEnergyCost`
- `SecondaryAbilityAttackSustainEnergyCost`
- `TertiaryAbilityAttackSustainEnergyCost`
- `UltimateAttackSustainEnergyCost`

With matching interval properties:

- `PrimaryAttackSustainInterval`
- `SecondaryAttackSustainInterval`
- `PrimaryAbilityAttackSustainInterval`
- `SecondaryAbilityAttackSustainInterval`
- `TertiaryAbilityAttackSustainInterval`
- `UltimateAttackSustainInterval`

Ben10Mod handles upfront attack spending in the shared badge fire path. Most addon transformations should not manually subtract upfront badge attack energy inside `Shoot(...)`.

## Example Transformation

```csharp
using System.Collections.Generic;
using Ben10Mod;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Transformations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Addon.Content.Transformations.ShockRock;

public class ShockRockTransformation : Transformation {
    public override string FullID => "Ben10Addon:ShockRock";
    public override string TransformationName => "Shock Rock";
    public override string Description => "A conductive crystal alien built for ranged pressure and charged badge attacks.";
    public override string IconPath => "Ben10Addon/Content/Interface/ShockRockSelect";
    public override int TransformationBuffId => ModContent.BuffType<ShockRockBuff>();

    public override List<string> Abilities => new() {
        "Crystal bolt primary fire",
        "Charged burst secondary fire",
        "Shield projector action key",
        "Lance storm ultimate"
    };

    public override string PrimaryAttackName => "Crystal Bolt";
    public override string SecondaryAttackName => "Charged Burst";
    public override string PrimaryAbilityAttackName => "Shield Projector";
    public override string UltimateAttackName => "Lance Storm";

    public override int PrimaryAttack => ModContent.ProjectileType<Projectiles.ShockRockBolt>();
    public override int PrimaryAttackSpeed => 16;
    public override int PrimaryShootSpeed => 12;
    public override int PrimaryUseStyle => ItemUseStyleID.Shoot;

    public override int SecondaryAttack => ModContent.ProjectileType<Projectiles.ShockRockBurst>();
    public override int SecondaryAttackSpeed => 28;
    public override int SecondaryShootSpeed => 9;
    public override int SecondaryUseStyle => ItemUseStyleID.Shoot;
    public override int SecondaryEnergyCost => 4;

    public override int PrimaryAbilityAttack => ModContent.ProjectileType<Projectiles.ShockRockShieldNode>();
    public override int PrimaryAbilityAttackSpeed => 22;
    public override int PrimaryAbilityAttackShootSpeed => 0;
    public override int PrimaryAbilityAttackUseStyle => ItemUseStyleID.HoldUp;
    public override int PrimaryAbilityAttackEnergyCost => 8;
    public override bool PrimaryAbilityAttackSingleUse => true;

    public override int UltimateAttack => ModContent.ProjectileType<Projectiles.ShockRockLanceStorm>();
    public override int UltimateAttackSpeed => 20;
    public override int UltimateShootSpeed => 0;
    public override int UltimateUseStyle => ItemUseStyleID.HoldUp;
    public override int UltimateEnergyCost => 35;

    public override int GetMoveSetIndex(OmnitrixPlayer omp) {
        return omp.IsUltimateAbilityActive ? 1 : 0;
    }

    protected override IReadOnlyList<TransformationAttackProfile> GetPrimaryAttackProfiles() {
        return CreateMoveSetProfiles(
            CreatePrimaryAttackProfile(),
            new TransformationAttackProfile {
                DisplayName = "Overcharged Crystal Bolt",
                ProjectileType = ModContent.ProjectileType<Projectiles.ShockRockBolt>(),
                DamageMultiplier = 1.25f,
                UseTime = 12,
                ShootSpeed = 15f,
                UseStyle = ItemUseStyleID.Shoot,
                Channel = false,
                NoMelee = true,
                ArmorPenetration = 4
            }
        );
    }

    public override void UpdateEffects(Player player, OmnitrixPlayer omp) {
        player.GetDamage<HeroDamage>() += 0.08f;
        player.endurance += 0.06f;
    }
}
```

## Example Transformation Buff

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

If the buff does not set `currentTransformationId`, the player will not be considered transformed into your form.

## Custom `Shoot(...)` Overrides

You only need to override `Shoot(...)` if the default projectile spawning is not enough.

Use a custom override when you need:

- cursor placement
- projectile spread
- special sentry limits
- melee hitboxes
- transformation-specific branching behavior

Remember:

- shared attack cost handling already happens before `Shoot(...)`
- loaded attack cleanup also happens outside your transformation
- mouse-targeted and multiplayer-sensitive attacks often need owner-side guards or synced aim

So your custom `Shoot(...)` usually only needs to worry about the actual attack behavior.

## Building A Badge

The badge itself is usually simple.

### What The Badge Owns

- base damage
- rank identity
- recipe

### What The Transformation Owns

- projectile choice
- timing
- style
- channeling
- armor penetration
- attack naming
- attack energy cost

### Example Badge

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

## Building An Omnitrix

The base `Omnitrix` class already supports:

- max energy
- regen and drain
- transformation duration rules
- swap costs
- damage-to-energy rules
- evolution rules
- hand visuals

### Example Omnitrix

```csharp
using Ben10Mod.Content.Items.Accessories;

namespace Ben10Addon.Content.Items.Accessories;

public class TacticalOmnitrix : Omnitrix {
    public override int MaxOmnitrixEnergy => 900;
    public override int OmnitrixEnergyRegen => 5;
    public override int OmnitrixEnergyDrain => 2;
    public override bool UseEnergyForTransformation => true;
    public override int TranformationSwapCost => 60;
}
```

For real Omnitrix items you will usually also implement:

- texture loading
- hand texture registration
- `Clone`
- `SaveData`
- `LoadData`

## Unlocking A Transformation

Recommended helper:

```csharp
using Ben10Mod.Content;

TransformationHandler.AddTransformation(player, "Ben10Addon:ShockRock");
```

Direct player call also works:

```csharp
using Ben10Mod;

player.GetModPlayer<OmnitrixPlayer>().UnlockTransformation("Ben10Addon:ShockRock");
```

## Codex Unlock Conditions

Addon transformations can provide codex unlock text in two ways.

Recommended for transformation-owned logic:

```csharp
public override string GetUnlockConditionText(OmnitrixPlayer omp)
    => "Defeat Shock Rock's boss encounter.";
```

Or register a condition string through `Mod.Call`:

```csharp
ModContent.GetInstance<Ben10Mod>().Call(
    "RegisterTransformationUnlockCondition",
    "Ben10Addon:ShockRock",
    "Defeat Shock Rock's boss encounter.");
```

## Child Transformations And Branching Forms

Use child transformations for:

- ultimate forms that should behave like full transformations
- alternate forms
- branch forms

Important properties:

- `ChildTransformation`
- `ChildTransformations`
- `ParentTransformation`
- `ParentStepDownDelay`
- `StepDownToParentOnRepeatedTransform`

Use the transform-key hooks when you need custom branching conditions.

## Optional: Material Absorption Addon Content

Ben10Mod also exposes absorbable-material registration through `Mod.Call`.

Current command:

- `RegisterAbsorbableMaterial`

This is useful if your addon adds new bars or materials that should work with the Osmosian absorption system.

## Recommended Checklist Before Shipping An Addon

Make sure:

- your transformation ID uses your own mod prefix
- the transformation buff sets `currentTransformationId`
- your icon path exists
- your attacks are defined on the transformation, not hardcoded into the badge
- your action-slot design matches the current timed-ability versus badge-attack model
- your attack names are set in the transformation API
- your moveset-dependent attacks use the profile system
- your attack costs use the built-in cost properties
- your mouse-driven attacks are safe in multiplayer
- your unlock path actually grants the transformation

## Practical Tip

If you are not sure how to implement something, copy the built-in content type that is closest to your goal and simplify from there. Ben10Mod's extension model is much easier to follow by example than by trying to treat the whole system as a blank-slate framework.
