# Ben10Mod Development Guide

This guide is for contributors working inside the Ben10Mod source itself.

It covers:

- local project setup
- build expectations
- the current gameplay model
- practical rules for adding or changing content

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

For a compile-only check, this is usually the fastest pass:

```bash
dotnet build Ben10Mod.csproj /t:Compile /p:UseSharedCompilation=false
```

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

- the badge uses a shared attack-selection state machine
- base combat starts from primary or secondary fire
- right click swaps those base modes and backs out of loaded special attacks
- `F`, `G`, `H`, and `U` can each either activate a timed ability or load a badge attack
- loaded badge attacks temporarily replace the base attack selection
- attack costs are attached to attack profiles
- sustain costs are attached to attack profiles too
- affordability is checked in the shared badge path
- attack energy is spent in the shared badge fire path, not ad hoc inside most transformations

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
4. add projectiles, helper items, and visuals
5. define attack names and attack profiles
6. define action-slot behavior for `F`, `G`, `H`, and `U`
7. add unlock logic through boss, event, or item progression
8. verify the transformation appears in the roster UI and the attack HUD

### Adding Moveset-Based Attack Variants

Use movesets when one transformation needs different attack packages in different states.

Current shared pattern:

1. override `GetMoveSetIndex(OmnitrixPlayer omp)`
2. return one or more `TransformationAttackProfile` entries from `GetPrimaryAttackProfiles()` or the equivalent slot method
3. use per-profile `DisplayName` when the attack name should change by state

This is the preferred replacement for hardcoding badge stat swaps in several manual branches.

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

### Adding Or Changing Armor

Hero armor content currently lives in two places:

- custom Plumber sets in `Content/Items/Armour/PlumberArmorSets.cs`
- vanilla Hero helmets in `Content/Items/Armour/VanillaHeroHelmets.cs`

If a set bonus has runtime logic, keep the state and effect code close to the armor system that owns it instead of scattering it through unrelated gameplay files.

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
- reusable projectile helpers belong in `OmnitrixProjectile`
- reusable NPC status behavior belongs in `NpcEffects`

### Avoid New Hardcoded Content Checks

Prefer:

- virtual properties
- virtual methods
- registries
- loader lookups
- profile selection through `GetMoveSetIndex(...)`

Avoid adding fresh transformation-specific checks unless there is a strong reason.

### Use Stable Transformation IDs

Save data, UI, unlocks, and addon integration all rely on transformation string IDs.

Do not casually rename shipped IDs.

### Name Attacks In The Transformation Layer

The current-attack HUD reads names from the transformation API.

Use:

- `PrimaryAttackName`
- `SecondaryAttackName`
- `PrimaryAbilityAttackName`
- `SecondaryAbilityAttackName`
- `TertiaryAbilityAttackName`
- `UltimateAttackName`

If a name changes by moveset, use the attack profile `DisplayName`.

The active-ability HUD reads timed ability names from:

- `PrimaryAbilityName`
- `SecondaryAbilityName`
- `TertiaryAbilityName`
- `UltimateAbilityName`

If a timed ability does not define one yet, the HUD falls back to the slot label.

### Use HeroDamage For Transformation Combat

Transformation combat should feel like one class.

That means:

- transformation projectiles should use `HeroDamage`
- child or spawned projectiles should inherit the same class behavior
- passive stat bonuses meant for transformation combat should affect `HeroDamage`

## Multiplayer Rules

Many recent systems are multiplayer-sensitive. When changing gameplay:

- do not build NPC logic around `Main.LocalPlayer`
- mouse-targeted attacks should be owner-driven and synced
- teleports, possession, and similar stateful movement should be server-authoritative
- cursor-placed attacks and sentries usually need owner-only spawn guards
- custom projectile aim often needs `SendExtraAI/ReceiveExtraAI`

If you are writing gameplay that depends on the local mouse or local player state, stop and decide what the server and remote clients need to know.

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

If you are touching multiplayer-sensitive combat:

- [Ben10Mod.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Ben10Mod.cs)
- [OmnitrixProjectile.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixProjectile.cs)
- [NpcEffects.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/NpcEffects.cs)

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
- whether the selected attack profile has a projectile and enough energy

### The HUD Shows A Bad Attack Name

Check:

- the transformation's `...AttackName` properties
- the active moveset index
- the active profile `DisplayName`

### Singleplayer Works But Multiplayer Does Not

Check:

- whether the spawn should be owner-only
- whether the state change should happen on the server
- whether any code path depends on `Main.MouseWorld` or `Main.LocalPlayer`
- whether custom projectile state needs `SendExtraAI/ReceiveExtraAI`

### Build Works On One Machine But Not Another

Check:

- that both machines use the expected tModLoader data folders
- `dotnet restore`
- IDE cache state
- the local `FNA3D` runtime files in the tModLoader install

## Documentation Rule Of Thumb

If you change:

- controls
- action-slot behavior
- the badge attack model
- moveset selection
- progression unlocks
- player-facing Omnitrix progression
- armor set behavior

update the docs in the same pass. The player guide, architecture guide, and addon guide should all describe the same current system.
