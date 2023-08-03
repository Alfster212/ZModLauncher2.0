using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using ZModLauncher.Core;
using ZModLauncher.CustomControls;
using ZModLauncher.Manifests;
using ZModLauncher.Pages;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Helpers.AnimationHelper;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Helpers.CommandLineHelper;
using static ZModLauncher.Helpers.IOHelper;
using static ZModLauncher.Constants.GlobalStringConstants;
using static ZModLauncher.Helpers.ThreadHelper;
using static ZModLauncher.Managers.DownloadManager;
using static ZModLauncher.Helpers.CardOptionsButtonHelper;
using static ZModLauncher.Helpers.CardFavoriteButtonHelper;
using Application = System.Windows.Application;

namespace ZModLauncher.Managers;

/// <summary>
///     Manages all internal and external library actions and operations.
/// </summary>
public class LibraryManager
{
    /// <summary>
    ///     Manifest manager specific to the current instance of the library manager.
    /// </summary>
    public static ManifestManager ManifestManager;
    /// <summary>
    ///     Animation for the refresh button control within the user interface.
    /// </summary>
    private static Storyboard _refreshButtonAnim;
    /// <summary>
    ///     Controls various thread related operations, to better execute multiple tasks.
    /// </summary>
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    /// <summary>
    ///     Mods database manager specific to the current instance of the library manager.
    /// </summary>
    public static readonly DatabaseManager ModsDbManager = new();
    /// <summary>
    ///     Dropbox files manager specific to the current instance of the library manager.
    /// </summary>
    public static readonly DropboxFileManager FileManager = new();
    /// <summary>
    ///     Sets the currently focused game when the user clicks on a game in the library.
    /// </summary>
    public static Game FocusedGame;
    /// <summary>
    ///     The main library page where games, mods, and other settings can be interacted with.
    /// </summary>
    public static MainPage MainPage;
    /// <summary>
    ///     The default card image which displays on an item card with no manually assigned image.
    /// </summary>
    private static BitmapImage _defaultCardImage;

    /// <summary>
    ///     Configures the environment for the library using the specified store page.
    /// </summary>
    /// <param name="mainPage"></param>
    public static void ConfigureLibrary(MainPage mainPage)
    {
        MainPage = mainPage;
        ManifestManager = new ManifestManager(FileManager);
        SetupAllResources();
    }

    /// <summary>
    ///     Configures all necessary user interface resources for the store page.
    /// </summary>
    public static void SetupAllResources()
    {
        _defaultCardImage = LocateResource<BitmapImage>(Application.Current, DefaultCardImageKey);
        _refreshButtonAnim = LocateResource<Storyboard>(MainPage, RefreshButtonAnimKey);
        ApplyStoryboardAnim(_refreshButtonAnim, MainPage.refreshButton);
    }

    /// <summary>
    ///     Attempts to locate the specified UI resource using a parent container and name key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="container"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static T LocateResource<T>(dynamic container, string key)
    {
        return (T)container.FindResource(key);
    }

    /// <summary>
    ///     Clears the library of games or mods, and optionally displays a clear message when done.
    /// </summary>
    /// <param name="showEmptyLibraryMessage"></param>
    private static void ClearLibrary(bool showEmptyLibraryMessage)
    {
        MainPage.library.Children.Clear();
        if (showEmptyLibraryMessage) Show(MainPage.emptyLibraryMessage);
        else Collapse(MainPage.emptyLibraryMessage);
        Collapse(MainPage.loadLibraryProgressBar);
    }

    /// <summary>
    ///     Asserts the visibility of the hint label in the tags box.
    /// </summary>
    public static void AssertTagsBoxHintLabelVisibility()
    {
        if (MainPage.tagsBox.listBox.Items.Count == 0)
            MainPage.tagsBox.HintLabel.Visibility = Visibility.Visible;
    }

    /// <summary>
    ///     Attempts to retrieve the item in the tags selector matching the specified tag.
    /// </summary>
    /// <param name="tagText"></param>
    /// <returns></returns>
    public static ComboBoxItem GetTagsSelectorItem(string tagText)
    {
        return MainPage.tagsSelector.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content.ToString() == tagText);
    }

    /// <summary>
    ///     Checks to see if the specified tag is already being used in the tags box.
    /// </summary>
    /// <param name="tagText"></param>
    /// <returns></returns>
    public static bool IsTagBeingUsed(string tagText)
    {
        return MainPage.tagsBox.listBox.Items.Cast<TagItem>().Any(i => i.TagText == tagText);
    }

    /// <summary>
    ///     Clears any currently set tags in the tags box and tags selector.
    /// </summary>
    private static void ClearTags()
    {
        MainPage.tagsBox.listBox.Items.Clear();
        AssertTagsBoxHintLabelVisibility();
        MainPage.tagsSelector.Items.Clear();
        MainPage.tagsSelector.SelectedIndex = -1;
    }

    /// <summary>
    ///     Updates the displayed progress percentage associated with an item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static async Task<LibraryItemCard> UpdateItemCardProgress(LibraryItemCard card, LibraryItem item)
    {
        await RunBackgroundAction(() => { card = RefreshItemCard(card, item); });
        return card;
    }

    /// <summary>
    ///     Changes the visibility of the traffic light for a mod's associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void AssertTrafficLightVisibility(LibraryItemCard card, LibraryItem item)
    {
        if (item is not Mod mod) return;
        card.TrafficLightVisibility = mod.IsInstalled && mod.IsUpdated && !mod.IsLaunchable && !mod.IsBusy ? Visibility.Visible : Visibility.Collapsed;
        card.TrafficLightColor = mod.IsEnabled ? Brushes.LimeGreen : Brushes.Red;
    }

    /// <summary>
    ///     Updates the status label for an item's associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void UpdateItemCardStatus(LibraryItemCard card, LibraryItem item)
    {
        AssertTrafficLightVisibility(card, item);
        card.Status = item switch
        {
            Mod mod => mod.IsDownloadPaused ? $"Download Paused ({mod.Progress}%)" :
                mod.IsReconnecting ? ReconnectingStatus :
                mod.IsQueuing ? QueuingStatus :
                mod.IsBusy ? mod.IsWaiting ? WaitingStatus :
                mod.IsExtracting ? InstallingStatus :
                !mod.IsUpdated ? $"{UpdatingStatus} ({mod.InstalledUpdates}/{mod.UpdateFiles.Count}) ({mod.Progress}%)" : $"{DownloadingStatus} ({mod.Progress}%)" :
                mod.IsInstalled ? !mod.IsUpdated ? UpdateStatus :
                mod.IsLaunchable ? LaunchNowStatus :
                mod.IsToggling ? TogglingStatus :
                mod.IsEnabled ? EnabledStatus : DisabledStatus :
                mod.Uri == null ? mod.IsLocal ? NotInstalledStatus : ComingSoonStatus : DownloadStatus,
            Game game => game.IsInstalled ? PlayNowStatus : NotInstalledStatus,
            _ => card.Status
        };
    }

    /// <summary>
    ///     Checks to see if the library is completely empty.
    /// </summary>
    /// <returns></returns>
    private static bool IsLibraryEmpty()
    {
        return MainPage.library.Children.Count == 0;
    }

    /// <summary>
    ///     Retrieves all currently stored item cards within the library as a list of item cards.
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<LibraryItemCard> GetAllItemCards()
    {
        return MainPage.library.Children.OfType<LibraryItemCard>();
    }

    /// <summary>
    ///     Checks the visibility of all the item cards, and shows the clear message if all are collapsed.
    /// </summary>
    private static void VerifyVisibleItemCardsLibraryState()
    {
        if (GetAllItemCards().All(i => i.Visibility == Visibility.Collapsed)
            && MainPage.loadLibraryProgressBar.Visibility == Visibility.Collapsed)
            Show(MainPage.emptyLibraryMessage);
    }

    /// <summary>
    ///     Checks to see whether an item card's name matches the currently set search query.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private static bool DoesItemCardMatchSearch(LibraryItemCard card)
    {
        string[] tokens = MainPage.searchBox.textBox.Text.ToLower().Split(' ');
        bool isMatch = false;
        foreach (string token in tokens)
        {
            bool doesSearchMatch = card.Title.ToLower().Contains(token) && (tokens.Length == 1 || token.Length > 1);
            bool isTagsBoxEmpty = MainPage.tagsBox.listBox.Items.Count == 0;
            bool doTagsMatch = isTagsBoxEmpty || card.Tags != null && MainPage.tagsBox.listBox.Items.Cast<TagItem>().All(i => card.Tags.Contains(i.TagText));
            isMatch = doesSearchMatch && doTagsMatch;
        }
        return isMatch;
    }

    /// <summary>
    ///     Checks to see whether the item card matches the currently selected filter option.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private static bool DoesItemCardMatchGeneralFilter(LibraryItemCard card)
    {
        return MainPage.filterBox.SelectedIndex == 0
            || MainPage.filterBox.SelectedIndex == 1
            && card.Status is LaunchNowStatus or EnabledStatus or DisabledStatus or PlayNowStatus
            || MainPage.filterBox.SelectedIndex == 2
            && card.ItemIsFavorited
            || MainPage.filterBox.SelectedIndex == 3
            && card.Status is DisabledStatus
            || MainPage.filterBox.SelectedIndex == 4
            && card.Status is EnabledStatus
            || MainPage.filterBox.SelectedIndex == 5
            && card.Status is LaunchNowStatus;
    }

    /// <summary>
    ///     Checks to see whether the item card matches the currently set search query and filter option.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private static bool DoesItemCardMatchFilters(LibraryItemCard card)
    {
        return DoesItemCardMatchSearch(card) && DoesItemCardMatchGeneralFilter(card);
    }

    /// <summary>
    ///     Filters all item cards in the library using the currently set search query and filter option.
    /// </summary>
    public static void FilterLibrary()
    {
        Collapse(MainPage.emptyLibraryMessage);
        foreach (LibraryItemCard card in MainPage.library.Children)
        {
            Collapse(card);
            if (DoesItemCardMatchFilters(card)) Show(card);
        }
        VerifyVisibleItemCardsLibraryState();
    }

    /// <summary>
    ///     Sorts all item cards in the library using the currently set sorting option.
    /// </summary>
    public static void SortLibrary()
    {
        IEnumerable<LibraryItemCard> cards = GetAllItemCards().ToList();
        if (!cards.Any()) return;
        cards = MainPage.sortbyBox.SelectedIndex switch
        {
            0 => cards.OrderBy(x => x.Title).ToList(),
            1 => cards.OrderByDescending(x => x.Title).ToList(),
            _ => cards
        };
        cards = cards.OrderByDescending(x => x.ItemIsFavorited).ToList();
        ClearLibrary(false);
        foreach (LibraryItemCard card in cards)
            MainPage.library.Children.Add(card);
        VerifyVisibleItemCardsLibraryState();
    }

    /// <summary>
    ///     Replaces a specified item card with another one, usually used for refreshing data.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="newCard"></param>
    private static void ReplaceItemCardWith(UIElement card, UIElement newCard)
    {
        try
        {
            int originalCardIndex = MainPage.library.Children.IndexOf(card);
            MainPage.library.Children.RemoveAt(originalCardIndex);
            MainPage.library.Children.Insert(originalCardIndex, newCard);
            SortLibrary();
            FilterLibrary();
        }
        catch (ArgumentOutOfRangeException) { }
    }

    /// <summary>
    ///     Refreshes the specified item card in the library using the parameters of its associated item.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static LibraryItemCard RefreshItemCard(LibraryItemCard card, LibraryItem item)
    {
        if (IsLibraryEmpty()) return card;
        LibraryItemCard newCard = card.Clone();
        UpdateItemCardStatus(newCard, item);
        AddItemCardFavoriteItemButtonEvents(newCard, item);
        if (item is Game or Mod { IsReconnecting: false, IsBusy: false, IsToggling: false })
            AddItemCardClickEvent(newCard, item);
        if (item is Mod { IsDownloadPaused: false, IsBusy: false, IsQueuing: false, IsExtracting: false, IsToggling: false }) AddItemCardOptionsButtonEvents(newCard, item);
        if (item is Mod { IsBusy: true, IsUpdated: true, IsQueuing: false, IsExtracting: false, IsToggling: false }) AddItemCardOptionsButtonEvents(newCard, item);
        newCard = SetItemCardImage(newCard, item);
        ReplaceItemCardWith(card, newCard);
        return newCard;
    }

    /// <summary>
    ///     Updates the toggle status label for the specified mod's associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    private static LibraryItemCard UpdateModCardToggleStatus(LibraryItemCard card, Mod mod)
    {
        mod.IsToggling = !mod.IsToggling;
        UpdateItemCardStatus(card, mod);
        card = RefreshItemCard(card, mod);
        return card;
    }

    /// <summary>
    ///     Toggles the specified mod and updates the status of its associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    private static async Task<LibraryItemCard> ToggleModAndUpdateCardStatus(LibraryItemCard card, Mod mod)
    {
        card = UpdateModCardToggleStatus(card, mod);
        await Task.Run(() => mod.Toggle());
        card = UpdateModCardToggleStatus(card, mod);
        return card;
    }

    /// <summary>
    ///     Disables the navigation controls within the library.
    /// </summary>
    private static void DisableNavigationControls()
    {
        Disable(MainPage.backButton);
        Disable(MainPage.refreshButton);
        Disable(MainPage.addLocalModButton);
    }

    /// <summary>
    ///     Enables the navigation controls within the library.
    /// </summary>
    private static void EnableNavigationControls()
    {
        Enable(MainPage.backButton);
        Enable(MainPage.refreshButton);
        Enable(MainPage.addLocalModButton);
    }

    /// <summary>
    ///     Used after a mod download or update operation, performs necessary post-cleanup steps
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    private static LibraryItemCard PostInstallOrUpdateCleanup(LibraryItemCard card, Mod mod)
    {
        bool isOtherModBusy = GetAllItemCards().Any(i => (i.Status.Contains(DownloadingStatus)
                || i.Status.Contains(UpdatingStatus))
            && i.Title != card.Title);
        if (!isOtherModBusy) EnableNavigationControls();
        if (!mod.IsUpdated)
        {
            NativeManifest.WriteMod(mod);
            mod.CheckForUpdates();
        }
        card = RefreshItemCard(card, mod);
        return card;
    }

    /// <summary>
    ///     Installs or updates the specified mod, and updates its associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    public static async Task<LibraryItemCard> InstallOrUpdateMod(LibraryItemCard card, Mod mod)
    {
        LibraryItemCard progressCard = card;
        DisableNavigationControls();
        if (!mod.IsUpdated)
        {
            if (!mod.IsLaunchable && mod.IsEnabled) await mod.Toggle();
            LibraryItemCard originalCard = card.Clone();
            mod.UpdateFiles = mod.FilterValidUpdateFiles(mod.UpdateFiles);
            foreach (string updateFilePath in mod.UpdateFiles)
            {
                string localPath = $"{FocusedGame.LocalPath}\\{Path.GetFileNameWithoutExtension(updateFilePath)}";
                mod.InstalledUpdates = mod.UpdateFiles.IndexOf(updateFilePath) + 1;
                card = await DownloadModFileAndUpdateCardStatus(progressCard, mod, updateFilePath, localPath);
                if (Directory.Exists(localPath)) Directory.Delete(localPath, true);
                mod.Version = mod.GetUpdateFileVersionInfo(updateFilePath)[1];
                ReplaceItemCardWith(card, originalCard);
                card = originalCard;
                progressCard = card;
            }
        }
        else
        {
            NativeManifest.RevertModVersionToBase(mod);
            string localPath = $"{FocusedGame.LocalPath}\\{Path.GetFileNameWithoutExtension(mod.LocalPath)}";
            card = await DownloadModFileAndUpdateCardStatus(progressCard, mod, mod.Uri, localPath);
        }
        card = PostInstallOrUpdateCleanup(card, mod);
        return card;
    }

    /// <summary>
    ///     Attaches the default mod click events to the specified item card associated with the mod.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <returns></returns>
    private static async Task<LibraryItemCard> AddItemCardModClickEvent(LibraryItemCard card, Mod mod)
    {
        if (card.FavoriteItemButton.IsMouseOver)
        {
            card = AddItemCardFavoriteItemButtonClickEvent(card, mod);
            return card;
        }
        if (mod.Uri == null && !mod.IsInstalled || card.OptionsButton.IsMouseOver) return card;
        if (mod.IsUpdated && mod.IsInstalled)
        {
            if (mod.IsLaunchable) await LaunchExecutable(Path.GetDirectoryName(mod.ExecutablePath), mod.ExecutablePath);
            else card = await ToggleModAndUpdateCardStatus(card, mod);
            return card;
        }
        card = await InstallOrUpdateMod(card, mod);
        return card;
    }

    /// <summary>
    ///     Attempts to download a game resource from the specified URI to the set destination path.
    /// </summary>
    /// <param name="destinationPath"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static async Task<bool> DownloadGameResource(string destinationPath, string uri)
    {
        IDownloadResponse<FileMetadata> resourceFile = await FileManager.DownloadFile(uri);
        if (resourceFile == null) return false;
        Stream stream = await resourceFile.GetContentAsStreamAsync();
        if (File.Exists(destinationPath)) File.Delete(destinationPath);
        return await WriteStreamToFile(stream, destinationPath);
    }

    /// <summary>
    ///     Attempts to download the shared toggle macro associated with the specified game, if it exists.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static async Task DownloadSharedToggleMacro(Game game)
    {
        if (game.SharedToggleMacro != null) await game.SharedToggleMacro.Download(game);
    }

    /// <summary>
    ///     Attempts to download the shared merge macro associated with the specified game, if it exists.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static async Task DownloadSharedMergeMacro(Game game)
    {
        if (game.SharedMergeMacro != null) await game.SharedMergeMacro.Download(game);
    }

    /// <summary>
    ///     Runs an integrity check for the specified game if necessary.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static async Task RunIntegrityCheck(Game game)
    {
        if (game.IntegrityCheckerUri == null || NativeManifest.HasRunIntegrityCheck(game)) return;
        game.HasRunIntegrityCheck = false;
        string integrityCheckerPath = $"{game.LocalPath}\\{Path.GetFileName(game.IntegrityCheckerUri)}";
        if (!await DownloadGameResource(integrityCheckerPath, game.IntegrityCheckerUri)) return;
        await LaunchExecutable(game.LocalPath, integrityCheckerPath, $"\"{game.LocalPath}\"", true, true);
        if (File.Exists(integrityCheckerPath)) File.Delete(integrityCheckerPath);
        game.HasRunIntegrityCheck = true;
        NativeManifest.WriteGame(game);
    }

    /// <summary>
    ///     Attaches the default game click events to the specified item card associated with the game.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="game"></param>
    /// <returns></returns>
    private static async Task<LibraryItemCard> AddItemCardGameClickEvent(LibraryItemCard card, Game game)
    {
        if (card.FavoriteItemButton.IsMouseOver)
        {
            card = AddItemCardFavoriteItemButtonClickEvent(card, game);
            return card;
        }
        if (card.OptionsButton.IsMouseOver) return card;
        if (game.IsInstalled)
        {
            FocusedGame = game;
            if (game.ImageUri != null) MainPage.gamesButton.ImageIcon = new ImageBrush(game.Image);
            await LoadLibrary<Mod>(game, true);
        }
        else
        {
            ManifestManager.ReadManifestFiles<NativeManifest>(game);
            if (game.LocalPath == null || !File.Exists(game.ExecutablePath))
            {
                FolderBrowserDialog dialog = new();
                dialog.Description = GameInstallFolderPrompt;
                if (dialog.ShowDialog() != DialogResult.OK) return card;
                if (!ManifestManager.ConfigureGameFromDatabase(game, dialog.SelectedPath, game.Name))
                {
                    ShowErrorDialog(GameExecutableError);
                    return card;
                }
                game.LocalPath = dialog.SelectedPath;
                NativeManifest.WriteGame(game);
            }
            card = RefreshItemCard(card, game);
            return card;
        }
        return card;
    }

    /// <summary>
    ///     Attaches the appropriate click events to the specified item's card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    private static void AddItemCardClickEvent(LibraryItemCard card, LibraryItem item)
    {
        card.PreviewMouseDown += async (_, _) =>
        {
            card = item switch
            {
                Mod mod => await AddItemCardModClickEvent(card, mod),
                Game game => await AddItemCardGameClickEvent(card, game),
                _ => card
            };
        };
    }

    /// <summary>
    ///     Sets the image of the specified item's card depending on the set image URI address.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private static LibraryItemCard SetItemCardImage(LibraryItemCard card, LibraryItem item)
    {
        item.Image ??= _defaultCardImage;
        if (item.Image == _defaultCardImage || item is Mod { Uri: null, IsInstalled: false } or Game { IsInstalled: false })
        {
            FormatConvertedBitmap convertedBitmap = new();
            convertedBitmap.BeginInit();
            convertedBitmap.Source = item.Image;
            convertedBitmap.DestinationFormat = PixelFormats.Gray8;
            convertedBitmap.EndInit();
            card.ImageSource = convertedBitmap;
        }
        else card.ImageSource = item.Image;
        if (item is Mod { IsUpdated: false }) card.ImageOpacity = 0.40;
        else card.ImageOpacity = 1;
        return card;
    }

    /// <summary>
    ///     Adds the specified item to the library, which can be a game or mod
    /// </summary>
    /// <param name="item"></param>
    private static void AddItemToLibrary(LibraryItem item)
    {
        LibraryItemCard card = new() { Title = item.Name };
        if (IsLibraryEmpty()) Collapse(MainPage.emptyLibraryMessage);
        UpdateItemCardStatus(card, item);
        AddItemCardFavoriteItemButtonEvents(card, item);
        AddItemCardClickEvent(card, item);
        AddItemCardOptionsButtonEvents(card, item);
        card.ItemIsFavorited = item.IsFavorited;
        card = SetItemCardImage(card, item);
        MainPage.library.Children.Add(card);
    }

    /// <summary>
    ///     Checks to see if the specified type argument corresponds to the Mod data-type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static bool IsItemTypeMod<T>()
    {
        return typeof(T) == typeof(Mod);
    }

    /// <summary>
    ///     Creates a new mod using the specified Dropbox folder metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="itemFiles"></param>
    /// <returns></returns>
    private static T CreateModFromDropboxFolder<T>(LibraryItem item, List<Metadata> itemFiles)
    {
        if (item is not Mod mod) return (T)(object)item;
        mod.ReadArchiveFilesInfo(itemFiles);
        mod.SetMetadata(itemFiles);
        if (ModsDbManager.Database == null || mod.Uri == null) return (T)(object)mod;
        mod.SetDownloadableProperties();
        return (T)(object)mod;
    }

    /// <summary>
    ///     Creates a new game using the specified Dropbox folder metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="itemFiles"></param>
    /// <returns></returns>
    private static async Task<T> CreateGameFromDropboxFolder<T>(LibraryItem item, IEnumerable<Metadata> itemFiles)
    {
        Game game = item as Game;
        await ManifestManager.ReadAllManifests(game);
        if (game == null) return (T)(object)item;
        foreach (Metadata metadata in itemFiles.Where(itemFile => itemFile.Name.EndsWith(".exe")))
        {
            FileMetadata fileMetadata = (FileMetadata)metadata;
            string exeName = fileMetadata.Name.ToLower();
            Macro sharedMacro = new(fileMetadata.ContentHash, fileMetadata.PathDisplay);
            if (exeName.Contains(ExecutableIntegrityCheckerKey)) game.IntegrityCheckerUri = fileMetadata.PathDisplay;
            else if (exeName.Contains(ExecutableModTogglerKey)) game.SharedToggleMacro = sharedMacro;
            else if (exeName.Contains(ExecutableModMergerKey)) game.SharedMergeMacro = sharedMacro;
        }
        return (T)(object)item;
    }

    /// <summary>
    ///     Creates a new game or mod using the specified Dropbox folder metadata.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="folder"></param>
    /// <returns></returns>
    private static async Task<T> CreateItemFromDropboxFolder<T>(Metadata folder) where T : LibraryItem, new()
    {
        int numTokens = IsItemTypeMod<T>() ? 4 : 3;
        List<Metadata> itemFiles = FileManager.GetFolderFiles(folder, numTokens, out string[] pathTokens);
        if (itemFiles == null) return null;
        T item = new()
        {
            Name = pathTokens[numTokens - 1],
            ImageUri = itemFiles.FirstOrDefault(i => IsFileAnImage(i.Name))?.PathDisplay
        };
        if (!IsItemTypeMod<T>()) return await CreateGameFromDropboxFolder<T>(item, itemFiles);
        ((Mod)(object)item).GameName = pathTokens[numTokens - 2];
        return CreateModFromDropboxFolder<T>(item, itemFiles);
    }

    /// <summary>
    ///     Clears the currently set search query.
    /// </summary>
    private static void ClearSearch()
    {
        MainPage.searchBox.textBox.Text = "";
    }

    /// <summary>
    ///     Begins initiation of the loading library state to prepare the necessary games or mods.
    /// </summary>
    /// <returns></returns>
    private static async Task EnterLoadingLibraryState<T>()
    {
        await _semaphore.WaitAsync();
        ClearSearch();
        ClearLibrary(false);
        ClearTags();
        Play(_refreshButtonAnim);
        Show(MainPage.loadLibraryProgressBar);
        if (IsItemTypeMod<T>()) MainPage.ShowModFilters();
        else
        {
            MainPage.gamesButton.ImageIcon = null;
            MainPage.HideModFilters();
        }
    }

    /// <summary>
    ///     Exits the current library loading state, completing the ibrary load operation.
    /// </summary>
    private static void ExitLoadingLibraryState()
    {
        Stop(_refreshButtonAnim);
        Collapse(MainPage.loadLibraryProgressBar);
        _semaphore.Release();
    }

    /// <summary>
    ///     Checks to see if the library is currently in the process of loading items.
    /// </summary>
    /// <returns></returns>
    public static bool IsCurrentlyLoadingLibrary()
    {
        return _semaphore.CurrentCount == 0;
    }

    /// <summary>
    ///     Attempts to configure a collection of items based on certain parameters, and returns the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    private static IEnumerable<T> GetMatchingItems<T>(LibraryItem item, IEnumerable<T> items) where T : LibraryItem
    {
        List<T> filteredItems = new();
        foreach (T currentItem in items)
        {
            switch (currentItem)
            {
                case Mod mod when item.Name == "" || IsMatching(mod.GameName, item.Name):
                    mod.Configure();
                    filteredItems.Add(currentItem);
                    break;
                case Game:
                    filteredItems.Add(currentItem);
                    break;
            }
        }
        return filteredItems;
    }

    /// <summary>
    ///     Checks to see if the section title is currenty set to the specified title.
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    private static bool IsCurrentSectionTitle(string title)
    {
        return IsMatching(MainPage.sectionTitle.Text, title);
    }

    /// <summary>
    ///     Refreshes all items in the library.
    /// </summary>
    /// <returns></returns>
    public static async Task RefreshLibrary()
    {
        if (IsCurrentSectionTitle(GamesSectionTitle)) await LoadLibrary<Game>();
        else await LoadLibrary<Mod>(FocusedGame);
    }

    /// <summary>
    ///     Changes the library user interface using the specified game's parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    private static void GetMatchingLibraryUIState<T>(LibraryItem item) where T : LibraryItem
    {
        string sectionTitle = IsItemTypeMod<T>() ? item == null ? "" : item.Name : GamesSectionTitle;
        MainPage.sectionTitle.Text = sectionTitle;
        MainPage.gamesButton.Content = sectionTitle;
        if (IsItemTypeMod<T>())
        {
            Show(MainPage.backButton);
            Show(MainPage.playButton);
            Show(MainPage.addLocalModButton);
        }
        else
        {
            Collapse(MainPage.backButton);
            Collapse(MainPage.playButton);
            Collapse(MainPage.addLocalModButton);
        }
    }

    /// <summary>
    ///     Refreshes the mods database and refreshes the manifest to ensure everything is up-to-date.
    /// </summary>
    /// <returns></returns>
    private static async Task RefreshModsDatabaseAndManifest()
    {
        if (ModsDbManager.Database == null)
        {
            ModsDbManager.FileManager = FileManager;
            await ModsDbManager.ReadDatabase(ModsDbFileName);
        }
        NativeManifest.ReadJSON();
    }

    /// <summary>
    ///     Adds all local mods to the library, referenced by their corresponding entries from the manifest.
    /// </summary>
    private static void AddLocalModsToLibrary()
    {
        List<Mod> mods = NativeManifest.GetAllLocalMods();
        foreach (Mod mod in mods) AddItemToLibrary(mod);
    }

    /// <summary>
    ///     Verifies the specified game's state to validate its integrity checker and macros, if they exist.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    private static async Task VerifyGameState(Game game)
    {
        if (game == null) return;
        await RunIntegrityCheck(game);
        await DownloadSharedToggleMacro(game);
        await DownloadSharedMergeMacro(game);
    }

    /// <summary>
    ///     Loads the library using the specified parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="game"></param>
    /// <param name="verifyGameState"></param>
    /// <returns></returns>
    public static async Task LoadLibrary<T>(Game game = null, bool verifyGameState = false) where T : LibraryItem, new()
    {
        if (IsCurrentlyLoadingLibrary()) return;
        await EnterLoadingLibraryState<T>();
        GetMatchingLibraryUIState<T>(game);
        await FileManager.GetAllFilesAndFolders();
        if (FileManager.Files != null)
        {
            await RefreshModsDatabaseAndManifest();
            if (verifyGameState) await VerifyGameState(game);
            List<T> items = new();
            foreach (Metadata folder in FileManager.Files.Where(i => i.IsFolder))
            {
                T item = await CreateItemFromDropboxFolder<T>(folder);
                if (item != null) items.Add(item);
            }
            await FileManager.GetItemThumbnailImages(items);
            foreach (T item in GetMatchingItems(game, items)) AddItemToLibrary(item);
        }
        if (typeof(T) == typeof(Mod)) AddLocalModsToLibrary();
        SortLibrary();
        FilterLibrary();
        if (IsLibraryEmpty()) Show(MainPage.emptyLibraryMessage);
        ExitLoadingLibraryState();
    }
}