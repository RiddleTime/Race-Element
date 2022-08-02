using ACC_Manager.Util.SystemExtensions;
using ACCManager.Util;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {

        public About()
        {
            InitializeComponent();

            buttonDiscord.Click += (sender, e) => Process.Start("https://discord.gg/26AAEW5mUq"); ;
            buttonGithub.Click += (sender, e) => Process.Start("https://github.com/RiddleTime/ACC-Manager");


            this.Loaded += (s, e) => Task.Run(new Action(CheckNewestVersion));

            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue)
                    FillReleaseNotes();
                else
                    stackPanelReleaseNotes.Children.Clear();

                ThreadPool.QueueUserWorkItem(x =>
                {
                    Thread.Sleep(5 * 1000);
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                });
            };
        }

        private async void CheckNewestVersion()
        {
#if DEBUG
            TitleBar.Instance.SetAppTitle("Dev");
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("ACC-Manager"), new Uri("https://github.com/RiddleTime/ACC-Manager.git"));
                var allTags = await client.Repository.GetAllTags("RiddleTime", "ACC-Manager");

                if (allTags != null && allTags.Count > 0)
                {
                    RepositoryTag latest = allTags.First();

                    long localVersion = VersionToLong(Assembly.GetEntryAssembly().GetName().Version);
                    long remoteVersion = VersionToLong(new Version(latest.Name));

                    if (localVersion > remoteVersion)
                        TitleBar.Instance.SetAppTitle("Beta");

                    if (remoteVersion > localVersion)
                    {
                        Release release = await client.Repository.Release.GetLatest("RiddleTime", "ACC-Manager");

                        if (release != null)
                        {
                            await Dispatcher.BeginInvoke(new Action(() =>
                             {
                                 MainWindow.Instance.EnqueueSnackbarMessage($"A new version of ACC Manager is available: {latest.Name}");
                                 Button openReleaseButton = new Button()
                                 {
                                     Margin = new Thickness(5, 0, 0, 0),
                                     Content = $"Download {latest.Name} at GitHub",
                                     ToolTip = $"Release notes:\n{release.Body}"
                                 };
                                 ToolTipService.SetShowDuration(openReleaseButton, int.MaxValue);
                                 openReleaseButton.Click += (s, e) => Process.Start(release.HtmlUrl);
                                 ReleaseStackPanel.Children.Add(openReleaseButton);
                             }));
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        private long VersionToLong(Version VersionInfo)
        {
            string major = $"{VersionInfo.Major}".FillStart(4, '0');
            string minor = $"{VersionInfo.Minor}".FillStart(4, '0');
            string build = $"{VersionInfo.Build}".FillStart(4, '0');
            string revision = $"{VersionInfo.Revision}".FillStart(4, '0');
            string versionString = major + minor + build + revision;

            long.TryParse(versionString, out long version);
            return version;
        }

        private void FillReleaseNotes()
        {
            stackPanelReleaseNotes.Children.Clear();
            ReleaseNotes.Notes.ToList().ForEach(note =>
            {
                TextBlock noteTitle = new TextBlock()
                {
                    Text = note.Key,
                    Style = Resources["MaterialDesignBody1TextBlock"] as Style,
                    FontWeight = FontWeights.Bold,
                    FontStyle = FontStyles.Oblique
                };
                TextBlock noteDescription = new TextBlock()
                {
                    Text = note.Value,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style
                };

                StackPanel changePanel = new StackPanel() { Margin = new Thickness(0, 10, 0, 0) };
                changePanel.Children.Add(noteTitle);
                changePanel.Children.Add(noteDescription);

                stackPanelReleaseNotes.Children.Add(changePanel);
            });
        }
    }
}
