using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace ACCManager.Controls.Settings.AccSettings
{
    /// <summary>
    /// Interaction logic for AccServerListSettings.xaml
    /// </summary>
    public partial class AccServerListSettings : UserControl
    {
        public AccServerListSettings()
        {
            InitializeComponent();

            this.Loaded += (s, e) => FillListView();
        }

        private void FillListView()
        {
            List<UnlistedAccServer> list = new List<UnlistedAccServer>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new UnlistedAccServer() { Name = $"test {i}", Description = $"Description {i}", Server = $"SomeDomainOrIP {i}" });
            }

            listViewServers.Items.Clear();
            foreach (var unlistedAccServer in list)
            {
                TextBlock serverBlock = new TextBlock()
                {
                    Text = unlistedAccServer.Name,
                    DataContext = unlistedAccServer
                };
                listViewServers.Items.Add(serverBlock);
            }
        }

        private class UnlistedAccServer
        {
            public Guid Guid { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Server { get; set; }
        }
    }
}
