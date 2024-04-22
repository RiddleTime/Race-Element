---
layout: post
title: Twitch Chat Bot Commands
description: A list of all available chat bot commands and how to use them
tags: guide
---

## Race Element Chat Commands
Every Race Element Chat Commands starts with a `+`

## Available Commands
### `+app`
The link to the Race Element website and discord.
### `+damage`
The total damage in repair time, this data is only available when the streamer is actually driving the car.
### `+potential`
Calculates the potential best lap based on the fastest sectors from valid laps. Requires the driver to have set a valid lap.
### `+temps`
The current ambient and track temperature, will provide more information when the streamer is driving the car.
### `+purple`
The best valid lap in the lobby for the current session.
### `+ahead` and `+behind`
Race info for the car ahead or behind, this is based on the race position of the car that is currently viewed, so not relative on track.
### `+p`
Race info for the car at the requested global position. Use like `+p 1` to gain information about the car in position 1.
### `+#`
Race info for the car with the requested entry number. Use like `+# 992` to gain information about the car with entry/race number 992.
### `+session`
The current session type.
### `+track`
The current track.
### `+car`
The current car.
### `+angle`
The steering angle(lock-to-lock) for the current car.
