using System;
using System.Diagnostics;

namespace ZModLauncher.Core;

/// <summary>
///     Represents a game within the launcher which may reference mods.
/// </summary>
public class Game : LibraryItem
{
    /// <summary>
    ///     Optionally set Epic Games executable path used for overriding.
    /// </summary>
    public string EpicExecPath;
    /// <summary>
    ///     Determines whether the integrity check for the game has already been run.
    /// </summary>
    public bool HasRunIntegrityCheck = true;
    /// <summary>
    ///     URL address at which the integrity checker executable is stored.
    /// </summary>
    public string IntegrityCheckerUri;
    /// <summary>
    ///     The service provider of the game, based upon a manifest file type.
    /// </summary>
    public Type Provider;
    /// <summary>
    ///     Represents the shared merge macro associated with the game.
    /// </summary>
    public Macro SharedMergeMacro;
    /// <summary>
    ///     Represents the shared toggle macro associated with the game.
    /// </summary>
    public Macro SharedToggleMacro;
    /// <summary>
    ///     Optionally set Steam executable path used for overriding.
    /// </summary>
    public string SteamExecPath;

    /// <summary>
    ///     Sets the game's version number based on the metadata of the game's set executable.
    /// </summary>
    public void SetVersionFromExecutable()
    {
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);
        Version version = new(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
        Version = version;
    }
}