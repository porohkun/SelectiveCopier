namespace SelectiveCopier.Converters;

using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Abstractions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Получает системную иконку по пути и признаку папки.
/// Если система иконку не отдала, подставляет векторную заглушку из ресурсов.
/// </summary>
public sealed class IconConverter : IMultiValueConverter
{
    private IIconService? _iconService;

    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            return null;

        if (values is not [string path, bool isDirectory])
            return null;

        _iconService ??= (Application.Current as App)?.Services.GetService<IIconService>();

        return _iconService?.GetIcon(path, isDirectory) ?? Fallback(isDirectory);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static ImageSource? Fallback(bool isDirectory) =>
        Application.Current?.TryFindResource(isDirectory ? "FolderIcon" : "FileIcon") as ImageSource;
}