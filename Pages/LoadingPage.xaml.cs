using System;
using System.Windows.Controls;
using System.Windows.Threading;
using ZModLauncher.Core;
using ZModLauncher.Managers;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Pages;

/// <summary>
///     Interaction logic for LoadingPage.xaml
/// </summary>
public partial class LoadingPage : Page
{
    private readonly bool _shouldLoadSignInPage;
    private readonly DispatcherTimer _signInPageTimer = new();

    public LoadingPage()
    {
        InitializeComponent();
        _shouldLoadSignInPage = !IsPreviousPage("SignInBrowser") && RequiresSignIn();
        PrepareLauncher();
    }

    public static bool RequiresSignIn()
    {
        LauncherConfigManager configManager = new();
        return configManager.LauncherConfig[PatreonClientIdKey]?.ToString() != "";
    }

    private async void PrepareLauncher()
    {
        LauncherUpdater updater = new();
        await updater.CheckForUpdates();
        _signInPageTimer.Tick += SignInPageTimerTick;
        _signInPageTimer.Interval = new TimeSpan(0, 0, 2);
        _signInPageTimer.Start();
    }

    private void SignInPageTimerTick(object sender, EventArgs e)
    {
        NavigateToPage(_shouldLoadSignInPage ? "SignInPage" : "PrepareLauncherPage");
        _signInPageTimer.Stop();
    }
}