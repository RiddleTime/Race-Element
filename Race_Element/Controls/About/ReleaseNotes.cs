using System.Collections.Generic;

namespace RaceElement.Controls
{
    public static class ReleaseNotes
    {
        internal readonly static Dictionary<string, string> Notes = new Dictionary<string, string>()
        {
            {"0.1.6.9", "- Fuel Info HUD: Add option to change fuel bar color based on fuel percentage."+
                        "- Add Laptime Table HUD: configurable rows and information."},
            {"0.1.6.8", "- HUD:"+
                        "\n  - Fix: Dropdown menus not saving."+
                        "\n  - Input Trace: customizable height."+
                        "\n  - Oversteer Trace:"+
                        "\n    - Customizable height."+
                        "\n    - Increase selectable max slip angle."},
            {"0.1.6.7", "- Added Laptime Delta HUD: a customizable lap delta bar." },
            {"0.1.6.6", "- Data tab: Added context menu to copy race weekends to clipboard and show them in windows explorer."+
                        "\n- Input Bars HUD: Add proper option to either set the bars horizontal of vertical. Vertical is default."+
                        "\n- Inputs HUD is now known as Steering HUD: Added additional text display options and setting color."},
            {"0.1.6.5", "- Setup Browser: Added option to copy a setup json directly to the clipboard."+
                        "\n- Reworked all custom context menus in the app: Added icons and improved design."+
                        "\n- Tyre Info HUD: Reworked tyre temperature text."},
            {"0.1.6.4", "- Internal optimizations."+
                        "\n- Prevent HUDs from showing during replay whilst being in a session."},
            {"0.1.6.3", "- Added color picker to Current Gear HUD."},
            {"0.1.6.2", "- Added splash screen when starting the app."+
                        "\n- Added Current Gear HUD."},
            {"0.1.6.1", "- Hotfix: Fix app crash during movement mode activation." },
            {"0.1.6.0", "- Better precision and clarity for Shift Indicator HUD."+
                        "\n- Improved visibility of IMO/OMI coloring for Tyre Info HUD."+
                        "\n- Improved memory management of HUDs."},
            {"0.1.5.9", "- Shift Indicator HUD:" +
                        "\n  - Added configurable opacity for each color state."+
                        "\n  - Added background for RPM Text where as the font size now scales with bar height."},
            {"0.1.5.8", "- Damage HUD will now display during movement mode even if autohide is enabled."+
                        "\n- Colors of Shift Indicator HUD can now be adjusted."},
            {"0.1.5.7", "- Improved rendering quality of pressures in tyre info hud." },
            {"0.1.5.6", "- Shift Indicator HUD: Revert original base color." },
            {"0.1.5.5", "- HUDs: When the Window toggle is enabled the HUD windows will now be always enabled and allow for better detection by Streaming and VR apps."+
                        "\n- Reworked Tyre Info HUD, added guidelines when repositioning and always display psi."+
                        "\n   - Added option to show 2 decimals for tyre pressures."},
            {"0.1.5.4", "- Reworked Input and Oversteer trace."+
                        "\n- Reworked background of shift indicator."+
                        "\n- Reworked internal structure of the app."+
                        "\n- Updated About Tab."+
                        "\n- Open livery json now opens a new explorer window and selects the livery json file."},
            {"0.1.5.3", "- HUDs: Added Opacity Slider, this will allow you to set the overal transparency for each HUD separately."+
                        "\n- Fix setup hider." },
            {"0.1.5.2", "- Liveries can now be dragged and dropped ontop of the app."+
                        "\n- Liveries Viewer: Added competitor name to viewer if it's used in the livery."+
                        "\n- Further design changes according to rebrand."},
            {"0.1.5.1", "- Reworked Titlebar, Icon and About Tab."+
                        "\n- HUD: Added boost gauge, showing boost percentage."},
            {"0.1.5.0", "- ACC Manager is now known as Race Element."+
                        "\n- Removed request for admin rights, if you have run as admin enabled, disable it."+
                        "\n- You can copy all the entire contents of the ACC Manager folder to the Race Element folder. This to keep all your data and settings."},
            {"0.1.4.1", "- Added request for admin rights, the app uses this to detect whether ACC is running. Without these rights you may experience unexpected app crashes." },
            {"0.1.4.0", "- Reworked hud internals."+
                        "\n- Reduced idle cpu usage to almost 0% when ACC is not running."},
            {"0.1.3.2", "- Fix bug when repositioning huds."},
            {"0.1.3.1", "- Internal optimizations."+
                        "\n- Improved contrast of hud text panels."+
                        "\n- HUDs now show when the engine is not running."},
            {"0.1.3.0", "- Design of Damage HUD, blends in better. Increased Font Size." },
            {"0.1.2.2", "- HUD: Added Damage HUD, displaying body damage in repair time and suspension damage in percentage."+
                        "\n- Added track condition to basic lap telemetry recording."},
            {"0.1.2.1", "- Reworked design of several HUDs."+
                        "\n   - Input bars: Added option to switch around throttle and brake bars."+
                        "\n   - Inputs: Added gradual contrast."+
                        "\n   - Input and Oversteer Traces: Added gradual contrast."},
            {"0.1.2.0", "- Main folder changed from My Documents/ACC Manager to %AppData%/ACC Manager."+
                        "\n   - You can copy the old Data folder containing your race weekends to the new location."+
                        "\n- HUD Configuration:" +
                        "\n   - Redesign of HUD Configuration Controls: Grouped Controls."+
                        "\n   - This resets your HUD Configuration. Unfortunately ;)"+
                        "\n   - Right clicking inside of the configuration controls toggles activation of viewed HUD."+
                        "\n   - Scroll the sliders to change the value."+
                        "\n- Added button to ACC Manager settings to open the ACC Manager folder."},
            {"0.1.1.2", "- HUD: Input bars overlay now has the option to set the bars to horizontal mode."+
                        "\n- HUD: Added beta version of Race Info Overlay."},
            {"0.1.1.1", "- Changed dry tyres for Lamborghini Super Trofeo cars to GT4." },
            {"0.1.1.0", "- Auto Updater added: in the about tab when a new version is released there will be a 1 click button to update the app."+
                        "\n- HUDs: Improvements to maintaining rendering frequency (Thank you FBalazs)."+
                        "\n- ACC Manager: Fixed persistence of minimize app to system tray option."},
            {"0.1.0.5", "- Liveries Tab: Added button to refresh the livery trees."+
                        "\n- ACC Manager: Added option to minimize the app to the system tray (default off)."+
                        "\n- Decrease startup time."+
                        "\n- Added startup counter (global)."},
            {"0.1.0.3", "- HUDs: Added Always On top option to each overlay, allowing you to hide the overlay in the game whilst streaming using the Window option."+
                        "\n- HUD: Added Input Bars Overlay, displaying live throttle and brake inputs with vertical progress bars."+
                        "\n- Liveries: Removed generating, import and export of dds_0 files."},
            {"0.1.0.1", "- OBS Websocket updated to version 5, using the setup hider now requires OBS version 28 or higher."+
                        "\n- Updated design theme."+
                        "\n- HUD tab: the title & description is now 1 big button to toggle hud activation."},
            {"0.0.9.0", "- Added Race Weekends tab to Telemetry tab."+
                        "\n- ACC Manager now saves race weekend data(lap and sector times)."+
                        "\n- Titlebar: Added icons for automatic steering lock and the stream setup hider."+
                        "\n- Setup Importer drag and drop now works on any part of the app."+
                        "\n- Changed order of main menu tabs." },
            {"0.0.8.2", "- HUD: Added Oversteer trace overlay. Displaying under and oversteer."+
                        "\n- HUDs tab: Double click overlays on the list items to toggle overlays on and off."+
                        "\n- Setups tab: Add strategy section to viewer and comparison."+
                        "\n- Internal memory usage enhancements."},
            {"0.0.8.1", "- Setups tab: Right click to copy a setup to another track."+
                        "\n- Setups Viewer: Reworked the table cell alignment."},
            {"0.0.8.0", "- HUD tab: New Design showing previews of overlays."+
                        "\n- Setup Comparison: Highlights differences for each individual value with colors."+
                        "\n- ECU Maps: Updated (By Mominon)."},
            {"0.0.7.9", "- TreeViews: Fixed open/close bug when clicking the leaves."+
                        "\n- Livery Viewer: Increased snappiness and decreased memory usage."+
                        "\n- ACC Settings: Added serverlist so you can manage unlisted servers and quickly enable or disable the serverList.json."+
                        "\n- HUDs: Overlay options will only show when hovering else the description of the overlay will be displayed."+
                        "\n- HUDs: Add a Window option to each overlay, allowing it to be captured by streaming applications."},
            {"0.0.7.8", "- Setups: Added Setup Importer, drag and drop your setup json file in the Setup Page/Tab."+
                        "\n- HUDs: Added a preview mode."+
                        "\n- HUD: Increased performance for Shift Indicator."},
            {"0.0.7.7", "- HUDs: Multiple monitors are now supported." +
                        "\n- HUDs: Increased performance for info tables/panels."},
            {"0.0.7.6", "- ACC Manager now remembers the last main tab you opened."+
                        "\n- ACC Manager now remembers the last position of the GUI."+
                        "\n- HUD: Added Debug Output overlay showing all kind of logging."+
                        "\n- HUD: Increased performance for several overlays."+
                        "\n- HUD: When hovering an overlay in the GUI, spacebar or enter key enabled/disables the overlay."+
                        "\n- Lap tracker: Fixed an issue with finalizing lap data."},
            {"0.0.7.5", "- Setup Conversion: Fixed for McLaren 720s."+
                        "\n- HUD: Shift indicator now draws vertical lines for every 1000 rpm available."},
            {"0.0.7.4", "- Hardware Settings: Added automatic steering hard lock based per car."+
                        "\n- HUD: Made stint info configurable for Fuel Info overlay."+
                        "\n- HUD: Input trace overlay can now be sized up to 800 datapoints (be aware of cpu usage)."},
            {"0.0.7.3", "- Stream settings: Fixed a crash (sigh..)." },
            {"0.0.7.2","- HUD: Current tyre set is now configurable for the Car Info Overlay."+
                       "\n- Stream settings: Added Setup Hider for OBS and Streamlabs."},
            {"0.0.7.1", "- HUDs: Improve rendering performance."+
                        "\n- HUDs: Prevent huds from rendering when pausing during hotlap mode."+
                        "\n- HUD: Added Shift Indicator overlay, including an option to display when the pit limiter is activated."},
            {"0.0.7.0", "- HUD: Added configurable data collection/refresh rate for input trace."+
                        "\n- HUD: Lap delta overlay's recorded laps are now reset in between practice/qualifying/race."},
            {"0.0.6.9", "- HUD: Added configurable potentional best lap time to lap delta overlay."+
                        "\n- HUD: Fixed brake temps multiplication in tyre info overlay."},
            {"0.0.6.8", "- HUDs: Overlays now also render when reposition is enabled."+
                        "\n- HUD: Fixed fuel progress bar for Fuel Info Overlay."},
            {"0.0.6.7", "- HUDs: Improved 'should render' status detection."+
                        "\n- HUD: Fixed average brake temps in Tyre Info Overlay."+
                        "\n- HUD: Added Inputs Overlay by Floriwan."+
                        "\n- Livery exporter: Added option to exclude dds files."},
            {"0.0.6.6", "- HUD: Added configurable data point amount for the Input Trace Overlay."+
                        "\n- HUD: Lap Delta bar outlines positive or negative delta color."+
                        "\n- HUD: Added subtle value background and row lines to info table and panel."+
                        "\n- HUD: Added mouse cursor when repositioning overlays."},
            {"0.0.6.5", "- Liveries tab: Added tree item in tag tree showing all cars that aren't tagged yet."+
                        "\n- HUD: Reposition hotkey has been changed to (Ctrl + Home), it was conflicting with other global hotkeys."+
                        "\n- HUD: Changed font for values in the info panel and added drop shadow."+
                        "\n- HUD: Lap Delta overlay now displays sector delta based on your fastest sectors."},
            {"0.0.6.4", "- HUD: Added hotkey (Ctrl + Shift + M) for enabling the reposition of overlays"+
                        "\n- HUD: Overlays can now be scaled on with 1% steps, providing more precision."+
                        "\n- HUD: Overlays can now be moved with the arrow keys as well for finetuning of position."+
                        "\n- Added startup argument to start in minimized window state: ( /StartMinimized )."+
                        "\n- Added Tyre Info overlay, this can be placed on top of the kunos tyre info."},
            {"0.0.6.3", "- Lap Data Collector: Fix Average Fuel usage calculation after adding new fuel." },
            {"0.0.6.2", "- Integrate broadcast data, available for overlay developers."+
                        "\n- HUD: Added Car Info Overlay."+
                        "\n- HUD: Accelerometer Overlay is now scalable."+
                        "\n- HUD: Fuel Info Overlay:"+
                        "\n    - Fuel bar turns red when fuel tank reaches 15% or lower."+
                        "\n    - Fuel Time is green if you have enough for stint/session and red if not."+
                        "\n    - Suggested fuel no longer shows negative values."+
                        "\n    - Fuel buffer laps now user selectable. Choice of 0-3 laps (Default: 0)"+
                        "\n- HUD: Lap Delta Overlay:"+
                        "\n    - Added configurable Lap Type (in/out/regular)."+
                        "\n    - Added configurable Max Delta value."+
                        "\n    - Added tooltips."+
                        "\n- HUD: Track Info Overlay:" +
                        "\n    - Added configurable Time of Day information."+
                        "\n    - Added tooltips."},
            {"0.0.6.1", "- Improve logging for overlays." },
            {"0.0.6.0", "- HUD Tab:"+
                        "\n    - Scroll the scale control to adjust the scale."+
                        "\n    - Click Scroll/middle mouse button overlay lines to enable reposition."+
                        "\n    - Left Click overlay line to toggle."+
                        "\n- HUD: ECU Map Overlay can now be scaled and repositioned."+
                        "\n- Trees in the GUI now expand on single click."},
            {"0.0.5.9", "- About Tab: Added information about HUDs."+
                        "\n- Added Play ACC Button, launches Assetto Corsa Competizione using the steam link."+
                        "\n- HUD: Added Fuel Info Overlay (By KrisV147)."+
                        "\n- HUD: Added Lap Delta Overlay."+
                        "\n- HUD Tab: Some overlays are now scalable."},
            {"0.0.5.8", "- HUD Tab: Added design."+
                        "\n- HUD: Added option to input trace for disabling steering input."},
            {"0.0.5.7", "- HUD: Added Track Info overlay."+
                        "\n- HUD: Added Accelerometer overlay."+
                        "\n- HUD: Some overlays can be configured."},
            {"0.0.5.6", "- HUD: Certain overlays can now be repositioned, this position will be automatically saved."+
                        "\n- HUD: Enabled overlays will be re-enabled during the next start of ACC Manager."},
            {"0.0.5.5", "- The Livery browser will re-expand previously expanded items after refreshing."+
                        "\n- Added hotkey(Delete) to quickly open up the delete livery interface."},
            {"0.0.5.4", "- Decrease draw rate for tyre pressure trace by 90%."+
                        "\n- Updated system.net.http reference due to possible vulnerability."},
            {"0.0.5.3", "- Added screen that pops-up after importing liveries, allowing you to tag them."+
                        "\n- Setup conversions added by KrisV147:"+
                        "\n - BMW M2 Cup 2020."+
                        "\n - Lamborghini Huracán ST Evo2 2021."+
                        "\n - Porsche 992 GT3 Cup 2021."+
                        "\n - All existing in-game cars are now supported for the setup viewer/comparison."},
            {"0.0.5.2", "- Setup conversion added: Ferrari 488 Challenge Evo 2020(By KrisV147)."+
                        "\n- Tyre pressure trace: now auto-detects gt3/gt4(gtc)/wet tyres."+
                        "\n- Tyre pressure trace: draws specific parts in the trace by color, green = good, red is too high, blue is too low."},
            {"0.0.5.1", "- Setup conversions added:"+
                        "\n - Audi R8 LMS Evo 2019."+
                        "\n - Bentley Continental GT3 2015."+
                        "\n - BMW M6 GT3 2017."+
                        "\n - Lamborghini Gallardo G3 Reiter 2017."+
                        "\n - Lamborghini Huracán ST 2015."+
                        "\n - McLaren 650S GT3 2015."+
                        "\n - Mercedes-AMG GT3 2015."+
                        "\n - Porsche 911 II GT3 Cup 2017." },
            {"0.0.5.0", "- Added ECU map HUD (bottom-right), still very simple look."+
                        "\n- Repositioned Input Trace HUD."+
                        "\n- Added upper and lower bounds to Tires Traces (working for gt3 dry only atm)."},
            {"0.0.4.9", "- Setup conversions added by KrisV147:"+
                        "\n - Audi R8 LMS GT4 2016."+
                        "\n - BMW M4 GT4 2018."+
                        "\n - Chevrolet Camaro GT4 R 2017."+
                        "\n - Ginetta G55 GT4 2012."+
                        "\n - Ktm Xbow GT4 2016."+
                        "\n - Maserati Gran Turismo MC GT4 2016."+
                        "\n - McLaren 570s GT4 2016."+
                        "\n - Mercedes AMG GT4 2016."+
                        "\n - Porsche 718 Cayman GT4 MR 2019."+
                        "\n- Livery Browser: displays sub-item count in tree header." +
                        "\n- HUD: Tire pressure input trace changed."},
            {"0.0.4.8", "- HUD tab: Added overlays: Very very simple tire pressure trace overlay and a shared memory static data overlay." },
            {"0.0.4.7", "- Added HUD tab: Simple checkbox to enable the input trace overlay." },
            {"0.0.4.6", "- Setup conversion added: Aston Martin V12 Vantage GT3 2013(by KrisV147), Alpine A110 GT4(by KrisV147) and Aston Martin Vantage AMR GT4 2018(by KrisV147)." },
            {"0.0.4.5", "- Input Trace: Should be drawing less gpu cost." },
            {"0.0.4.4", "- Telemetry Debug: Add test overlay for input traces. Enabled with Draw On Game checkbox in Telemetry Debug (doesn't work with full-screen mode)." },
            {"0.0.4.3", "- Livery Displayer: Sponsors Image is now displayed ontop of Decals image."+
                        "\n- Livery Displayer: Revamped layout to increase size of livery preview."+
                        "\n- Livery Importer: Improved feedback for user when importing an unsupported archive."+
                        "\n- Livery Tagger: When adding tags to cars, only the Tags tab in the Livery Browser will be updated."+
                        "\n- Setup conversion added: Emil Frey Jaguar G3 2012."},
            {"0.0.4.2", "- Livery Tags: Cars under a tag are now alphabetically sorted."+
                        "\n- ACCM: Allow window resizing."+
                        "\n- Livery Displayer: preview images scale with window resizing."},
            {"0.0.4.1", "- Livery Tags: Cannot enter an empty tag name anymore."+
                        "\n- Livery Browser: Now displays liveries without a team name set."},
            {"0.0.4.0", "- Livery Browser: Add livery tagging system." },
            {"0.0.3.9", "- Livery Deleter: shows the to be deleted skin in the livery displayer."+
                        "\n- Add new app icon."+
                        "\n- Fix issue with importer which caused acc manager to keep a file stream open."+
                        "\n- Add more botched import scenarios for livery archives that only contain 1 livery."},
            {"0.0.3.8", "- Setup conversion added: Aston Martin V8 Vantage GT3 2019(by FBalazs)."+
                        "\n- Livery Browser: added context menu option to delete liveries."},
            {"0.0.3.7", "- Setups tab: Car names have the year added."+
                        "\n- Setups tab: Added button to right click menu to open car directory."+
                        "\n- Setups tab: Added button to browser tab to refresh the setup list."+
                        "\n- Setup conversion added: Lamborghini Huracán GT3 2015, Nissan GT-R Nismo GT3 2015, Ferrari 488 GT3 2018."+
                        "\n- Livery Importer: Added 1 file filter for all supported types."+
                        "\n- Livery Importer: Add strategy for botched archives (containing no separate folders)."},
            {"0.0.3.6", "- About tab: When a new release is available a new button allows you to visit the latest release and download the newest version." },
            {"0.0.3.5", "- About tab: Added buttons for Discord and GitHub."+
                        "\n- Bulk DDS Generator: generate button is disabled when all DDS files have been generated."},
            {"0.0.3.4", "- Added Bulk DDS Generator: Got many skins without dds files? this will generate the dds files. Sit back and relax." },
            {"0.0.3.3", "- Livery browser: added two tabs for grouping style, Car model type and Teams."+
                        "\n- Livery viewer: fix crash when generating dds files."},
            {"0.0.3.2", "- Fuel calcalator: added button to fetch data from game."+
                        "\n- Livery browser: fix for fetching the liveries to display them in the list."},
            {"0.0.3.1", "- Telemetry: Display Array data" },
            {"0.0.3.0", "- Add Telemetry tab, very basic for now, might still have some data bugs, also not all data is shown as data yet."+
                        "\n- Add car model name to livery viewer."},
            {"0.0.2.10","- Add features panel to about tab" },
            {"0.0.2.9", "- Setup conversion added: Bentley Continental GT3 2018, Lamborghini Huracán GT3 Evo, Lexus RC F GT3, Nissan GT-R Nismo GT3 2018." },
            {"0.0.2.8", "- Added exception handling so the manager will not crash for windows version lower than 11." },
            {"0.0.2.7", "- Livery exporter: Added skin pack exporter, right click teams or skins to add."+
                        "\n- ACC Manager Folder: Added dedicated folder for the acc Manager, My Documents/ACC Manager."+
                        "\n- Added logger which logs to the dedicated folder/Log."},
            {"0.0.2.6", "- Icon: Something not too fancy, but it shows what the tool does."+
                        "\n- Setup conversion: fixed Porsche 911 II GT3."},
            {"0.0.2.5", "- Livery Displayer: Added button to generate dds files."+
                        "\n- Livery Exporter: Now includes dds files if available."},
            {"0.0.2.4", "- Livery Displayer: Added buttons to open livery json and livery folder."+
                        "\n- Livery Displayer: Added nationality, Paint materials for body and rims."},
            {"0.0.2.3", "- Add setup conversion for: Mercedes-AMG GT3 2020, BMW M4 GT3, Ferrari 488 GT3 Evo."+
                        "\n- Fuel calculator: fixed fuel value parsing when windows language is set differently."},
            {"0.0.2.2", "- Livery exporter: exported zips are now according to acc folder structure."+
                        "\n- Livery exporter: export team with all the skins inside in a single zip."+
                        "\n- Livery importer: added support for archives with multiple liveries."},
            {"0.0.2.1", "- Livery Browser: Skins are grouped by team name." },
            {"0.0.2.0", "- Add Livery Browser, displays: team name, display name, car number, decals image and sponsors image."+
                        "\n- Add Livery exporter: right click livery to save as zip."+
                        "\n- Add Livery importer: supports (.7z, .rar, .zip), can import multiple archives at once, though multiple liveries per archive is not supported yet."},
            {"0.0.1.5", "- Right click track in setup browser to open folder in file explorer." },
            {"0.0.1.4", "- Add Simple Fuel Calculator." },
            {"0.0.1.3", "- Setup comparison now highlights setup when different."+
                        "\n- Add setup conversion for: Audi R8 LMS, Honda NSX GT3, Porsche 911 GT3 R."},
            {"0.0.1.2", "- Added more icons."+
                "\n- Added custom Titlebar."},
            {"0.0.1.1", "- Setup Browser added."+
                        "\n- Setup Comparison added: Right click setups in the setup browser to add them to the comparison."+
                        "\n- Add setup conversion for: Audi R8 LMS evo II(by Jubka), Honda NSX GT3 Evo, Mclaren 720S GT3, Porsche II GT3 R."},
            {"0.0.1.0", "- Material Design added." },
        };
    }
}
