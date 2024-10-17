const t=`---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide
---

<p>Race Element is now in the process of adding support for sims other than ACC. Currently in development is support for iRacing, Assetto Corsa 1 and RaceRoom. </p>
<p>While development is ongoing there might be some bugs that we need help with identifying. Below is a list of HUDs and their development state. Alpha HUDs might have a larger number of bugs than beta HUDs. In general if Race Element hangs or doesn&#39;t show proper values, try restarting it.</p>
<h1 id="prequisites">Prequisites</h1>
<p>For some games you need are required to do some small steps so Race Element has access to all data it needs.</p>
<h2 id="assetto-corsa-1">Assetto Corsa 1</h2>
<p>AC 1 needs the Crew Chief plugin installed. It will provide more telemetry on opponents cars than the telemetry that comes with AC1 out of the box. Once Crew Chief installs the plugin, it doesn&#39;t need to run while playing AC and using Race Element HUDs. The installation steps are:</p>
<ol>
<li>Install Crew Chief from <a href="https://thecrewchief.org/forumdisplay.php?28-Download-and-Links">https://thecrewchief.org/forumdisplay.php?28-Download-and-Links</a></li>
<li>Start Crew Chief. Select &quot;Assetto Corsa&quot; as &quot;Game&quot; and use the &quot;Start Crew Chief Button&quot;. It should provide a dialog saying it will install the Crew Chief plugin</li>
<li>Start AC1. Select the &quot;Crew Chief&quot; app on the right border where all the apps are listed. At this point Crew Chief does not have to be running anymore.</li>
</ol>
<h1 id="multi-sim-huds">Multi-Sim HUDs</h1>
<p>In order to use HUDs for the sim, select the sim on the top right.</p>
<p>Depending on the chosen sim the list of currently supported HUDs will change. ACC supports all HUDs (except Bar Spotter) and is not mentioned here.</p>
<table>
<thead>
<tr>
<th>HUD</th>
<th>Description</th>
<th>AC1</th>
<th>iRacing</th>
</tr>
</thead>
<tbody><tr>
<td>Bar Spotter</td>
<td>Shows the overlap with other cars</td>
<td>No support</td>
<td>Beta</td>
</tr>
<tr>
<td>Current Gear</td>
<td>Shows the current gear</td>
<td>Suported</td>
<td>Supported</td>
</tr>
<tr>
<td>Fuel Info</td>
<td>Shows the the available fuel and how many laps it lasts</td>
<td>No Support (yet)</td>
<td>Alpha</td>
</tr>
<tr>
<td>Input Bars/Trace</td>
<td>Shows Throttle, Brakes and Steering input</td>
<td>Supported</td>
<td>Supported</td>
</tr>
<tr>
<td>Lap Delta Bar</td>
<td>Shows the deleta to the session best lap time</td>
<td>No Support (yet)</td>
<td>Beta</td>
</tr>
<tr>
<td>Live Standing</td>
<td>Shows the live positions</td>
<td>Beta</td>
<td>Beta</td>
</tr>
<tr>
<td>Shift Indicator</td>
<td>Shows the RPMs and when to shift</td>
<td>Supported</td>
<td>Supported</td>
</tr>
<tr>
<td>Track Bar</td>
<td>Shows nearby cars and their distance</td>
<td>Beta</td>
<td>Beta</td>
</tr>
<tr>
<td>Wind Direction</td>
<td>Shows wind direction relative to the car</td>
<td>Beta</td>
<td>Beta</td>
</tr>
</tbody></table>
`;export{t as default};
