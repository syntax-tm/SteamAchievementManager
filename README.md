<div align="center">
  <a href="https://github.com/syntax-tm/SteamAchievementManager/stargazers" aria-label="Star syntax-tm/SteamAchievementManager on GitHub">
    <img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/syntax-tm/SteamAchievementManager">
  </a>
  <a href="https://github.com/syntax-tm/SteamAchievementManager/actions/workflows/build.yml">
    <img src="https://github.com/syntax-tm/SteamAchievementManager/actions/workflows/build.yml/badge.svg">
  </a>
  <a href="https://github.com/syntax-tm/SteamAchievementManager/releases/latest">
    <img alt="GitHub release (with filter)" src="https://img.shields.io/github/v/release/syntax-tm/SteamAchievementManager">
  </a>
</div>

<p align="center">
  <img src="./resources/SAM_logo_light_blue.svg" style="max-width: 600px; width: 70%;">
</p>

## Overview

The Steam Achievement Manager lets you manage achievements, stats, and more for any currently supported app.

<p align="center">
  <a href="./resources/sam_screenshot.png">
    <img src="./resources/sam_screenshot.png" />
  </a>
</p>

This project is a fork of the [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager) project with the goal of updating to .NET Core and WPF. This is very much a work in progress.

## Project Structure

**SAM** is comprised of five projects.

![](./resources/SAM_projects_white.svg#gh-dark-mode-only)
![](./resources/SAM_projects_black.svg#gh-light-mode-only)

`SAM` and `SAM.Manager` are the two application projects that create an executable. `SAM` is the executable that will display your Steam library and let you select an app (game) to manage. When the manager is started, it's passed an ID for a Steam app after which it initializes the Steam API client with that AppID just like any game on Steam would. Steam will show you as in-game, record your play time, and you can earn trading cards (if you have drops left).

The reason why there's two executables is because the manager (`SAM.Manager`) needs to be in its own process anyways in order to actually manage the achievements and stats. Once a process is registered with the Steam client it's not possible to re-initialize the Client with a different AppID. Steam will also treat you as in-game until that Process and any child Processes have exited.

Both `SAM.API` and `SAM.Core` are libraries (`*.dll`). As its name suggests, `SAM.API` contains the native Steam API wrappers. `SAM.Core` is referenced by both executable projects (more on these later) and allows them to both use the same types, resources, etc. without having to duplicate code.

<div align="center">

```mermaid
%%{init: {"flowchart": {"htmlLabels": false, "width": "100%"}} }%%
flowchart TB
    subgraph Applicaitons
        S["SAM"]:::app
        SM["SAM.Manager"]:::app
    end
    subgraph Unit Tests
      SU["SAM.UnitTests"]:::unitTests
    end
    SC{{"SAM.Core"}}:::library
    SA{{"SAM.API"}}:::library
    S --> SC
    SM --> SC
    SC --> SA
    SU --> SC
    SU --> SA
    classDef app fill:#247FD4,stroke:#bbb,stroke-width:1px,color:#fff
    classDef library fill:#D63E48,stroke:#bbb,stroke-width:1px,color:#fff
    classDef unitTests fill:#9478F0,stroke:#bbb,stroke-width:1px,color:#fff
```

</div>

<table align="center">
    <tr>
        <th align="center">Legacy Project</th>
        <th>New Project</th>
        <th style="width: 80%;">Description</th>
    </tr>
    <tr>
        <td align="center"><b>SAM.Picker</b></td>
        <td align="center"><b>SAM</b></td>
        <td>The main executable used to select a game (or app) from your library</td>
    </tr>
    <tr>
        <td align="center"><b>SAM.Game</b></td>
        <td align="center"><b>SAM.Manager</b></td>
        <td>Allows for viewing and editing an app's achievements and stats</td>
    </tr>
    <tr>
        <td align="center"><b>SAM.API</b></td>
        <td align="center"><b>SAM.API</b></td>
        <td>Managed Steam API wrappers</td>
    </tr>
    <tr>
        <td align="center">-</th>
        <td align="center"><b>SAM.Core</b></td>
        <td>Common resources used by both <code>SAM</code> and <code>SAM.Manager</code>.</td>
    </tr>
</table>

---

## FAQ

### What is an App or App ID?

> An Application (or App) is the main representation of a product on Steam. An App generally has its own store page, it's own Community Hub, and is what appears in customers' libraries. Each App is represented by a unique ID called an App ID - that you'll see referenced throughout this documentation and used with the Steamworks API and Steamworks Web API. Generally a single product will not span multiple Applications. ([source](https://partner.steamgames.com/doc/store/application))

### Why does SAM let people cheat achievements?

Some games have achievements that are no longer reasonably or actually attainable. While SAM _can_ be used to abuse the achievement system, it also lets people who do care about achievements unlock achievements that would otherwise be impossible. One common example is achievements requiring you to play multiplayer on a game that no longer has any active players, or even dedicated servers. **SAM** is a potential solution for a game's poorly designed achievements.

---

## Acknowledgements

- [Devexpress MVVM](https://github.com/DevExpress/DevExpress.Mvvm.Free)
- [Font-Awesome-WPF](https://github.com/charri/Font-Awesome-WPF)
- [SteamCountries](https://github.com/RudeySH/SteamCountries)
- [WPF UI](https://github.com/lepoco/wpfui)

---

## Resources

- [DevExpress MVVM](https://docs.devexpress.com/WPF/15112/mvvm-framework)
- [Font-Awesome-WPF Documentation](https://github.com/charri/Font-Awesome-WPF/blob/master/README-WPF.md)
- [Steamworks API Overview](https://partner.steamgames.com/doc/sdk/api)
  - [Steamworks API](https://partner.steamgames.com/doc/api)
  - [Steamworks Web API](https://partner.steamgames.com/doc/webapi)
