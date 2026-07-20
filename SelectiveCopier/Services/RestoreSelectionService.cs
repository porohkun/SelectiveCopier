namespace SelectiveCopier.Services;

using System.IO;
using Abstractions;
using Infrastructure;
using Models;
using ViewModels;

[AsSingleton(typeof(IRestoreSelectionService))]
internal sealed class RestoreSelectionService : IRestoreSelectionService
{
    public async Task<RestoreResult> RestoreAsync(
        string targetPath,
        IReadOnlyList<FileSystemNodeViewModel> nodes,
        CancellationToken cancellationToken = default)
    {
        // Скан диска — в фоне; изменение дерева — на вызывающем потоке (UI).
        var relativePaths = await Task.Run(
            () => Directory
                .EnumerateFiles(targetPath, "*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(targetPath, file))
                .ToArray(),
            cancellationToken);

        var files = nodes
            .SelectMany(node => node.EnumerateSelfAndDescendants())
            .Where(node => !node.IsDirectory)
            .ToArray();

        // Корень дерева — это содержимое папки-источника, поэтому относительный путь
        // узла считаем от родителя верхнего уровня, а не от FullPath целиком.
        var byRelativePath = new Dictionary<string, FileSystemNodeViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
            byRelativePath[GetRelativePath(file)] = file;

        // Сброс текущего выбора: узлы дерева, а не только файлы, — чтобы папки пересчитались.
        foreach (var root in nodes)
            root.IsChecked = false;

        var restored = 0;
        var notFound = new List<string>();

        foreach (var relativePath in relativePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Файл, пропавший из источника либо отсеянный текущим фильтром, просто пропускаем.
            if (!byRelativePath.TryGetValue(relativePath, out var node))
            {
                notFound.Add(relativePath);
                continue;
            }

            node.IsChecked = true;
            restored++;
        }

        return new(restored, notFound);
    }

    /// <summary>
    /// Путь узла относительно папки-источника, собранный по цепочке родителей.
    /// </summary>
    private static string GetRelativePath(FileSystemNodeViewModel node)
    {
        var parts = new Stack<string>();

        for (var current = node; current is not null; current = current.Parent)
            parts.Push(current.Name);

        return Path.Combine([.. parts]);
    }
}