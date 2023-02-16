using RaceElement.Util.SystemExtensions;
using RaceElement.Controls.Util.Updater;
using RaceElement.Util;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class Info : UserControl
    {
        private bool HasAddedDownloadButton = false;


        public Info()
        {
            InitializeComponent();

            buttonDiscord.Click += (sender, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start https://discord.gg/26AAEW5mUq",
                WindowStyle = ProcessWindowStyle.Hidden,
            });
            buttonGithub.Click += (sender, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start https://github.com/RiddleTime/Race-Element",
                WindowStyle = ProcessWindowStyle.Hidden,
            });
            buttonDonate.Click += (sender, e) => Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start https://paypal.me/CompetizioneManager",
                WindowStyle = ProcessWindowStyle.Hidden,
            });

            new Thread(() => CheckNewestVersion()).Start();

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
            Thread.Sleep(2000);

            RemoveTempVersionFile();
#if DEBUG
            TitleBar.Instance.SetAppTitle("Dev");
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected

            try
            {
                if (HasAddedDownloadButton)
                    return;

                GitHubClient client = new GitHubClient(new ProductHeaderValue("Race-Element"), new Uri("https://github.com/RiddleTime/Race-Element.git"));
                var allTags = await client.Repository.GetAllTags("RiddleTime", "Race-Element");

                if (allTags != null && allTags.Count > 0)
                {
                    RepositoryTag latest = allTags.First();

                    long localVersion = VersionToLong(Assembly.GetEntryAssembly().GetName().Version);
                    long remoteVersion = VersionToLong(new Version(latest.Name));

                    if (localVersion > remoteVersion)
                        TitleBar.Instance.SetAppTitle("Beta");

                    if (remoteVersion > localVersion)
                    {
                        Release release = await client.Repository.Release.GetLatest("RiddleTime", "Race-Element");

                        if (release != null)
                        {
                            var accManagerAsset = release.Assets.Where(x => x.Name == "RaceElement.exe").First();

                            await Dispatcher.BeginInvoke(new Action(() =>
                             {
                                 MainWindow.Instance.EnqueueSnackbarMessage($"A new version of Race Element is available: {latest.Name}", " Open About tab ", new Action(() => { MainWindow.Instance.tabAbout.Focus(); }));
                                 Button openReleaseButton = new Button()
                                 {
                                     Margin = new Thickness(0, 0, 0, 0),
                                     Content = $"Update to {latest.Name}",
                                     ToolTip = $"Release notes:\n{release.Body}"
                                 };
                                 ToolTipService.SetShowDuration(openReleaseButton, int.MaxValue);
                                 openReleaseButton.Click += (s, e) =>
                                 {
                                     openReleaseButton.IsEnabled = false;
                                     MainWindow.Instance.EnqueueSnackbarMessage($"Updating to version... {latest.Name}, this may take a while..");
                                     new Thread(x =>
                                     {
                                         new AppUpdater().Update(accManagerAsset);
                                     }).Start();
                                 };
                                 ReleaseStackPanel.Children.Add(openReleaseButton);
                                 HasAddedDownloadButton = true;
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

        private void RemoveTempVersionFile()
        {
            try
            {
                string tempTargetFile = $"{FileUtil.RaceElementAppDataPath}AccManager.exe";
                FileInfo tempFile = new FileInfo(tempTargetFile);

                if (tempFile.Exists)
                    tempFile.Delete();
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(e);
            }
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
            Dispatcher.BeginInvoke(new Action(() =>
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
            }));
        }
    }
}
