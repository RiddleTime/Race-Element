const e=`---
title: Twitch Chat Bot Commands
slug: twitch-chat-bot-commands
description: A list of all available chat bot commands and how to use them
type: guide
---

<p><strong>Every Race Element Chat Command starts with: <code>+</code></strong></p>
<h1 id="available-commands">Available Commands</h1>
<ul>
<li><strong><code>+app</code></strong> <em>Links to the Race Element website and discord.</em></li>
<li><strong><code>+ahead</code> <code>+behind</code></strong> <em>Race info for the car ahead or behind, based on position, not relative.</em></li>
<li><strong><code>+p</code></strong> <em>Race info for the car at the requested global position. Use like <code>+p 1</code>.</em></li>
<li><strong><code>+#</code></strong> <em>Race info for the car with the requested entry number. Use like <code>+# 992</code>.</em></li>
<li><strong><code>+diff</code></strong> <em>Shows the difference in lap times and sectors for the currently viewed car vs the requested car. Use like <code>+diff ahead</code>, <code>+diff behind</code>, <code>+diff p 1</code>, <code>+diff # 1</code></em></li>
<li><strong><code>+temps</code></strong> <em>Current ambient and track temperature, with more detail when the car is being driven.</em></li>
<li><strong><code>+damage</code></strong> <em>Total damage in repair time, this data is only available when the driver is in the car.</em></li>
<li><strong><code>+purple</code></strong> <em>Best valid lap in the lobby for the current session.</em></li>
<li><strong><code>+potential</code></strong> <em>Potential best lap based on the fastest sectors from valid laps. Requires valid laps.</em></li>
<li><strong><code>+session</code></strong> <em>Current Session type.</em></li>
<li><strong><code>+track</code></strong> <em>Current track.</em></li>
<li><strong><code>+car</code></strong> <em>Current car.</em></li>
<li><strong><code>+angle</code></strong> <em>Steering angle(lock-to-lock) for the current car.</em></li>
<li><strong><code>+fuel</code></strong> <em>Calculates fuel, parameters are <code>[minutes] [liters/lap] [laptime]</code>, use like <code>+fuel 60 3 2:10</code>.</em></li>
</ul>
`;export{e as default};
