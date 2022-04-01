using ACCSetupApp.Controls.Setup;
using ACCSetupApp.SetupParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for SetupBrowser.xaml
    /// </summary>
    public partial class SetupBrowser : UserControl
    {
        private readonly ConversionFactory conversionFactory;
        private readonly FlowDocSetupRenderer setupRenderer;

        public SetupBrowser()
        {
            InitializeComponent();

            this.conversionFactory = new ConversionFactory();
            this.setupRenderer = new FlowDocSetupRenderer();

            FetchAllSetups();

            setupsTreeView.SelectedItemChanged += SetupsTreeView_SelectedItemChanged;

            new FlowDocSetupRenderer().LogSetup(ref flowDocument, $"{SetupsPath}\\honda_nsx_gt3_evo\\misano\\Aggressive_Preset.json");
        }

        private void SetupsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null)
            {
                if (e.NewValue.GetType() == typeof(TreeViewItem))
                {
                    TreeViewItem item = (TreeViewItem)e.NewValue;
                    if (item.DataContext != null)
                    {
                        if (item.DataContext.GetType() == typeof(FileInfo))
                        {
                            FileInfo file = (FileInfo)item.DataContext;
                            setupRenderer.LogSetup(ref flowDocument, file.FullName);
                        }
                    }
                }
            }
        }

        private void FetchAllSetups()
        {
            DirectoryInfo setupsDirectory = new DirectoryInfo(SetupsPath);

            setupsTreeView.Items.Clear();

            // Find car directories
            foreach (var carDir in setupsDirectory.GetDirectories())
            {
                if (carDir.GetDirectories().Any() && !carDir.Name.Contains(".git"))
                {
                    bool carHasSetups = false;

                    // Make Car Tree View Item
                    TextBlock carHeader = new TextBlock()
                    {
                        Text = conversionFactory.ParseCarName(carDir.Name),
                        Style = Resources["MaterialDesignSubtitle1TextBlock"] as Style,
                    };
                    TreeViewItem carTreeViewItem = new TreeViewItem() { Header = carHeader };

                    // find track directories in car dir
                    foreach (var trackDir in carDir.GetDirectories())
                    {
                        bool trackHasSetups = false;

                        string trackName = trackDir.Name;
                        trackName = Regex.Replace(trackName, "^[a-z]", m => m.Value.ToUpper());
                        trackName = trackName.Replace("_", " ");
                        TextBlock trackHeader = new TextBlock()
                        {
                            Text = conversionFactory.ParseCarName(trackName),
                            Style = Resources["MaterialDesignSubtitle2TextBlock"] as Style,
                        };
                        TreeViewItem trackTreeViewItem = new TreeViewItem() { Header = trackHeader, DataContext = trackDir };

                        // find setups in track dir
                        foreach (var trackFile in trackDir.GetFiles())
                        {
                            TextBlock setupHeader = new TextBlock()
                            {
                                Text = conversionFactory.ParseCarName(trackFile.Name.Replace(".json", "")),
                                Style = Resources["MaterialDesignDataGridTextColumnStyle"] as Style
                            };
                            TreeViewItem setupTreeViewItem = new TreeViewItem() { Header = setupHeader, DataContext = trackFile };

                            trackTreeViewItem.Items.Add(setupTreeViewItem);
                        }

                        // check for any setups so the tree view doesn't get cluttered with cars that have no setups
                        if (trackTreeViewItem.Items.Count > 0)
                        {
                            carHasSetups = true;
                            trackHasSetups = true;
                        }

                        if (trackHasSetups)
                            carTreeViewItem.Items.Add(trackTreeViewItem);
                    }

                    if (carHasSetups)
                        setupsTreeView.Items.Add(carTreeViewItem);
                }
            }

        }

        private string AccPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Assetto Corsa Competizione\\";
        private string SetupsPath => AccPath + "Setups\\";

    }
}
