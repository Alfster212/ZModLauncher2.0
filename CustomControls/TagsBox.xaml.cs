using System;
using System.Windows;
using System.Windows.Controls;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for TagsBox.xaml
/// </summary>
public partial class TagsBox : UserControl
{
    public TextBlock HintLabel;
    public ListBox listBox;
    public TextBox textBox;

    public TagsBox()
    {
        InitializeComponent();
    }

    private void TextBox_Initialized(object sender, EventArgs e)
    {
        textBox = (TextBox)sender;
    }

    private void ClearBtn_Click(object sender, RoutedEventArgs e)
    {
        textBox.Clear();
        listBox.Items.Clear();
    }

    private void ListBox_Initialized(object sender, EventArgs e)
    {
        listBox = (ListBox)sender;
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        listBox.ScrollIntoView(listBox.SelectedItem);
    }

    private void TextBlock_Initialized(object sender, EventArgs e)
    {
        HintLabel = (TextBlock)sender;
    }
}