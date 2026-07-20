namespace SelectiveCopier.Services;

using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Abstractions;
using Infrastructure;

[AsSingleton(typeof(IIconService))]
internal sealed partial class IconService : IIconService
{
    private const string DirectoryCacheKey = "<dir>";

    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

    private readonly Dictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    public ImageSource? GetIcon(string path, bool isDirectory)
    {
        // Иконка зависит только от расширения, поэтому кэшируем по нему, а не по полному пути.
        var key = isDirectory
            ? DirectoryCacheKey
            : Path.GetExtension(path) is { Length: > 0 } extension
                ? extension
                : "<noext>";

        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var icon = Load(path, isDirectory);
            _cache[key] = icon;
            return icon;
        }
    }

    private static ImageSource? Load(string path, bool isDirectory)
    {
        var attributes = isDirectory
            ? FILE_ATTRIBUTE_DIRECTORY
            : FILE_ATTRIBUTE_NORMAL;

        var flags = SHGFI_ICON | SHGFI_SMALLICON | SHGFI_USEFILEATTRIBUTES;

        var info = new SHFILEINFO();

        unsafe
        {
            if (SHGetFileInfo(path, attributes, ref info, (uint)sizeof(SHFILEINFO), flags) == IntPtr.Zero
                || info.hIcon == IntPtr.Zero)
            {
                return null;
            }
        }

        try
        {
            var source = Imaging.CreateBitmapSourceFromHIcon(
                info.hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            source.Freeze();
            return source;
        }
        catch (COMException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        finally
        {
            DestroyIcon(info.hIcon);
        }
    }

    [LibraryImport("shell32.dll", EntryPoint = "SHGetFileInfoW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyIcon(IntPtr hIcon);

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        // Размер должен совпадать с нативным (MAX_PATH и 80 символов UTF-16),
        // иначе SHGetFileInfo отклонит вызов по cbFileInfo.
        public fixed char szDisplayName[260];
        public fixed char szTypeName[80];
    }
}