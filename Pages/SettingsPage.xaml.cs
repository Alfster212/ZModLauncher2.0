﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using ZModLauncher.Managers;
using ZModLauncher.Manifests;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Pages;

/// <summary>
///     Interaction logic for SettingsPage.xaml
/// </summary>
public partial class SettingsPage : Page
{
    // Using a DependencyProperty as the backing store for LauncherVersion.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LauncherVersionProperty =
        DependencyProperty.Register("LauncherVersion", typeof(string), typeof(SettingsPage));

    // Using a DependencyProperty as the backing store for YouTubeLink.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty YouTubeLinkProperty =
        DependencyProperty.Register("YouTubeLink", typeof(string), typeof(SettingsPage));

    // Using a DependencyProperty as the backing store for TwitterLink.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TwitterLinkProperty =
        DependencyProperty.Register("TwitterLink", typeof(string), typeof(SettingsPage));

    // Using a DependencyProperty as the backing store for YouTubeLink.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RoadmapLinkProperty =
        DependencyProperty.Register("RoadmapLink", typeof(string), typeof(SettingsPage));

    // Using a DependencyProperty as the backing store for FAQLink.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FAQLinkProperty =
        DependencyProperty.Register("FAQLink", typeof(string), typeof(SettingsPage));

    private static readonly string _loginInfoFolderPath = $"{NativeManifest.AppRootPath}\\{NativeManifest.ExecutableAppName}.exe.WebView2";

    public SettingsPage()
    {
        InitializeComponent();
        SetResourceLinks();
        IsInvokedFromSignIn();
        AssertClearButtonsVisibility();
        SetLauncherVersion();
    }

    public string LauncherVersion
    {
        get => (string)GetValue(LauncherVersionProperty);
        set => SetValue(LauncherVersionProperty, value);
    }

    public string YouTubeLink
    {
        get => (string)GetValue(YouTubeLinkProperty);
        set => SetValue(YouTubeLinkProperty, value);
    }

    public string TwitterLink
    {
        get => (string)GetValue(TwitterLinkProperty);
        set => SetValue(TwitterLinkProperty, value);
    }

    public string RoadmapLink
    {
        get => (string)GetValue(RoadmapLinkProperty);
        set => SetValue(RoadmapLinkProperty, value);
    }

    public string FAQLink
    {
        get => (string)GetValue(FAQLinkProperty);
        set => SetValue(FAQLinkProperty, value);
    }

    private void SetLauncherVersion()
    {
        LauncherVersion = "1.7.2";
    }

    private void SetResourceLinks()
    {
        LauncherConfigManager configManager = new();
        YouTubeLink = configManager.LauncherConfig[YouTubeResourceLinkKey]?.ToString();
        TwitterLink = configManager.LauncherConfig[TwitterResourceLinkKey]?.ToString();
        RoadmapLink = configManager.LauncherConfig[RoadmapResourceLinkKey]?.ToString();
        FAQLink = configManager.LauncherConfig[FAQResourceLinkKey]?.ToString();
    }

    private void IsInvokedFromSignIn()
    {
        if (IsPreviousPage(SignInPageName))
        {
            Collapse(signOutButton);
            Collapse(clearLauncherCacheButton);
        }
        else
        {
            Collapse(clearLoginInfoButton);
        }
    }

    private void AssertClearButtonsVisibility()
    {
        if (!Directory.Exists(_loginInfoFolderPath)) Collapse(clearLoginInfoButton);
        if (!NativeManifest.CanClearCache()) Collapse(clearLauncherCacheButton);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        GoBackFromCurrentPage();
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            if (((Hyperlink)e.Source).Inlines.FirstOrDefault() is Run)
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }
        catch { }
        e.Handled = true;
    }

    private void SignOutButton_Click(object sender, RoutedEventArgs e)
    {
        if (LoadingPage.RequiresSignIn()) NavigateToPage(SignInPageName);
        else Environment.Exit(0);
    }

    private void ClearLoginInfoButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(_loginInfoFolderPath)) ShowErrorDialog(LauncherLoginInfoClearError);
        else Directory.Delete(_loginInfoFolderPath, true);
        AssertClearButtonsVisibility();
    }

    private void ClearLauncherCacheButton_Click(object sender, RoutedEventArgs e)
    {
        if (ShowQuestionDialog(ClearLauncherCacheConfirmation) != MessageBoxResult.Yes) return;
        NativeManifest.ClearCache();
        AssertClearButtonsVisibility();
    }
}