# Steam Achievement Manager

<p align="center">
  <img src="./resources/SAM_logo_light_blue.svg">
</p>

## Overview

The Steam Achievement Manager lets you lock/unlock achievements for any currently supported app. Some games have achievements that are no longer reasonably, or actually, attainable. While SAM can be used to abuse the achiement system, it also lets people who do care about achievements unlock achievements that would otherwise be impossible. One great example is achievements requiring you to play multiplayer for a game that no longer has any active players. SAM is a potential solution for a game's poorly designed achievements.

This project is a fork of the [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager) project and update all of the old .NET Framework WinForms projects and leverage .NET Core (and WPF). This is very much a work in progress.

## Project Structure

The reason there's two seperate apps (the picker and the manager) is because the manager needs to be in its own process to be able to actually manage the achievements and stats. When the manager is started, it's passed an ID a Steam app. The manager then initializes the Steam API client with that app's ID just like any game on Steam would. Steam will show you as in-game, record your play time, and you could even earn trading cards (if you have drops left).

| Legacy Project | New Project | Info |
|:---------------|:------------|:-----|
| SAM.Picker     | SAM.WPF     | This is the main executable used to select a game (or app) from your library. |
| SAM.Game       | SAM.WPF.Manager | Handles the viewing and updating of an app's achievements and stats. |
| SAM.API        | SAM.WPF.Core | Contains the core functionality of SAM that both apps reference. |

_Note: The new 'SAM.WPF' project names are just temporary and will be updated later on._

## Building SAM.WPF

When building and running the app be sure to use the `x86` solution configuraiton to target 32-bit. The legacy projects do not support 64-bit and since they are being removed anyways it's better to just wait and implement the 64-bit Steam API then. `SAM.API\Steam` only calls `LoadLibrary` on the 32-bit `steamclient.dll`.  

```
path = Path.Combine(path, "steamclient.dll");
IntPtr module = Native.LoadLibraryEx(path, IntPtr.Zero, Native.LoadWithAlteredSearchPath);
```

## TODO

### General

- Logging
  - Add appender for events with warning or higher severity
- Settings
  - Create settings view
  - Ability to import/export
  - Automatically save and restore on startup
- Themeing
  - Add default light and dark themes
  - Add ability to set the theme's accent color
  - Add support for custom themes

### Picker

- Views
  - Add grid view for library
  - Add tile view for library
  - Add ability to filter apps in the library view

### Manager

- Views
  - Add ability to search and filter achievements grid
  - Add stats view
  - Add confirmation screen of changes before save

### Core

- General
  - Remove dependency on SAM.Picker, SAM.Game, and SAM.API
    - Research alternative client for the Steam API
- Stats
  - Add ability to manage stats
- Steamworks Manager
  - Add ability to include any DLC info when getting app info
- Isolated Storage Manager
  - Add expiration for cached resources to update

---

## Resources

- [Devexpress MVVM](https://github.com/DevExpress/DevExpress.Mvvm.Free)
  - [Documentation](https://docs.devexpress.com/WPF/15112/mvvm-framework)
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
  - [Documentation](https://mahapps.com/docs/)
- [Font-Awesome-WPF](https://github.com/charri/Font-Awesome-WPF)
  - [Documentation](https://github.com/charri/Font-Awesome-WPF/blob/master/README-WPF.md)
- [Steamworks API Overview](https://partner.steamgames.com/doc/sdk/api)
  - [Steamworks API](https://partner.steamgames.com/doc/api)
  - [Steamworks Web API](https://partner.steamgames.com/doc/webapi)

## Links


- [SAM (Old)](https://github.com/gibbed/SteamAchievementManager)
- [SAM (Old) Latest Release](https://github.com/gibbed/SteamAchievementManager/releases/latest)
