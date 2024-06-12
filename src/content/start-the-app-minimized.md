---
title: "How To: Start the app minimized"
slug: start-the-app-minimized
description: How to start the app in minimized mode
type: guide 
---
# Startup Parameter
Race Element has got a command line parameter that allows you to start it minimized: `/StartMinimized`

# Starting the app and ACC using one batch file.
You could make a batch file that for example starts race element in minimized mode and launches Assetto Corsa Competizione through steam.
```
START "Race Element" "PATH_TO_FOLDER_OF_RACE_ELEMENT\RaceElement.exe" /StartMinimized
START steam://rungameid/805550
```
