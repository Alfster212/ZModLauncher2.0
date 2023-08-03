using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ZModLauncher.Core;
using ZModLauncher.Manifests;
using static ZModLauncher.Constants.GlobalStringConstants;
using static ZModLauncher.Helpers.IOHelper;
using static ZModLauncher.Helpers.StringHelper;

namespace ZModLauncher.Managers;

/// <summary>
///     Manages reading and parsing data from a particular manifest type within the launcher.
/// </summary>
public class ManifestManager
{
    /// <summary>
    ///     File extension which describes the manifest file type.
    /// </summary>
    private static string _fileExtension;
    /// <summary>
    ///     Folder paths pointing to folders containing manifest files which match the set type.
    /// </summary>
    private static string[] _folderPaths;
    /// <summary>
    ///     Manager for interacting with the database of stored games within the launcher.
    /// </summary>
    private static readonly DatabaseManager _gamesDbManager = new();

    /// <summary>
    ///     Creates a new manager with the specified Dropbox file manager for context.
    /// </summary>
    /// <param name="fileManager"></param>
    public ManifestManager(DropboxFileManager fileManager)
    {
        _gamesDbManager.FileManager = fileManager;
    }

    /// <summary>
    ///     Configures the specified game from the games database given a local path and game name.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="localPath"></param>
    /// <param name="manifestGameName"></param>
    /// <returns></returns>
    public static bool ConfigureGameFromDatabase(Game game, string localPath, string manifestGameName)
    {
        if (_gamesDbManager.Database == null) return false;
        JToken gameEntry = _gamesDbManager.Database.GetValue(game.Name, StringComparison.OrdinalIgnoreCase);
        localPath = TrimFilePath(localPath, false);
        if (!Directory.Exists(localPath)) localPath = localPath.Replace(manifestGameName, manifestGameName.Replace(" ", ""));
        game.LocalPath = localPath;
        string steamExecPath = gameEntry?[GamesDatabaseSteamExecPathKey]?.ToString();
        string epicExecPath = gameEntry?[GamesDatabaseEpicExecPathKey]?.ToString();
        if (game.Provider == typeof(SteamManifest) && steamExecPath != null)
            game.SteamExecPath = steamExecPath;
        else if (game.Provider == typeof(EpicGamesManifest) && epicExecPath != null)
            game.EpicExecPath = epicExecPath;
        string execPath = gameEntry?[DatabaseExecutableKey]?.ToString();
        game.ExecutablePath = $"{game.LocalPath}\\{execPath}";
        game.Tags = gameEntry?[DatabaseTagsKey]?.ToObject<string[]>();
        game.PopulateTagsSelector();
        game.IsInstalled = File.Exists(game.ExecutablePath);
        game.IsFavorited = NativeManifest.IsFavorited(game);
        if (game.IsInstalled) game.SetVersionFromExecutable();
        return game.IsInstalled;
    }

    /// <summary>
    ///     Attempts to read all manifest files of the given type which reference the specified game.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="game"></param>
    public void ReadManifestFiles<T>(Game game) where T : Manifest, new()
    {
        if (game.LocalPath != null) return;
        game.Provider = typeof(T) != typeof(NativeManifest) ? typeof(T) : null;
        if (typeof(T) == typeof(SteamManifest))
        {
            _folderPaths = SteamFolderPaths;
            _fileExtension = "*.vdf";
        }
        else if (typeof(T) == typeof(EpicGamesManifest))
        {
            _folderPaths = EpicFolderPaths;
            _fileExtension = "*.item";
        }
        if (typeof(T) != typeof(NativeManifest))
        {
            string[] manifestFilePaths = Array.Empty<string>();
            foreach (string path in _folderPaths)
            {
                manifestFilePaths = TryDirectoryGetFiles(path, _fileExtension);
                if (manifestFilePaths.Length > 0) break;
            }
            if (manifestFilePaths.Length == 0) return;
            foreach (string path in manifestFilePaths)
                new T { FilePath = path, GamesDatabase = _gamesDbManager.Database }.ReadGame(game);
        }
        else if (File.Exists(NativeManifest.FilePath))
            new NativeManifest { GamesDatabase = _gamesDbManager.Database }.ReadGame(game);
        else NativeManifest.WriteGame();
    }

    /// <summary>
    ///     Reads manifest files for all supported manifest types, and configures the specified game.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    public async Task ReadAllManifests(Game game)
    {
        if (_gamesDbManager.Database == null) await _gamesDbManager.ReadDatabase(GamesDbFileName);
        ReadManifestFiles<SteamManifest>(game);
        ReadManifestFiles<EpicGamesManifest>(game);
        ReadManifestFiles<NativeManifest>(game);
    }
}