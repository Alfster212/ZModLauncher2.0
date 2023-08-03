using System.IO;
using Newtonsoft.Json.Linq;
using ZModLauncher.Core;
using ZModLauncher.Managers;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Manifests;

/// <summary>
///     Manages reading and extracting data from Epic manifests to use for game detection.
/// </summary>
public class EpicGamesManifest : Manifest
{
    /// <summary>
    ///     Attempts to locate the manifest entry associated with the specified game to set parameters.
    /// </summary>
    /// <param name="game"></param>
    public override void ReadGame(Game game)
    {
        try
        {
            JObject manifest = JObject.Parse(File.ReadAllText(FilePath));
            string displayName = manifest[EpicGamesGameNameKey]?.ToString();
            if (!IsMatching(displayName, game.Name)) return;
            ManifestManager.ConfigureGameFromDatabase(game, manifest[EpicGamesInstallLocKey]?.ToString(), game.Name);
        }
        catch { }
    }
}