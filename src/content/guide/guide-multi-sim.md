---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide 
---
# Auto-switching
Race Element has an option in the main menu of the app that allows it to automatically switch itself to any supported running simulator. Without it you'll have to manually switch between games, you can do so by clicking the game selector in the left bottom of the app.

# Prequisites
The game you want to play with Race Element might require some extra steps, this so Race Element can receive all required data.
Below you can find a guide for each game that requires a setup. If you can't figure it out, ask in the Race ELement Discord.

# American Truck Simulator & Euro Truck Simulator 2
Both simulators require a plugin to be installed before any data can be read from the game.
1. For both simulators you will need to "Install" a plugin: *scs-telemetry.dll*
2. Download the latest release and Follow the installation instructions here: **<a href="https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation" target="_blank">https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation</a>**

# Automobilista 2
1. Go to Options
2. Go to System Options
3. Enable Telemetry

# DiRT Rally 2.0
1. Open Windows Explorer
2. Paste `%userprofile%\Documents\My Games\DiRT Rally 2.0\hardwaresettings` in the address bar and hit Enter.
3. Open the `hardware_settings_config.xml` with a text editor like notepad.
4. Find the category that starts with `<motion_platform>`
5. Find the line that starts with `<udp...`
6. Make sure it looks like this `<udp enabled="true" extradata="3" ip="127.0.0.1" port="20777" delay="1" />`
7. `enabled` should be `true` and `extradata` should be `3`
8. Save the file and start the DiRT Rally 2.0.

# Forza Horizon 4
1. Go to Settings (Read Step 9 if you use Microsoft Store Version)
2. Go to HUD AND GAMEPLAY
3. Set Data Out to `On`
4. Set Data Out IP Address to `127.0.0.1`
5. Set Data Out Port to `5300`
6. Restart Forza Horizon 5.
7. Restart Race Element.
8. If Windows asks permission for network access allow it else race element won't be able to receive data from Forza Horizon 4.
9. If you run the game from the Microsoft Store, so skip this step if you run it through Steam. Open a command prompt( CMD) as administrator and run the following command: `CheckNetIsolation.exe LoopbackExempt -a -n=Microsoft.SunriseBaseGame_8wekyb3d8bbwe` it allows the game to send the UDP packets to your local computer.

# Forza Horizon 5
1. Go to Options
2. Go to HUD AND GAMEPLAY
3. Set Data Out to `On`
4. Set Data Out IP Address to `127.0.0.1`
5. Set Data Out Port to `5300`
6. Restart Forza Horizon 5.
7. Restart Race Element.
8. If Windows asks permission for network access allow it else race element won't be able to receive data from Forza Horizon 5.
9. If you run the game from the Microsoft Store, so skip this step if you run it through Steam. Open a command prompt( CMD) as administrator and run the following command: `CheckNetIsolation.exe LoopbackExempt -a -n=microsoft.624F8B84B80_8wekyb3d8bbwe` it allows the game to send the UDP packets to your local computer.

# Forza Motorsport 8
1. Start Forza Motorsport 8
2. Go to Gameplay and HUD tab
3. Scroll to the bottom
4. Enable `Data Out`
5. Set Data Out IP Address to: `127.0.0.1`
6. Set Data Out Port to: `5300`
7. Set Data Out Packet Format to: `Car Dash`
8. If Windows asks permission for network access allow it else race element won't be able to receive data from Forza Motorsport 8.

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
10. If you see anything else than `null` as text in this file go to step 11. Else please read: https://community.lemansultimate.com/index.php?threads/shared-memory-plugin-not-loading.3705/#post-24870 and install the C++ redistributable(https://aka.ms/highdpimfc2013x64enu). Then launch and exit the game.
11. Set the rFactor2SharedMemoryMapPlugin64.dll to `" Enabled": 1,` and Save. 

# Project Motor Racing
1. Start Project Motor Racing
2. Go to Options (X on keyboard)
3. Go to Preferences
4. Set `UDP Enabled` to `On`
5. Set `UDP Frequency` to `60`
6. UDP Port should be `7576`
7. UDP Host should be `224.0.0.150`
8. Save (spacebar on keyboard)

# Richard Burns Rally( RSF)
1. Open the RSF Launcher
2. Enable the Adanced options
3. Go to the Telemetry Tab
4. The telemetry should be `127.0.0.1` : `6776`
5. Enable the `UDP Telemetry` checkbox.
6. Launch the game

# rFactor 2
1. Open the installation folder of rFactor 2
2. Go to `Bin64\Plugins`
3. Download the shared memory plugin https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file
4. Open the downloaded zip file and look for the `rFactor2SharedMemoryMapPlugin64.dll`
5. Extract that .dll file to the folder you've opened
6. Launch the game
7. Go to Options -> Gameplay: Set `rFactor2SharedMemoryMapPlugin64` to `ON`
8. Restart the game

# WRC Generations
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
