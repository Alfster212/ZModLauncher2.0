using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using ZModLauncher.Core;
using ZModLauncher.CustomControls;
using static ZModLauncher.Managers.LibraryManager;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;
using static ZModLauncher.Helpers.ThreadHelper;
using static ZModLauncher.Helpers.IOHelper;

namespace ZModLauncher.Managers;

public static class DownloadManager
{
    /// <summary>
    ///     Extracts the files and folders of the specified mod and updates its associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <param name="modFileZipPath"></param>
    /// <returns></returns>
    private static async Task<LibraryItemCard> ExtractModAndUpdateCardStatus(LibraryItemCard card, Mod mod, string modFileZipPath)
    {
        await Task.Run(async () =>
        {
            using (ZipArchive archive = ZipFile.OpenRead(modFileZipPath))
            {
                while (!mod.IsExtracted)
                {
                    mod.IsExtracting = true;
                    mod.IsWaiting = false;
                    await RunBackgroundAction(() => card = RefreshItemCard(card, mod));
                    mod.IsExtracted = ExtractToDirectory(archive, mod.IsLaunchable ? FocusedGame.LocalPath : mod.LocalPath, true);
                    if (mod.IsExtracted) continue;
                    mod.IsWaiting = true;
                    await RunBackgroundAction(() => card = RefreshItemCard(card, mod));
                    await Task.Delay(4000);
                }
                mod.IsExtracted = false;
            }
            if (File.Exists(modFileZipPath) && modFileZipPath.Contains(FocusedGame.LocalPath)) File.Delete(modFileZipPath);
            mod.IsExtracting = false;
            return card;
        });
        return card;
    }

    /// <summary>
    ///     Downloads the specified file associated with a mod and updates its associated item card.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="mod"></param>
    /// <param name="uri"></param>
    /// <param name="localPath"></param>
    /// <returns></returns>
    public static async Task<LibraryItemCard> DownloadModFileAndUpdateCardStatus(LibraryItemCard card, Mod mod, string uri, string localPath)
    {
        mod.IsBusy = true;
        await Task.Run(async () =>
        {
            while (mod.IsBusy)
            {
                mod.IsQueuing = true;
                card = await UpdateItemCardProgress(card, mod);
                mod.IsQueuing = false;
                IDownloadResponse<FileMetadata> response = await FileManager.DownloadFile(uri);
                if (response == null)
                {
                    mod.IsReconnecting = true;
                    card = await UpdateItemCardProgress(card, mod);
                    mod.IsReconnecting = false;
                    await Task.Delay(4000);
                    continue;
                }
                Stream stream = await response.GetContentAsStreamAsync();
                string modFileZipPath = $"{localPath}.zip";
                FileStream fileStream;
                try
                {
                    fileStream = File.OpenWrite(modFileZipPath);
                }
                catch
                {
                    ShowErrorDialog(ZipAccessError);
                    continue;
                }
                int prevProgress = 0;
                int streamBufferLength;
                bool resumeTask = false;
                do
                {
                    if (mod.IsCancellingDownload || mod.IsDownloadPaused || !IsCurrentPage(MainPageName) && !IsCurrentPage(SettingsPageName))
                    {
                        fileStream.Close();
                        stream.Close();
                        if (!mod.IsCancellingDownload) return card;
                        if (File.Exists(modFileZipPath) && !mod.IsDownloadPaused) File.Delete(modFileZipPath);
                        mod.IsBusy = false;
                        await RunBackgroundAction(() => card = RefreshItemCard(card, mod));
                        mod.IsCancellingDownload = false;
                        return card;
                    }
                    byte[] streamBuffer = new byte[Mod.DownloadChunkSize];
                    try
                    {
                        streamBufferLength = await stream.ReadAsync(streamBuffer, 0, Mod.DownloadChunkSize);
                    }
                    catch
                    {
                        fileStream.Close();
                        resumeTask = true;
                        break;
                    }
                    try
                    {
                        fileStream.Write(streamBuffer, 0, streamBufferLength);
                    }
                    catch
                    {
                        ShowErrorDialog(NotEnoughSpaceError);
                        return card;
                    }
                    mod.Progress = (int)((double)fileStream.Length / response.Response.Size * 100);
                    if (mod.Progress == prevProgress) continue;
                    prevProgress = mod.Progress;
                    if (!mod.IsOptionsButtonActivated) card = await UpdateItemCardProgress(card, mod);
                } while (stream.CanRead && streamBufferLength > 0);
                if (resumeTask) continue;
                fileStream.Close();
                card = await ExtractModAndUpdateCardStatus(card, mod, modFileZipPath);
                await RunBackgroundAction(() => card = RefreshItemCard(card, mod));
                mod.IsBusy = false;
                if (mod.IsUpdated) mod.Refresh();
            }
            return card;
        });
        return card;
    }
}