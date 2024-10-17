const e=`---
title: "HUDs and Streaming"
slug: huds-and-streaming
description: Settings up the HUDs with OBS or Streamlabs
type: guide
---

<h1 id="recording-the-entire-desktop">Recording the Entire Desktop</h1>
<p>If you want to share all your HUDs you can record the entire desktop since the HUDs themselves are not part of the game.</p>
<h1 id="recording-a-single-hud">Recording a Single HUD</h1>
<ol>
<li><em>Race Element:</em> <strong>Enable the &#39;Window&#39; option for the HUD you want to show.</strong></li>
<li><em>Race Element:</em> <strong>Enable Always Visible (blue eye button in HUD Tab).</strong></li>
<li><em>Stream Software:</em> <strong>Add a new <code>Window capture</code> source in your stream software.</strong></li>
<li><em>Stream Software:</em> <strong>Set the Capture method to <code>Windows Graphics Capture</code></strong></li>
<li><em>Stream Software:</em> <strong>Select the HUD Window.</strong></li>
<li><em>Stream Software:</em> <strong>Set Window match priority to <code>Window title must match</code></strong></li>
<li><em>(optional) Race Element:</em> <strong>If you don&#39;t want to see the hud and only use it for your stream, disable the always on top option for that HUD.</strong></li>
</ol>
`;export{e as default};
