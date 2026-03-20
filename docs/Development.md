# Ben10Mod Development Guide

This guide is for contributors working inside the Ben10Mod source itself.

It covers:

- local project setup
- build expectations
- the current gameplay model
- practical conventions for adding or changing content

## Project Setup

Ben10Mod is a normal tModLoader source mod project.

Important files:

- `Ben10Mod.csproj`
- `build.txt`
- `Ben10Mod.cs`

The project expects to live inside a standard tModLoader `ModSources` folder.

## Building

Typical command:

```bash
dotnet build Ben10Mod.csproj
```

That command performs two separate steps:

1. compile the C# project
2. run tModLoader's packaging/build tooling

When diagnosing build issues, treat those as separate layers.

### Known Environment Issue: `FNA3D`

On some machines, the final tModLoader packaging step fails because the local tModLoader install is missing the required `FNA3D` native library.

Important distinction:

- if the C# compile succeeds, your code changes may still be valid
- the later packaging failure can still be an environment problem

## Current Gameplay Model

The modern runtime has three main extension surfaces:

- `Transformation`
- `Omnitrix`
- `PlumbersBadge`

And one central state owner:

- `OmnitrixPlayer`

### Current Combat Model

The badge system is no longer just primary, alternate, and ultimate.

Current rules:

- the badge has a shared attack-selection state machine
- base combat starts from primary or secondary fire
- right click swaps those base modes
- `F`, `G`, `H`, and `U` can either:
  - activate timed abilities
  - or load badge attacks
- loaded badge attacks temporarily replace the base attack selection
- attack costs are attached to attack profiles
- affordability is checked in `PlumbersBadge.CanUseItem`
- attack energy is spent in the shared `PlumbersBadge.Shoot` path

If you are changing combat behavior, read:

- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)
- [Transformation.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Transformations/Transformation.cs)
- [PlumbersBadge.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Items/Weapons/PlumbersBadge.cs)

## Common Contributor Workflows

### Adding A New Built-In Transformation

Typical checklist:

1. create `Content/Transformations/<AlienName>/`
2. create the transformation class
3. create the transformation buff
4. add costume/equip assets if needed
5. add projectiles, helper items, or custom visuals
6. define the badge attacks and action-slot behavior
7. add unlock logic through boss, event, or item progression
8. verify the transformation appears in the roster UI

### Adding A New Built-In Omnitrix

Typical checklist:

1. subclass `Omnitrix`
2. set energy, drain, regen, and timing rules
3. load and register hand textures if needed
4. implement recipes or unlock logic
5. implement evolution rules if it upgrades into something else

### Adding A New Badge

Typical checklist:

1. subclass `PlumbersBadge`
2. set base damage and rank metadata
3. add recipes
4. verify the currently selected transformation exposes the attacks you expect

Most alien-specific weapon behavior should still stay in the transformation, not the badge item.

## Practical Code Conventions

### Use The Shared Systems

Prefer:

- `TransformationHandler.Transform(...)`
- `TransformationHandler.Detransform(...)`
- `TransformationHandler.AddTransformation(...)`

instead of hand-editing transformation state fields.

### Keep Behavior With Its Owner

Put code in the system that owns it:

- alien behavior belongs in `Transformation`
- Omnitrix resource rules belong in `Omnitrix`
- shared weapon-shell behavior belongs in `PlumbersBadge`
- cross-cutting player runtime state belongs in `OmnitrixPlayer`

### Avoid New Hardcoded Content Checks

Prefer:

- virtual properties
- virtual methods
- registries
- loader lookups

Avoid adding fresh content-specific checks unless there is a strong reason.

### Use Stable Transformation IDs

Save data, UI, unlocks, and addon integration all rely on transformation string IDs.

Do not casually rename shipped IDs.

### Keep HUD Text In The Transformation Layer

The current-attack HUD now reads attack labels from the transformation.

If a transformation has a custom name you want shown in the HUD, use:

- `PrimaryAttackDisplayName`
- `SecondaryAttackDisplayName`
- `PrimaryAbilityAttackDisplayName`
- `SecondaryAbilityAttackDisplayName`
- `TertiaryAbilityAttackDisplayName`
- `UltimateAttackDisplayName`

## Important Files To Read Before Bigger Changes

If you are touching transformation gameplay:

- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)
- [Transformation.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Transformations/Transformation.cs)
- [TransformationHandler.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/TransformationHandler.cs)

If you are touching UI:

- [AlienSelectionScreen.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Interface/AlienSelectionScreen.cs)
- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)

If you are touching progression:

- [bossTrackerNPC.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/bossTrackerNPC.cs)
- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)

## Troubleshooting

### A Transformation Unlocks But Cannot Be Used

Check:

- the transformation buff exists
- `TransformationBuffId` is valid
- the transformation is assigned to a roster slot
- an Omnitrix is equipped
- the player is not blocked by cooldown or missing energy

### A Keyed Attack Does Nothing

Check:

- whether that slot loads a badge attack instead of acting instantly
- whether a `Plumber's Badge` is equipped in-hand
- whether the current attack HUD shows the expected loaded mode
- whether the attack profile has a projectile and enough energy

### The HUD Shows A Bad Attack Name

Check:

- the transformation's attack display-name properties
- whether the transformation is falling back to projectile display names

### Build Works On One Machine But Not Another

Check:

- that both machines have the same tModLoader install structure
- `dotnet restore`
- IDE cache state
- the local `FNA3D` runtime files in the tModLoader install

## Documentation Rule Of Thumb

If you change:

- controls
- action-slot behavior
- the badge attack model
- progression unlocks
- player-facing Omnitrix progression

update the docs in the same pass. The player guide, architecture guide, and addon guide should all describe the same current system.
