# Ben10Mod Architecture Guide

This document explains how the main gameplay systems fit together.

It is intended for:

- contributors working directly in Ben10Mod
- addon authors who want to understand the runtime flow before extending it
- developers trying to debug why a transformation, Omnitrix, or badge is not behaving correctly

## Core Design

Ben10Mod is built around a player-centric state model.

The `OmnitrixPlayer` class is the central runtime state holder. It tracks:

- whether the player has an Omnitrix equipped
- which transformation is currently active
- which transformations are unlocked
- current roster assignment
- Omnitrix energy state
- ability toggles and cooldown flow
- movement state such as dashing or possession
- cosmetic and hand visual state

Most other systems either:

- mutate `OmnitrixPlayer`
- read data from `OmnitrixPlayer`
- or provide plug-in behavior that `OmnitrixPlayer` delegates to

## Main Runtime Pieces

### `Ben10Mod`

`Ben10Mod.cs` is the root mod entry point.

Responsibilities:

- load shaders and screen filters
- define packet types
- receive and process network packets

At the moment, the main explicit packet behavior is transformation unlock syncing.

### `OmnitrixPlayer`

`OmnitrixPlayer.cs` is the heart of the mod.

Responsibilities:

- persist unlock and roster data
- track active transformation state
- track Omnitrix energy
- forward lifecycle hooks to the current transformation
- drive ability activation
- manage Omnitrix visuals
- handle damage-to-energy conversion
- handle event and boss participation tracking

If you are unsure where a gameplay behavior lives, this file is the first place to inspect.

### `Transformation`

`Content/Transformations/Transformation.cs` defines the transformation API.

A transformation provides:

- metadata: ID, name, description, icon, ability text
- attack profile data used by badges
- buff identity
- lifecycle hooks such as `OnTransform`, `OnDetransform`, `UpdateEffects`, `PostUpdate`, `FrameEffects`
- combat hooks such as `ModifyHurt`, `OnHitNPC`, `CanHitNPCWithProjectile`
- ability hooks such as `TryActivatePrimaryAbility`

Transformations are the main source of alien-specific behavior.

### `TransformationLoader`

`Content/Transformations/TransformationLoader.cs` is the shared transformation registry.

Each transformation registers itself automatically in `Transformation.Register()`.

This registry is used by:

- roster UI
- transformation activation
- unlock lookups
- save data resolution

### `TransformationHandler`

`Content/TransformationHandler.cs` is a utility layer around the core state machine.

Responsibilities:

- begin transformation
- end transformation
- apply effects and sounds
- add unlocks
- query unlock state

When possible, code that wants to transform or unlock something should go through `TransformationHandler` rather than rewriting the state transitions manually.

### `Omnitrix`

`Content/Items/Accessories/Omnitrix.cs` is the Omnitrix base API.

The Omnitrix system now follows the same extension philosophy as transformations and badges.

The base class provides:

- energy capacity and regen rules
- energy drain rules
- damage-to-energy rules
- transformation timing rules
- swap cost rules
- detransform behavior
- evolution hooks
- hand visual hooks
- local keybind-driven transformation selection and activation

The important architectural change is that `OmnitrixPlayer` no longer hardcodes specific Omnitrix types. It delegates behavior to the currently equipped `Omnitrix`.

### `PlumbersBadge`

`Content/Items/Weapons/PlumbersBadge.cs` is the generic badge weapon shell.

Responsibilities:

- define base weapon stats
- consume Omnitrix energy when needed
- apply generic timing reset
- ask the current transformation how to behave

Badges do not own alien-specific combat logic. Transformations do.

## Data Flow

### Transformation Flow

The normal flow is:

1. some code decides to transform the player
2. `TransformationHandler.Transform(...)` is called
3. the transformation is resolved through `TransformationLoader`
4. `OmnitrixPlayer.currentTransformationId` is set
5. the transformation buff is applied
6. every tick, the buff keeps `currentTransformationId` and `isTransformed` in sync
7. `OmnitrixPlayer` forwards update hooks to the active transformation

### Detransformation Flow

The normal detransform path is:

1. `TransformationHandler.Detransform(...)` is called
2. cooldowns and ability cleanup are handled
3. transformation buffs are cleared
4. `currentTransformationId` is cleared
5. transformation flags are reset
6. `OnDetransform` is called on the previous transformation

### Badge Attack Flow

When a player uses a Plumber's Badge:

1. the badge checks whether the player is transformed
2. the badge asks the current transformation to modify badge stats
3. the badge asks the transformation for the current energy cost
4. the badge consumes energy if needed
5. the badge calls `transformation.Shoot(...)`

The selected attack profile depends on:

- primary attack by default
- secondary attack when `altAttack` is toggled
- ultimate attack when `ultimateAttack` is active

### Unlock Flow

Unlocks are stored as transformation string IDs in `unlockedTransformations`.

Unlocks may come from:

- boss progression
- event progression
- items
- addon content

In multiplayer, unlocks are synced by packet from server to client.

## UI Systems

### Omnitrix Slot

`Content/Interface/OmnitrixSlot.cs` defines the custom Omnitrix accessory slot.

Accepted content:

- any `ModItem` inheriting from `Omnitrix`

This is why addon Omnitrixes work without additional slot registration changes.

### Alien Roster UI

`Content/Interface/AlienSelectionScreen.cs` renders:

- the active roster slots
- the list of unlocked transformations
- the selected transformation's icon, description, and abilities

The UI resolves everything through the transformation loader, which is why addon transformations appear automatically if they register correctly and are unlocked.

### Omnitrix Energy Bar

`UISystem` also draws the Omnitrix energy bar.

It reads directly from `OmnitrixPlayer`:

- `omnitrixEquipped`
- `omnitrixEnergy`
- `omnitrixEnergyMax`

## Progression Systems

### Boss Unlocks

`bossTrackerNPC.cs` tracks:

- damage contribution by player
- whether an NPC counts as a boss
- multi-segment boss special cases
- transformation unlock rewards
- Omnitrix evolution triggers

This file is the bridge between Terraria progression and Ben10Mod unlock progression.

### Event Unlocks

`OmnitrixPlayer` also tracks participation in specific events and grants unlocks when those events end successfully.

This means some transformation unlock logic lives in:

- `bossTrackerNPC.cs`
- `OmnitrixPlayer.cs`

## File Layout Conventions

The codebase mostly follows these conventions:

- `Content/Transformations/<AlienName>/` for alien-specific files
- transformation class, costume item, and textures are grouped together
- `Content/Buffs/Transformations/` for transformation buff assets and classes
- `Content/Items/Accessories/` for Omnitrixes and accessory helpers
- `Content/Items/Weapons/` for badges
- `Content/Projectiles/` for alien attacks and utility projectiles

The folder structure is not perfectly uniform yet, but this is the intended organization.

## Patterns To Follow When Adding Code

When adding new gameplay content, prefer these patterns:

- put alien-specific logic in a `Transformation` subclass
- put Omnitrix-specific rules in an `Omnitrix` subclass
- keep badges generic unless the badge itself really needs unique behavior
- use `TransformationHandler` for unlock/transform transitions
- identify transformations by stable string IDs

Avoid:

- adding new hardcoded type checks in `OmnitrixPlayer`
- keying behavior off transformation display names when an override or explicit hook would be cleaner
- duplicating roster or unlock logic outside the shared systems

## Common Debugging Entry Points

If something is broken, these are the most useful places to inspect.

### Transformation does not appear in roster

Check:

- transformation class exists and loads
- `FullID` is stable and unique
- `IconPath` is valid
- the transformation was actually unlocked
- `TransformationBuffId` is valid

### Player transforms but visuals do not change

Check:

- the buff sets the correct `currentTransformationId`
- `FrameEffects` is implemented
- equip textures were loaded in the costume item

### Badge does nothing

Check:

- player is transformed
- current transformation resolves correctly
- the transformation defines attack data
- energy cost is not blocking the attack

### Omnitrix behavior is wrong

Check:

- which `Omnitrix` subclass is equipped
- whether that subclass overrides the expected timing or energy behavior
- whether the problem is in `OmnitrixPlayer` state or in the Omnitrix override
