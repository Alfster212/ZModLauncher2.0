using Newtonsoft.Json.Linq;
using ZModLauncher.Core;

namespace ZModLauncher.Manifests;

/// <summary>
///     Represents a generic manifest file which contains information about various games.
/// </summary>
public abstract class Manifest
{
    /// <summary>
    ///     File path pointing to the location of the manifest file.
    /// </summary>
    public string FilePath;
    /// <summary>
    ///     Database represented as a JSON object containing all supported games within the launcher.
    /// </summary>
    public JObject GamesDatabase;

    /// <summary>
    ///     Attempts to read the specified game from the manifest in order to configure it.
    /// </summary>
    /// <param name="game"></param>
    public abstract void ReadGame(Game game);
}