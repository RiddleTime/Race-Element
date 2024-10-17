const e=`---
title: "Using the Liveries Tab"
slug: how-to-use-liveries
description: ACC Liveries and Race Element
type: guide
---

<p>The Liveries tab in Race Element provides functionality to view liveries for Assetto Corsa Competizione. These liveries consist of a json file and a folder where the actual decals and sponsors are located.</p>
<h1 id="browsing-liveries">Browsing liveries</h1>
<p>By right clicking a livery in the list, options appear to open the json file of the livery and the custom skin directory of that livery. 
This should give you easy access to the correct folder when for example creating a livery and you want to open up the files inside.
You can also Delete the livery files, this is permanent so be warned!</p>
<p>If you have manually imported a livery you can click the refresh button to rescan the livery folders.</p>
<h1 id="importing-liveries">Importing liveries</h1>
<p>You can either Drag and Drop &quot;.rar, .7z and .zip&quot; on top of Race Element or click the Import Liveries button and select them by hand. </p>
<p>The importer will scan the archive for the livery json files and according to these files will look for the custom skins folders.
Once a list appears of succesfully imported liveries you can set a tag for liveries by highlighting them.</p>
<h1 id="exporting-liveries">Exporting liveries</h1>
<p>A single livery can be exported by right clicking the livery and clicking &quot;Save Skin as zip archive&quot;.
Skin packs can be created by right clicking single liveries/Teams/Cars and clicking Add to Skin Pack.
After you have chosen the liveries you can decide to not include the dds files as these will increase the size of your skin pack.</p>
<h1 id="tagging-liveries">Tagging Liveries</h1>
<p>Liveries once imported can be tagged with a so called Tag. Livery Tags can be exported by right clicking in them in the list.
Before you can tag a livery you will need to create a new Tag which can be done in the Tags tab.</p>
<h1 id="generating-ddsdirectdraw-surface-files">Generating DDS(DirectDraw Surface) files</h1>
<p>The Liveries tool allows you to generate the DDS files which the game uses in the showroom and in the races.
There is a button to generate them in bulk or when the selected skin contains for example a sponsors.png, but no sponsors_1.dds, a button will appear in the display panel to generate the files. This might take a while and takes use of your GPU.</p>
`;export{e as default};
