const e=`---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide
---

<h1 id="auto-switching">Auto-switching</h1>
<p>Race Element has an option in the main menu of the app that allows it to automatically switch itself to any supported running simulator. Without it you&#39;ll have to manually switch between games, you can do so by clicking the game selector in the left bottom of the app.</p>
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
<h2 id="le-mans-ultimate">Le Mans Ultimate</h2>
<ol>
<li>Open the installation folder of Le Mans Ultimate</li>
<li>Go to <code>Plugins</code></li>
<li>Download the shared memory plugin <a href="https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file">https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file</a></li>
<li>Open the downloaded zip file and look for the <code>rFactor2SharedMemoryMapPlugin64.dll</code></li>
<li>Extract that .dll file to the folder you&#39;ve opened</li>
<li>Launch the game and exit it</li>
<li>Open the installation folder of Le Mans Ultimate</li>
<li>Go to <code>UserData\\player</code></li>
<li>Open <code>CustomPluginVariables.json</code> with a text editor like notepad</li>
<li>Set the rFactor2SharedMemoryMapPlugin64.dll to <code>&quot; Enabled&quot;: 1,</code> and Save</li>
</ol>
<h2 id="forza-horizon-5">Forza Horizon 5</h2>
<ol>
<li>Go to Options</li>
<li>Go to HUD AND GAMEPLAY</li>
<li>Set Data Out to <code>On</code></li>
<li>Set Data Out IP Address to <code>127.0.0.1</code></li>
<li>Set Data Out Port to <code>5300</code></li>
<li>Restart Forza Horizon 5.</li>
<li>Restart Race Element.</li>
<li>If Windows asks permission for network access allow it else race element won&#39;t be able to receive data from Forza Horizon 5.</li>
</ol>
<h2 id="rfactor-2">rFactor 2</h2>
<ol>
<li>Open the installation folder of rFactor 2</li>
<li>Go to <code>Bin64\\Plugins</code></li>
<li>Download the shared memory plugin <a href="https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file">https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file</a></li>
<li>Open the downloaded zip file and look for the <code>rFactor2SharedMemoryMapPlugin64.dll</code></li>
<li>Extract that .dll file to the folder you&#39;ve opened</li>
<li>Launch the game</li>
<li>Go to Options -&gt; Gameplay: Set &quot;rFactor2SharedMemoryMapPlugin64&quot; to <code>ON</code></li>
<li>Restart the game</li>
</ol>
<h2 id="wrc-generations">WRC Generations</h2>
<ol>
<li>Open Windows Explorer</li>
<li>Paste <code>%userprofile%\\Documents\\My Games\\WRCG\\</code> in the address bar and hit Enter.</li>
<li>Open <code>UserSettings.cg</code> with a text editor like notepad.</li>
<li>Scroll to the bottom and add or set these 4 lines to:</li>
</ol>
<pre><code>  <span class="token constant">WRC</span><span class="token punctuation">.</span>Telemetry<span class="token punctuation">.</span>EnableTelemetry <span class="token operator">=</span> <span class="token boolean">true</span>
  <span class="token constant">WRC</span><span class="token punctuation">.</span>Telemetry<span class="token punctuation">.</span>TelemetryPort <span class="token operator">=</span> <span class="token number">20777</span>
  <span class="token constant">WRC</span><span class="token punctuation">.</span>Telemetry<span class="token punctuation">.</span>TelemetryAdress <span class="token operator">=</span> <span class="token string">"127.0.0.1"</span>
  <span class="token constant">WRC</span><span class="token punctuation">.</span>Telemetry<span class="token punctuation">.</span>TelemetryRate <span class="token operator">=</span> <span class="token number">60</span></code></pre><ol start="5">
<li>Save the file.</li>
</ol>
`;export{e as default};
