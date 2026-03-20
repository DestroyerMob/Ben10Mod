# Ben10Mod Architecture Guide

This document explains how the current runtime is put together.

It is intended for:

- contributors working inside Ben10Mod
- addon authors who want the current mental model before extending it
- anyone debugging why a transformation, Omnitrix, ability, or badge attack is behaving incorrectly

## Core Runtime Model

Ben10Mod is built around a player-centric state model.

`OmnitrixPlayer` is the main state container. It tracks:

- whether the player has an Omnitrix equipped
- which transformation is active
- which transformations are unlocked
- which five transformations are assigned to the active roster
- Omnitrix energy and drain/regen state
- timed ability flags and cooldown flow
- the current badge attack selection state
- progression participation for bosses and events
- transformation-scale and custom-visual state

Most systems either:

- mutate `OmnitrixPlayer`
- read `OmnitrixPlayer`
- or plug alien-specific behavior into hooks that `OmnitrixPlayer` calls

## Main Runtime Pieces

### `Ben10Mod`

`Ben10Mod.cs` is the mod entry point.

Responsibilities:

- load shaders and filters
- define packet types
- handle unlock and absorption sync packets

### `OmnitrixPlayer`

`OmnitrixPlayer.cs` is the main runtime coordinator.

Responsibilities:

- persist unlock and roster data
- track the active transformation
- route update hooks into the current transformation
- drive timed abilities and cooldown expiration
- manage the attack-selection state machine
- manage Omnitrix energy-facing state
- track event participation and unlocks
- expose UI-friendly state such as the current attack label and pulse

If you are unsure where a gameplay rule lives, start here first.

### `Transformation`

`Content/Transformations/Transformation.cs` is the alien API.

A transformation owns:

- ID, display name, description, icon, and ability text
- badge attack profiles
- action-slot behavior for primary, secondary, tertiary, and ultimate
- passive hooks such as `UpdateEffects`, `PostUpdate`, and `FrameEffects`
- combat hooks such as `OnHitNPC`, `ModifyHurt`, and projectile hit hooks
- branching and child-transformation behavior

Transformations are the main source of alien-specific gameplay.

### `TransformationLoader`

`Content/Transformations/TransformationLoader.cs` is the shared transformation registry.

Each transformation registers automatically. The loader is then used by:

- roster UI
- transformation activation
- save/load resolution
- unlock lookups

### `TransformationHandler`

`Content/TransformationHandler.cs` is the shared state-transition helper.

Use it for:

- transform
- detransform
- unlocks
- transformation cleanup

Code should prefer this utility layer over manually changing player transformation fields.

### `Omnitrix`

`Content/Items/Accessories/Omnitrix.cs` is the base Omnitrix API.

It owns:

- max energy
- regen and drain
- transformation duration rules
- swap costs
- transform/detransform behavior
- damage-to-energy rules
- evolution hooks
- hand-visual hooks

The important architectural point is that `OmnitrixPlayer` no longer hardcodes specific Omnitrix items. It delegates to the currently equipped `Omnitrix`.

### `PlumbersBadge`

`Content/Items/Weapons/PlumbersBadge.cs` is the generic badge weapon shell.

It owns:

- base weapon stats and rank identity
- resetting the held-item defaults before a transformation modifies them
- checking whether the currently selected attack is affordable
- consuming attack energy in the shared fire path
- calling into the active transformation's `Shoot(...)`

Alien-specific attack logic belongs in transformations, not badges.

## Action Slots And Attack Selection

The modern combat model is split into two layers:

- action slots
- badge attack selection

### Action Slots

The action keys are:

- primary
- secondary
- tertiary
- ultimate

Each slot can be configured by a transformation as either:

- a timed ability
- or a badge attack loader

That means `F`, `G`, `H`, and `U` are not just buff toggles anymore. A transformation decides what each slot means.

### Badge Attack Selection

The badge uses a small state machine.

The currently selected attack can be:

- primary
- secondary
- primary ability attack
- secondary ability attack
- tertiary ability attack
- ultimate attack

Key points:

- right click with the badge swaps between the base primary and secondary modes
- loading an action-key attack temporarily replaces the base attack
- some loaded attacks are single-use
- some persist until replaced or cleared
- the current selection drives badge stats, energy cost, HUD text, and projectile selection

## Runtime Flows

### Transformation Flow

Normal transform flow:

1. some code decides to transform the player
2. `TransformationHandler.Transform(...)` resolves the target transformation
3. the transformation buff is applied
4. the buff sets `currentTransformationId` and marks the player transformed
5. `OmnitrixPlayer` forwards lifecycle hooks to the active transformation every tick

### Detransform Flow

Normal detransform flow:

1. `TransformationHandler.Detransform(...)` is called
2. timed abilities and loaded attacks are cleaned up
3. cooldowns are applied when appropriate
4. transformation buffs are cleared
5. `currentTransformationId` is cleared

### Timed Ability Flow

When an action key is bound to a timed ability:

1. `OmnitrixPlayer` checks slot availability and cooldowns
2. activation cost is checked
3. the relevant ability buff is applied
4. the transformation reads that state through `PrimaryAbilityEnabled`, `IsPrimaryAbilityActive`, and related flags
5. cooldown is applied when the ability ends

### Badge Attack Flow

When the player fires a `Plumber's Badge`:

1. the badge checks whether the player is transformed
2. the badge asks the current transformation to modify badge stats
3. `CanUseItem` checks whether the currently selected attack can be afforded
4. the shared `Shoot` path consumes the current attack cost
5. the badge calls `transformation.Shoot(...)`
6. if the selected attack was a loaded ability-attack, post-fire cleanup such as single-use clearing happens

This is important because costs are now tied to the currently selected attack profile, not split across different manual item paths.

## UI Systems

### Roster UI

`Content/Interface/AlienSelectionScreen.cs` renders:

- the active five-slot roster
- the unlocked transformation list
- the selected transformation's description and abilities

Because it resolves transformations through the loader, addon transformations appear automatically if they register correctly and are unlocked.

### Omnitrix Energy Bar

The `UISystem` also draws the Omnitrix energy bar.

It reads directly from `OmnitrixPlayer`:

- `omnitrixEquipped`
- `omnitrixEnergy`
- `omnitrixEnergyMax`

### Current Attack HUD

The same UI system now draws a current-attack panel under the energy bar.

It shows:

- the active attack slot label
- the current attack display name
- the per-shot energy cost
- a short pulse when the attack selection changes

That HUD is driven from `OmnitrixPlayer` plus transformation-supplied attack display names.

## Progression Systems

### Boss Unlocks

`bossTrackerNPC.cs` handles boss-based unlock progression.

It tracks:

- damage contribution per player
- which NPCs count as tracked encounters
- multi-segment boss edge cases
- unlock rewards
- Omnitrix evolution triggers

### Event Unlocks

`OmnitrixPlayer` handles event-based unlocks.

It tracks:

- whether the player participated in the event
- whether the event completed successfully
- which transformation should be unlocked when the event ends

This is why unlock logic is split across:

- `bossTrackerNPC.cs`
- `OmnitrixPlayer.cs`

## Addon-Relevant Design Rules

These are the important extension rules that the current architecture is built around:

- transformations are identified by stable string IDs
- transformations auto-register
- Omnitrixes are subclasses, not hardcoded type checks inside the player
- badges stay generic and ask the transformation what to do
- attack costs belong to attack profiles
- timed abilities and attack-loading slots share the same action-key framework

## Practical Debugging Tips

If something is wrong with transformation behavior:

- inspect `OmnitrixPlayer.cs`
- inspect the active transformation class
- inspect `TransformationHandler.cs`

If something is wrong with badge combat:

- inspect `PlumbersBadge.cs`
- inspect the current transformation's attack profile data
- inspect the transformation's `Shoot(...)` override if it has one

If something is wrong with unlock progression:

- inspect `bossTrackerNPC.cs`
- inspect the event unlock logic in `OmnitrixPlayer.cs`
