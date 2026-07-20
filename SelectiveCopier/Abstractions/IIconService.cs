namespace SelectiveCopier.Abstractions;

using System.Windows.Media;

/// <summary>
/// Системные иконки файлов и папок.
/// </summary>
public interface IIconService
{
    /// <summary>
    /// Возвращает иконку для файла по его расширению либо иконку папки.
    /// Результат кэшируется и заморожен — пригоден для многократной привязки.
    /// </summary>
    ImageSource? GetIcon(string path, bool isDirectory);
}