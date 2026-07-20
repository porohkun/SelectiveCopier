namespace SelectiveCopier.Abstractions;

using Models;
using ViewModels;

/// <summary>
/// Восстановление выбора по содержимому папки-цели.
/// </summary>
public interface IRestoreSelectionService
{
    /// <summary>
    /// Отмечает в дереве файлы, присутствующие в папке-цели. Текущий выбор сбрасывается.
    /// </summary>
    Task<RestoreResult> RestoreAsync(
        string targetPath,
        IReadOnlyList<FileSystemNodeViewModel> nodes,
        CancellationToken cancellationToken = default);
}