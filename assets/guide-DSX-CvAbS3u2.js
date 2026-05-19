const e=`---
title: "Using DSX for Dualsense Active Triggers"
slug: DSX
description: How to set up DSX and Race Element
type: guide
---

<h1 id="what-are-active-triggers">What are active triggers?</h1>
<p>Active triggers is Force Feedback based on the physics of the car which is applied to the left and right trigger. It allows you to feel under and oversteer during acceleration and braking. It makes uses of the DualSense Adaptive Trigger Mechanism.</p>
<h1 id="supported-games">Supported Games</h1>
<ul>
<li>Assetto Corsa</li>
<li>Assetto Corsa Competizione</li>
<li>Assetto Corsa EVO</li>
<li>Assetto Corsa Rally</li>
<li>DiRT Rally 2.0</li>
<li>Forza Horizon 4</li>
<li>Forza Horizon 5</li>
<li>Forza Horizon 6</li>
<li>Forza Motorsport 8</li>
<li>Le Mans Ultimate</li>
<li>Project Motor Racing</li>
<li>Richard Burns Rally</li>
<li>RaceRoom</li>
<li>rFactor 2</li>
<li>WRC Generations</li>
</ul>
<h1 id="dsx">DSX</h1>
<ol>
<li>Open DSX and at the left bottom click Settings (Don&#39;t have DSX yet? <strong><a href="https://store.steampowered.com/app/1812620/DSX/">Get DSX on Steam</a></strong> )</li>
<li>Open the Networking tab</li>
<li>Make sure the Firewall Rules are okay, if it&#39;s not then allow DSX to fix it.</li>
<li>Enable <code>Incoming UDP</code></li>
<li>Make <code>Incoming UDP port</code> should be set to <code>6969</code>.</li>
<li>Do not enable Outgoing UDP.</li>
</ol>
<h1 id="race-element">Race Element</h1>
<ol>
<li>Open Race Element (Don&#39;t have Race Element yet? <strong><a href="/guide/how-to-get-started">Race Element Download and Installation Guide</a></strong> )</li>
<li>Some Games require you to configure a data stream, make sure to read the <strong><a href="/guide/multi-sim">Multi-Sim Guide</a></strong></li>
<li>Make sure the game selection is set correctly(left-bottem of the app), this should happen automatically unless disabled</li>
<li>Open the HUD Tab</li>
<li>Click DSX</li>
<li>To activate the DSX HUD read <strong><a href="/guide/how-to-use-huds">The guide for using the HUD Tab</a></strong></li>
<li>You can alter behaviour for Braking and Acceleration, the port that is used to connect to DSX should be set to 6969. This is not related to what you see in the multi-sim guide.</li>
</ol>
`;export{e as default};
