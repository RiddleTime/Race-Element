---
title: HUD Profiles are added
slug: hud-profiles-added
description: You can now save and load your HUD settings at any time
date: 2026-11-1
type: news
---
# Profiles
With the 2.8.0.0 release, Race Element is now able to save your active settings to a profile. These profiles are name based and are stored on disk in the same manner as the HUD settings were saved before.

# Under the hood
The change wasn't easy to create as everything on the HUD side is as abstract as it can be. What it did make easy is removing the newtonsoft dependency and replacing it with the integrated .net version.
Providing there was going to be a change anyway, the life cycle of HUDs has also been rewritten and decoupled from a class that's still too big but has been dumbed down.

# What's to come?
Internally the system has code added that will provide Conditional Profiles, these will be able to activate automatically based on conditions within the game.
