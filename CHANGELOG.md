## 0.6.0

### 0.6.0-alpha

#### General

- Upgraded to .NET 8
- Updated [WPF-UI](https://github.com/lepoco/wpfui) to version 3.0.4

#### SAM

- Added status bar item for favorites
- Added ability to show hidden apps
- Added ability to unhide all apps
- Added ability to show only favorited apps
- Added autocomplete to search
- Added overlay for favorited apps
- Added overlay for hidden apps
- Added overlay menu for quickly adding/removing favorites and hiding/showing apps
- Added `View on SteamGridDB` menu option
- Added support for displaying [Steam Grid](https://www.steamgriddb.com/) assets
  - **Note:** Animated images are not currently supported
- Added support for loading Steam's `librarycache` images
- Implemented app refresh queue
  - This should help users with larger Steam libraries from having issues with the Steamworks Web API
- **[BUG]** Fixed error in debug output when launching a web page
- **[BUG]** Fixed hidden apps status bar item count not updating
- **[BUG]** Fixed library search textbox alignment

#### SAM.Manager

- **[BUG]** Ability to filter achievements using the drop down has been temporarily removed

#### Planned

##### SAM

- Ability to customize favorites overlay color
- Ability to customize hidden overlay color
- Basic command line options
  - For automating resettings stats/achievements, generating reports, etc.
- Save and auto load `Show Hidden` and `Show Only Favorites` view settings

#### Known Issues

- An `OutOfMemoryException` will be thrown when attempting to load an animated Grid image
  - This should only be visible in the output when debugging and can be ignored
- Startup SlpashScreen's TitleBar was removed
  - During startup, you will not be able to move the SplashScreen or close it using the Window buttons

### 0.6.0-alpha

- Updated logging
- **[BUG]** Removed library caching

## 0.5.0

- **[BUG]** Fixed issue with increment only stats allowing invalid values
- **[BUG]** Fixed issue with increment only stats not displaying properly

## 0.4.0

- [BUG] Fixed console window showing when running app

## 0.3.0

- Added search functionality to library view
- Improved application startup time

### 0.3.0-beta

- Added search functionality to library view

### 0.3.0-alpha

- Improved application startup time

## 0.2.0

- Added new release workflow
- Added ability to Refresh Steam library
- Switched ViewModels to source generators
- **[BUG]** Fixed issue with the "Show Hidden" button not working for Achievements

## 0.1.0

- Initial release
