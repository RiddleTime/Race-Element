---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide 
---
# Auto-switching
Race Element has an option in the main menu of the app that allows it to automatically switch itself to any supported running simulator. Without it you'll have to manually switch between games, you can do so by clicking the game selector in the left bottom of the app.

# Prequisites
Below you can find a guide for each game that requires it, make sure to do it else Race Element won't receive any data! 
If you really need help ask in the discord, else translate this page to your preferred language.

# American Truck Simulator & Euro Truck Simulator 2
Both simulators require a plugin to be installed before any data can be read from the game.
1. For both simulators you will need to "Install" a plugin: *scs-telemetry.dll*
2. Download the latest release and Follow the installation instructions here: **<a href="https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation" target="_blank">https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation</a>**

# Automobilista 2
1. Go to Options
2. Go to System Options
3. Enable Telemetry

# Le Mans Ultimate
1. Open the installation folder of Le Mans Ultimate
2. Go to `Plugins`
3. Download the shared memory plugin https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file
4. Open the downloaded zip file and look for the `rFactor2SharedMemoryMapPlugin64.dll`
5. Extract that .dll file to the folder you've opened
6. Launch the game and exit it
7. Open the installation folder of Le Mans Ultimate
8. Go to `UserData\player`
9. Open `CustomPluginVariables.json` with a text editor like notepad
10. Set the rFactor2SharedMemoryMapPlugin64.dll to `" Enabled": 1,` and Save

## Forza Horizon 5
1. Go to Options
2. Go to HUD AND GAMEPLAY
3. Set Data Out to `On`
4. Set Data Out IP Address to `127.0.0.1`
5. Set Data Out Port to `5300`
6. Restart Forza Horizon 5.
7. Restart Race Element.
8. If Windows asks permission for network access allow it else race element won't be able to receive data from Forza Horizon 5.

## rFactor 2
1. Open the installation folder of rFactor 2
2. Go to `Bin64\Plugins`
3. Download the shared memory plugin https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file
4. Open the downloaded zip file and look for the `rFactor2SharedMemoryMapPlugin64.dll`
5. Extract that .dll file to the folder you've opened
6. Launch the game
7. Go to Options -> Gameplay: Set `rFactor2SharedMemoryMapPlugin64` to `ON`
8. Restart the game

## WRC Generations
1. Open Windows Explorer
2. Paste `%userprofile%\Documents\My Games\WRCG\` in the address bar and hit Enter.
3. Open `UserSettings.cg` with a text editor like notepad.
4. Scroll to the bottom and add or set these 4 lines to:
```
  WRC.Telemetry.EnableTelemetry = true
  WRC.Telemetry.TelemetryPort = 20777
  WRC.Telemetry.TelemetryAdress = "127.0.0.1"
  WRC.Telemetry.TelemetryRate = 60
```
5. Save the file.
