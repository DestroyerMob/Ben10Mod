# Ben10Mod Development Guide

This guide is for people working inside the Ben10Mod source itself.

It covers:

- project setup
- build expectations
- common editor issues
- debugging tips
- conventions for adding new code

## Project Setup

Ben10Mod is a tModLoader mod source project.

Important files:

- `Ben10Mod.csproj`
- `build.txt`
- `Properties/launchSettings.json`

The project imports `..\tModLoader.targets`, so it expects to live inside a normal tModLoader `ModSources` folder.

## Building

Typical command:

```bash
dotnet build Ben10Mod.csproj
```

This performs two distinct steps:

1. compile the C# project
2. invoke tModLoader's mod packaging/build tooling

If the C# compile succeeds but the final mod build fails, the failure may be environmental rather than code-related.

## Known Environment Gotcha: `FNA3D`

On some machines, especially with direct command-line builds, the final tModLoader packaging step may fail with an `FNA3D` native library error.

What that means:

- the C# code may already have compiled correctly
- tModLoader's build tool could not load a required native runtime library from the local tModLoader installation

This is usually an environment/setup problem, not a gameplay code problem.

When diagnosing build issues, separate:

- C# compiler errors
- tModLoader packaging/runtime environment errors

## Rider / IDE Notes

If Rider or another IDE shows false errors even though the project builds:

- reload all projects
- invalidate caches
- delete `bin` and `obj`
- run `dotnet restore`

For addon projects that reference Ben10Mod, Rider may show false red squiggles when its design-time restore gets stale even though command-line builds succeed.

## Common Contributor Workflows

### Adding a new built-in transformation

Typical checklist:

1. add a new folder in `Content/Transformations/<AlienName>/`
2. create the transformation class
3. create the transformation buff
4. create the costume/equip item if needed
5. add textures
6. add projectiles and helper items
7. add unlock logic through boss, event, or item progression
8. verify it appears in the roster UI and transforms correctly

### Adding a new built-in Omnitrix

Typical checklist:

1. subclass `Omnitrix`
2. set energy and timing properties
3. implement hands-on texture loading
4. implement inventory drawing if you need visual state changes
5. add recipes or unlock logic
6. if it branches into child forms, override the child transformation hooks and Omnitrix capability hooks

### Adding a new badge

Typical checklist:

1. subclass `PlumbersBadge`
2. set base damage and rank metadata
3. add a recipe
4. ensure current transformations expose the attacks you expect the badge to fire

## Code Conventions

These are the practical conventions to follow in this repo.

### Use the shared systems

Prefer using:

- `TransformationHandler.Transform`
- `TransformationHandler.Detransform`
- `TransformationHandler.AddTransformation`

instead of duplicating transformation state transitions by hand.

### Keep behavior close to ownership

Put code in the system that owns it:

- alien behavior belongs in `Transformation`
- Omnitrix resource behavior belongs in `Omnitrix`
- generic weapon shell behavior belongs in `PlumbersBadge`

### Avoid new hardcoded content checks

The mod has historically used some concrete type checks. The current direction is to move away from those.

If you are adding something new, prefer:

- virtual methods
- overridable properties
- registry lookups

over:

- `if item is SpecificOmnitrix`
- `if transformation name == "SomeAlien"`

unless there is a strong reason to keep that logic explicit.

### Use stable string IDs for transformations

Save data and multiplayer sync use transformation IDs directly.

Avoid renaming IDs once shipped.

## Important Systems To Read Before Making Larger Changes

If you are touching transformation gameplay:

- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)
- [Transformation.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Transformations/Transformation.cs)
- [TransformationHandler.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/TransformationHandler.cs)

If you are touching Omnitrix behavior:

- [Omnitrix.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Items/Accessories/Omnitrix.cs)
- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)
- [OmnitrixSlot.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Interface/OmnitrixSlot.cs)

If you are touching combat routing:

- [PlumbersBadge.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Items/Weapons/PlumbersBadge.cs)
- current transformation implementations

If you are touching progression:

- [bossTrackerNPC.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/bossTrackerNPC.cs)
- [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)

## Troubleshooting

### Transformation unlocks but cannot transform

Check:

- the transformation buff exists
- `TransformationBuffId` is valid
- the roster slot contains the exact transformation ID
- the Omnitrix is equipped and not blocked by cooldown

### Transformation exists but does not show in the roster UI

Check:

- `IconPath` exists
- the transformation was unlocked
- the transformation class loads without type errors

### Omnitrix shows but has wrong visuals

Check:

- `Load()` registered the expected hands-on textures
- `HandsOnTextureKey`, `CooldownHandsOnTextureKey`, and `UpdatingHandsOnTextureKey` match the registration names

### Build works on one machine but not another

Check:

- both machines have the same tModLoader installation structure
- `dotnet restore` has been run
- IDE caches are clean
- the local tModLoader runtime libraries exist

## Suggested Future Documentation Targets

Areas that may deserve their own dedicated docs later:

- art and asset pipeline
- shader setup
- boss/event unlock balancing rules
- multiplayer sync model beyond transformation unlocks
