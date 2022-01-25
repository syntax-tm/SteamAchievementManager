# Steam Achievement Manager

<p align="center">
  <img src="./resources/SAM_logo_light_blue.svg">
</p>

## Overview

The Steam Achievement Manager lets you lock/unlock achievements for any currently supported app. Some games have achievements that are no longer reasonably, or actually, attainable. While SAM can be used to abuse the achiement system, it also lets people who do care about achievements unlock achievements that would otherwise be impossible. One great example is achievements requiring you to play multiplayer for a game that no longer has any active players. SAM is a potential solution for a game's poorly designed achievements.

This project is a fork of the [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager) project and update all of the old .NET Framework WinForms projects and leverage .NET Core (and WPF). This is very much a work in progress.

## Project Structure

The reason there's two seperate apps (the picker and the manager) is because the manager needs to be in its own process to be able to actually manage the achievements and stats. When the manager is started, it's passed an ID a Steam app. The manager then initializes the Steam API client with that app's ID just like any game on Steam would. Steam will show you as in-game, record your play time, and you could even earn trading cards (if you have drops left).

<table>
    <tr>
        <th>Legacy Project</th>
        <th>New Project</th>
        <th>Description</th>
    </tr>
    <tr>
        <th>SAM.Picker</th>
        <th>SAM</th>
        <td>This is the main executable used to select a game (or app) from your library.</td>
    </tr>
    <tr>
        <th>SAM.Game</th>
        <th>SAM.Manager</th>
        <td>Handles the viewing and updating of an app's achievements and stats.</td>
    </tr>
    <tr>
        <th>SAM.API</th>
        <th>SAM.API</th>
        <td>Managed Steam API wrappers.</td>
    </tr>
    <tr>
        <th> </th>
        <th>SAM.Core</th>
        <td> shared resources used by both the <code>SAM</code> and <code>SAM.Manager</code> projects.</td>
    </tr>
</table>

## TODO

### General

- Steam
  - Prompt to start Steam when process is not currently running
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
