using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZModLauncher.CustomControls;

/// <summary>
///     Interaction logic for LibraryItemCard.xaml
/// </summary>
public partial class AddLocalModDialog : UserControl
{
    public AddLocalModDialog()
    {
        InitializeComponent();
    }

    private void AddLocalModBackButton_OnClick(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
    }

    private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Keyboard.ClearFocus();
    }

    private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        Mod mod = Mod.CreateLocalMod(nameBox.InputText, folderBox.InputText, imageBox.InputText, macroBox.IsChecked, shouldFavoriteBox.IsChecked, launchExeBox.InputText);
        if (mod == null) return;
        statusLabel.Visibility = Visibility.Visible;
        statusLabel.Text = $"Installing {mod.Name}...";
        await mod.InstallLocal(folderBox.InputText, imageBox.InputText, launchExeBox.InputText);
        await Task.Delay(1000);
        statusLabel.Text = $"{mod.Name} is now installed! Refreshing library...";
        await Task.Delay(1000);
        statusLabel.Visibility = Visibility.Collapsed;
        Visibility = Visibility.Collapsed;
        await LibraryManager.RefreshLibrary();
    }
}