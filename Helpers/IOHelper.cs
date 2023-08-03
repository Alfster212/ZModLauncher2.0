using System;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ZModLauncher.Constants.GlobalStringConstants;
using static ZModLauncher.Helpers.UIHelper;

namespace ZModLauncher.Helpers;

/// <summary>
///     Contains various file and folder related helper methods.
/// </summary>
public static class IOHelper
{
    /// <summary>
    ///     Retrieves the destination relative file path of the specified file path using the set source directory.
    /// </summary>
    /// <param name="sourceDirPath"></param>
    /// <param name="destDirPath"></param>
    /// <param name="sourceFilePath"></param>
    /// <returns></returns>
    public static string GetDestRelativeFilePath(string sourceDirPath, string destDirPath, string sourceFilePath)
    {
        string destRelativePath = sourceFilePath.Replace(sourceDirPath, destDirPath);
        return destRelativePath;
    }

    /// <summary>
    ///     Attempts to move all files and folders from the specified source directory to the destination directory.
    /// </summary>
    /// <param name="sourceDirPath"></param>
    /// <param name="destDirPath"></param>
    public static void TryDirectoryCopy(string sourceDirPath, string destDirPath)
    {
        if (!Directory.Exists(destDirPath)) Directory.CreateDirectory(destDirPath);
        foreach (string dirPath in Directory.GetDirectories(sourceDirPath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(sourceDirPath, destDirPath));
        foreach (string newPath in Directory.GetFiles(sourceDirPath, "*.*", SearchOption.AllDirectories))
            TryFileCopy(newPath, newPath.Replace(sourceDirPath, destDirPath));
    }

    /// <summary>
    ///     Attempts to move the file at the specified source path to the location at the destination path.
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destFilePath"></param>
    /// <returns></returns>
    public static bool TryFileCopy(string sourceFilePath, string destFilePath)
    {
        try
        {
            string destDirPath = Path.GetDirectoryName(destFilePath) ?? "";
            if (!Directory.Exists(destDirPath)) Directory.CreateDirectory(destDirPath);
            File.Copy(sourceFilePath, destFilePath, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Assigns all default image file filters to the specified open file dialog.
    /// </summary>
    /// <param name="dialog"></param>
    public static void AssignDialogImagesFileFilter(OpenFileDialog dialog)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
        string sep = string.Empty;
        foreach (ImageCodecInfo c in codecs)
        {
            string codecName = c.CodecName.Substring(8).Replace("Codec", "Files").Trim();
            dialog.Filter = string.Format(@"{0}{1}{2} ({3})|{3}", dialog.Filter, sep, codecName, c.FilenameExtension);
            sep = "|";
        }
        dialog.Filter = string.Format(@"{0}{1}{2} ({3})|{3}", dialog.Filter, sep, "All Files", "*.*");
        dialog.FilterIndex = 5;
    }

    /// <summary>
    ///     Attempts to delete the file at the specified file path.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool TryFileDelete(string filePath)
    {
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Attempts to delete the directory at the specified folder path recursively.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    public static bool TryDirectoryDelete(string folderPath)
    {
        try
        {
            Directory.Delete(folderPath, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Attempts to retrieve all files and folders from the specified path.
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="fileExt"></param>
    /// <returns></returns>
    public static string[] TryDirectoryGetFiles(string folderPath, string fileExt = "*.*")
    {
        try
        {
            return Directory.GetFiles(folderPath, fileExt, SearchOption.AllDirectories);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <summary>
    ///     Attempts to write the specified buffer stream to the specified file path.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<bool> WriteStreamToFile(Stream stream, string filePath)
    {
        try
        {
            FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            fileStream.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Attempts to create a ZIP file and write it to the specified file path.
    /// </summary>
    /// <param name="filePath"></param>
    public static void CreateZipFromFile(string filePath)
    {
        try
        {
            using FileStream fileStream = new($"{filePath}.zip", FileMode.CreateNew);
            using ZipArchive archive = new(fileStream, ZipArchiveMode.Create, true);
            archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
        }
        catch { }
    }

    /// <summary>
    ///     Attempts to extract the ZIP file at the specified file path and delete it afterwards.
    /// </summary>
    /// <param name="zipPath"></param>
    public static void ExtractAndDeleteZip(string zipPath)
    {
        if (!File.Exists(zipPath)) return;
        try
        {
            using FileStream fileStream = new(zipPath, FileMode.Open);
            using ZipArchive archive = new(fileStream, ZipArchiveMode.Read);
            archive.ExtractToDirectory(Path.GetDirectoryName(zipPath));
            fileStream.Close();
            File.Delete(zipPath);
        }
        catch { }
    }

    /// <summary>
    ///     Attempts to extract the specified ZIP archive to the destination directory.
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="destinationDirectoryName"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public static bool ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
    {
        if (!overwrite)
        {
            archive.ExtractToDirectory(destinationDirectoryName);
            return true;
        }
        DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
        string destinationDirectoryFullPath = di.FullName;
        foreach (ZipArchiveEntry file in archive.Entries)
        {
            string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));
            if (file.Name == "")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(completeFileName)!);
                continue;
            }
            try
            {
                file.ExtractToFile(completeFileName, true);
            }
            catch (InvalidDataException)
            {
                ShowErrorDialog(ZipFormatError);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
        return true;
    }
}