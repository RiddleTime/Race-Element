const e=`---
title: "Setup Hider for Streaming"
slug: setup-hider
description: Setting up the setup hider with OBS or Streamlabs
type: guide 
---

# How does it work?
Assetto Corsa Competizione has data for developers that allows us to see when the Setup Screen is open. Race Element uses this with the integration of streamlabs or OBS. It sends a signal to the configured streaming software to make a Source item visible or to hide it based on the data from Assetto Corsa Competizione.

# OBS
Make sure you are running OBS version 28 or higher.
1. *OBS*: **Add Any type of Source to your active Scene, name it \`SetupHider\`. This can be text, image, video, a website, basically anything even a Scene Source.**
2. *OBS*: **At the top menu: Click Tools and Open the WebSocket Server Settings.**
3. *OBS*: **In the WebSocket Server Settings window, Click \`Show Connect Info\`**
4. *Race Element:* **Open the Main Settings Tab on the left -> Open the Streaming Tab.**
5. *Race Element:* **Set the Software to \`OBS\`**
6. *OBS/Race Element*: **Copy the Server IP, Server Port and Server Password from OBS and paste them in Race Element's OBS Settings.**
7. *Race Element:*: **Click Save and then at the bottom Enable the SetupHider.**

# StreamLabs
1. *StreamLabs*: **Add Any type of Source to your active Scene, name it \`SetupHider\`. This can be text, image, video, a website, basically anything even a Scene Source.**
2. *Race Element:* **Open the Main Settings Tab on the left -> Open the Streaming Tab.**
3. *Race Element:* **Set the Software to \`StreamLabs\`.**
4. *Race Element:* **Click Save and then at the bottom Enable the SetupHider.**
`;export{e as default};
