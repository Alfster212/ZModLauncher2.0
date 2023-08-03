using System.Windows;
using System.Windows.Controls;
using ZModLauncher.Managers;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for Popup.xaml
/// </summary>
public partial class TagItem : UserControl
{
    // Using a DependencyProperty as the backing store for TagTextProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TagTextProperty =
        DependencyProperty.Register("TagText", typeof(string), typeof(TagItem));

    public TagItem()
    {
        InitializeComponent();
    }

    public string TagText
    {
        get => (string)GetValue(TagTextProperty);
        set => SetValue(TagTextProperty, value);
    }

    private void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        LibraryManager.MainPage.tagsBox.listBox.Items.Remove(this);
        LibraryManager.GetTagsSelectorItem(TagText).Visibility = Visibility.Visible;
        LibraryManager.AssertTagsBoxHintLabelVisibility();
        LibraryManager.FilterLibrary();
    }
}