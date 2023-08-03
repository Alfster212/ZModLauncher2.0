using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZModLauncher.Core;
using ZModLauncher.CustomControls;
using ZModLauncher.Managers;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Helpers;

public static class CardOptionsButtonHelper
{
    /// <summary>
    ///     Attempts to display the options button for the specified item card if it has not been loaded.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static async Task AssertItemCardOptionsButtonLoadedState(LibraryItemCard card)
    {
        while (card.OptionsButton == null) await Task.Delay(1000);
    }

    /// <summary>
    ///     Retrieves the desired item from an item card's associated options button using an index value.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static ComboBoxItem GetItemCardOptionsButtonItem(LibraryItemCard card, int index)
    {
        return (ComboBoxItem)card.OptionsButton.Items.GetItemAt(index);
    }

    /// <summary>
    ///     Creates a new readonly options button item containing the specified content, and returns the item.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private static ComboBoxItem CreateReadonlyOptionsButtonItem(object content)
    {
        return new ComboBoxItem
        {
            IsEnabled = false,
            Content = content
        };
    }

    /// <summary>
    ///     Sets the visibility of the delete option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void SetItemCardDeleteButtonVisibility(LibraryItemCard card, LibraryItem item)
    {
        ComboBoxItem deleteButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.Delete);
        deleteButton.Visibility = item.IsInstalled || item is Mod { IsLocal: true } ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the visibility of the folder option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void SetItemCardOpenLocalFolderButtonVisibility(LibraryItemCard card, LibraryItem item)
    {
        ComboBoxItem openLocalFolderButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.OpenLocalFolder);
        openLocalFolderButton.Visibility = item.IsInstalled ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the visibility of the mod info option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetModCardModInfoButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem modInfoButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.ModInfo);
        modInfoButton.Visibility = mod.ModInfoUri != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the visibility of the edit option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetModCardEditButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem editButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.Edit);
        editButton.Visibility = mod.IsInstalled && mod.IsLocal ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the visibility of the direct download option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetModCardDirectDownloadButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem directDownloadButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.DirectDownload);
        directDownloadButton.Visibility = !mod.IsInstalled && mod.DirectDownloadUri != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the visibility of the download option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetModCardCancelDownloadButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem cancelDownloadButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.CancelDownload);
        cancelDownloadButton.Visibility = !mod.IsDownloadPaused && !mod.IsInstalled && mod.IsBusy ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Sets the visibility of the pause download option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetModCardPauseDownloadButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem pauseDownloadButton = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.PauseDownload);
        pauseDownloadButton.Visibility = !mod.IsInstalled && mod.IsBusy ? Visibility.Visible : Visibility.Collapsed;
        pauseDownloadButton.Content = mod.IsDownloadPaused ? "Resume Download" : "Pause Download";
    }

    /// <summary>
    ///     Sets the visibility of the author name option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetItemCardAuthorNameButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem authorName = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.AuthorName);
        authorName.IsEnabled = false;
        authorName.Visibility = !string.IsNullOrEmpty(mod.AuthorName) ? Visibility.Visible : Visibility.Collapsed;
        authorName.Content = $"By {mod.AuthorName}";
    }

    /// <summary>
    ///     Sets the visibility of the first published option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetItemCardFirstPublishedButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem firstPublished = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.FirstPublished);
        firstPublished.IsEnabled = false;
        firstPublished.Visibility = mod.FirstPublished != null ? Visibility.Visible : Visibility.Collapsed;
        firstPublished.Content = $"Published {mod.FirstPublished:M/d/y}";
    }

    /// <summary>
    ///     Sets the visibility of the last updated option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    private static void SetItemCardLastUpdatedButtonVisibility(LibraryItemCard card, Mod mod)
    {
        ComboBoxItem lastUpdated = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.LastUpdated);
        lastUpdated.IsEnabled = false;
        lastUpdated.Visibility = mod.LastUpdated != null ? Visibility.Visible : Visibility.Collapsed;
        lastUpdated.Content = $"Modified {mod.LastUpdated:M/d/y}";
    }

    /// <summary>
    ///     Sets the visibility of the version info option in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void SetItemCardVersionInfoButtonVisibility(LibraryItemCard card, LibraryItem item)
    {
        ComboBoxItem versionInfo = GetItemCardOptionsButtonItem(card, CardOptionsButtonItems.Version);
        versionInfo.IsEnabled = false;
        versionInfo.Visibility = item.IsInstalled && item.Version != null ? Visibility.Visible : Visibility.Collapsed;
        versionInfo.Content = $"Version {item.Version}";
    }

    /// <summary>
    ///     Adds all tags associated with the specified item to the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void AddItemTagsToOptionsButton(LibraryItemCard card, LibraryItem item)
    {
        if (item.Tags == null) return;
        card.OptionsButton.Items.Add(CreateReadonlyOptionsButtonItem("Tags:"));
        card.Tags = new List<string>();
        foreach (string tagString in item.Tags)
        {
            card.Tags.Add(tagString);
            card.OptionsButton.Items.Add(CreateReadonlyOptionsButtonItem($"• {tagString}"));
        }
    }

    /// <summary>
    ///     Sets the visibility of the options button items for the specified item's associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task SetItemCardOptionsButtonItemVisibilityStates(LibraryItemCard card, LibraryItem item)
    {
        await AssertItemCardOptionsButtonLoadedState(card);
        card.OptionsButton.Visibility = Visibility.Visible;
        if (item is Mod mod)
        {
            SetItemCardDeleteButtonVisibility(card, mod);
            SetItemCardOpenLocalFolderButtonVisibility(card, mod);
            SetModCardModInfoButtonVisibility(card, mod);
            SetModCardEditButtonVisibility(card, mod);
            SetModCardDirectDownloadButtonVisibility(card, mod);
            SetModCardCancelDownloadButtonVisibility(card, mod);
            SetModCardPauseDownloadButtonVisibility(card, mod);
            SetItemCardAuthorNameButtonVisibility(card, mod);
            SetItemCardFirstPublishedButtonVisibility(card, mod);
            SetItemCardLastUpdatedButtonVisibility(card, mod);
            SetItemCardVersionInfoButtonVisibility(card, mod);
        }
        else card.OptionsButton.Items.Clear();
        AddItemTagsToOptionsButton(card, item);
        if (card.OptionsButton.Items.Cast<ComboBoxItem>().All(i => i.Visibility != Visibility.Visible))
            card.OptionsButton.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    ///     Removes focus from the currently selected item in the options button for the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static async Task ResetItemCardOptionsButtonSelection(LibraryItemCard card)
    {
        await AssertItemCardOptionsButtonLoadedState(card);
        card.OptionsButton.SelectedItem = null;
    }

    /// <summary>
    ///     Attaches the desired type of click events to the specified mod's item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    public static void AddItemCardOptionsButtonSelectionEvent(LibraryItemCard card, Mod mod)
    {
        card.OptionsButton.SelectionChanged += async (_, _) =>
        {
            switch (card.OptionsButton.SelectedIndex)
            {
                case CardOptionsButtonItems.Delete:
                {
                    MessageBoxResult shouldDeleteMod = ShowQuestionDialog(DeleteModConfirmation);
                    if (shouldDeleteMod != MessageBoxResult.Yes)
                    {
                        await ResetItemCardOptionsButtonSelection(card);
                        return;
                    }
                    await mod.Delete();
                    if (mod.IsLocal) LibraryManager.MainPage.library.Children.Remove(card);
                    else card = LibraryManager.RefreshItemCard(card, mod);
                    break;
                }
                case CardOptionsButtonItems.Edit:
                    LibraryManager.MainPage.addLocalModDialog.PopulateControlValuesFromMod(mod);
                    LibraryManager.MainPage.addLocalModDialog.IsInEditMode = true;
                    LibraryManager.MainPage.addLocalModDialog.Visibility = Visibility.Visible;
                    break;
                case CardOptionsButtonItems.OpenLocalFolder:
                    mod.OpenLocalFolder();
                    break;
                case CardOptionsButtonItems.ModInfo:
                    Process.Start(mod.ModInfoUri.AbsoluteUri);
                    break;
                case CardOptionsButtonItems.DirectDownload:
                    Process.Start(mod.DirectDownloadUri.AbsoluteUri);
                    break;
                case CardOptionsButtonItems.CancelDownload:
                    mod.IsCancellingDownload = true;
                    break;
                case CardOptionsButtonItems.PauseDownload:
                    mod.IsDownloadPaused = !mod.IsDownloadPaused;
                    if (!mod.IsDownloadPaused) await LibraryManager.InstallOrUpdateMod(card, mod);
                    break;
            }
            await ResetItemCardOptionsButtonSelection(card);
        };
    }

    /// <summary>
    ///     Attaches the default click events for items in the options button of the specified item's card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    public static void AddItemCardOptionsButtonEvents(LibraryItemCard card, LibraryItem item)
    {
        card.Loaded += async (_, _) =>
        {
            await SetItemCardOptionsButtonItemVisibilityStates(card, item);
            card.OptionsButton.DropDownOpened += (_, _) => { item.IsOptionsButtonActivated = true; };
            card.OptionsButton.DropDownClosed += (_, _) => { item.IsOptionsButtonActivated = false; };
            if (item is Mod mod) AddItemCardOptionsButtonSelectionEvent(card, mod);
        };
    }

    public static class CardOptionsButtonItems
    {
        public const int Delete = 0;
        public const int Edit = 1;
        public const int OpenLocalFolder = 2;
        public const int ModInfo = 3;
        public const int DirectDownload = 4;
        public const int CancelDownload = 5;
        public const int PauseDownload = 6;
        public const int AuthorName = 7;
        public const int FirstPublished = 8;
        public const int LastUpdated = 9;
        public const int Version = 10;
    }
}