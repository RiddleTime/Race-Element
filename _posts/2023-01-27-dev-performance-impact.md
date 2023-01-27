---
layout: post
title: Performance impact
description: Don\'t take shortcuts
tags: dev
---

### Initial thoughts
Creating UI Elements as a developer will make you take shortcuts, such as using easier to design frameworks such as NodeJS which allow you to use HTML/CSS. But in contrast to native solutions this has a very big hit on performance. In theory using NodeJS and Electron allows you to create desktop \"web applications\". But in no way they were supposed to be used to create HUDs. Applications using these kind of frameworks are impacted by extraordinary memory usage and most of the time unnecessary cpu and gpu usage.  

### Going native
To improve the performance impact of HUDs the only way is to go native and not use any overhead created by additional framework layers. This methodology initially required extra effort but is paying off in the long-term. Race Element is only using approximately 130 MegaBytes of ram whilst displaying all available HUDs. Where as other solutions quickly creep up to 500 MegaBytes of ram for the same UI Elements.

### Further thoughts
Using OpenGL and DirectX is another solution to provide HUDs. It still has to be seen whether using more low level access to the gpu has any extra benefit. Though it can improve the user experience since it would not require users to run their simulator in windowed borderless mode.
