const e=`---
title: "How To: HUDs and Streaming"
slug: huds-and-streaming
description: Settings up the HUDs with OBS or Streamlabs
type: guide 
---

# Recording the Entire Desktop
If you want to share all your HUDs you can record the entire desktop since the HUDs themselves are not part of the game.

# Recording a Single HUD

1. *Race Element:* **Enable the 'Window' option for the HUD you want to show.**
2. *Race Element:* **Enable Always Visible (blue eye button in HUD Tab).**
3. *Stream Software:* **Add a new \`Window capture\` source in your stream software.**
4. *Stream Software:* **Set the Capture method to \`\`\`Windows Graphics Capture\`\`\`**
5. *Stream Software:* **Select the HUD Window.**
6. *Stream Software:* **Set Window match priority to \`\`\`Window title must match\`\`\`**
7. *(optional) Race Element:* **If you don't want to see the hud and only use it for your stream, disable the always on top option for that HUD.**
`;export{e as default};
