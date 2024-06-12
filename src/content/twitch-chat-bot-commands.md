---
title: Twitch Chat Bot Commands
slug: twitch-chat-bot-commands
description: A list of all available chat bot commands and how to use them
type: guide 
---
**Every Race Element Chat Command starts with: `+`**

# Available Commands
**`+app`** *Links to the Race Element website and discord.*
**`+ahead`  `+behind`** *Race info for the car ahead or behind, based on position, not relative.*
**`+p`** *Race info for the car at the requested global position. Use like `+p 1`.*
**`+#`** *Race info for the car with the requested entry number. Use like `+# 992`.*
**`+temps`** *Current ambient and track temperature, with more detail when the car is being driven.*
**`+damage`** *Total damage in repair time, this data is only available when the driver is in the car.*
**`+purple`** *Best valid lap in the lobby for the current session.*
**`+potential`** *Potential best lap based on the fastest sectors from valid laps. Requires valid laps.*
**`+session`** *Current Session type.*
**`+track`** *Current track.*
**`+car`** *Current car.*
**`+angle`** *Steering angle(lock-to-lock) for the current car.*
**`+fuel`** *Calculates fuel, parameters are `[minutes] [liters/lap] [laptime]`, use like `+fuel 60 3 2:10`.*
