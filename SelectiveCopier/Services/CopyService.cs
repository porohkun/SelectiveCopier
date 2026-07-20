namespace SelectiveCopier.Services;

using System.IO;
using Abstractions;
using Infrastructure;
using ViewModels;

[AsSingleton(typeof(ICopyService))]
internal sealed class CopyService : ICopyService
{
    public Task<int> CopyAsync(
        string sourcePath,
        string targetPath,
        IEnumerable<FileSystemNodeViewModel> nodes,
        CancellationToken cancellationToken = default)
    {
        // Список формируем до очистки цели: цель может лежать внутри источника.
        var files = nodes
            .SelectMany(node => node.EnumerateSelfAndDescendants())
            .Where(node => !node.IsDirectory && node.IsChecked == true)
            .Select(node => node.FullPath)
            .ToArray();

        return Task.Run(
            () =>
            {
                ClearTarget(targetPath);
                Directory.CreateDirectory(targetPath);

                var copied = 0;

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relativePath = Path.GetRelativePath(sourcePath, file);
                    var destination = Path.Combine(targetPath, relativePath);

                    // Папки-контейнеры создаются только под реально копируемые файлы,
                    // поэтому ветки без выбранных элементов в цель не попадают.
                    var destinationDirectory = Path.GetDirectoryName(destination);

                    if (!string.IsNullOrEmpty(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);

                    File.Copy(file, destination, true);
                    copied++;
                }

                return copied;
            },
            cancellationToken);
    }

    /// <summary>
    /// Удаляет всё содержимое папки-цели, саму папку оставляет.
    /// </summary>
    private static void ClearTarget(string targetPath)
    {
        if (!Directory.Exists(targetPath))
            return;

        var directory = new DirectoryInfo(targetPath);

        foreach (var subDirectory in directory.GetDirectories())
            subDirectory.Delete(true);

        foreach (var file in directory.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
            file.Delete();
        }
    }
}