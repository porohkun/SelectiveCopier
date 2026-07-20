namespace SelectiveCopier.Models;

/// <summary>
/// Неизменяемый снимок элемента файловой системы. Именно это кэшируется после чтения с диска —
/// без применения каких-либо фильтров.
/// </summary>
public sealed class FileSystemEntry
{
    public FileSystemEntry(string name, string fullPath, bool isDirectory, IReadOnlyList<FileSystemEntry> children)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Children = children;
    }

    /// <summary>
    /// Имя файла с расширением или имя папки.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Полный путь на диске.
    /// </summary>
    public string FullPath { get; }

    public bool IsDirectory { get; }

    /// <summary>
    /// Дочерние элементы. Для файлов — пустой список.
    /// </summary>
    public IReadOnlyList<FileSystemEntry> Children { get; }
}
