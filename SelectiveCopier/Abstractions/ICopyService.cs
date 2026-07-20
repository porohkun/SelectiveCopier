namespace SelectiveCopier.Abstractions;

using ViewModels;

/// <summary>
/// Копирование отмеченных файлов в папку-цель.
/// </summary>
public interface ICopyService
{
    /// <summary>
    /// Копирует отмеченные файлы, воссоздавая структуру папок относительно источника.
    /// Существующая папка-цель предварительно очищается.
    /// </summary>
    /// <returns>Количество скопированных файлов.</returns>
    Task<int> CopyAsync(
        string sourcePath,
        string targetPath,
        IEnumerable<FileSystemNodeViewModel> nodes,
        CancellationToken cancellationToken = default);
}