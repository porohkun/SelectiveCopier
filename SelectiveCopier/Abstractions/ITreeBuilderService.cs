namespace SelectiveCopier.Abstractions;

using System.Text.RegularExpressions;
using Models;
using ViewModels;

/// <summary>
/// Построение отображаемого дерева из кэша с применением фильтра-регулярки.
/// </summary>
public interface ITreeBuilderService
{
    /// <summary>
    /// Строит узлы дерева из кэша.
    /// </summary>
    /// <param name="root">Кэш, прочитанный с диска.</param>
    /// <param name="fileFilter">
    /// Регулярка, применяемая только к именам файлов (с расширением).
    /// <see langword="null"/> — фильтрация не выполняется.
    /// </param>
    /// <returns>Узлы верхнего уровня. Пустые ветки отброшены.</returns>
    IReadOnlyList<FileSystemNodeViewModel> Build(FileSystemEntry? root, Regex? fileFilter);
}