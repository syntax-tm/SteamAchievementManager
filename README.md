<h1>
  <div align="center">
    <img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/syntax-tm/SteamAchievementManager">
    <img alt="Build Badge" src="https://github.com/syntax-tm/SteamAchievementManager/actions/workflows/build.yml/badge.svg">
    <img alt="GitHub issues" src="https://img.shields.io/github/issues/syntax-tm/SteamAchievementManager">
    <img alt="GitHub release" src="https://img.shields.io/github/v/release/syntax-tm/SteamAchievementManager">
    <img alt="GitHub pre-release" src="https://img.shields.io/github/v/release/syntax-tm/SteamAchievementManager?include_prereleases">
  </div>
</h1>

<p align="center">
  <img alt="SAM Logo" src="./resources/SAM_logo_default.svg" style="max-width: 600px; width: 70%;">
</p>

## Overview

The Steam Achievement Manager lets you manage achievements, stats, and more for any currently supported Steam app.

<p align="center">
  <a href="./resources/screenshots/SAM.png">
    <img alt="SAM Screenshot" src="./resources/screenshots/SAM.png" />
  </a>
</p>

This project is an actively updated fork of the [Steam Achievement Manager](https://github.com/gibbed/SteamAchievementManager).

## Project Structure

**SAM** is comprised of five projects.

![](./resources/SAM_projects_white.svg#gh-dark-mode-only)
![](./resources/SAM_projects_black.svg#gh-light-mode-only)

`SAM` and `SAM.Console` (_WIP_) are the two application projects that create an executable. `SAM` is the executable that will display your Steam library and let you select an app (game) to manage. When managing a game, the app ID is passed as an argument to `SAM.exe` and it initializes the Steam API client with that AppID the same way that normal games would. Steam will show you as in-game, record your play time, and you can earn trading cards (assuming of course you have drops left).

Both `SAM.API` and `SAM.Core` are libraries (`*.dll`). As its name suggests, `SAM.API` contains the native Steam API wrappers. `SAM.Core` is referenced by both executable projects (more on these later) and allows them to both use the same types, resources, etc. without having to duplicate code.

<div align="center">

```mermaid
%%{init: {"flowchart": {"htmlLabels": false, "width": "100%"}} }%%
flowchart TB
    subgraph Applicaitons
        S["SAM"]:::app
        SM["SAM.Console"]:::app
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
        <th align="center">New Project</th>
        <th style="width: 80%;">Description</th>
    </tr>
    <tr>
        <td align="center"><b>SAM.Picker</b></td>
        <td align="center"><b>SAM</b></td>
        <td>The main executable used to select a game (or app) from your library</td>
    </tr>
    <tr>
        <td align="center">-</th>
        <td align="center"><b>SAM.Console</b></td>
        <td>Command line interface for SAM for use with console, automation, etc. <i>This is still in development and will be released in a future version.</i></td>
    </tr>
    <tr>
        <td align="center"><b>SAM.Game</b></td>
        <td align="center"><b>SAM</b></td>
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
        <td>Common resources used by both <code>SAM</code> and <code>SAM.Console</code>.</td>
    </tr>
</table>

## Sponsors

<p align="center">
  <img alt="JetBrains" src="./resources/ref/JetBrains_Logo_2016.svg" width="120" />
</p>

A special thank you to [JetBrains](https://www.jetbrains.com/) for their continued [Support of Open-Source Projects](https://www.jetbrains.com/community/opensource/#support) like **SAM**.

> [!NOTE]
> Active **SAM** contributors are eligible to receieve complimentary licenses [^1] for **all** **JetBrains** products. For questions regarding eligability please refer to the [Open Source FAQ](https://sales.jetbrains.com/hc/en-gb/categories/13706169183250-Free-Licenses-for-OSS-development).

## Acknowledgements

<p align="center">
  <a href="https://github.com/DevExpress/DevExpress.Mvvm.Free">DevExpress</a> • <a href="https://github.com/RudeySH/SteamCountries">SteamCountries</a> • <a href="https://github.com/lepoco/wpfui">WPF UI</a>
</p>

[^1]: For non-commercial development
