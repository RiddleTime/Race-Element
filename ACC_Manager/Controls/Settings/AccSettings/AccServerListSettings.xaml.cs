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
            buttonNew.Click += ButtonNew_Click;
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            stackPanelServerDescription.Children.Clear();

            StackPanel namePanel = new StackPanel() { Orientation = Orientation.Horizontal };
            Label nameLabel = new Label() { Content = "Name", Width = 80 };
            TextBox nameBox = new TextBox() { Width = 150 };
            namePanel.Children.Add(nameLabel);
            namePanel.Children.Add(nameBox);

            StackPanel descriptionPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            Label descriptionLabel = new Label() { Content = "Description", Width = 80 };
            TextBox descriptionBox = new TextBox() { Width = 150 };
            descriptionPanel.Children.Add(descriptionLabel);
            descriptionPanel.Children.Add(descriptionBox);


            StackPanel serverPanel = new StackPanel() { Orientation = Orientation.Horizontal };
            Label serverLabel = new Label() { Content = "Server", Width = 80 };
            TextBox serverBox = new TextBox() { Width = 150 };
            serverPanel.Children.Add(serverLabel);
            serverPanel.Children.Add(serverBox);

            StackPanel buttonPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
            Button saveButton = new Button() { Content = "Save" };
            saveButton.Click += (s, ev) =>
            {
                UnlistedAccServer newServer = new UnlistedAccServer()
                {
                    Name = nameBox.Text,
                    Description = descriptionBox.Text,
                    Server = serverBox.Text,
                    Guid = Guid.NewGuid()
                };

                if (newServer.Name != string.Empty && newServer.Description != string.Empty && newServer.Server != string.Empty)
                {
                    var unlistedServerSettings = _unlistedServerSettingsJson.LoadJson();
                    unlistedServerSettings.UnlistedServers.Add(newServer);
                    _unlistedServerSettingsJson.SaveJson(unlistedServerSettings);
                    MainWindow.Instance.EnqueueSnackbarMessage("Saved new unlisted server.");
                    stackPanelServerDescription.Children.Clear();
                    FillListView();
                }
            };
            buttonPanel.Children.Add(saveButton);
            Button cancelButton = new Button() { Content = "Cancel", Margin = new Thickness(5, 0, 0, 0) };
            cancelButton.Click += (s, ev) => stackPanelServerDescription.Children.Clear();
            buttonPanel.Children.Add(cancelButton);

            stackPanelServerDescription.Children.Add(namePanel);
            stackPanelServerDescription.Children.Add(descriptionPanel);
            stackPanelServerDescription.Children.Add(serverPanel);
            stackPanelServerDescription.Children.Add(buttonPanel);
        }

        private void ListViewServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewServers.SelectedItems.Count > 0)
            {
                stackPanelServerDescription.Children.Clear();

                var unlistedServer = ((listViewServers.SelectedItem as ListViewItem).DataContext as UnlistedAccServer);

                StackPanel togglePanel = new StackPanel() { Orientation = Orientation.Horizontal, Cursor = Cursors.Hand };
                ToggleButton toggle = new ToggleButton();
                if (_accSettings.UnlistedAccServer == unlistedServer.Guid)
                    toggle.IsChecked = true;
                Label toggleLabel = new Label() { Content = toggle.IsChecked.Value ? " Deactivate" : " Activate" };

                toggle.Checked += (s, ev) =>
                {
                    _accSettings.UnlistedAccServer = unlistedServer.Guid;
                    AccSettings.SaveJson(_accSettings);

                    AccServerListJson serverList = _accServerListSettingsJson.LoadJson();
                    serverList.leagueServerIp = unlistedServer.Server;
                    _accServerListSettingsJson.SaveJson(serverList);
                    FillListView();
                    toggleLabel.Content = toggle.IsChecked.Value ? " Deactivate" : " Activate";
                };
                toggle.Unchecked += (s, ev) =>
                {
                    _accSettings.UnlistedAccServer = Guid.Empty;
                    AccSettings.SaveJson(_accSettings);
                    _accServerListSettingsJson.Delete();
                    FillListView();
                    toggleLabel.Content = toggle.IsChecked.Value ? " Deactivate" : " Activate";
                };


                togglePanel.MouseLeftButtonUp += (s, ev) => toggle.IsChecked = !toggle.IsChecked; ;
                togglePanel.Children.Add(toggle);
                togglePanel.Children.Add(toggleLabel);
                stackPanelServerDescription.Children.Add(togglePanel);

                stackPanelServerDescription.Children.Add(new Label()
                {
                    Content = $"Name: {unlistedServer.Name}"
                });
                stackPanelServerDescription.Children.Add(new Label()
                {
                    Content = $"Description: {unlistedServer.Description}"
                });
                stackPanelServerDescription.Children.Add(new Label()
                {
                    Content = $"Server address: {unlistedServer.Server}"
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
                        Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    };

                    if (unlistedAccServer.Guid == _accSettings.UnlistedAccServer)
                        serverTextBlock.Foreground = Brushes.Green;

                    ListViewItem listItem = new ListViewItem
                    {
                        Content = serverTextBlock,
                        DataContext = unlistedAccServer,
                        ContextMenu = GetListContextMenu(unlistedAccServer),
                    };

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

                if (_accSettings.UnlistedAccServer == unlistedAccServer.Guid)
                    _accServerListSettingsJson.Delete();

                stackPanelServerDescription.Children.Clear();
                FillListView();
            };

            menu.Items.Add(deleteTagButton);

            return menu;
        }

        public class AccServerListJson : IGenericSettingsJson
        {
#pragma warning disable IDE1006 // Naming Styles
            public string leagueServerIp { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }
        internal class AccServerListSettingsJson : AbstractSettingsJson<AccServerListJson>
        {
            public override string Path => FileUtil.AccPath + "Config\\";
            public override string FileName => "serverList.json";

            public override AccServerListJson Default()
            {
                return new AccServerListJson() { leagueServerIp = String.Empty };
            }
        }

        public class UnlistedAccServer
        {
            public Guid Guid { get; set; } = Guid.Empty;
            public string Name { get; set; }
            public string Description { get; set; }
            public string Server { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is UnlistedAccServer)
                {
                    return this.Guid == ((UnlistedAccServer)obj).Guid;
                }
                return false;
            }
        }
        public class UnlistedServersJson : IGenericSettingsJson
        {
            public List<UnlistedAccServer> UnlistedServers { get; set; } = new List<UnlistedAccServer>();
        }
        internal class UnlistedServersSettingsJson : AbstractSettingsJson<UnlistedServersJson>
        {
            public override string Path => FileUtil.AccManangerSettingsPath;
            public override string FileName => "ACC_UnlistedServers.json";

            public override UnlistedServersJson Default()
            {
                return new UnlistedServersJson() { UnlistedServers = new List<UnlistedAccServer>() };
            }
        }
    }
}
