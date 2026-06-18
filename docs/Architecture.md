# Ben10Mod Architecture Guide

This document explains how the current runtime is put together.

It is intended for:

- contributors working inside Ben10Mod
- addon authors who want the current mental model before extending it
- anyone debugging why a transformation, Omnitrix, ability, badge attack, or armor effect is behaving incorrectly

## Core Runtime Model

Ben10Mod is still player-centric, but `OmnitrixPlayer` is no longer intended to be the owner of every rule and every raw field.

The current model is:

- `OmnitrixPlayer` coordinates the active form and Terraria player hooks.
- Focused controller/state classes own major Omnitrix subsystems.
- Transformations define alien kits and route alien-specific behavior through the shared hooks.
- NPC-side alien statuses live behind `AlienIdentityGlobalNPC`, with newer mechanics moving into focused state objects.

Most gameplay systems should now either:

- ask `OmnitrixPlayer` for the active transformation or exposed runtime facade
- use the relevant controller/state object for subsystem policy
- implement alien-specific behavior inside a transformation, projectile, `ModPlayer`, or focused NPC state helper

Avoid adding new unrelated state directly to `OmnitrixPlayer` unless it is genuinely coordination state.

## Main Runtime Pieces

### `Ben10Mod`

`Ben10Mod.cs` is the mod entry point.

Responsibilities:

- load shaders and filters
- define packet types
- delegate multiplayer packet handling to `Common/Networking/OmnitrixPacketHandler`
- initialize shared content registries

`HandlePacket` now only delegates to `OmnitrixPacketHandler`. Packet serialization helpers remain in `OmnitrixPacketRouter`, while packet behavior is split by feature family:

- `OmnitrixStatePacketHandler`: transformation unlocks, roster sync, palette sync, speed settings, event participation, Omnitrix evolution sync
- `MaterialAbsorptionPacketHandler`: Osmosian material absorption request/feedback/sync
- `TransformationAbilityPacketHandler`: server-authoritative cursor or movement ability execution
- `GhostFreakPacketHandler`: Ghostfreak possession request/state sync
- `OmnitrixAccessoryPacketHandler`: armor dodge visuals and completed Omnitrix revival sync

### `OmnitrixPlayer`

`OmnitrixPlayer.cs` is the main runtime coordinator, split across partial files while the architecture continues to move responsibility into focused owners.

Responsibilities:

- track the active transformation
- route lifecycle hooks into the current transformation
- bridge Terraria `ModPlayer` hooks to the current transformation
- expose HUD-friendly current-attack and resource data
- coordinate controller/state objects that own Omnitrix subsystems
- preserve compatibility accessors used by older code during the refactor

If you are unsure where a gameplay rule lives, start here to identify the active subsystem, then move to the controller/state class that owns that rule.

### Omnitrix Controllers And State

Runtime Omnitrix state is split across focused classes:

- `OmnitrixEnergyController`: current/max energy, regen, drain, bonus capacity, spending, restoring, clamping
- `TransformationRosterState`: five-slot roster, unlocked transformations, favourites, newly unlocked entries, slot/unlock normalization
- `AbilityController`: F/G/H/U timed ability flags, loaded ability state, originating transformation IDs
- `AttackSelectionController`: badge attack selection, base selection, loaded attack usage, attack pulse/serial state, energy-gain locks
- `TransformationCustomizationStore`: transformation palette, costume, custom-name, preset, normalization, and customization sync state policy
- `TransformationProgressionSystem`: boss/event progression, unlock eligibility, codex unlock text, and event participation
- `PossessionController`: Ghostfreak possession runtime state
- `OmnitrixInputController`: UI open/close input policy
- `MaterialAbsorptionPlayer`: Osmosian/anodite material absorption state and effects

New code should prefer adding policy methods to these owners instead of mutating their raw fields from elsewhere. `OmnitrixPlayer` should act as a facade for UI, save/load, and Terraria hook compatibility when older call sites still expect player methods.

### `Transformation`

`Content/Transformations/Transformation.cs` is the alien API.

A transformation owns:

- ID, name, description, icon, and roster text
- passive effects and movement behavior
- primary and secondary badge attacks
- primary, secondary, tertiary, and ultimate action-slot behavior
- moveset-dependent attack profiles
- attack naming for the HUD
- child and branch transformation behavior

Transformations are the main source of alien-specific gameplay.

### `TransformationLoader`

`Content/Transformations/TransformationLoader.cs` is the shared transformation registry.

The loader is used by:

- roster UI
- transformation activation
- save/load resolution
- unlock lookups
- addon content discovery

### `TransformationHandler`

`Content/TransformationHandler.cs` is the shared state-transition helper.

Use it for:

- transform
- detransform
- unlocks
- cleanup when transformations end or fail

Code should prefer this utility layer over manually editing transformation state fields.

### `Omnitrix`

`Content/Items/Accessories/Omnitrix.cs` is the base Omnitrix API.

It owns:

- max energy
- regen and drain
- timeout versus sustained-energy behavior
- swap costs
- branch and evolution costs
- duration rules
- hand-visual hooks

The important architectural point is that `OmnitrixPlayer` delegates Omnitrix-specific behavior to the equipped Omnitrix instead of hardcoding it.

### `PlumbersBadge`

`Content/Items/Weapons/PlumbersBadge.cs` is the generic badge weapon shell.

It owns:

- base weapon stats and rank identity
- resetting held-item defaults before transformations modify them
- asking the active transformation for the current selected attack profile
- affordability checks
- shared attack energy consumption
- post-fire cleanup for single-use loaded attacks

Alien-specific firing behavior belongs in transformations, not badges.

## Action Slots And Attack Selection

The modern combat model is split into two layers:

- action slots
- badge attack selection

### Action Slots

The public action keys are:

- primary
- secondary
- tertiary
- ultimate

Each slot can be configured by a transformation as either:

- a timed ability
- or a badge attack loader

That means `F`, `G`, `H`, and `U` are not simple buff toggles anymore. A transformation decides what each slot means.

### Badge Attack Selection

The badge uses an attack-selection state machine.

The currently selected attack can be:

- primary
- secondary
- primary ability attack
- secondary ability attack
- tertiary ability attack
- ultimate attack

Key points:

- right click with the badge swaps between the base primary and secondary modes
- loaded action-key attacks temporarily replace the base mode
- some loaded attacks are single-use
- some persist until replaced
- the current selection drives badge stats, projectile choice, energy cost, HUD text, and post-fire cleanup

## Attack Profiles And Movesets

The shared attack model now uses `TransformationAttackProfile`.

Each profile can define:

- display name
- projectile type
- damage multiplier
- use time
- shoot speed
- use style
- channeling and melee rules
- armor penetration
- upfront energy cost
- sustain energy cost
- sustain interval
- whether the attack is single-use

Transformations can supply one or more profiles per slot through methods like:

- `GetPrimaryAttackProfiles()`
- `GetSecondaryAttackProfiles()`
- `GetPrimaryAbilityAttackProfiles()`
- `GetUltimateAttackProfiles()`

`GetMoveSetIndex(...)` picks which profile index is active for the current state.

This is how one transformation can swap entire attack packages cleanly without hardcoding badge logic in several places.

## Shared Combat Helpers

### `OmnitrixProjectile`

`OmnitrixProjectile.cs` is the main shared projectile helper layer.

It currently handles responsibilities such as:

- forcing transformation projectiles onto `HeroDamage`
- Magistrata-style projectile visuals
- special projectile sync and behavior helpers
- some projectile resizing and inherited behavior

### `OmnitrixItem`

`OmnitrixItem.cs` is the shared item-side helper layer.

This is where the mod patches common held-item behavior that needs to feel like a bigger or more transformation-aware player rather than a separate custom weapon system.

### `NpcEffects`

`NpcEffects.cs` is the shared NPC-side status layer.

It owns reusable NPC effects such as:

- freeze-style debuffs
- custom damage-over-time debuffs
- possession-related shared behavior

Transformation-specific NPC logic should not accumulate here unless it truly is reusable or cross-cutting.

### `AlienIdentityGlobalNPC`

`Content/NPCs/AlienIdentityGlobalNPC.cs` is the shared bridge for alien-specific NPC status effects that need per-NPC state.

It is useful because projectiles and transformations can use one consistent lookup:

```csharp
target.GetGlobalNPC<AlienIdentityGlobalNPC>()
```

But it should not become the permanent home for every alien mechanic. New or heavily reworked alien status systems should prefer focused state helpers, for example:

```text
Content/NPCs/States/GhostFreakNpcState.cs
Content/NPCs/States/WaterHazardNpcState.cs
Content/NPCs/States/JetrayNpcState.cs
Content/NPCs/States/HeatBlastNpcState.cs
Content/NPCs/States/RathNpcState.cs
Content/NPCs/States/SnareOhNpcState.cs
Content/NPCs/States/AlienXNpcState.cs
Content/NPCs/States/WhampireNpcState.cs
Content/NPCs/States/PeskyDustNpcState.cs
Content/NPCs/States/FasttrackNpcState.cs
Content/NPCs/States/AstrodactylNpcState.cs
Content/NPCs/States/BlitzwolferNpcState.cs
Content/NPCs/States/EchoEchoNpcState.cs
Content/NPCs/States/FrankenstrikeNpcState.cs
Content/NPCs/States/HumungousaurNpcState.cs
Content/NPCs/States/LodestarNpcState.cs
Content/NPCs/States/BigChillNpcState.cs
Content/NPCs/States/EyeGuyNpcState.cs
```

The global NPC can expose compatibility methods such as `ApplyGhostFreakFear(...)`, `AddWaterHazardSoak(...)`, or `ApplyJetrayLock(...)`, but the actual rule logic should live in the focused state object.

### Armor Runtime

The custom Hero armor behavior lives primarily in:

- `Content/Items/Armour/PlumberArmorSets.cs`
- `Content/Items/Armour/VanillaHeroHelmets.cs`

Those files define:

- custom armor set bonuses
- Hero helmet variants for vanilla sets
- runtime helper `ModPlayer` state for armor set effects

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
5. current transformation state is cleared

### Timed Ability Flow

When an action key is bound to a timed ability:

1. `OmnitrixPlayer` checks slot availability and cooldowns
2. activation cost is checked
3. the relevant ability buff is applied
4. the transformation reads that state through `IsPrimaryAbilityActive`, `IsSecondaryAbilityActive`, and related flags
5. cooldown is applied when the ability ends

### Badge Attack Flow

When the player fires a `Plumber's Badge`:

1. the badge checks whether the player is transformed
2. the active transformation modifies the badge stats based on the current attack selection
3. the shared path checks whether the selected attack can be afforded
4. the shared path consumes upfront attack cost
5. the badge calls `transformation.Shoot(...)`
6. post-fire cleanup handles single-use loaded attacks and selection fallback

### Sustained Attack Cost Flow

Some attacks do not only spend energy once.

Examples include:

- channel beams
- growing ultimates
- other attacks that have an active upkeep cost

Those attacks use the attack profile sustain-cost fields, and the relevant shared projectile base is responsible for spending the repeated cost while the attack is still active.

## UI Systems

### Roster UI

`Content/Interface/AlienSelectionScreen.cs` renders:

- the active five-slot roster
- the unlocked transformation list
- the selected transformation's description and abilities

Because it resolves transformations through the loader, addon transformations appear automatically if they register correctly and are unlocked.

### Omnitrix Energy Bar

The UI system also draws the Omnitrix energy bar.

It reads directly from `OmnitrixPlayer`:

- Omnitrix equipped state
- current energy
- current max energy

### Current Attack HUD

The same UI system draws a current-attack panel under the energy bar.

It shows:

- the active attack slot label
- the current attack display name
- the per-shot energy cost
- a short pulse when the attack selection changes

That HUD is driven from `OmnitrixPlayer` plus transformation-supplied attack names or moveset profile names.

## Progression Systems

### Boss And Event Unlocks

Progression is driven primarily through:

- `bossTrackerNPC.cs`
- `TransformationProgressionSystem`
- thin facade methods in `OmnitrixPlayer.Progression.cs`

The important pattern is that unlocks are usually data-driven against boss and event completion, not hand-waved through item acquisition alone.

### Roster Assignment Versus Unlocks

Unlocking a transformation and assigning it to the active five-slot roster are separate steps.

That is why progression bugs can appear in two distinct places:

- the player owns the transformation but has not assigned it
- the player has assigned a slot, but the unlock data did not persist or load correctly

## Multiplayer Rules

Ben10Mod now has several systems that are multiplayer-sensitive.

When adding or changing gameplay, keep these rules in mind:

- mouse-targeted actions should be owner-driven and synced
- server-authoritative state is required for teleports, possession, and similar stateful movement
- custom projectile aim that depends on the mouse should use synced extra AI or packets
- NPC-side effects should not depend on `Main.LocalPlayer`
- owner-only spawns are usually correct for cursor-placed attacks and sentries

If a feature behaves correctly in singleplayer but strangely in multiplayer, these are the first rules to check.

## Practical Debugging Tips

If a keyed attack does nothing:

- confirm whether the slot is an ability or a badge attack loader
- check the current attack HUD
- check the transformation's active moveset index
- check the selected attack profile for projectile type and energy cost

If the HUD text is wrong:

- check the transformation's `...AttackName` properties
- check moveset profile `DisplayName` overrides

If an attack behaves differently online:

- check owner-only spawn guards
- check packet paths or `SendExtraAI/ReceiveExtraAI`
- check whether NPC or projectile logic is reading local-only state
