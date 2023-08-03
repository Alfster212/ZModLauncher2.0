using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using static ZModLauncher.Helpers.UIHelper;
using static ZModLauncher.Constants.GlobalStringConstants;

namespace ZModLauncher.Managers;

/// <summary>
///     Manages the launcher config in order to setup, for example, branding and auth tokens.
/// </summary>
public class LauncherConfigManager
{
    /// <summary>
    ///     Filename of the launcher config, which is embedded in the launcher executable.
    /// </summary>
    private static readonly string _launcherConfigName = LauncherConfigName;
    /// <summary>
    ///     JSON object representing the data contained within the launcher config.
    /// </summary>
    public JObject LauncherConfig;

    /// <summary>
    ///     Creates a new manager and reads the launcher config.
    /// </summary>
    public LauncherConfigManager()
    {
        Read();
    }

    /// <summary>
    ///     Displays an error if the launcher config could not successfully be read.
    /// </summary>
    private static void ShowLauncherConfigError()
    {
        ShowErrorDialog(LauncherConfigError);
        Environment.Exit(0);
    }

    /// <summary>
    ///     Attempts to read the data from the launcher  config for use in the rest of the application.
    /// </summary>
    public void Read()
    {
        Assembly launcherAssembly = Assembly.GetExecutingAssembly();
        Stream stream = launcherAssembly.GetManifestResourceStream(_launcherConfigName);
        if (stream == null)
        {
            ShowLauncherConfigError();
            return;
        }
        StreamReader reader = new(stream);
        try
        {
            LauncherConfig = JObject.Parse(reader.ReadToEnd());
        }
        catch
        {
            ShowLauncherConfigError();
        }
    }
}