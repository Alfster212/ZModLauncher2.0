using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using ZModLauncher.Constants;
using ZModLauncher.Core;
using ZModLauncher.Manifests;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Managers;

/// <summary>
/// Manages Dropbox connections to send/receive internal files and folders for use in the launcher
/// </summary>
public class DropboxFileManager : OAuthConstants
{
    /// <summary>
    /// The DropboxClient specific to the current instance of the manager.
    /// </summary>
    private static DropboxClient _client;
    /// <summary>
    /// Contains all currently cached file/folder metadata for managing files.
    /// </summary>
    public IList<Metadata> Files;

    /// <summary>
    /// Creates a new manager and configures default parameters and connection settings.
    /// </summary>
    public DropboxFileManager()
    {
        LauncherConfigManager configManager = new();
        RefreshToken = configManager.LauncherConfig[DropboxRefreshTokenKey]?.ToString();
        ClientId = configManager.LauncherConfig[DropboxClientIdKey]?.ToString();
        ClientSecret = configManager.LauncherConfig[DropboxClientSecretKey]?.ToString();
        HttpClient httpClient = new(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 }) { Timeout = TimeSpan.FromMinutes(1000) };
        DropboxClientConfig clientConfig = new($"{NativeManifest.ExecutableAppName}") { HttpClient = httpClient };
        _client = new DropboxClient(RefreshToken, ClientId, ClientSecret, clientConfig);
    }

    /// <summary>
    /// Retrieves all file metadata entries stored within the specified remote Dropbox folder.
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="wantedNumTokens"></param>
    /// <param name="pathTokens"></param>
    /// <returns></returns>
    public List<Metadata> GetFolderFiles(Metadata folder, int wantedNumTokens, out string[] pathTokens)
    {
        pathTokens = AssertExtractPathTokens(folder.PathDisplay, wantedNumTokens);
        return pathTokens == null ? null : GetMatchingFiles(folder.Name, wantedNumTokens - 1);
    }

    /// <summary>
    /// Retrieves all file and folder metadata entries from the Dropbox launcher app folder.
    /// </summary>
    /// <returns></returns>
    public async Task GetAllFilesAndFolders()
    {
        try
        {
            await _client.RefreshAccessToken(null);
            Files = (await _client.Files.ListFolderAsync("", true)).Entries;
        }
        catch
        {
            Files = null;
        }
    }

    /// <summary>
    /// Retrieves all file and folder metadata entries which match the specified token parameters.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="tokenIndex"></param>
    /// <param name="compareToken"></param>
    /// <returns></returns>
    private List<Metadata> GetMatchingFiles(string token, int tokenIndex, string compareToken = "")
    {
        return Files.Where(i =>
        {
            string[] tokens = ExtractPathTokens(i.PathDisplay);
            bool isMatching = i.IsFile && IsMatching(tokens.ElementAtOrDefault(tokenIndex), token) && tokens.ElementAtOrDefault(tokenIndex + 1) == i.Name;
            if (compareToken != "") return isMatching && i.PathDisplay.Contains(compareToken);
            return isMatching;
        }).ToList();
    }

    /// <summary>
    /// Attempts to retrieve all image thumbnails for the specified library items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public async Task GetItemThumbnailImages<T>(IEnumerable<T> items) where T : LibraryItem
    {
        List<T> filteredItems = items.Where(i => i.ImageUri != null).ToList();
        IEnumerable<ThumbnailArg> itemThumbnailArgs = filteredItems.Select(i =>
            new ThumbnailArg(i.ImageUri, ThumbnailFormat.Jpeg.Instance,
                ThumbnailSize.W640h480.Instance, ThumbnailMode.FitoneBestfit.Instance));
        IEnumerable<IEnumerable<ThumbnailArg>> thumbnailArgBatches = itemThumbnailArgs
            .Select((x, i) => new { Index = i, Value = x }).GroupBy(x => x.Index / 25)
            .Select(x => x.Select(v => v.Value));
        IList<GetThumbnailBatchResultEntry> itemThumbnails = new List<GetThumbnailBatchResultEntry>();
        foreach (IEnumerable<ThumbnailArg> thumbnailArgBatch in thumbnailArgBatches)
        {
            IList<GetThumbnailBatchResultEntry> currentBatchThumbnails = (await _client.Files.GetThumbnailBatchAsync(thumbnailArgBatch)).Entries;
            foreach (GetThumbnailBatchResultEntry thumbnail in currentBatchThumbnails) itemThumbnails.Add(thumbnail);
        }
        for (int i = 0; i < itemThumbnails.Count; ++i)
            filteredItems[i].SetImageFromStream(new MemoryStream(Convert.FromBase64String(itemThumbnails[i].AsSuccess.Value.Thumbnail)));
    }

    /// <summary>
    /// Downloads the specified file from Dropbox and returns a file metadata response.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public async Task<IDownloadResponse<FileMetadata>> DownloadFile(string filePath)
    {
        try
        {
            return await _client.Files.DownloadAsync(filePath);
        }
        catch
        {
            return null;
        }
    }
}