# Ben10Mod

Ben10Mod is a tModLoader mod that adds Ben 10 style transformation gameplay to Terraria.

From a developer point of view, the mod revolves around three main gameplay systems:

- `Transformation`: defines alien forms, passive effects, abilities, badge attacks, visuals, and metadata
- `Omnitrix`: defines Omnitrix-specific resource rules, transformation timing, evolution, and hand visuals
- `PlumbersBadge`: defines the weapon shell used to fire a transformation's attacks

The codebase is now structured so that those systems can be extended by addon mods without hardcoding individual content types into the player logic.

## Documentation

Start here depending on what you are trying to do:

- [Player Guide](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/PLAYER_GUIDE.md)
- [Architecture Guide](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/docs/Architecture.md)
- [Development Guide](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/docs/Development.md)
- [Addon API Guide](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/docs/AddonAPI.md)

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

If you are trying to understand the gameplay loop, read these in order:

1. [OmnitrixPlayer.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/OmnitrixPlayer.cs)
2. [Transformation.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Transformations/Transformation.cs)
3. [Omnitrix.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Items/Accessories/Omnitrix.cs)
4. [PlumbersBadge.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Items/Weapons/PlumbersBadge.cs)
5. [TransformationHandler.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/TransformationHandler.cs)

If you are trying to build an addon, start with:

1. [Addon API Guide](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/docs/AddonAPI.md)
2. one built-in transformation implementation such as:
   [HeatBlastTransformation.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Transformations/HeatBlast/HeatBlastTransformation.cs)
3. one built-in Omnitrix implementation such as:
   [PrototypeOmnitrix.cs](/Users/ethanhellyer/Library/Application%20Support/Terraria/tModLoader/ModSources/Ben10Mod/Content/Items/Accessories/PrototypeOmnitrix.cs)

## Current State

This mod is still under active development. Some systems are cleanly generalized already, while other areas still reflect the mod's historical evolution.

The main extension APIs are intentionally stable:

- transformations are identified by string IDs
- transformations auto-register into the shared loader
- badges are transformation-driven
- Omnitrixes now use a shared base API instead of player-side hardcoding

## Credits

- Author: DestroyerMob
- Spriter: Eye of Rage
- Discord: [https://discord.gg/XD7qN7DbdD](https://discord.gg/XD7qN7DbdD)
