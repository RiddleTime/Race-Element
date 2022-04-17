using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static ACCSetupApp.Controls.LiveryTagging;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for LiveryTagCreator.xaml
    /// </summary>
    public partial class LiveryTagCreator : UserControl
    {
        public static LiveryTagCreator Instance { get; private set; }

        public LiveryTagCreator()
        {
            InitializeComponent();

            this.Visibility = Visibility.Hidden;

            buttonCreate.Click += ButtonCreate_Click;
            buttonCancel.Click += ButtonCancel_Click;
            textBoxNewTagName.PreviewTextInput += TextBoxNewTagName_PreviewTextInput;

            Instance = this;
        }

        private void TextBoxNewTagName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string originalText = ((TextBox)e.OriginalSource).Text;

            char[] illegalCharacters = System.IO.Path.GetInvalidFileNameChars();
            foreach (char illegalChar in illegalCharacters)
            {
                if (e.Text.Contains(illegalChar))
                {
                    textBoxNewTagName.Text = originalText;
                    e.Handled = true;
                    break;
                }
            }
        }

        public void Open()
        {
            textBoxNewTagName.Text = String.Empty;
            this.Visibility = Visibility.Visible;
            textBoxNewTagName.Focus();
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            string newTagName = textBoxNewTagName.Text.Trim();
            if (newTagName.Length == 0)
            {
                MainWindow.Instance.EnqueueSnackbarMessage("Please enter a name for your tag.");
                return;
            }

            List<LiveryTag> allTags = LiveryTagging.GetAllTags();

            foreach (LiveryTag tag in allTags)
            {
                if (tag.Name.Equals(newTagName))
                {
                    MainWindow.Instance.EnqueueSnackbarMessage("Tag already exists.");
                    return;
                }
            }

            LiveryTagging.CreateNewTag(newTagName);
            this.Visibility = Visibility.Hidden;

            LiveryBrowser.Instance.FetchAllCars();

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
