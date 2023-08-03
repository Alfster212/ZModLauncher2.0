using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZModLauncher.Managers;

namespace ZModLauncher.Core;

/// <summary>
///     Represents a generic item within the launcher library.
/// </summary>
public abstract class LibraryItem
{
    /// <summary>
    ///     Path pointing to the location of an executable file associated with the item.
    /// </summary>
    public string ExecutablePath;
    /// <summary>
    ///     List of file paths each of which point to files which are referenced by the item.
    /// </summary>
    public List<string> Files;
    /// <summary>
    ///     An image associated with the item to be displayed in the library.
    /// </summary>
    public BitmapImage Image;
    /// <summary>
    ///     The URL address which points to an online location hosting the image for the item.
    /// </summary>
    public string ImageUri;
    /// <summary>
    ///     Determines if the library item is favorited by the user.
    /// </summary>
    public bool IsFavorited;
    /// <summary>
    ///     Checks to see if the library item is currently installed locally.
    /// </summary>
    public bool IsInstalled;
    /// <summary>
    ///     Whether the options button control associated with the item's library card has been activated.
    /// </summary>
    public bool IsOptionsButtonActivated;
    /// <summary>
    ///     Path pointing to where the item is stored locally on the disk.
    /// </summary>
    public string LocalPath;
    /// <summary>
    ///     The name of the item which is then displayed in the library.
    /// </summary>
    public string Name;
    /// <summary>
    ///     Tags which describe certain attributes associated with the library item.
    /// </summary>
    public string[] Tags;
    /// <summary>
    ///     Version number associated with the item, used for versioning purposes.
    /// </summary>
    public Version Version;

    /// <summary>
    ///     Adds all unique tags associated with the library item to the tags selector.
    /// </summary>
    public void PopulateTagsSelector()
    {
        if (Tags == null) return;
        foreach (string tag in Tags)
        {
            if (LibraryManager.MainPage.tagsSelector.Items.Cast<ComboBoxItem>().All(i => i.Content.ToString() != tag))
                LibraryManager.MainPage.tagsSelector.Items.Add(new ComboBoxItem { Content = tag });
        }
    }

    /// <summary>
    ///     Sets the image property of the item using a stream containing image data.
    /// </summary>
    /// <param name="stream"></param>
    public void SetImageFromStream(Stream stream)
    {
        Image = new BitmapImage();
        Image.BeginInit();
        Image.StreamSource = stream;
        Image.CacheOption = BitmapCacheOption.None;
        Image.EndInit();
    }

    /// <summary>
    ///     Populates the list of files associated with the item by performing a directory search.
    /// </summary>
    public void SetFiles()
    {
        Files ??= Directory.GetFiles(LocalPath, "*.*", SearchOption.AllDirectories).ToList();
    }
}