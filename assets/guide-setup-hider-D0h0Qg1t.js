const e=`---
title: "Setup Hider for Streaming"
slug: setup-hider
description: Setting up the setup hider with OBS or Streamlabs
type: guide
---

<h1 id="how-does-it-work">How does it work?</h1>
<p>Assetto Corsa Competizione has data for developers that allows us to see when the Setup Screen is open. Race Element uses this with the integration of streamlabs or OBS. It sends a signal to the configured streaming software to make a Source item visible or to hide it based on the data from Assetto Corsa Competizione.</p>
<h1 id="obs">OBS</h1>
<p>Make sure you are running OBS version 28 or higher.</p>
<ol>
<li><em>OBS</em>: <strong>Add Any type of Source to your active Scene, name it <code>SetupHider</code>. This can be text, image, video, a website, basically anything even a Scene Source.</strong></li>
<li><em>OBS</em>: <strong>At the top menu: Click Tools and Open the WebSocket Server Settings.</strong></li>
<li><em>OBS</em>: <strong>In the WebSocket Server Settings window, Make sure <code>Enable WebSocket Server</code> is checked.</strong></li>
<li><em>OBS</em>: <strong>In the WebSocket Server Settings window, Click <code>Show Connect Info</code></strong></li>
<li><em>Race Element:</em> <strong>Open the Main Settings Tab on the left -&gt; Open the Streaming Tab.</strong></li>
<li><em>Race Element:</em> <strong>Set the Software to <code>OBS</code></strong></li>
<li><em>OBS/Race Element</em>: <strong>Copy the Server IP, Server Port and Server Password from OBS and paste them in Race Element&#39;s OBS Settings.</strong></li>
<li><em>Race Element:</em>: <strong>Click Save and then at the bottom Enable the SetupHider.</strong></li>
</ol>
<h1 id="streamlabs">StreamLabs</h1>
<ol>
<li><em>StreamLabs</em>: <strong>Add Any type of Source to your active Scene, name it <code>SetupHider</code>. This can be text, image, video, a website, basically anything even a Scene Source.</strong></li>
<li><em>Race Element:</em> <strong>Open the Main Settings Tab on the left -&gt; Open the Streaming Tab.</strong></li>
<li><em>Race Element:</em> <strong>Set the Software to <code>StreamLabs</code>.</strong></li>
<li><em>Race Element:</em> <strong>Click Save and then at the bottom Enable the SetupHider.</strong></li>
</ol>
`;export{e as default};
