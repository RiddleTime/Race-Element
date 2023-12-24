using MaterialDesignThemes.Wpf;
using RaceElement.Controls.Util;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RaceElement.Controls;

/// <summary>
/// Interaction logic for AccServerListSettings.xaml
/// </summary>
public partial class AccServerListSettings : UserControl
{
    private readonly AccSettings _accSettings;
    private AccSettingsJson _accSettingsJson;

    private readonly AccServerListSettingsJson _accServerListSettingsJson = new();
    private readonly UnlistedServersSettingsJson _unlistedServerSettingsJson = new();

    public AccServerListSettings()
    {
        InitializeComponent();
        _accSettings = new AccSettings();
        _accSettingsJson = _accSettings.Get();

        this.Loaded += (s, e) => FillListView();
        this.listViewServers.SelectionChanged += ListViewServers_SelectionChanged;
        buttonNew.Click += ButtonNew_Click;
    }

    private void ButtonNew_Click(object sender, RoutedEventArgs e)
    {
        stackPanelServerDescription.Children.Clear();

        StackPanel namePanel = new() { Orientation = Orientation.Horizontal };
        Label nameLabel = new() { Content = "Name", Width = 80 };
        TextBox nameBox = new() { Width = 150 };
        namePanel.Children.Add(nameLabel);
        namePanel.Children.Add(nameBox);

        StackPanel descriptionPanel = new() { Orientation = Orientation.Horizontal };
        Label descriptionLabel = new() { Content = "Description", Width = 80 };
        TextBox descriptionBox = new() { Width = 150 };
        descriptionPanel.Children.Add(descriptionLabel);
        descriptionPanel.Children.Add(descriptionBox);


        StackPanel serverPanel = new() { Orientation = Orientation.Horizontal };
        Label serverLabel = new() { Content = "Server", Width = 80 };
        TextBox serverBox = new() { Width = 150 };
        serverPanel.Children.Add(serverLabel);
        serverPanel.Children.Add(serverBox);

        StackPanel buttonPanel = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        Button saveButton = new() { Content = "Save" };
        saveButton.Click += (s, ev) =>
        {
            UnlistedAccServer newServer = new()
            {
                Name = nameBox.Text,
                Description = descriptionBox.Text,
                Server = serverBox.Text,
                Guid = Guid.NewGuid()
            };

            if (newServer.Name != string.Empty && newServer.Description != string.Empty && newServer.Server != string.Empty)
            {
                var unlistedServerSettings = _unlistedServerSettingsJson.Get();
                unlistedServerSettings.UnlistedServers.Add(newServer);
                _unlistedServerSettingsJson.Save(unlistedServerSettings);
                MainWindow.Instance.EnqueueSnackbarMessage("Saved new unlisted server.");
                stackPanelServerDescription.Children.Clear();
                FillListView();
            }
        };
        buttonPanel.Children.Add(saveButton);
        Button cancelButton = new() { Content = "Cancel", Margin = new Thickness(5, 0, 0, 0) };
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

            StackPanel togglePanel = new() { Orientation = Orientation.Horizontal, Cursor = Cursors.Hand };
            ToggleButton toggle = new();
            if (_accSettingsJson.UnlistedAccServer == unlistedServer.Guid)
                toggle.IsChecked = true;
            Label toggleLabel = new() { Content = toggle.IsChecked.Value ? " Deactivate" : " Activate" };

            toggle.Checked += (s, ev) =>
            {
                _accSettingsJson.UnlistedAccServer = unlistedServer.Guid;
                _accSettings.Save(_accSettingsJson);

                AccServerListJson serverList = _accServerListSettingsJson.Get();
                serverList.leagueServerIp = unlistedServer.Server;
                _accServerListSettingsJson.Save(serverList);
                FillListView();
                toggleLabel.Content = toggle.IsChecked.Value ? " Deactivate" : " Activate";
            };
            toggle.Unchecked += (s, ev) =>
            {
                _accSettingsJson.UnlistedAccServer = Guid.Empty;
                _accSettings.Save(_accSettingsJson);
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
        _accSettingsJson = _accSettings.Get();

        UnlistedServersJson unlistedServersJson = _unlistedServerSettingsJson.Get();

        listViewServers.Items.Clear();

        if (unlistedServersJson.UnlistedServers != null)
            foreach (var unlistedAccServer in unlistedServersJson.UnlistedServers)
            {
                TextBlock serverTextBlock = new()
                {
                    Text = $"{unlistedAccServer.Name}",
                    Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                };

                if (unlistedAccServer.Guid == _accSettingsJson.UnlistedAccServer)
                    serverTextBlock.Foreground = Brushes.Green;

                ListViewItem listItem = new()
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
        ContextMenu menu = ContextMenuHelper.DefaultContextMenu();

        MenuItem deleteEntry = ContextMenuHelper.DefaultMenuItem("Delete", PackIconKind.Delete);
        deleteEntry.ToolTip = "Warning! This permanently deletes this entry.";
        deleteEntry.Click += (e, s) =>
        {
            var serverList = _unlistedServerSettingsJson.Get();
            serverList.UnlistedServers.Remove(unlistedAccServer);
            _unlistedServerSettingsJson.Save(serverList);
            menu.IsOpen = false;

            if (_accSettingsJson.UnlistedAccServer == unlistedAccServer.Guid)
                _accServerListSettingsJson.Delete();

            stackPanelServerDescription.Children.Clear();
            FillListView();
        };
        menu.Items.Add(deleteEntry);

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
        public override string Path => FileUtil.RaceElementSettingsPath;
        public override string FileName => "ACC_UnlistedServers.json";

        public override UnlistedServersJson Default()
        {
            return new UnlistedServersJson() { UnlistedServers = new List<UnlistedAccServer>() };
        }
    }
}
