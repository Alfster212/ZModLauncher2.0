using System;
using System.IO;
using System.Linq;
using ZModLauncher.Core;

namespace ZModLauncher.Helpers;

/// <summary>
///     Assists with performing various string related comparisons and operations.
/// </summary>
public static class StringHelper
{
    /// <summary>
    ///     Trims any backslash characters from the start or end of a file path string.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="trimStart"></param>
    /// <returns></returns>
    public static string TrimFilePath(string path, bool trimStart)
    {
        if (trimStart && path.StartsWith("\\")) return path.Substring(1);
        return path.EndsWith("\\") ? path.Substring(0, path.Length - 1) : path;
    }

    /// <summary>
    ///     Attempts to retrieve the absolute URI from a specified URI string.
    /// </summary>
    /// <param name="uriString"></param>
    /// <returns></returns>
    public static Uri GetAbsoluteUri(string uriString)
    {
        Uri.TryCreate(uriString, UriKind.Absolute, out Uri uri);
        return uri;
    }

    /// <summary>
    ///     Generates a Dropbox encoded file hash using a path pointing to a desired local file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetFileHash(string filePath)
    {
        DropboxContentHasher hasher = new();
        byte[] buf = new byte[1024];
        using (FileStream file = File.OpenRead(filePath))
        {
            while (true)
            {
                int n = file.Read(buf, 0, buf.Length);
                if (n <= 0) break;
                hasher.TransformBlock(buf, 0, n, buf, 0);
            }
        }
        hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        string hexHash = DropboxContentHasher.ToHex(hasher.Hash);
        return hexHash;
    }

    /// <summary>
    ///     Extracts a specified substring from a specified target string.
    /// </summary>
    /// <param name="baseString"></param>
    /// <param name="leftBoundString"></param>
    /// <param name="rightBoundString"></param>
    /// <returns></returns>
    public static string ExtractString(string baseString, string leftBoundString, string rightBoundString)
    {
        int startStringIndex = baseString.IndexOf(leftBoundString, StringComparison.Ordinal) + leftBoundString.Length;
        int endStringIndex = baseString.IndexOf(rightBoundString, StringComparison.Ordinal);
        return baseString.Substring(startStringIndex, endStringIndex - startStringIndex);
    }

    /// <summary>
    ///     Extracts the individual string tokens from a specified file path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static string[] ExtractPathTokens(string path, char delimiter = ' ')
    {
        return delimiter == ' ' ? path.Split('/', '\\') : path.Split(delimiter);
    }

    /// <summary>
    ///     Attempts to extract the individual string tokens from a specified file path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="wantedNumTokens"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static string[] AssertExtractPathTokens(string path, int wantedNumTokens, char delimiter = ' ')
    {
        string[] pathTokens = ExtractPathTokens(path, delimiter);
        return pathTokens.Length != wantedNumTokens ? null : pathTokens;
    }

    /// <summary>
    ///     Checks to see if a file name is presumed to reference an image by checking its file extension.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool IsFileAnImage(string fileName)
    {
        return Enum.GetNames(typeof(ImageFileExtensions)).Any(i =>
            fileName.ToLower().EndsWith($".{i.ToLower()}"));
    }

    /// <summary>
    ///     Checks to see if two specified strings are equal to each other regardless of letter case or culture.
    /// </summary>
    /// <param name="firstString"></param>
    /// <param name="secondString"></param>
    /// <returns></returns>
    public static bool IsMatching(string firstString, string secondString)
    {
        return string.Equals(firstString, secondString, StringComparison.CurrentCultureIgnoreCase);
    }
}