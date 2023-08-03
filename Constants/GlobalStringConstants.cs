using Microsoft.Win32;
using ZModLauncher.Manifests;

namespace ZModLauncher.Constants;

/// <summary>
///     Stores all global string constant values used throughout the launcher for various purposes.
/// </summary>
public class GlobalStringConstants
{
    public const string GamesSectionTitle = "Games";
    public const string LaunchNowStatus = "Launch Now";
    public const string EnabledStatus = "Enabled";
    public const string DisabledStatus = "Disabled";
    public const string TogglingStatus = "Toggling...";
    public const string ComingSoonStatus = "Coming Soon";
    public const string DownloadStatus = "Download";
    public const string UpdateStatus = "Update";
    public const string DownloadingStatus = "Downloading";
    public const string UpdatingStatus = "Updating";
    public const string QueuingStatus = "Queuing...";
    public const string ReconnectingStatus = "Reconnecting...";
    public const string NotInstalledStatus = "Not Detected";
    public const string PlayNowStatus = "Play Now";
    public const string InstallingStatus = "Installing...";
    public const string WaitingStatus = "Waiting...";
    public const string GamesDbFileName = "games.json";
    public const string ModsDbFileName = "mods.json";
    public const string ManifestFileName = "native.manifest";
    public const string DefaultCardImageKey = "DefaultCardImage";
    public const string RefreshButtonAnimKey = "RefreshButtonAnim";
    public const string MainPageName = "MainPage";
    public const string LoadingPageName = "LoadingPage";
    public const string SettingsPageName = "SettingsPage";
    public const string SignInPageName = "SignInPage";
    public const string NoInternetResponse = "__jstcache";
    public const string ActivePatronStatus = "active_patron";
    public const string DeclinedPatronStatus = "declined_patron";
    public const string ManifestGamesKey = "Games";
    public const string ManifestModsKey = "Mods";
    public const string ManifestGameNameKey = "GameName";
    public const string ManifestLocalPathKey = "LocalPath";
    public const string ManifestHasRunIntegrityCheckKey = "HasRunIntegrityCheck";
    public const string ExecutableIntegrityCheckerKey = "integritychecker";
    public const string ExecutableModTogglerKey = "modtoggler";
    public const string ExecutableModMergerKey = "modmerger";
    public const string ManifestStatusKey = "Status";
    public const string ManifestVersionKey = "Version";
    public const string ManifestImageUriKey = "ImageUri";
    public const string ManifestSharedToggleMacroKey = "IsUsingSharedToggleMacro";
    public const string ManifestMergeableFilesKey = "MergeableFiles";
    public const string ManifestIsLocalKey = "IsLocal";
    public const string ManifestIsLaunchableKey = "IsLaunchable";
    public const string ManifestExecutablePathKey = "ExecutablePath";
    public const string ManifestIsFavoritedKey = "IsFavorited";
    public const string ModsDatabaseIsPreFavorited = "IsPreFavorited";
    public const string ModsDatabaseAuthorNameKey = "AuthorName";
    public const string GamesDatabaseLocalPathKey = "LocalPath";
    public const string GamesDatabaseSteamExecPathKey = "SteamExecPath";
    public const string GamesDatabaseEpicExecPathKey = "EpicExecPath";
    public const string DatabaseExecutableKey = "ExecutablePath";
    public const string DatabaseTagsKey = "Tags";
    public const string ModsDatabaseNativeMacroKey = "NativeToggleMacroPath";
    public const string ModsDatabaseSharedToggleMacroKey = "IsUsingSharedToggleMacro";
    public const string ModsDatabaseMergeableFilesKey = "MergeableFiles";
    public const string ModsDatabaseModInfoUriKey = "ModInfoUri";
    public const string ModsDatabaseDirectDownloadUriKey = "DirectDownloadUri";
    public const string EpicGamesGameNameKey = "DisplayName";
    public const string EpicGamesInstallLocKey = "InstallLocation";
    public const string PatreonRedirectUriKey = "PatreonRedirectUri";
    public const string PatreonClientIdKey = "PatreonClientId";
    public const string PatreonClientSecretKey = "PatreonClientSecret";
    public const string PatreonCreatorUrlKey = "PatreonCreatorUrl";
    public const string DropboxRefreshTokenKey = "DropboxRefreshToken";
    public const string DropboxClientIdKey = "DropboxClientId";
    public const string DropboxClientSecretKey = "DropboxClientSecret";
    public const string YouTubeResourceLinkKey = "YouTubeResourceLink";
    public const string TwitterResourceLinkKey = "TwitterResourceLink";
    public const string RoadmapResourceLinkKey = "RoadmapResourceLink";
    public const string FAQResourceLinkKey = "FAQResourceLink";
    public const string PrepareLauncherMessageLinkKey = "PrepareLauncherMessage";
    public const string RejectTierIdKey = "RejectTierId";
    public const string IsLauncherOfflineForMaintenanceKey = "IsLauncherOfflineForMaintenance";
    public const string GameInstallFolderPrompt = "Select the game's installation folder:";
    public const string LocalModLaunchExeMessage = "The executable cannot be located outside of the mod's folder.";
    public const string LocalModRequiredInfoMessage = "You must specify both a name and folder path for the mod.";
    public const string DeleteModConfirmation = "Are you sure you want to delete this mod?";
    public const string ClearLauncherCacheConfirmation = "Are you sure you want to clear the cache? It is not recommended unless absolutely necessary.";
    public const string AppPermissionsError = "The launcher does not appear to be configured correctly, ensure that you have Microsoft Edge installed.";
    public const string ZipAccessError = "The mod's archive is being used by another process, the download will attempt to re-queue.";
    public const string NotEnoughSpaceError = "There is not enough space to download/extract the mod, please try again later.";
    public const string LauncherLoginInfoClearError = "The requested user login information could not be cleared.";
    public const string InternetConnectionError = "A network connection could not be established to check for launcher updates, do you want to retry?";
    public const string ZipFormatError = "The mod's archive was delivered in an incorrect format, please have the developers re-upload it.";
    public const string GameExecutableError = "The game's executable or online database could not be read.";
    public const string ModExecutableError = "The mod's executable or online database could not be read.";
    public const string LauncherConfigError = "The internal launcher configuration could not be read.";
    public const string LauncherMaintenanceMessage = "The launcher is currently offline for mandatory maintenance, please try again later.";
    public static readonly string LauncherConfigName = $"{NativeManifest.ActualAppName}.launcherconfig.json";
    public static readonly string[] SteamFolderPaths =
    {
        $"{Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "")}\\steamapps",
        $"{Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath", "")}\\steamapps",
        $"{Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "InstallPath", "")}\\steamapps",
        $"{Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "SteamPath", "")}\\steamapps"
    };
    public static readonly string[] EpicFolderPaths =
    {
        $"{Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Epic Games\\EpicGamesLauncher", "AppDataPath", "")}Manifests",
        $"{Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Epic Games\\EpicGamesLauncher", "AppDataPath", "")}Manifests",
        $"{Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Epic Games\\EpicGamesLauncher", "AppDataPath", "")}Manifests",
        $"{Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\WOW6432Node\\Epic Games\\EpicGamesLauncher", "AppDataPath", "")}Manifests"
    };
}