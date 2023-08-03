using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ZModLauncher.Core;
using ZModLauncher.CustomControls;
using ZModLauncher.Managers;
using static ZModLauncher.Helpers.UIHelper;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ZModLauncher.Pages;

/// <summary>
///     Interaction logic for StorePage.xaml
/// </summary>
public partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        LibraryManager.ConfigureLibrary(this);
    }

    private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
    {
        NavigateToPage("SettingsPage");
    }

    private void GamesMenuButton_Loaded(object sender, RoutedEventArgs e)
    {
        gamesButton.IsChecked = true;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LibraryManager.RefreshLibrary();
    }

    private async void LoadGames(object sender, RoutedEventArgs e)
    {
        await LibraryManager.LoadLibrary<Game>();
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        string launchExecPath = LibraryManager.FocusedGame.ExecutablePath;
        if (LibraryManager.FocusedGame.SteamExecPath != null)
            launchExecPath = LibraryManager.FocusedGame.SteamExecPath;
        else if (LibraryManager.FocusedGame.EpicExecPath != null)
            launchExecPath = LibraryManager.FocusedGame.EpicExecPath;
        Process.Start(launchExecPath);
    }

    private static bool IsLibraryConfigured()
    {
        return LibraryManager.MainPage != null;
    }

    private void SortByBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLibraryConfigured()) return;
        LibraryManager.SortLibrary();
    }

    private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLibraryConfigured()) return;
        LibraryManager.FilterLibrary();
    }

    private void SearchBox_Loaded(object sender, RoutedEventArgs e)
    {
        searchBox.textBox.TextChanged += TextBoxOnTextChanged;
    }

    private static void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
    {
        LibraryManager.FilterLibrary();
    }

    private void ChangeModFiltersVisibility(Visibility visibility)
    {
        ((ComboBoxItem)filterBox.Items.GetItemAt(3)).Visibility = visibility;
        ((ComboBoxItem)filterBox.Items.GetItemAt(4)).Visibility = visibility;
        ((ComboBoxItem)filterBox.Items.GetItemAt(5)).Visibility = visibility;
    }

    public void HideModFilters()
    {
        ChangeModFiltersVisibility(Visibility.Collapsed);
    }

    public void ShowModFilters()
    {
        ChangeModFiltersVisibility(Visibility.Visible);
    }

    private async void BackButton_Click(object sender, RoutedEventArgs e)
    {
        await LibraryManager.LoadLibrary<Game>();
    }

    private void AddLocalModButton_Click(object sender, RoutedEventArgs e)
    {
        addLocalModDialog.PopulateControlValuesFromMod(new Mod());
        addLocalModDialog.IsInEditMode = false;
        addLocalModDialog.Visibility = Visibility.Visible;
    }

    private void RefreshButton_MouseEnter(object sender, MouseEventArgs e)
    {
        refreshButton.HasPopup = !LibraryManager.IsCurrentlyLoadingLibrary();
    }

    private void TagsSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBoxItem selectedTag = (ComboBoxItem)tagsSelector.SelectedItem;
        string selectedTagString = selectedTag?.Content.ToString();
        if (string.IsNullOrEmpty(selectedTagString)) return;
        TagItem tag = new() { TagText = selectedTagString };
        if (LibraryManager.IsTagBeingUsed(tag.TagText)) return;
        ((ComboBoxItem)tagsSelector.SelectedItem).Visibility = Visibility.Collapsed;
        tagsSelector.SelectedIndex = -1;
        LibraryManager.MainPage.tagsBox.HintLabel.Visibility = Visibility.Collapsed;
        LibraryManager.MainPage.tagsBox.listBox.Items.Add(tag);
        LibraryManager.MainPage.tagsBox.listBox.ScrollIntoView(tag);
        LibraryManager.FilterLibrary();
    }
}