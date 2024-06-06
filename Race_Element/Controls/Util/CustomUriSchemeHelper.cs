using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;


namespace RaceElement.Controls.Util;
/// <summary>
/// From https://github.com/gro-ove/actools
/// </summary>
public static class CustomUriSchemeHelper
{
    public static bool OptionRegisterEachTime = true;

    private const string UriSchemeLabel = "raceelement";
    public const string UriScheme = UriSchemeLabel + ":";

    private const string ClassName = "raceelement";
    private const string AppTitle = "Race Element";

    private const string Version = "1";

    private const string KeyRegisteredLocation = "CustomUriSchemeHelper.RegisteredLocation";
    private const string KeyRegisteredVersion = "CustomUriSchemeHelper.RegisteredVersion";

    private static void RegisterClass(string className, string title, bool urlProtocol, int iconId, bool isEnabled, string openCommand)
    {
        var path = $@"Software\Classes\{className}";
        if (!isEnabled)
        {
            Registry.CurrentUser.DeleteSubKeyTree(path);
            return;
        }

        using (var key = Registry.CurrentUser.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree))
        {
            if (key == null) return;

            if (urlProtocol)
            {
                key.SetValue("URL Protocol", "", RegistryValueKind.String);
            }

            key.SetValue("", title, RegistryValueKind.String);
            string assemblyStart = AppContext.BaseDirectory + Path.DirectorySeparatorChar + "RaceElement.exe";
            using (var iconKey = key.CreateSubKey(@"DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {

                iconKey?.SetValue("", $@"{assemblyStart},{iconId}", RegistryValueKind.String);
            }

            using (var commandKey = key.CreateSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                commandKey?.SetValue("", string.Format(openCommand, $@"""{assemblyStart}"""), RegistryValueKind.String);
            }
        }
    }

    private static void RegisterExtension(string ext, string description, bool isEnabled, int iconId)
    {
        var className = $@"{ClassName}{ext.ToLowerInvariant()}";
        RegisterClass(className, description, false, iconId, isEnabled, @"{0} ""%1""");

        var path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + ext;
        if (!isEnabled)
        {
            Registry.CurrentUser.DeleteSubKeyTree(path);
            return;
        }

        using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + ext,
                RegistryKeyPermissionCheck.ReadWriteSubTree))
        {
            if (key == null) return;
            using (var progIds = key.CreateSubKey(@"OpenWithProgids", RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                progIds?.SetValue(className, new byte[0], RegistryValueKind.None);
            }

            try
            {
                using (var choiceKey = key.CreateSubKey(@"UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    choiceKey?.SetValue("Progid", className, RegistryValueKind.String);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private static void EnsureRegistered()
    {

        try
        {
            var isEnabled = true;
            var isFilesIntegrationEnabled = true;

            if (isEnabled)
            {
                RegisterClass(ClassName, AppTitle, true, 0, true, @"{0} ""%1""");
                RegisterExtension(@".accsetup", "Provides support for ACC Setups", isFilesIntegrationEnabled, 1);
                //RegisterExtension(@".acreplay", ToolsStrings.Common_AcReplay, isFilesIntegrationEnabled, 2);
                //RegisterExtension(@".cmpreset", ToolsStrings.Common_CmPreset, isFilesIntegrationEnabled, 3);
            }

            //ValuesStorage.Set(KeyRegisteredLocation, MainExecutingFile.Location);
            //ValuesStorage.Set(KeyRegisteredVersion, Version);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Can’t register: " + e);
        }
    }

    public static void Initialize()
    {
        EnsureRegistered();
    }

    public static void RegisterDiscordClass(string discordAppId)
    {
        try
        {
            RegisterClass($@"discord-{discordAppId}", AppTitle, true, 0, true, @"{0} ""--discord-cmd=%1""");
        }
        catch (Exception e)
        {
            Debug.WriteLine("Can’t register: " + e);
        }
    }

    private static void OnSettingsChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        //if (propertyChangedEventArgs.PropertyName == nameof(SettingsHolder.CommonSettings.RegistryMode))
        //{
        //    EnsureRegistered();
        //}
    }
}

