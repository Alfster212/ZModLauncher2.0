using System.Collections.Generic;

namespace ZModLauncher.Core;

public class LocalModParams
{
    public string FolderPath;
    public string ImagePath;
    public string LaunchPath;
    public List<string> MergeableFiles;
    public string Name;
    public bool? ShouldFavorite;
    public bool? UseSharedToggleMacro;
}