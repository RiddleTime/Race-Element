var e=`---
title: HUD Profiles are added
slug: hud-profiles-added
description: You can now save and load your HUD settings at any time
date: 2026-11-01
type: news
---

<h1 id="profiles">Profiles</h1>
<p>With the 2.8.0.0 release, Race Element is now able to save your active settings to a profile. These profiles are name based and are stored on disk in the same manner as the HUD settings were saved before.</p><h1 id="under-the-hood">Under the hood</h1>
<p>The change wasn&#39;t easy to create as everything on the HUD side is as abstract as it can be. What it did make easy is removing the newtonsoft dependency and replacing it with the integrated .net version.
Providing there was going to be a change anyway, the life cycle of HUDs has also been rewritten and decoupled from a class that&#39;s still too big but has been dumbed down.</p><h1 id="whats-to-come">What&#39;s to come?</h1>
<p>Internally the system has code added that will provide Conditional Profiles, these will be able to activate automatically based on conditions within the game.</p>`;export{e as default};