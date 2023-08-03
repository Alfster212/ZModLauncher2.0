using System;
using System.IO;
using System.Linq;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Newtonsoft.Json.Linq;
using ZModLauncher.Core;
using ZModLauncher.Managers;
using static ZModLauncher.Helpers.StringHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Manifests;

/// <summary>
///     Manages reading and extracting data from Steam manifests to use for game detection.
/// </summary>
public class SteamManifest : Manifest
{
    /// <summary>
    ///     Retrieves the value of a parent's child node at a specified numerical index.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="childIndex"></param>
    /// <returns></returns>
    private static VToken GetChildValue(VToken parent, int childIndex)
    {
        return ((VProperty)((VProperty)parent).Value.Children().ElementAt(childIndex)).Value;
    }

    /// <summary>
    ///     Attempts to locate the manifest entry associated with the specified game to set parameters.
    /// </summary>
    /// <param name="game"></param>
    public override void ReadGame(Game game)
    {
        try
        {
            VProperty baseManifest = VdfConvert.Deserialize(File.ReadAllText(FilePath));
            foreach (VToken libraryFolder in baseManifest.Value.Children())
            {
                string folderPath = $"{GetChildValue(libraryFolder, 0)}\\steamapps";
                string[] manifestFilePaths = Directory.GetFiles(folderPath, "*.acf");
                foreach (string filePath in manifestFilePaths)
                {
                    VProperty manifest = VdfConvert.Deserialize(File.ReadAllText(filePath));
                    string name = GetChildValue(manifest, 3).ToString();
                    string installDirName = GetChildValue(manifest, 5).ToString();
                    if (!IsMatching(name, game.Name)) continue;
                    if (GamesDatabase == null) return;
                    JToken gameEntry = GamesDatabase.GetValue(game.Name, StringComparison.OrdinalIgnoreCase);
                    ManifestManager.ConfigureGameFromDatabase(game, $"{folderPath}\\common\\{installDirName}\\{gameEntry?[GamesDatabaseLocalPathKey]}", game.Name);
                    return;
                }
            }
        }
        catch { }
    }
}