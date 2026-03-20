# Ben10Mod

Ben10Mod is a tModLoader mod that adds Ben 10 style transformation gameplay to Terraria.

At a high level:

- equip an `Omnitrix` in the custom Omnitrix slot
- unlock transformations through bosses, events, and progression
- transform into aliens with their own passives, abilities, and badge attacks
- use a `Plumber's Badge` as the shared weapon shell for alien attacks

The codebase is structured so built-in content and addon content both plug into the same shared runtime.

## Documentation

Use the doc set that matches what you are trying to do:

- [Player Guide](docs/PLAYER_GUIDE.md)
- [Architecture Guide](docs/Architecture.md)
- [Development Guide](docs/Development.md)
- [Addon API Guide](docs/AddonAPI.md)

## Current Gameplay Model

These are the important current rules:

- `Transformation` owns alien-specific behavior: passives, visuals, attacks, abilities, and metadata.
- `Omnitrix` owns transformation timing, energy rules, swap costs, evolution, and hand visuals.
- `PlumbersBadge` is a generic weapon shell that asks the current transformation how to behave.
- `F`, `G`, `H`, and `U` are action keys.
- depending on the transformation and slot, an action key can either:
  - activate a timed ability
  - or load a badge attack
- badge attacks require the `Plumber's Badge` to actually fire
- timed abilities do not
- right click while holding a badge swaps between the base primary and secondary badge attacks
- the current loaded attack and its energy cost are shown under the Omnitrix energy bar

## Repository Map

Important files and folders:

- `Ben10Mod.cs`
  Root mod entry point. Loads shaders and handles packets.
- `OmnitrixPlayer.cs`
  Central player-side state for transformations, energy, ability flags, attack selection, progression, and UI-facing state.
- `Content/Transformations/`
  Base transformation API plus built-in transformations.
- `Content/Items/Accessories/Omnitrix.cs`
  Base Omnitrix API.
- `Content/Items/Weapons/PlumbersBadge.cs`
  Base badge API and shared attack fire path.
- `Content/TransformationHandler.cs`
  Shared helper methods for transforming, detransforming, and unlocks.
- `Content/Interface/`
  Roster UI, energy bar, and current-attack HUD.
- `Keybinds/KeybindSystem.cs`
  All registered keybinds.
- `bossTrackerNPC.cs`
  Boss-based unlock progression.

## Quick Start For Contributors

If you want to understand the runtime flow, start here:

1. [OmnitrixPlayer.cs](OmnitrixPlayer.cs)
2. [Transformation.cs](Content/Transformations/Transformation.cs)
3. [Omnitrix.cs](Content/Items/Accessories/Omnitrix.cs)
4. [PlumbersBadge.cs](Content/Items/Weapons/PlumbersBadge.cs)
5. [TransformationHandler.cs](Content/TransformationHandler.cs)

If you want to build an addon, start here:

1. [Addon API Guide](docs/AddonAPI.md)
2. a built-in transformation such as [HeatBlastTransformation.cs](Content/Transformations/HeatBlast/HeatBlastTransformation.cs)
3. a built-in Omnitrix such as [PrototypeOmnitrix.cs](Content/Items/Accessories/PrototypeOmnitrix.cs)

## Build Note

`dotnet build Ben10Mod.csproj` does two things:

1. compiles the C# project
2. runs tModLoader's packaging/build step

If the C# compile succeeds but the final packaging step fails with an `FNA3D` error, that is usually a local tModLoader environment problem rather than a gameplay-code error.

## Current State

Ben10Mod is still in active development. The shared transformation, Omnitrix, badge, and addon-extension systems are now much more generalized than the early versions of the project, but content balance and presentation are still evolving.

## Credits

- Author: DestroyerMob
- Spriter: Eye of Rage
- Discord: [https://discord.gg/XD7qN7DbdD](https://discord.gg/XD7qN7DbdD)
