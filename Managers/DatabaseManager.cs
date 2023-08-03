using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Newtonsoft.Json.Linq;

namespace ZModLauncher.Managers;

/// <summary>
/// Manages reading and parsing entries from a generic launcher JSON database.
/// </summary>
public class DatabaseManager
{
    /// <summary>
    /// The database used to read and parse entries from, stored as a JSON object.
    /// </summary>
    public JObject Database;
    /// <summary>
    /// The DropboxFileManager specific to the current instance of the manager.
    /// </summary>
    public DropboxFileManager FileManager;

    /// <summary>
    /// Attempts to read the database stored in Dropbox referenced by the specified database name.
    /// </summary>
    /// <param name="databaseName"></param>
    /// <returns></returns>
    public async Task ReadDatabase(string databaseName)
    {
        foreach (Metadata folder in FileManager.Files.Where(i => i.IsFolder))
        {
            List<Metadata> files = FileManager.GetFolderFiles(folder, 2, out _);
            Metadata database = files?.FirstOrDefault(i => i.Name == databaseName);
            if (database == null) return;
            try
            {
                Database = JObject.Parse(await (await FileManager.DownloadFile(database.PathDisplay)).GetContentAsStringAsync());
            }
            catch
            {
                break;
            }
        }
    }
}