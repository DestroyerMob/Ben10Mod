# Ben10Mod

Ben10Mod is a tModLoader mod that adds Ben 10 style transformation gameplay to Terraria.

From a developer point of view, the mod revolves around three main gameplay systems:

- `Transformation`: defines alien forms, passive effects, abilities, badge attacks, visuals, and metadata
- `Omnitrix`: defines Omnitrix-specific resource rules, transformation timing, evolution, and hand visuals
- `PlumbersBadge`: defines the weapon shell used to fire a transformation's attacks

The codebase is now structured so that those systems can be extended by addon mods without hardcoding individual content types into the player logic.

## Documentation

Start here depending on what you are trying to do:

- [Player Guide](docs/PLAYER_GUIDE.md)
- [Architecture Guide](docs/Architecture.md)
- [Development Guide](docs/Development.md)
- [Addon API Guide](docs/AddonAPI.md)

## Repo Overview

Important files and folders:

- `Ben10Mod.cs`
  Root mod entry point. Loads shaders and handles network packets.
- `OmnitrixPlayer.cs`
  Main state container for Omnitrix gameplay, transformation state, abilities, energy, visuals, and unlock tracking.
- `Content/Transformations/`
  Base transformation API plus all built-in transformation content.
- `Content/Items/Accessories/Omnitrix.cs`
  Base Omnitrix API.
- `Content/Items/Weapons/PlumbersBadge.cs`
  Base Plumber's Badge API.
- `Content/TransformationHandler.cs`
  Shared helper methods for transforming, detransforming, and unlocking transformations.
- `Content/Interface/`
  Omnitrix UI, custom slot UI, and roster UI.
- `Keybinds/KeybindSystem.cs`
  Registers all keybinds used by the mod.
- `bossTrackerNPC.cs`
  Boss and event progression unlock logic.

## Quick Start For Developers

If you are trying to understand the gameplay loop, read these:

1. [OmnitrixPlayer.cs](OmnitrixPlayer.cs)
2. [Transformation.cs](Content/Transformations/Transformation.cs)
3. [Omnitrix.cs](Content/Items/Accessories/Omnitrix.cs)
4. [PlumbersBadge.cs](Content/Items/Weapons/PlumbersBadge.cs)
5. [TransformationHandler.cs](Content/TransformationHandler.cs)

If you are trying to build an addon, start with:

1. [Addon API Guide](docs/AddonAPI.md)
2. one built-in transformation implementation such as:
   [HeatBlastTransformation.cs](Content/Transformations/HeatBlast/HeatBlastTransformation.cs)
3. one built-in Omnitrix implementation such as:
   [PrototypeOmnitrix.cs](Content/Items/Accessories/PrototypeOmnitrix.cs)

## Current State

This mod is still under active development. Some systems are cleanly generalized already, while other areas still reflect the mod's historical evolution.

The main extension APIs are intentionally stable:

- transformations are identified by string IDs
- transformations auto-register into the shared loader
- badges are transformation-driven
- Omnitrixes now use a shared base API instead of player-side hardcoding
- Osmosian material absorption can be extended through the addon API and `Mod.Call`

## Credits

- Author: DestroyerMob
- Spriter: Eye of Rage
- Discord: [https://discord.gg/XD7qN7DbdD](https://discord.gg/XD7qN7DbdD)
