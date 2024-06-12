const e="---\ntitle: Twitch Chat Bot Commands\nslug: twitch-chat-bot-commands\ndescription: A list of all available chat bot commands and how to use them\ntype: guide \n---\nEvery Race Element Chat Command starts with: `+`\n\n# Available Commands\n**`+app`** *Links to the Race Element website and discord.*\n**`+ahead`  `+behind`** *Race info for the car ahead or behind, based on position, not relative.*\n**`+p`** *Race info for the car at the requested global position. Use like `+p 1`.*\n**`+#`** *Race info for the car with the requested entry number. Use like `+# 992`.*\n**`+temps`** *Current ambient and track temperature, with more detail when the car is being driven.*\n**`+damage`** *Total damage in repair time, this data is only available when the driver is in the car.*\n**`+purple`** *Best valid lap in the lobby for the current session.*\n**`+potential`** *Potential best lap based on the fastest sectors from valid laps. Requires valid laps.*\n**`+session`** *Current Session type.*\n**`+track`** *Current track.*\n**`+car`** *Current car.*\n**`+angle`** *Steering angle(lock-to-lock) for the current car.*\n**`+fuel`** *Calculates fuel, parameters are `[minutes] [liters/lap] [laptime]`, use like `+fuel 60 3 2:10`.*\n";export{e as default};
