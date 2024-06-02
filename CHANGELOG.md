## 0.6.0

#### General

- Consolidated **SAM** and **SAM.Manager** into a single project
- **[BUG]** Fixed crash caused by achievements missing a lock or unlocked image
- **[BUG]** Fixed main window not activating after startup
- **[BUG]** Fixed splash screen title bar crashing

#### SAM.Manager

- Removed **SAM.Manager**

### 0.6.0-beta

#### General

- Fixed publish settings
  - The main executable files should now contain their framework and package references
- Fixed splash screen responsiveness

#### SAM

- Added main menu
  - Added menu item to export library
  - Added menu item for filtering favorite apps
  - Added menu item for showing hidden apps
  - Added menu item to reset all settings
  - Added menu item to open log directory
  - Moved the search box to be inline with the menu

#### SAM.Manager

- **[BUG]** Fixed window icon not showing in title bar

### 0.6.0-alpha

#### General

- Upgraded to .NET 8
- Updated [WPF-UI](https://github.com/lepoco/wpfui) to version 3.0.4
- Updated logging

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
- **[BUG]** Removed library caching

#### SAM.Manager

- **[BUG]** Ability to filter achievements using the drop down has been temporarily removed

#### Known Issues

- An `OutOfMemoryException` will be thrown when attempting to load an animated Grid image
  - This should only be visible in the output when debugging and can be ignored
- Startup SlpashScreen's TitleBar was removed
  - During startup, you will not be able to move the SplashScreen or close it using the Window buttons

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
