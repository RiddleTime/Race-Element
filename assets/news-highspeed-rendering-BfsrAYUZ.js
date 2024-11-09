const e=`---
title: Highspeed Rendering
slug: highspeed-rendering
description: Rendering at 200 Hz with minimal CPU usage
date: 2024-11-09
type: news
---

<h1 id="intro">Intro</h1>
<p>Simulators are able to provide data at rates higher than 60 Hz, monitors these days are able to render beyond 100 Hz, so why are HUDs in most cases not rendering at high speeds?
It&#39;s obvious that higher refresh rates cost more computing power but thanks to clever engineering Race Element can render quick with minimal overhead.</p>
<h1 id="efficiency-and-feedback">Efficiency and Feedback</h1>
<p>The first HUD in Race Element that is able to render at 200 Hz is the newly added Shift Bar HUD. It will allow you to spot the right moment instead of losing laptime whilst waiting for a 30-60 hz shift bar to blink.
Another HUD which demonstrates the highspeed rendering is the 3D HUD in the pitwall, which can ultimately push the limits of the current render pipeline at 500Hz and beyond.</p>
<h1 id="what-is-next">What is next?</h1>
<p>After experiencing the shift from 60 Hz to 200 hz it is only a matter of time for other HUDs to gain quicker refresh rates. 
The new Shift Bar HUD serves as an example that Race Element&#39;s renderer is not only quick but that it&#39;s capable doing so with minimal cpu usage. </p>
`;export{e as default};
