const a=`---
title: "Starting Race Element Minimized"
slug: start-the-app-minimized
description: How to start the app in minimized mode
type: guide
---

<h1 id="startup-parameter">Startup Parameter</h1>
<p>Race Element has got a command line parameter that allows you to start it minimized: <code>/StartMinimized</code></p>
<h1 id="starting-the-app-and-acc-using-one-batch-file">Starting the app and ACC using one batch file.</h1>
<p>You could make a batch file that for example starts race element in minimized mode and launches Assetto Corsa Competizione through steam.</p>
<pre><code><span class="token constant">START</span> <span class="token string">"Race Element"</span> <span class="token string">"PATH_TO_FOLDER_OF_RACE_ELEMENT\\RaceElement.exe"</span> <span class="token operator">/</span>StartMinimized
<span class="token constant">START</span> steam<span class="token operator">:</span><span class="token operator">/</span><span class="token operator">/</span>rungameid<span class="token operator">/</span><span class="token number">805550</span></code></pre>`;export{a as default};
