<p align="center">
  <img src="../resources/SAM_API_logo_dark_blue.svg">
</p>

## Description

The Steam API wrapper used by Steam Achievement Manager.

## Usage

```csharp
using SAM.API;

// the App ID is used to uniquely identify an app on Steam. this
// can only be set ONCE PER PROCESS when calling Client.Initialize
// if you need to look up an App ID or other information, SteamDB is
// a great resource. Some examples:
// 
//   Resident Evil 5 (21690):
//   https://steamdb.info/app/21690/
//
//   Left 4 Dead 2 (550):
//   https://steamdb.info/app/550/
var appId = 0;

using var client = new Client();
client.Initialize(appId);
```
