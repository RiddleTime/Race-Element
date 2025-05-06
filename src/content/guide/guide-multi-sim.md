---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide 
---
# Auto-switching
Race Element has an option in the main menu of the app that allows it to automatically switch itself to any supported running simulator. Without it you'll have to manually switch between games, you can do so by clicking the game selector in the left bottom of the app.

# Prequisites
For some games you need are required to do some small steps so Race Element has access to all data it needs.

## Assetto Corsa 1
AC 1 needs the Crew Chief plugin installed. It will provide more telemetry on opponents cars than the telemetry that comes with AC1 out of the box. Once Crew Chief installs the plugin, it doesn't need to run while playing AC and using Race Element HUDs. The installation steps are:

1. Install Crew Chief from https://thecrewchief.org/forumdisplay.php?28-Download-and-Links
2. Start Crew Chief. Select "Assetto Corsa" as "Game" and use the "Start Crew Chief Button". It should provide a dialog saying it will install the Crew Chief plugin
3. Start AC1. Select the "Crew Chief" app on the right border where all the apps are listed. At this point Crew Chief does not have to be running anymore.

## American Truck Simulator & Euro Truck Simulator 2
Both simulators require a plugin to be installed before any data can be read from the game.
1. For both simulators you will need to "Install" a plugin: *scs-telemetry.dll*
2. Download the latest release and Follow the installation instructions here: **<a href="https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation" target="_blank">https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation</a>**

## Automobilista 2
1. Go to Options
2. Go to System Options
3. Enable Telemetry
