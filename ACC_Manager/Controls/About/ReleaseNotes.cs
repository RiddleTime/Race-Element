using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls
{
    public static class ReleaseNotes
    {
        public readonly static Dictionary<string, string> Notes = new Dictionary<string, string>()
        {
            {"0.0.5.2", "- Setup conversions added by KrisV147:"+
                        "\n-Ferrari 488 Challenge Evo 2020"+
                        "\n- Tyre pressure trace: now auto-detects gt3/gt3(gtc)/wet tyres"},
            {"0.0.5.1", "- Setup conversions added: Audi R8 LMS Evo 2019, Bentley Continental GT3 2015, BMW M6 GT3 2017, Lamborghini Gallardo G3 Reiter 2017, Lamborghini Huracán ST 2015, McLaren 650S GT3 2015, Mercedes-AMG GT3 2015 and Porsche 911 II GT3 Cup 2017." },
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
