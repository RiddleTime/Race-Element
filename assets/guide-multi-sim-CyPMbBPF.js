const e=`---
title: "Multi-Sim"
slug: multi-sim
description: Getting Race Element Ready for Multi-Sim
type: guide
---

<h1 id="auto-switching">Auto-switching</h1>
<p>Race Element has an option in the main menu of the app that allows it to automatically switch itself to any supported running simulator. Without it you&#39;ll have to manually switch between games, you can do so by clicking the game selector in the left bottom of the app.</p>
<h1 id="prequisites">Prequisites</h1>
<p>The game you want to play with Race Element might require some extra steps, this so Race Element can receive all required data.
Below you can find a guide for each game that requires a setup. If you can&#39;t figure it out, ask in the Race ELement Discord.</p>
<h1 id="american-truck-simulator--euro-truck-simulator-2">American Truck Simulator &amp; Euro Truck Simulator 2</h1>
<p>Both simulators require a plugin to be installed before any data can be read from the game.</p>
<ol>
<li>For both simulators you will need to &quot;Install&quot; a plugin: <em>scs-telemetry.dll</em></li>
<li>Download the latest release and Follow the installation instructions here: <strong><a href="https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation" target="_blank">https://github.com/RenCloud/scs-sdk-plugin?tab=readme-ov-file#installation</a></strong></li>
</ol>
<h1 id="automobilista-2">Automobilista 2</h1>
<ol>
<li>Go to Options</li>
<li>Go to System Options</li>
<li>Enable Telemetry</li>
</ol>
<h1 id="dirt-rally-20">DiRT Rally 2.0</h1>
<ol>
<li>Open Windows Explorer</li>
<li>Paste <code>%userprofile%\\Documents\\My Games\\DiRT Rally 2.0\\hardwaresettings</code> in the address bar and hit Enter.</li>
<li>Open the <code>hardware_settings_config.xml</code> with a text editor like notepad.</li>
<li>Find the category that starts with <code>&lt;motion_platform&gt;</code></li>
<li>Find the line that starts with <code>&lt;udp...</code></li>
<li>Make sure it looks like this <code>&lt;udp enabled=&quot;true&quot; extradata=&quot;3&quot; ip=&quot;127.0.0.1&quot; port=&quot;20777&quot; delay=&quot;1&quot; /&gt;</code></li>
<li><code>enabled</code> should be <code>true</code> and <code>extradata</code> should be <code>3</code></li>
<li>Save the file and start the DiRT Rally 2.0.</li>
</ol>
<h1 id="forza-horizon-4">Forza Horizon 4</h1>
<ol>
<li>Go to Settings (Read Step 9 if you use Microsoft Store Version)</li>
<li>Go to HUD AND GAMEPLAY</li>
<li>Set Data Out to <code>On</code></li>
<li>Set Data Out IP Address to <code>127.0.0.1</code></li>
<li>Set Data Out Port to <code>5300</code></li>
<li>Restart Forza Horizon 5.</li>
<li>Restart Race Element.</li>
<li>If Windows asks permission for network access allow it else race element won&#39;t be able to receive data from Forza Horizon 4.</li>
<li>If you run the gam from the Microsoft Store, so skip this step if you run it through Steam. Open a command prompt( CMD) as administrator and run the following command: <code>CheckNetIsolation.exe LoopbackExempt -a -n=Microsoft.SunriseBaseGame_8wekyb3d8bbwe</code> it allows the game to send the UDP packets to your local computer.</li>
</ol>
<h1 id="forza-horizon-5">Forza Horizon 5</h1>
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
<h1 id="forza-motorsport-8">Forza Motorsport 8</h1>
<ol>
<li>Start Forza Motorsport 8</li>
<li>Go to Gameplay and HUD tab</li>
<li>Scroll to the bottom</li>
<li>Enable <code>Data Out</code></li>
<li>Set Data Out IP Address to: <code>127.0.0.1</code></li>
<li>Set Data Out Port to: <code>5300</code></li>
<li>Set Data Out Packet Format to: <code>Car Dash</code></li>
<li>If Windows asks permission for network access allow it else race element won&#39;t be able to receive data from Forza Motorsport 8.</li>
<li></li>
</ol>
<h1 id="le-mans-ultimate">Le Mans Ultimate</h1>
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
<li>If you see anything else than <code>null</code> as text in this file go to step 11. Else please read: <a href="https://community.lemansultimate.com/index.php?threads/shared-memory-plugin-not-loading.3705/#post-24870">https://community.lemansultimate.com/index.php?threads/shared-memory-plugin-not-loading.3705/#post-24870</a> and install the C++ redistributable(<a href="https://aka.ms/highdpimfc2013x64enu">https://aka.ms/highdpimfc2013x64enu</a>). Then launch and exit the game.</li>
<li>Set the rFactor2SharedMemoryMapPlugin64.dll to <code>&quot; Enabled&quot;: 1,</code> and Save.</li>
</ol>
<h1 id="project-motor-racing">Project Motor Racing</h1>
<ol>
<li>Start Project Motor Racing</li>
<li>Go to Options (X on keyboard)</li>
<li>Go to Preferences</li>
<li>Set <code>UDP Enabled</code> to <code>On</code></li>
<li>Set <code>UDP Frequency</code> to <code>60</code></li>
<li>UDP Port should be <code>7576</code></li>
<li>UDP Host should be <code>224.0.0.150</code></li>
<li>Save (spacebar on keyboard)</li>
</ol>
<h1 id="richard-burns-rally-rsf">Richard Burns Rally( RSF)</h1>
<ol>
<li>Open the RSF Launcher</li>
<li>Enable the Adanced options</li>
<li>Go to the Telemetry Tab</li>
<li>The telemetry should be <code>127.0.0.1</code> : <code>6776</code></li>
<li>Enable the <code>UDP Telemetry</code> checkbox.</li>
<li>Launch the game</li>
</ol>
<h1 id="rfactor-2">rFactor 2</h1>
<ol>
<li>Open the installation folder of rFactor 2</li>
<li>Go to <code>Bin64\\Plugins</code></li>
<li>Download the shared memory plugin <a href="https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file">https://www.mediafire.com/file/s6ojcr9zrs6q9ls/rf2_sm_tools_3.7.15.1.zip/file</a></li>
<li>Open the downloaded zip file and look for the <code>rFactor2SharedMemoryMapPlugin64.dll</code></li>
<li>Extract that .dll file to the folder you&#39;ve opened</li>
<li>Launch the game</li>
<li>Go to Options -&gt; Gameplay: Set <code>rFactor2SharedMemoryMapPlugin64</code> to <code>ON</code></li>
<li>Restart the game</li>
</ol>
<h1 id="wrc-generations">WRC Generations</h1>
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
