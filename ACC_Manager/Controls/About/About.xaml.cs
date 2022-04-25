using ACCSetupApp.SetupParser;
using ACCSetupApp.Util;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        // https://qwerty.dev/whitespace/
        private string FourEmSpace = " ";

        public About()
        {
            InitializeComponent();

            buttonDiscord.Click += (sender, e) => System.Diagnostics.Process.Start("https://discord.gg/26AAEW5mUq"); ;
            buttonGithub.Click += (sender, e) => System.Diagnostics.Process.Start("https://github.com/RiddleTime/ACC-Manager");

            SetCarConversionFeatures();
            FillReleaseNotes();
            ThreadPool.QueueUserWorkItem(x => CheckNewestVersion());
        }


        private void SetCarConversionFeatures()
        {
            List<string> gt3Names = ConversionFactory.GetAllNamesByClass(SetupConverter.CarClasses.GT3);
            textBlockSetupViewerGT3.Text = $"{FourEmSpace}GT3:\n";
            for (int i = 0; i < gt3Names.Count; i++)
            {
                textBlockSetupViewerGT3.Text += $"{FourEmSpace}- {gt3Names[i]}";
                if (i < gt3Names.Count - 1)
                {
                    textBlockSetupViewerGT3.Text += "\n";
                }
            }

            List<string> gt4Names = ConversionFactory.GetAllNamesByClass(SetupConverter.CarClasses.GT4);
            textBlockSetupViewerGT3.Text += $"\n\n{FourEmSpace}GT4:\n";
            if (gt4Names.Count > 0)
            {
                for (int i = 0; i < gt4Names.Count; i++)
                {
                    textBlockSetupViewerGT3.Text += $"{FourEmSpace}- {gt4Names[i]}";
                    if (i < gt4Names.Count - 1)
                    {
                        textBlockSetupViewerGT3.Text += "\n";
                    }
                }
            }
            else
            {
                textBlockSetupViewerGT3.Text += "None yet\n";
            }
        }

        private async void CheckNewestVersion()
        {
#if DEBUG
            return;
#endif
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("ACC-Manager"), new Uri("https://github.com/RiddleTime/ACC-Manager.git"));
                var allTags = await client.Repository.GetAllTags("RiddleTime", "ACC-Manager");

                if (allTags != null && allTags.Count > 0)
                {
                    RepositoryTag latest = allTags.First();
                    Debug.WriteLine($"Latest version: {latest.Name}");
                    if (!GetAssemblyFileVersion().Equals(latest.Name))
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
                                 openReleaseButton.Click += (s, e) => Process.Start(release.HtmlUrl);
                                 ReleaseStackPanel.Children.Add(openReleaseButton);
                             }));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(ex);
            }
        }

        private static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
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
