using ACC_Manager.Util.Settings;
using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ACCManager.Controls.Settings.AccUiSettings.AccServerListSettings.AccServerListSettingsFile;
using static ACCManager.Controls.Settings.AccUiSettings.AccServerListSettings.UnlistedServerListSettingsFile;

namespace ACCManager.Controls.Settings.AccUiSettings
{
    /// <summary>
    /// Interaction logic for AccServerListSettings.xaml
    /// </summary>
    public partial class AccServerListSettings : UserControl
    {
        private AccSettingsJson _accSettings = AccSettings.LoadJson();

        public AccServerListSettings()
        {
            InitializeComponent();

            this.Loaded += (s, e) => FillListView();
            this.listViewServers.SelectionChanged += ListViewServers_SelectionChanged;
        }

        private void ListViewServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewServers.SelectedItems.Count > 0)
            {
                stackPanelServerDescription.Children.Clear();

                var unlistedServer = ((listViewServers.SelectedItem as ListViewItem).DataContext as UnlistedAccServer);

                StackPanel togglePanel = new StackPanel() { Orientation = Orientation.Horizontal, Cursor = Cursors.Hand };
                ToggleButton toggle = new ToggleButton();
                toggle.Checked += (s, ev) =>
                {
                    _accSettings.UnlistedAccServer = unlistedServer.Guid;
                    AccSettings.SaveJson(_accSettings);

                    AccServerListJson serverList = AccServerListSettingsFile.LoadJson();
                    serverList.LeagueServerIp = unlistedServer.Server;
                    SaveJson(serverList);
                };
                toggle.Unchecked += (s, ev) =>
                {
                    _accSettings.UnlistedAccServer = Guid.Empty;
                    AccSettings.SaveJson(_accSettings);
                    AccServerListSettingsFile.Delete();
                };
                Label toggleLabel = new Label() { Content = " Activate" };

                if (_accSettings.UnlistedAccServer == unlistedServer.Guid)
                    toggle.IsChecked = true;

                togglePanel.MouseLeftButtonUp += (s, ev) => toggle.IsChecked = !toggle.IsChecked; ;

                togglePanel.Children.Add(toggle);
                togglePanel.Children.Add(toggleLabel);

                stackPanelServerDescription.Children.Add(togglePanel);
                stackPanelServerDescription.Children.Add(new Label()
                {
                    Content = $"{unlistedServer.Name}"
                });
                stackPanelServerDescription.Children.Add(new Label()
                {
                    Content = $"{unlistedServer.Description}"
                });
                stackPanelServerDescription.Children.Add(new Label()
                {
                    Content = $"{unlistedServer.Server}"
                });
            }
        }

        private void FillListView()
        {
            _accSettings = AccSettings.LoadJson();

            UnlistedServersJson unlistedServersJson = UnlistedServerListSettingsFile.LoadJson();

            listViewServers.Items.Clear();
            if (unlistedServersJson.UnlistedServers != null)
                foreach (var unlistedAccServer in unlistedServersJson.UnlistedServers)
                {
                    TextBlock serverTextBlock = new TextBlock()
                    {
                        Text = $"{unlistedAccServer.Name}",
                    };

                    if (unlistedAccServer.Guid == _accSettings.UnlistedAccServer)
                        serverTextBlock.Foreground = Brushes.Green;

                    ListViewItem listItem = new ListViewItem
                    {
                        Content = serverTextBlock,
                        DataContext = unlistedAccServer
                    };

                    listItem.ContextMenu = GetListContextMenu(unlistedAccServer);

                    listViewServers.Items.Add(listItem);
                }
        }

        private ContextMenu GetListContextMenu(UnlistedAccServer unlistedAccServer)
        {
            ContextMenu menu = new ContextMenu()
            {
                Style = Resources["MaterialDesignContextMenu"] as Style,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0))
            };

            Button deleteTagButton = new Button()
            {
                Content = $"Delete",
                CommandParameter = unlistedAccServer,
                Style = Resources["MaterialDesignRaisedButton"] as Style,
                Margin = new Thickness(0),
                Height = 30,
                ToolTip = "Warning! This permantely deletes this entry.",
                VerticalAlignment = VerticalAlignment.Center,
            };
            deleteTagButton.Click += (e, s) =>
            {
                var serverList = UnlistedServerListSettingsFile.LoadJson();
                serverList.UnlistedServers.Remove(unlistedAccServer);
                SaveJson(serverList);
                menu.IsOpen = false;
            };


            menu.Items.Add(deleteTagButton);

            return menu;
        }

        internal class AccServerListSettingsFile
        {
            public class AccServerListJson
            {
                public string LeagueServerIp { get; set; }

                public static AccServerListJson Default()
                {
                    return new AccServerListJson()
                    {
                        LeagueServerIp = String.Empty
                    };
                }
            }

            private const string FileName = "serverList.json";
            private static FileInfo ServerListFile => new FileInfo(FileUtil.AccPath + "Config\\" + FileName);

            public static AccServerListJson LoadJson()
            {
                if (!ServerListFile.Exists)
                    return AccServerListJson.Default();

                try
                {
                    using (FileStream fileStream = ServerListFile.OpenRead())
                    {
                        return ReadJson(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return AccServerListJson.Default();
            }

            public static AccServerListJson ReadJson(Stream stream)
            {
                string jsonString = string.Empty;
                try
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        jsonString = reader.ReadToEnd();
                        jsonString = jsonString.Replace("\0", "");
                        reader.Close();
                        stream.Close();
                    }

                    return JsonConvert.DeserializeObject<AccServerListJson>(jsonString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                return AccServerListJson.Default();
            }

            public static void SaveJson(AccServerListJson accSettings)
            {
                string jsonString = JsonConvert.SerializeObject(accSettings, Formatting.Indented);

                if (!ServerListFile.Exists)
                    if (!Directory.Exists(FileUtil.AccManangerSettingsPath))
                        Directory.CreateDirectory(FileUtil.AccManangerSettingsPath);

                File.WriteAllText(FileUtil.AccManangerSettingsPath + FileName, jsonString);
            }

            public static void Delete()
            {
                if (ServerListFile.Exists)
                    ServerListFile.Delete();
            }
        }

        internal class UnlistedServerListSettingsFile
        {
            public class UnlistedAccServer
            {
                public Guid Guid { get; set; } = Guid.Empty;
                public string Name { get; set; }
                public string Description { get; set; }
                public string Server { get; set; }
            }

            public class UnlistedServersJson
            {
                public List<UnlistedAccServer> UnlistedServers { get; set; }

                public static UnlistedServersJson Default()
                {
                    return new UnlistedServersJson()
                    {
                        UnlistedServers = new List<UnlistedAccServer>()
                    };
                }
            }

            private const string FileName = "ACC.json";
            private static FileInfo AccSettingsFile => new FileInfo(FileUtil.AccManangerSettingsPath + FileName);

            public static UnlistedServersJson LoadJson()
            {
                if (!AccSettingsFile.Exists)
                    return UnlistedServersJson.Default();

                try
                {
                    using (FileStream fileStream = AccSettingsFile.OpenRead())
                    {
                        return ReadJson(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                return UnlistedServersJson.Default();
            }

            public static UnlistedServersJson ReadJson(Stream stream)
            {
                string jsonString = string.Empty;
                try
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        jsonString = reader.ReadToEnd();
                        jsonString = jsonString.Replace("\0", "");
                        reader.Close();
                        stream.Close();
                    }

                    return JsonConvert.DeserializeObject<UnlistedServersJson>(jsonString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                return UnlistedServersJson.Default();
            }

            public static void SaveJson(UnlistedServersJson accSettings)
            {
                string jsonString = JsonConvert.SerializeObject(accSettings, Formatting.Indented);

                if (!AccSettingsFile.Exists)
                    if (!Directory.Exists(FileUtil.AccManangerSettingsPath))
                        Directory.CreateDirectory(FileUtil.AccManangerSettingsPath);

                File.WriteAllText(FileUtil.AccManangerSettingsPath + FileName, jsonString);
            }
        }

    }
}
