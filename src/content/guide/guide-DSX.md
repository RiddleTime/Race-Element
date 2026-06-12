---
title: "Using DSX for Dualsense Active Triggers"
slug: DSX
description: How to set up DSX and Race Element
type: guide 
---
# What are active triggers?
Active triggers is Force Feedback based on the physics of the car which is applied to the left and right trigger. It allows you to feel under and oversteer during acceleration and braking. It makes uses of the DualSense Adaptive Trigger Mechanism.

# Supported Games
- Assetto Corsa
- Assetto Corsa Competizione
- Assetto Corsa EVO
- Assetto Corsa Rally
- DiRT Rally 2.0
- Forza Horizon 4
- Forza Horizon 5
- Forza Horizon 6
- Forza Motorsport 8
- Le Mans Ultimate
- Project Motor Racing
- Richard Burns Rally
- RaceRoom
- rFactor 2
- WRC Generations

# DSX
1. Open DSX and at the left bottom click Settings (Don't have DSX yet? **[Get DSX on Steam](https://store.steampowered.com/app/1812620/DSX/)** )
2. Open the Networking tab
3. Make sure the Firewall Rules are okay, if it's not then allow DSX to fix it.
4. Enable `Incoming UDP`
5. Make `Incoming UDP port` should be set to `6969`.
6. Do not enable Outgoing UDP.

# DSY
1. You can alternatively use DSY which is free, but is very basic compared to DSX.
2. https://github.com/WujekFoliarz/DualSenseY-v2
3. I personally use DSX for the features it provides.

# Race Element
1. Open Race Element (Don't have Race Element yet? **[Race Element Download and Installation Guide](/guide/how-to-get-started)** )
2. Some Games require you to configure a data stream, make sure to read the **[Multi-Sim Guide](/guide/multi-sim)**
3. Make sure the game selection is set correctly(left-bottem of the app), this should happen automatically unless disabled
4. Open the HUD Tab
5. Click DSX
6. **To activate the DSX HUD read [The guide for using the HUD Tab](/guide/how-to-use-huds)**, Once it's activated you'll see it marked green in the HUD list and the options will be grayed out.
7. You can alter behaviour for Braking and Acceleration, the port that is used to connect to DSX should be set to 6969. This is not related to what you see in the multi-sim guide.
