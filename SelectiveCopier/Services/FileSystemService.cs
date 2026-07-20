namespace SelectiveCopier.Services;

using System.IO;
using Abstractions;
using Infrastructure;
using Models;

[AsSingleton(typeof(IFileSystemService))]
internal sealed class FileSystemService : IFileSystemService
{
    public Task<FileSystemEntry?> ReadAsync(string rootPath, CancellationToken cancellationToken = default) =>
        Task.Run(
            () =>
            {
                if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
                    return null;

                var root = new DirectoryInfo(rootPath);

                return IsReparsePoint(root)
                    ? null
                    : ReadDirectory(root, cancellationToken);
            },
            cancellationToken);

    private static FileSystemEntry ReadDirectory(DirectoryInfo directory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var children = new List<FileSystemEntry>();

        try
        {
            foreach (var subDirectory in directory.EnumerateDirectories())
            {
                if (IsReparsePoint(subDirectory))
                    continue;

                children.Add(ReadDirectory(subDirectory, cancellationToken));
            }

            foreach (var file in directory.EnumerateFiles())
            {
                if (IsReparsePoint(file))
                    continue;

                children.Add(new(file.Name, file.FullName, false, []));
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Недоступные ветки просто пропускаем.
        }
        catch (IOException)
        {
        }

        return new(directory.Name, directory.FullName, true, children);
    }

    private static bool IsReparsePoint(FileSystemInfo info) =>
        info.Attributes.HasFlag(FileAttributes.ReparsePoint);
}