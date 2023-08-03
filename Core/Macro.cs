using System.IO;
using System.Threading.Tasks;
using ZModLauncher.Managers;
using static ZModLauncher.Helpers.StringHelper;

namespace ZModLauncher.Core;

public class Macro
{
    public string LocalPath;
    public string OnlineHash;
    public string Uri;

    public Macro(string onlineHash, string uri)
    {
        OnlineHash = onlineHash;
        Uri = uri;
    }

    public async Task Download(Game game)
    {
        if (Uri == null) return;
        LocalPath = $"{game.LocalPath}\\{Path.GetFileName(Uri)}";
        if (File.Exists(LocalPath) && OnlineHash == GetFileHash(LocalPath)) return;
        if (!await LibraryManager.DownloadGameResource(LocalPath, Uri)) LocalPath = null;
    }
}