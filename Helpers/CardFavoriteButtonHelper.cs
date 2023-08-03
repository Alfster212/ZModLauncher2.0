using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ZModLauncher.Core;
using ZModLauncher.CustomControls;
using ZModLauncher.Managers;
using ZModLauncher.Manifests;

namespace ZModLauncher.Helpers;

public static class CardFavoriteButtonHelper
{
    /// <summary>
    ///     Attempts to display the favorite item button for the specified item card if it has not been loaded.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static async Task AssertItemCardFavoriteItemButtonLoadedState(LibraryItemCard card)
    {
        while (card.FavoriteItemButton == null || card.FavoriteItemButtonIndicator == null) await Task.Delay(1000);
    }

    /// <summary>
    ///     Attaches the default click event to the favorite item button associated with the specified item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static LibraryItemCard AddItemCardFavoriteItemButtonClickEvent(LibraryItemCard card, LibraryItem item)
    {
        item.IsFavorited = !item.IsFavorited;
        card.ItemIsFavorited = item.IsFavorited;
        switch (item)
        {
            case Game game:
                NativeManifest.WriteGame(game);
                break;
            case Mod mod:
                NativeManifest.WriteMod(mod);
                break;
        }
        LibraryManager.SortLibrary();
        LibraryManager.FilterLibrary();
        return card;
    }

    /// <summary>
    ///     Configures the necessary parameters for the specified item card's favorite item button based on the specified
    ///     library item.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    public static void ConfigureItemCardFavoriteItemButton(LibraryItemCard card, LibraryItem item)
    {
        card.FavoriteItemButton.Visibility = item.IsFavorited && item is not Mod { IsBusy: true } ? Visibility.Visible : Visibility.Collapsed;
        card.FavoriteItemButtonIndicator.Foreground = item.IsFavorited ? Brushes.White : Brushes.Black;
        card.FavoriteItemButton.MouseEnter += (_, _) =>
            card.FavoriteItemButtonIndicator.Foreground = !item.IsFavorited || item is Mod { IsBusy: true } ? Brushes.White : Brushes.Black;
        card.FavoriteItemButton.MouseLeave += (_, _) =>
            card.FavoriteItemButtonIndicator.Foreground = !item.IsFavorited || item is Mod { IsBusy: true } ? Brushes.Black : Brushes.White;
    }

    /// <summary>
    ///     Attaches the default favorite item button events to the specified item card associated with the library item.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="item"></param>
    public static void AddItemCardFavoriteItemButtonEvents(LibraryItemCard card, LibraryItem item)
    {
        card.Loaded += async (_, _) =>
        {
            await AssertItemCardFavoriteItemButtonLoadedState(card);
            ConfigureItemCardFavoriteItemButton(card, item);
        };
        card.MouseEnter += (_, _) => card.FavoriteItemButton.Visibility = item is not Mod { IsBusy: true } ? Visibility.Visible : Visibility.Collapsed;
        card.MouseLeave += (_, _) => card.FavoriteItemButton.Visibility = item.IsFavorited && item is not Mod { IsBusy: true } ? Visibility.Visible : Visibility.Collapsed;
    }
}