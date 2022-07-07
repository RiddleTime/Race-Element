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

            FillReleaseNotes();

            this.Loaded += (s, e) => ThreadPool.QueueUserWorkItem(x => CheckNewestVersion());
        }

        private async void CheckNewestVersion()
        {
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
                    {
                        await Dispatcher.BeginInvoke(new Action(() =>
                        {
#if DEBUG
                            TitleBar.Instance.SetAppTitle("Dev");
#else
                            TitleBar.Instance.SetAppTitle("Beta");
#endif
                        }));

                    }

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
        }

        private long VersionToLong(Version VersionInfo)
        {
            string major = $"{VersionInfo.Major}".FillStart(4, '0');
            string minor = $"{VersionInfo.Minor}".FillStart(4, '0');
            string build = $"{VersionInfo.Build}".FillStart(4, '0');
            string revision = $"{VersionInfo.Revision}".FillStart(4, '0');
            string versionString = major + minor + build + revision;

            long version;
            long.TryParse(versionString, out version);
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
