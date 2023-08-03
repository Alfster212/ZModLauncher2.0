using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZModLauncher.Core;
using ZModLauncher.Managers;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Manifests;

/// <summary>
///     Represents a manifest file containing all local game and mod related information.
/// </summary>
public class NativeManifest : Manifest
{
    /// <summary>
    ///     Codename of the launcher used for fetching internally stored data.
    /// </summary>
    public static readonly string ActualAppName = "ZModLauncher";
    /// <summary>
    ///     Name of the launcher executable file, primarily used for updating the launcher.
    /// </summary>
    public static readonly string ExecutableAppName = Assembly.GetExecutingAssembly().GetName().Name;
    /// <summary>
    ///     Path pointing to the root folder containing all necessary launcher files and folders.
    /// </summary>
    public static readonly string AppRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    /// <summary>
    ///     Path pointing to the manifest file to be read and parsed by the launcher.
    /// </summary>
    public new static string FilePath = $"{AppRootPath}\\{ManifestFileName}";
    /// <summary>
    ///     JSON object representing the data contained within the manifest file.
    /// </summary>
    public static JObject Manifest = GetDefaultManifest();

    /// <summary>
    ///     Creates a new instance of the manifest class and attempts to read the manifest file.
    /// </summary>
    public NativeManifest()
    {
        ReadJSON();
    }

    /// <summary>
    ///     Retrieves a JSON object representing a default manifest file.
    /// </summary>
    /// <returns></returns>
    private static JObject GetDefaultManifest()
    {
        JObject manifest = new()
        {
            { ManifestGamesKey, new JObject() },
            { ManifestModsKey, new JObject() }
        };
        return manifest;
    }

    /// <summary>
    ///     Attempts to read the manifest file as a JSON object, writing a default manifest otherwise.
    /// </summary>
    public static void ReadJSON()
    {
        try
        {
            Manifest = JObject.Parse(File.ReadAllText(FilePath));
        }
        catch
        {
            WriteJSON();
        }
    }

    /// <summary>
    ///     Serializes the currently cached manifest JSON as a string and writes it to the set file path.
    /// </summary>
    private static void WriteJSON()
    {
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(Manifest, Formatting.Indented));
    }

    /// <summary>
    ///     Attempts to configure the specified game using its associated entry from the manifest.
    /// </summary>
    /// <param name="game"></param>
    public override void ReadGame(Game game)
    {
        if (Manifest[ManifestGamesKey]?[game.Name] == null) return;
        string localPath = Manifest[ManifestGamesKey]?[game.Name]![ManifestLocalPathKey]?.ToString();
        if (localPath != null) ManifestManager.ConfigureGameFromDatabase(game, localPath, game.Name);
    }

    /// <summary>
    ///     Deserializes the specified mod entry from the manifest into a Mod object, if it exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="entry"></param>
    /// <param name="shouldBeLocal"></param>
    /// <returns></returns>
    private static Mod DeserializeModEntry(string name, JToken entry, bool shouldBeLocal = false)
    {
        string gameName = entry?[ManifestGameNameKey]?.ToString();
        if (gameName != LibraryManager.FocusedGame.Name) return null;
        if (shouldBeLocal && (entry?[ManifestIsLocalKey] == null || !bool.Parse(entry[ManifestIsLocalKey].ToString()))) return null;
        string mergeableFilesEntry = entry?[ManifestMergeableFilesKey]?.ToString();
        Mod mod = new()
        {
            Name = name,
            GameName = gameName,
            IsLocal = shouldBeLocal,
            LocalPath = entry?[ManifestLocalPathKey]?.ToString(),
            ImageUri = entry?[ManifestImageUriKey]?.ToString(),
            IsLaunchable = bool.Parse(entry?[ManifestIsLaunchableKey]?.ToString() ?? "False"),
            IsUsingSharedToggleMacro = bool.Parse(entry?[ManifestSharedToggleMacroKey]?.ToString() ?? "False"),
            ExecutablePath = entry?[ManifestExecutablePathKey]?.ToString()
        };
        string modVersionString = entry?[ManifestVersionKey]?.ToString();
        if (!string.IsNullOrEmpty(modVersionString)) mod.Version = Version.Parse(modVersionString);
        if (!string.IsNullOrEmpty(mergeableFilesEntry)) mod.MergeableFiles = JArray.Parse(mergeableFilesEntry).Children<JToken>().Select(i => i.ToString()).ToList();
        if (mod.ImageUri != null && File.Exists(mod.ImageUri)) mod.SetImageFromStream(new MemoryStream(File.ReadAllBytes(mod.ImageUri)));
        return mod;
    }

    /// <summary>
    ///     Retrieves all local mod entries from the manifest as a list.
    /// </summary>
    /// <returns></returns>
    public static List<Mod> GetAllLocalMods()
    {
        List<Mod> localMods = new();
        if (Manifest[ManifestModsKey] == null) return new List<Mod>();
        foreach (JToken entryToken in Manifest[ManifestModsKey])
        {
            JToken entry = entryToken.Children().ElementAtOrDefault(0);
            Mod mod = DeserializeModEntry(((JProperty)entryToken).Name, entry, true);
            if (mod == null) continue;
            mod.Refresh();
            localMods.Add(mod);
        }
        return localMods;
    }

    /// <summary>
    ///     Checks to see if the integrity check property for all games in the manifest can be reset.
    /// </summary>
    /// <returns></returns>
    public static bool CanClearCache()
    {
        return Manifest[ManifestGamesKey] != null
            && Manifest[ManifestGamesKey].Any(i
                => bool.Parse(i.Children().ElementAtOrDefault(0)?[ManifestHasRunIntegrityCheckKey]?.ToString() ?? "False"));
    }

    /// <summary>
    ///     Resets certain properties for the specified manifest entry.
    /// </summary>
    /// <param name="entryToken"></param>
    private static void ResetManifestEntry(JToken entryToken)
    {
        JToken entry = entryToken.Children().ElementAtOrDefault(0);
        if (entry == null) return;
        entry[ManifestHasRunIntegrityCheckKey] = false;
        ((JObject)entry).Property(ManifestIsFavoritedKey)?.Remove();
    }

    /// <summary>
    ///     Resets the integrity check property for all games in the manifest.
    /// </summary>
    public static void ClearCache()
    {
        ReadJSON();
        if (Manifest[ManifestGamesKey] == null) return;
        foreach (JToken entryToken in Manifest[ManifestGamesKey])
            ResetManifestEntry(entryToken);
        foreach (JToken entryToken in Manifest[ManifestModsKey]!)
            ResetManifestEntry(entryToken);
        WriteJSON();
    }

    /// <summary>
    ///     Determines whether an integrity check for the specified game has been run.
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    public static bool HasRunIntegrityCheck(Game game)
    {
        if (Manifest[ManifestGamesKey]![game.Name] == null) return false;
        return Manifest[ManifestGamesKey]![game.Name]![ManifestHasRunIntegrityCheckKey] != null
            && bool.Parse(Manifest[ManifestGamesKey]![game.Name]![ManifestHasRunIntegrityCheckKey]?.ToString()!);
    }

    /// <summary>
    ///     Determines if the specified library item has the IsFavorited property.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool HasIsFavoritedProperty(LibraryItem item)
    {
        string categoryKey = item is Mod ? ManifestModsKey : ManifestGamesKey;
        return Manifest[categoryKey]?[item.Name]?[ManifestIsFavoritedKey] != null;
    }

    /// <summary>
    ///     Determines if the specified library item is favorited.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static bool IsFavorited(LibraryItem item)
    {
        string categoryKey = item is Mod ? ManifestModsKey : ManifestGamesKey;
        if (Manifest[categoryKey]![item.Name] == null) return false;
        return Manifest[categoryKey]![item.Name]![ManifestIsFavoritedKey] != null && bool.Parse(Manifest[categoryKey]![item.Name]![ManifestIsFavoritedKey]?.ToString()!);
    }

    /// <summary>
    ///     Creates or replaces an entry for the specified game, and writes it to the manifest.
    /// </summary>
    /// <param name="game"></param>
    public static void WriteGame(Game game = null)
    {
        if (game != null)
        {
            JObject gameEntry = new()
            {
                [ManifestLocalPathKey] = game.LocalPath,
                [ManifestHasRunIntegrityCheckKey] = game.HasRunIntegrityCheck,
                [ManifestIsFavoritedKey] = game.IsFavorited
            };
            Manifest[ManifestGamesKey]![game.Name] = gameEntry;
        }
        WriteJSON();
    }

    // TODO: Store the mod entries in a member variable to prevent passing them around in a function argument

    /// <summary>
    ///     Attempts to retrieve mod entries which match the specified mod's game name.
    /// </summary>
    /// <returns></returns>
    private static List<JToken> GetModEntries(Mod mod)
    {
        List<JToken> entries = Manifest[ManifestModsKey]?.Children().Where(i => ((JProperty)i).Value[ManifestGameNameKey]?.ToString() == mod.GameName).ToList();
        return entries?.OrderBy(i => i.Value<JProperty>().Name).ToList();
    }

    /// <summary>
    ///     Attempts to retrieve the index of the mod entry associated with the specified mod.
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="entries"></param>
    /// <returns></returns>
    private static int GetModEntryIndex(LibraryItem mod, List<JToken> entries)
    {
        return entries.FindIndex(i => i.Value<JProperty>().Name == mod.Name);
    }

    /// <summary>
    ///     Attempts to retrieve enabled mods backwards/forwards from or around the specified mod.
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="filter"></param>
    /// <param name="forwards"></param>
    /// <returns></returns>
    public static List<Mod> FindEnabledMods(Mod mod, bool filter = false, bool forwards = false)
    {
        List<JToken> entries = GetModEntries(mod);
        int index = GetModEntryIndex(mod, entries);
        if (index == -1) return new List<Mod>();
        List<Mod> enabledMods = new();
        for (int i = 0; i < entries.Count; ++i)
        {
            if (filter && (forwards ? i <= index : i >= index)) continue;
            string name = entries[i].Value<JProperty>().Name;
            JToken entry = ((JProperty)entries[i]).Value;
            JToken status = entry[ManifestStatusKey];
            bool.TryParse(status?.ToString(), out bool isEnabled);
            if (!isEnabled) continue;
            Mod enabledMod = DeserializeModEntry(name, entry);
            enabledMod.Refresh();
            enabledMods.Add(enabledMod);
        }
        return enabledMods;
    }

    /// <summary>
    ///     Attempts to retrieve an enabled mod previous to the specified mod.
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    public static Mod FindPreviousEnabledMod(Mod mod)
    {
        return FindEnabledMods(mod, true).LastOrDefault();
    }

    /// <summary>
    ///     Creates or replaces an entry for the specified mod, and writes it to the manifest if desired.
    /// </summary>
    /// <param name="mod"></param>
    /// <param name="forceOverwrite"></param>
    public static void WriteMod(Mod mod, bool forceOverwrite = true)
    {
        if (Manifest[ManifestModsKey]![mod.Name] != null && !forceOverwrite) return;
        JObject modEntry = new();
        if (!mod.IsLaunchable) modEntry[ManifestStatusKey] = mod.IsEnabled;
        if (mod.Version != null) modEntry[ManifestVersionKey] = mod.Version.ToString();
        modEntry[ManifestGameNameKey] = LibraryManager.FocusedGame.Name;
        if (!string.IsNullOrEmpty(mod.LocalPath)) modEntry[ManifestLocalPathKey] = mod.LocalPath;
        if (mod.MergeableFiles.Count > 0)
            modEntry[ManifestMergeableFilesKey] = JsonConvert.SerializeObject(mod.MergeableFiles);
        if (mod.IsLocal)
        {
            modEntry[ManifestIsLocalKey] = mod.IsLocal;
            if (mod.ImageUri != null) modEntry[ManifestImageUriKey] = mod.ImageUri;
            modEntry[ManifestIsLaunchableKey] = mod.IsLaunchable;
            modEntry[ManifestSharedToggleMacroKey] = mod.IsUsingSharedToggleMacro;
            if (mod.IsLaunchable) modEntry[ManifestExecutablePathKey] = mod.ExecutablePath;
        }
        modEntry[ManifestIsFavoritedKey] = mod.IsFavorited;
        Manifest[ManifestModsKey]![mod.Name] = modEntry;
        WriteJSON();
    }

    /// <summary>
    ///     Attempts to delete the entry associated with the specified mod from the manifest.
    /// </summary>
    /// <param name="item"></param>
    public static void DeleteMod(LibraryItem item)
    {
        if (Manifest[ManifestModsKey]![item.Name] == null) return;
        ((JObject)Manifest.SelectToken(ManifestModsKey))?.Property(item.Name)?.Remove();
        WriteJSON();
    }

    /// <summary>
    ///     Reverts the specified mod's version to the base mod version.
    /// </summary>
    /// <param name="mod"></param>
    public static void RevertModVersionToBase(Mod mod)
    {
        if (mod.Version == null) return;
        Version baseModVersion = mod.GetBaseModFileVersion(mod.Uri);
        if (baseModVersion == null) return;
        mod.Version = baseModVersion;
        WriteMod(mod);
    }
}