using ACC_Manager.Util;
using ACC_Manager.Util.Settings;
using ACCManager.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ACCManager.Controls.Settings.AccUiSettings
{
    /// <summary>
    /// Interaction logic for AccServerListSettings.xaml
    /// </summary>
    public partial class AccServerListSettings : UserControl
    {
        private AccSettingsJson _accSettings = AccSettings.LoadJson();
        private readonly AccServerListSettingsJson _accServerListSettingsJson = new AccServerListSettingsJson();
        private readonly UnlistedServersSettingsJson _unlistedServerSettingsJson = new UnlistedServersSettingsJson();

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

                    AccServerListJson serverList = _accServerListSettingsJson.LoadJson();
                    serverList.LeagueServerIp = unlistedServer.Server;
                    _accServerListSettingsJson.SaveJson(serverList);
                };
                toggle.Unchecked += (s, ev) =>
                {
                    _accSettings.UnlistedAccServer = Guid.Empty;
                    AccSettings.SaveJson(_accSettings);
                    _accServerListSettingsJson.Delete();
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

            UnlistedServersJson unlistedServersJson = _unlistedServerSettingsJson.LoadJson();

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
                var serverList = _unlistedServerSettingsJson.LoadJson();
                serverList.UnlistedServers.Remove(unlistedAccServer);
                _unlistedServerSettingsJson.SaveJson(serverList);
                menu.IsOpen = false;
            };

            menu.Items.Add(deleteTagButton);

            return menu;
        }

        public class AccServerListJson : IGenericSettingsJson
        {
            public string LeagueServerIp { get; set; }
        }
        internal class AccServerListSettingsJson : AbstractSettingsJson<AccServerListJson>
        {
            public override string Path => FileUtil.AccPath + "Config\\";
            public override string FileName => "serverList.json";

            public override AccServerListJson Default()
            {
                return new AccServerListJson()
                {
                    LeagueServerIp = String.Empty
                };
            }
        }

        public class UnlistedAccServer
        {
            public Guid Guid { get; set; } = Guid.Empty;
            public string Name { get; set; }
            public string Description { get; set; }
            public string Server { get; set; }
        }
        public class UnlistedServersJson : IGenericSettingsJson
        {
            public List<UnlistedAccServer> UnlistedServers { get; set; }
        }
        internal class UnlistedServersSettingsJson : AbstractSettingsJson<UnlistedServersJson>
        {
            public override string Path => FileUtil.AccManangerSettingsPath;
            public override string FileName => "ACC.json";

            public override UnlistedServersJson Default()
            {
                return new UnlistedServersJson()
                {
                    UnlistedServers = new List<UnlistedAccServer>()
                };
            }
        }
    }
}
