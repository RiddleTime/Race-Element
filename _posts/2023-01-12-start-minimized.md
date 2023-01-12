---
layout: post
title: Starting the app minimized
description: How to start the app in minimized mode
tags: guide
---

## Start the app minimized

Race Element has got a command line paramater that allows you to start it minimized: `/StartMinimized`

### Starting the app and ACC using one batch file.
You could make a batch file that for example starts race element in minimized mode and launches Assetto Corsa Competizione through steam.
```
START "ACC Manager" "PATH_TO_FOLDER_OF_RACE_ELEMENT\RaceElement.exe" /StartMinimized
START steam://rungameid/805550
```
