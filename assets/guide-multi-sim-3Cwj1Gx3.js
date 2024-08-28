const e=`---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide 
---
Race Element is now in the process of adding support for sims other than ACC. Currently in development is support for iRacing and Assetto Corsa 1. 

While development is ongoing there might be some bugs that we need help with identifying. Below is a list of HUDs and their development state. Alpha HUDs might have a larger number of bugs than beta HUDs. In general if Race Element hangs or doesn't show proper values, try restarting it.

# Prequisites

For some games you need are required to do some small steps so Race Element has access to all data it needs.

## Assetto Corsa 1
AC 1 needs the Crew Chief plugin installed. It will provide more telemetry on opponents cars than the telemetry that comes with AC1 out of the box. Once Crew Chief installs the plugin, it doesn't need to run while playing AC and using Race Element HUDs. The installation steps are:

1. Install Crew Chief from https://thecrewchief.org/forumdisplay.php?28-Download-and-Links
2. Start Crew Chief. Select "Assetto Corsa" as "Game" and use the "Start Crew Chief Button". It should provide a dialog saying it will install the Crew Chief plugin
3. Start AC1. Select the "Crew Chief" app on the right border where all the apps are listed. At this point Crew Chief does not have to be running anymore.

# Multi-Sim HUDs
In order to use HUDs for the sim, select the sim on the top right.

Depending on the chosen sim the list of currently supported HUDs will change. ACC supports all HUDs (except Bar Spotter) and is not mentioned here.

| HUD              | Description                                                  | AC1              | iRacing   |
| ---------------- | ------------------------------------------------------------ |------------------|-----------|
| Bar Spotter      | Shows the overlap with other cars                            | No support       | Beta      |    
| Current Gear     | Shows the current gear                                       | Suported         | Supported |
| Fuel Info        | Shows the the available fuel and how many laps it lasts      | No Support (yet) | Alpha     |
| Input Bars/Trace | Shows Throttle, Brakes and Steering input                    | Supported        | Supported |
| Lap Delta Bar    | Shows the deleta to the session best lap time                | No Support (yet) | Beta      |
| Live Standing    | Shows the live positions                                     | Beta             | Beta      |
| Shift Indicator  | Shows the RPMs and when to shift                             | Supported        | Supported |
| Track Bar        | Shows nearby cars and their distance                         | Beta             | Beta      |
| Wind Direction   | Shows wind direction relative to the car                     | Beta             | Beta      |
`;export{e as default};
