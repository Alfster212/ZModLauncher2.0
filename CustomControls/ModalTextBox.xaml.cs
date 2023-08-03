using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using static ZModLauncher.Helpers.IOHelper;
using UserControl = System.Windows.Controls.UserControl;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for ModalTextBox.xaml
/// </summary>
public partial class ModalTextBox : UserControl
{
    // Using a DependencyProperty as the backing store for IsInputEmptyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsInputEmptyProperty =
        DependencyProperty.Register("IsInputEmpty", typeof(bool), typeof(ModalTextBox));

    // Using a DependencyProperty as the backing store for FileFilterProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty InputTextProperty =
        DependencyProperty.Register("InputText", typeof(string), typeof(ModalTextBox));

    // Using a DependencyProperty as the backing store for FileFilterProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FileFilterProperty =
        DependencyProperty.Register("FileFilter", typeof(string), typeof(ModalTextBox));

    // Using a DependencyProperty as the backing store for IsBrowsingForFolderProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsBrowsingForFolderProperty =
        DependencyProperty.Register("IsBrowsingForFolder", typeof(bool), typeof(ModalTextBox));

    // Using a DependencyProperty as the backing store for HasBrowseButtonProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HasBrowseButtonProperty =
        DependencyProperty.Register("HasBrowseButton", typeof(Visibility), typeof(ModalTextBox));

    // Using a DependencyProperty as the backing store for HintTextProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HintTextProperty =
        DependencyProperty.Register("HintText", typeof(string), typeof(ModalTextBox));

    // Using a DependencyProperty as the backing store for HeaderTextProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register("HeaderText", typeof(string), typeof(ModalTextBox));

    public ModalTextBox()
    {
        InitializeComponent();
    }

    public bool IsInputEmpty
    {
        get => (bool)GetValue(IsInputEmptyProperty);
        set => SetValue(IsInputEmptyProperty, value);
    }

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    public string FileFilter
    {
        get => (string)GetValue(FileFilterProperty);
        set => SetValue(FileFilterProperty, value);
    }

    public bool IsBrowsingForFolder
    {
        get => (bool)GetValue(IsBrowsingForFolderProperty);
        set => SetValue(IsBrowsingForFolderProperty, value);
    }

    public Visibility HasBrowseButton
    {
        get => (Visibility)GetValue(HasBrowseButtonProperty);
        set => SetValue(HasBrowseButtonProperty, value);
    }

    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsBrowsingForFolder)
        {
            FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() != DialogResult.OK) return;
            InputText = dialog.SelectedPath;
        }
        else
        {
            OpenFileDialog dialog = new();
            if (FileFilter == "Images") AssignDialogImagesFileFilter(dialog);
            else dialog.Filter = FileFilter;
            if (dialog.ShowDialog() != DialogResult.OK) return;
            InputText = dialog.FileName;
        }
    }

    private void ModalTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        InputText = modalTextBox.Text;
        IsInputEmpty = string.IsNullOrEmpty(InputText);
    }

    private void ModalTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        modalTextBox.Text = "Init";
        modalTextBox.Text = "";
    }
}