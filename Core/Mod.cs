using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Newtonsoft.Json.Linq;
using ZModLauncher.Managers;
using ZModLauncher.Manifests;
using static ZModLauncher.Helpers.IOHelper;
using static ZModLauncher.Helpers.CommandLineHelper;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Constants.GlobalStringConstants;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Manifests.NativeManifest;

namespace ZModLauncher.Core;

/// <summary>
///     Represents a single game mod with an associated file and folder structure.
/// </summary>
public class Mod : LibraryItem
{
    /// <summary>
    ///     The size of each chunk to be downloaded from the mod's associated download stream.
    /// </summary>
    public static int DownloadChunkSize = 1024;
    /// <summary>
    ///     The name of the author who originally published the mod.
    /// </summary>
    public string AuthorName;
    /// <summary>
    ///     Represents the database entry for the mod containing shared properties on the host server.
    /// </summary>
    public JToken DatabaseEntry;
    /// <summary>
    ///     The URL address used for linking users to a direct download of the mod, if necessary.
    /// </summary>
    public Uri DirectDownloadUri;
    /// <summary>
    ///     The first recorded date and time in which the mod's files were uploaded to the host server.
    /// </summary>
    public DateTime? FirstPublished;
    /// <summary>
    ///     The name of the game associated with the mod.
    /// </summary>
    public string GameName;
    /// <summary>
    ///     Base game files which are associated with any of the mod's files.
    /// </summary>
    public List<string> GameRelativeFiles = new();
    /// <summary>
    ///     The number of installed updates for the mod while the mod is being updated.
    /// </summary>
    public int InstalledUpdates;
    /// <summary>
    ///     Whether the mod is currently busy with a local or network related operation.
    /// </summary>
    public bool IsBusy;
    /// <summary>
    ///     Whether the end-user has requested cancellation while downloading this mod.
    /// </summary>
    public bool IsCancellingDownload;
    /// <summary>
    ///     Whether the download associated with the mod has been temporarily paused by the user.
    /// </summary>
    public bool IsDownloadPaused;
    /// <summary>
    ///     Whether the mod is currently enabled, if this mod is a toggleable mod.
    /// </summary>
    public bool IsEnabled;
    /// <summary>
    ///     Whether the mod's files have been extracted, after an attempted extraction operation.
    /// </summary>
    public bool IsExtracted;
    /// <summary>
    ///     Whether the mod is attempting to extract its files and folders after a download operation.
    /// </summary>
    public bool IsExtracting;
    /// <summary>
    ///     Whether the mod is a launchable mod, otherwise it is considered to be a toggleable mod.
    /// </summary>
    public bool IsLaunchable;
    /// <summary>
    ///     Whether the mod is a locally created mod, as opposed to a mod shared on the host server.
    /// </summary>
    public bool IsLocal;
    /// <summary>
    ///     Whether the mod is currently being queued for a download or update operation.
    /// </summary>
    public bool IsQueuing;
    /// <summary>
    ///     Whether the mod is attempting to re-establish a network connection following an interruption.
    /// </summary>
    public bool IsReconnecting;
    /// <summary>
    ///     Whether the mod is currently being toggled on or off, if it is a toggleable mod.
    /// </summary>
    public bool IsToggling;
    /// <summary>
    ///     Whether the mod is updated to the latest version, if it is both a shared mod and updateable.
    /// </summary>
    public bool IsUpdated = true;
    /// <summary>
    ///     Whether the mod should use the game's shared toggle macro, should it exist, if the mod is toggleable.
    /// </summary>
    public bool IsUsingSharedToggleMacro = true;
    /// <summary>
    ///     Whether the mod is currently waiting for another process to release a required program resource(s).
    /// </summary>
    public bool IsWaiting;
    /// <summary>
    ///     The last recorded date and time in which the mod's files were updated on the host server.
    /// </summary>
    public DateTime? LastUpdated;
    /// <summary>
    ///     Files which are suitable for merging with files from other mods.
    /// </summary>
    public List<string> MergeableFiles = new();
    /// <summary>
    ///     The URL address used for linking users to a website containing information about the mod.
    /// </summary>
    public Uri ModInfoUri;
    /// <summary>
    ///     File path pointing to a native toggle macro for the mod, should it exist, if the mod is toggleable.
    /// </summary>
    public string NativeToggleMacroPath;
    /// <summary>
    ///     The amount of progress, represented as a percentage, made during a download or update operation.
    /// </summary>
    public int Progress;
    /// <summary>
    ///     The cumulative list of update files associated with the mod should it require an update(s).
    /// </summary>
    public List<string> UpdateFiles = new();
    /// <summary>
    ///     The URL address at which the mod's associated ZIP archive is stored on the host server.
    /// </summary>
    public string Uri;

    /// <summary>
    ///     Sets various metadata associated with the mod using the specified mod files.
    /// </summary>
    /// <param name="modFiles"></param>
    public void SetMetadata(List<Metadata> modFiles)
    {
        DatabaseEntry = LibraryManager.ModsDbManager.Database.GetValue(Name, StringComparison.OrdinalIgnoreCase);
        AuthorName = DatabaseEntry?[ModsDatabaseAuthorNameKey]?.ToString();
        if (modFiles.Count > 0)
        {
            FirstPublished = modFiles.Min(i => ((FileMetadata)i).ClientModified);
            LastUpdated = modFiles.Max(i => ((FileMetadata)i).ServerModified);
        }
        Tags = DatabaseEntry?[DatabaseTagsKey]?.ToObject<string[]>();
        PopulateTagsSelector();
        Uri modInfoUri = GetAbsoluteUri(DatabaseEntry?[ModsDatabaseModInfoUriKey]?.ToString());
        if (modInfoUri != null) ModInfoUri = modInfoUri;
        string isPreFavoritedString = DatabaseEntry?[ModsDatabaseIsPreFavorited]?.ToString();
        if (string.IsNullOrEmpty(isPreFavoritedString) || HasIsFavoritedProperty(this)) return;
        bool.TryParse(isPreFavoritedString, out bool isPreFavorited);
        IsFavorited = isPreFavorited;
        WriteMod(this);
    }

    /// <summary>
    ///     Sets various properties of the mod which should only be set if the mod is considered downloadable.
    /// </summary>
    public void SetDownloadableProperties()
    {
        Uri directDownloadUri = GetAbsoluteUri(DatabaseEntry?[ModsDatabaseDirectDownloadUriKey]?.ToString());
        if (directDownloadUri != null) DirectDownloadUri = directDownloadUri;
        NativeToggleMacroPath = DatabaseEntry?[ModsDatabaseNativeMacroKey]?.ToString();
        string isUsingSharedToggleMacro = DatabaseEntry?[ModsDatabaseSharedToggleMacroKey]?.ToString();
        if (isUsingSharedToggleMacro != null) IsUsingSharedToggleMacro = bool.Parse(isUsingSharedToggleMacro);
        string mergeableFilesEntry = DatabaseEntry?[ModsDatabaseMergeableFilesKey]?.ToString();
        if (!string.IsNullOrEmpty(mergeableFilesEntry))
        {
            List<JToken> mergeableFileTokens = JArray.Parse(mergeableFilesEntry).Children().ToList();
            MergeableFiles = mergeableFileTokens.Select(i => i.ToString()).ToList();
        }
        string execPath = DatabaseEntry?[DatabaseExecutableKey]?.ToString();
        if (execPath == null) return;
        ExecutablePath = execPath;
        IsLaunchable = true;
    }

    /// <summary>
    ///     Reads various information from the mod's associated archive files, and records version numbers.
    /// </summary>
    /// <param name="modFiles"></param>
    public void ReadArchiveFilesInfo(List<Metadata> modFiles)
    {
        List<Metadata> zipFiles = modFiles.Where(i => i.Name.EndsWith(".zip")).ToList();
        foreach (Metadata zipFile in zipFiles)
        {
            if (GetUpdateFileVersionInfo(zipFile.Name) == null)
            {
                SetModVersion(zipFile.Name);
                Uri = zipFile.PathDisplay;
            }
            else UpdateFiles.Add(zipFile.PathDisplay);
        }
        UpdateFiles.Sort();
    }

    /// <summary>
    ///     Edits the mod using the specified local properties.
    /// </summary>
    /// <param name="modParams"></param>
    public void Edit(LocalModParams modParams)
    {
        DeleteMod(this);
        Name = modParams.Name;
        LocalPath = modParams.FolderPath;
        ImageUri = modParams.ImagePath;
        IsUsingSharedToggleMacro = Convert.ToBoolean(modParams.UseSharedToggleMacro);
        MergeableFiles = modParams.MergeableFiles;
        IsFavorited = Convert.ToBoolean(modParams.ShouldFavorite);
        ExecutablePath = modParams.LaunchPath;
        WriteMod(this);
    }

    /// <summary>
    ///     Deletes the mod's associated files, and removes its entry from the manifest if necessary.
    /// </summary>
    /// <returns></returns>
    public async Task Delete()
    {
        if (!IsLaunchable && IsEnabled) await Toggle();
        if (IsLaunchable) TryFileDelete(ExecutablePath);
        else TryDirectoryDelete(LocalPath);
        DeleteMod(this);
        IsUpdated = true;
        Refresh();
    }

    /// <summary>
    ///     Proceeds to locally install the mod to the specified folder path, linking its optional image and launch path.
    /// </summary>
    /// <param name="modParams"></param>
    /// <returns></returns>
    public async Task InstallLocal(LocalModParams modParams)
    {
        string localPath = IsLaunchable ? LibraryManager.FocusedGame.LocalPath : LocalPath;
        await Task.Run(() =>
        {
            TryDirectoryCopy(modParams.FolderPath, localPath);
            TryFileCopy(modParams.ImagePath, ImageUri);
        });
        if (IsLaunchable) ExecutablePath = GetDestRelativeFilePath(modParams.FolderPath, localPath, modParams.LaunchPath);
        WriteMod(this);
    }

    /// <summary>
    ///     Determines if the specified local properties are valid for the mod.
    /// </summary>
    /// <param name="modParams"></param>
    /// <param name="isLaunchable"></param>
    /// <returns></returns>
    public static bool AreLocalPropertiesValid(LocalModParams modParams, bool isLaunchable)
    {
        if (string.IsNullOrEmpty(modParams.Name) || string.IsNullOrEmpty(modParams.FolderPath))
        {
            ShowInformationDialog(LocalModRequiredInfoMessage);
            return false;
        }
        if (!isLaunchable
            || TryDirectoryGetFiles(modParams.FolderPath).Any(i => i.Contains(Path.GetFileName(modParams.LaunchPath)))
            && !string.IsNullOrEmpty(modParams.LaunchPath)) return true;
        ShowInformationDialog(LocalModLaunchExeMessage);
        return false;
    }

    /// <summary>
    ///     Attempts to create a new local mod given the appropriate parameters.
    /// </summary>
    /// <param name="modParams"></param>
    /// <returns></returns>
    public static Mod CreateLocalMod(LocalModParams modParams)
    {
        bool isLaunchable = !string.IsNullOrEmpty(modParams.LaunchPath);
        Mod mod = new()
        {
            Name = modParams.Name,
            IsLaunchable = isLaunchable,
            IsUsingSharedToggleMacro = !isLaunchable && Convert.ToBoolean(modParams.UseSharedToggleMacro),
            MergeableFiles = modParams.MergeableFiles,
            IsLocal = true,
            IsFavorited = Convert.ToBoolean(modParams.ShouldFavorite)
        };
        string launchableLocalPath = Path.GetDirectoryName(modParams.LaunchPath)?.Replace($"{modParams.FolderPath}\\", "");
        mod.LocalPath = mod.IsLaunchable ? $"{LibraryManager.FocusedGame.LocalPath}\\{launchableLocalPath}" :
            $"{LibraryManager.FocusedGame.LocalPath}\\LauncherMods\\{mod.Name}";
        mod.ImageUri = string.IsNullOrEmpty(modParams.ImagePath) ? null : $"{mod.LocalPath}\\{Path.GetFileName(modParams.ImagePath)}";
        return mod;
    }

    /// <summary>
    ///     Attempts to navigate to the mod's locally stored folder in the native file explorer application.
    /// </summary>
    public void OpenLocalFolder()
    {
        if (IsEnabled)
        {
            string localPath = Path.GetDirectoryName(GameRelativeFiles[0]) ?? LocalPath;
            foreach (string filePath in GameRelativeFiles)
            {
                string[] trimmedPath = TrimFilePath(filePath.Replace(LibraryManager.FocusedGame.LocalPath, ""), true).Split('\\');
                if (trimmedPath.Length < 2) continue;
                localPath = Path.GetDirectoryName(filePath) ?? LocalPath;
                break;
            }
            Process.Start(localPath);
        }
        else Process.Start(LocalPath);
    }

    /// <summary>
    ///     Overrides the mod's previous enabled status with the currently set one.
    /// </summary>
    private void UpdateEnabledStatus()
    {
        JToken modStatus = NativeManifest.Manifest[ManifestModsKey]?[Name]?[ManifestStatusKey];
        if (modStatus != null) IsEnabled = bool.Parse(modStatus.ToString());
    }

    /// <summary>
    ///     Retrieves the version number associated with the specified base mod file, should it exist.
    /// </summary>
    /// <param name="baseModFileName"></param>
    /// <returns></returns>
    public Version GetBaseModFileVersion(string baseModFileName)
    {
        string[] tokens = AssertExtractPathTokens(baseModFileName, 2, '_');
        if (tokens == null) return null;
        Version.TryParse(Path.GetFileNameWithoutExtension(tokens[1]), out Version version);
        return version;
    }

    /// <summary>
    ///     Retrieves the version number associated with the specified update file.
    /// </summary>
    /// <param name="updateFileName"></param>
    /// <returns></returns>
    public Version[] GetUpdateFileVersionInfo(string updateFileName)
    {
        List<Version> info = new();
        string[] tokens = AssertExtractPathTokens(updateFileName, 4, '_');
        if (tokens == null) return null;
        Version.TryParse(tokens[1], out Version gameVersion);
        Version.TryParse(Path.GetFileNameWithoutExtension(tokens[3]), out Version modVersion);
        if (gameVersion == null || modVersion == null) return null;
        info.AddRange(new[] { gameVersion, modVersion });
        return info.ToArray();
    }

    /// <summary>
    ///     Retrieves the locally stored version number for the current mod from the manifest.
    /// </summary>
    /// <returns></returns>
    private string GetModManifestVersion()
    {
        return NativeManifest.Manifest[ManifestModsKey]?[Name]?[ManifestVersionKey]?.ToString();
    }

    /// <summary>
    ///     Retrieves the specified mod's filename without it containing a version number.
    /// </summary>
    /// <param name="modFileName"></param>
    /// <returns></returns>
    public string GetModFileDirWithoutVersion(string modFileName)
    {
        return modFileName.Substring(0, modFileName.IndexOf('_'));
    }

    /// <summary>
    ///     Sets the base mod version using the version number in the specified mod filename, if it exists.
    /// </summary>
    /// <param name="baseModFileName"></param>
    public void SetModVersion(string baseModFileName)
    {
        Version baseModFileVersion = GetBaseModFileVersion(baseModFileName);
        if (baseModFileVersion == null) return;
        string manifestVersion = GetModManifestVersion();
        Version = manifestVersion == null ? baseModFileVersion : Version.Parse(manifestVersion);
    }

    /// <summary>
    ///     Retrieves a filtered list of update files which are compatible with the mod's current version.
    /// </summary>
    /// <param name="updateFiles"></param>
    /// <returns></returns>
    public List<string> FilterValidUpdateFiles(List<string> updateFiles)
    {
        for (int i = UpdateFiles.Count - 1; i >= 0; --i)
        {
            string updateFilePath = UpdateFiles[i];
            Version[] updateFileInfo = GetUpdateFileVersionInfo(updateFilePath);
            if (LibraryManager.FocusedGame.Version < updateFileInfo[0] || Version >= updateFileInfo[1] && !File.Exists($"{LocalPath}\\{Path.GetFileName(updateFilePath)}"))
                updateFiles.RemoveAt(i);
        }
        return updateFiles;
    }

    /// <summary>
    ///     Checks to see if any updates are available for the mod.
    /// </summary>
    public void CheckForUpdates()
    {
        List<string> filteredUpdateFiles = FilterValidUpdateFiles(UpdateFiles.ToList());
        IsUpdated = filteredUpdateFiles.Count == 0 || Version == null;
    }

    /// <summary>
    ///     Sets the mod's associated game relative files.
    /// </summary>
    private void SetGameRelativeFiles()
    {
        foreach (string path in Files)
            GameRelativeFiles.Add(GetFilePathRelativeToGame(path));
    }

    /// <summary>
    ///     Configures the mod using the currently focused game's properties.
    /// </summary>
    public void Configure()
    {
        if (Uri == null) return;
        string modFileDir = Path.GetFileNameWithoutExtension(Uri);
        if (Version != null) modFileDir = GetModFileDirWithoutVersion(modFileDir);
        LocalPath = IsLaunchable ? $"{LibraryManager.FocusedGame.LocalPath}\\{Path.GetDirectoryName(ExecutablePath)}"
            : $"{LibraryManager.FocusedGame.LocalPath}\\LauncherMods\\{modFileDir}";
        ExecutablePath = IsLaunchable ? $"{LibraryManager.FocusedGame.LocalPath}\\{ExecutablePath}" : $"{LocalPath}\\{ExecutablePath}";
        if (NativeToggleMacroPath != null) NativeToggleMacroPath = $"{LocalPath}\\{NativeToggleMacroPath}";
        Refresh();
    }

    /// <summary>
    ///     Refreshes all necessary mod properties.
    /// </summary>
    public void Refresh()
    {
        IsInstalled = Directory.Exists(LocalPath);
        if (IsLaunchable) IsInstalled = File.Exists(ExecutablePath);
        IsFavorited = IsFavorited(this);
        if (!IsInstalled) return;
        WriteMod(this, false);
        CheckForUpdates();
        LibraryManager.FocusedGame.SetFiles();
        SetFiles();
        SetGameRelativeFiles();
        UpdateEnabledStatus();
    }

    /// <summary>
    ///     Retrieves the game relative file path of the specified mod file path.
    /// </summary>
    /// <param name="modFilePath"></param>
    /// <returns></returns>
    private string GetFilePathRelativeToGame(string modFilePath)
    {
        return $"{LibraryManager.FocusedGame.LocalPath}\\{TrimFilePath(modFilePath.Replace(LocalPath, ""), true)}";
    }

    /// <summary>
    ///     Launches a toggle macro associated with the mod using the specified directory and filename.
    /// </summary>
    /// <param name="workingDir"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task LaunchToggleMacro(string workingDir, string fileName)
    {
        string modFolderName = Path.GetFileNameWithoutExtension(LocalPath);
        await LaunchExecutable(workingDir, fileName, $"\"{LibraryManager.FocusedGame.LocalPath}\" \"{modFolderName}\"", true);
    }

    /// <summary>
    ///     Launches the game's shared toggle macro associated with the mod, if it has one.
    /// </summary>
    /// <returns></returns>
    private async Task LaunchSharedToggleMacro()
    {
        await LaunchToggleMacro(LibraryManager.FocusedGame.LocalPath, LibraryManager.FocusedGame.SharedToggleMacro.LocalPath);
    }

    /// <summary>
    ///     Launches the game's shared merge macro associated with the mod, if it has one.
    /// </summary>
    /// <returns></returns>
    private static async Task LaunchSharedMergeMacro(string sourceModFilePath, string targetModFilePath, string mergedModFilePath)
    {
        string workingDir = LibraryManager.FocusedGame.LocalPath;
        string fileName = LibraryManager.FocusedGame.SharedMergeMacro.LocalPath;
        await LaunchExecutable(workingDir, fileName, $"\"{sourceModFilePath}\" \"{targetModFilePath}\" \"{mergedModFilePath}\"", true);
    }

    /// <summary>
    ///     Launches the mod's native toggle macro, if it has one.
    /// </summary>
    /// <returns></returns>
    private async Task LaunchNativeToggleMacro()
    {
        await LaunchToggleMacro(LocalPath, NativeToggleMacroPath);
    }

    /// <summary>
    ///     Toggles all of the mods in the specified mod list.
    /// </summary>
    /// <param name="mods"></param>
    /// <returns></returns>
    private static async Task ToggleMods(List<Mod> mods)
    {
        foreach (Mod mod in mods) await mod.Toggle(false);
    }

    /*
    * TODO: Change the way in which games/mods are accessed internally within the codebase in order to:
    * 1. Allow for future implementation of an offline access mode within the launcher
    * 2. Maintain consistency across the codebase to make backend development easier
    */

    /// <summary>
    ///     Toggles the mod on or off, moving the mod's files and setting appropriate parameters.
    /// </summary>
    /// <returns></returns>
    public async Task Toggle(bool shouldFindEnabledMods = true)
    {
        IsEnabled = !IsEnabled;
        WriteMod(this);
        Mod previousEnabledMod = IsEnabled ? FindPreviousEnabledMod(this) : null;
        List<Mod> enabledMods = shouldFindEnabledMods ? IsEnabled ? FindEnabledMods(this, true, true) : FindEnabledMods(this) : new List<Mod>();
        await ToggleMods(enabledMods);
        foreach (string modFilePath in Files)
        {
            string modFileName = Path.GetFileName(modFilePath);
            string relativeGameModFilePath = GetFilePathRelativeToGame(modFilePath);
            bool previousEnabledModFileIsMergeable = previousEnabledMod != null && previousEnabledMod.MergeableFiles.Contains(modFileName);
            bool currentModFileIsMergeable = MergeableFiles.Contains(modFileName);
            if (previousEnabledModFileIsMergeable && currentModFileIsMergeable)
            {
                string previousEnabledModFilePath = previousEnabledMod.GameRelativeFiles.Find(i => i.EndsWith(modFileName));
                await LaunchSharedMergeMacro(previousEnabledModFilePath, modFilePath, relativeGameModFilePath);
            }
            else
            {
                string relativeGameModFileDir = Path.GetDirectoryName(relativeGameModFilePath);
                if (File.Exists(relativeGameModFilePath))
                {
                    string relativeGameModZipFilePath = $"{relativeGameModFilePath}.zip";
                    switch (IsEnabled)
                    {
                        case true:
                            CreateZipFromFile(relativeGameModFilePath);
                            break;
                        case false:
                            TryFileDelete(relativeGameModFilePath);
                            ExtractAndDeleteZip(relativeGameModZipFilePath);
                            break;
                    }
                }
                if (!IsEnabled) continue;
                if (relativeGameModFileDir != null && !Directory.Exists(relativeGameModFileDir))
                    Directory.CreateDirectory(relativeGameModFileDir);
                TryFileCopy(modFilePath, relativeGameModFilePath);
            }
        }
        await ToggleMods(enabledMods);
        if (LibraryManager.FocusedGame.SharedToggleMacro.LocalPath != null && IsUsingSharedToggleMacro) await LaunchSharedToggleMacro();
        if (NativeToggleMacroPath != null) await LaunchNativeToggleMacro();
    }
}