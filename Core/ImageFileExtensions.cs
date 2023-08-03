using System.Diagnostics.CodeAnalysis;

namespace ZModLauncher.Core;

/// <summary>
///     Contains all supported image file extensions within the launcher.
/// </summary>
#pragma warning disable IDE0079
[SuppressMessage("ReSharper", "UnusedMember.Global")]
#pragma warning restore IDE0079
public enum ImageFileExtensions
{
    JPG,
    JPEG,
    BMP,
    GIF,
    PNG,
    WEBP
}