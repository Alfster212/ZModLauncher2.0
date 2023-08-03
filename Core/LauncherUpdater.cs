using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Dropbox.Api.Files;
using ZModLauncher.Managers;
using ZModLauncher.Manifests;
using ZModLauncher.Pages;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;
using static ZModLauncher.Helpers.IOHelper;

namespace ZModLauncher.Core;

/// <summary>
///     Manages all launcher update operations to ensure that the launcher is always update to date.
/// </summary>
public class LauncherUpdater
{
    /// <summary>
    ///     Denotes the current locally stored version of the launcher for update comparisons.
    /// </summary>
    private static readonly Version _currentLauncherVersion = Version.Parse(new SettingsPage().LauncherVersion);
    /// <summary>
    ///     The file manager specific to the current instance of the updater.
    /// </summary>
    private static DropboxFileManager _fileManager;

    /// <summary>
    ///     Checks to see if the launcher requires updating by comparing against the latest launcher version.
    /// </summary>
    /// <returns></returns>
    public async Task CheckForUpdates()
    {
        LauncherConfigManager configManager = new();
        bool.TryParse(configManager.LauncherConfig[IsLauncherOfflineForMaintenanceKey]?.ToString(), out bool isOffline);
        while (true)
        {
            string backupLauncherExecutableDir = $"{NativeManifest.AppRootPath}\\launcher_backup";
            TryDirectoryDelete(backupLauncherExecutableDir);
            _fileManager = new DropboxFileManager();
            await _fileManager.GetAllFilesAndFolders();
            if (_fileManager.Files == null)
            {
                if (ShowErrorDialog(InternetConnectionError, MessageBoxButton.YesNo) == MessageBoxResult.Yes) continue;
                break;
            }
            Metadata updateFile = _fileManager.Files.FirstOrDefault(i => i.Name.Contains(NativeManifest.ExecutableAppName) && i.Name.EndsWith(".exe"));
            if (updateFile == null) break;
            string[] nameTokens = AssertExtractPathTokens(updateFile.Name, 2, '_');
            if (nameTokens == null) break;
            Version.TryParse(Path.GetFileNameWithoutExtension(nameTokens[1]), out Version updateFileVersion);
            if (updateFileVersion == null) break;
            if (_currentLauncherVersion < updateFileVersion)
            {
                Stream stream = await (await _fileManager.DownloadFile(updateFile.PathDisplay)).GetContentAsStreamAsync();
                string launcherName = Assembly.GetExecutingAssembly().GetName().Name;
                string launcherExecutablePath = $"{NativeManifest.AppRootPath}\\{launcherName}.exe";
                string backupLauncherExecutablePath = $"{backupLauncherExecutableDir}\\{launcherName}.exe";
                if (!Directory.Exists(backupLauncherExecutableDir)) Directory.CreateDirectory(backupLauncherExecutableDir);
                TryFileDelete(backupLauncherExecutablePath);
                File.Move(launcherExecutablePath, $"{backupLauncherExecutableDir}\\{launcherName}.exe");
                await WriteStreamToFile(stream, launcherExecutablePath);
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
            break;
        }
        if (isOffline)
        {
            ShowInformationDialog(LauncherMaintenanceMessage);
            Environment.Exit(0);
        }
    }
}