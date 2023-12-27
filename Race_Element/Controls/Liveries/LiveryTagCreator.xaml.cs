using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static RaceElement.Controls.LiveryTagging;

namespace RaceElement.Controls;

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

        if (newTagName.Equals("No Tags"))
        {
            MainWindow.Instance.EnqueueSnackbarMessage("This name is reserved.");
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
