const e=`---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide
---

<h1 id="prequisites">Prequisites</h1>
<p>For some games you need are required to do some small steps so Race Element has access to all data it needs.</p>
<h2 id="assetto-corsa-1">Assetto Corsa 1</h2>
<p>AC 1 needs the Crew Chief plugin installed. It will provide more telemetry on opponents cars than the telemetry that comes with AC1 out of the box. Once Crew Chief installs the plugin, it doesn&#39;t need to run while playing AC and using Race Element HUDs. The installation steps are:</p>
<ol>
<li>Install Crew Chief from <a href="https://thecrewchief.org/forumdisplay.php?28-Download-and-Links">https://thecrewchief.org/forumdisplay.php?28-Download-and-Links</a></li>
<li>Start Crew Chief. Select &quot;Assetto Corsa&quot; as &quot;Game&quot; and use the &quot;Start Crew Chief Button&quot;. It should provide a dialog saying it will install the Crew Chief plugin</li>
<li>Start AC1. Select the &quot;Crew Chief&quot; app on the right border where all the apps are listed. At this point Crew Chief does not have to be running anymore.</li>
</ol>
<h2 id="american-truck-simulator--euro-truck-simulator-2">American Truck Simulator &amp; Euro Truck Simulator 2</h2>
<p>Both simulators require a plugin to be installed before any data can be read from the game.</p>
<ol>
<li>For both simulators you will need to &quot;Install&quot; a plugin: <em>scs-telemetry.dll</em></li>
<li>Download the latest release and Follow the installation instructions here: <strong><a href="https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation" target="_blank">https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation</a></strong></li>
</ol>
<h2 id="automobilista-2">Automobilista 2</h2>
<ol>
<li>Go to Options</li>
<li>Go to System Options</li>
<li>Enable Telemetry</li>
</ol>
`;export{e as default};
