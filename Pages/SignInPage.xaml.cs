using System.Windows;
using System.Windows.Controls;
using static ZModLauncher.Helpers.UIHelper;

namespace ZModLauncher.Pages;

/// <summary>
///     Interaction logic for SignInPage.xaml
/// </summary>
public partial class SignInPage : Page
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        NavigateToPage("SettingsPage");
    }

    private void PatreonButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("SignInBrowser");
    }
}