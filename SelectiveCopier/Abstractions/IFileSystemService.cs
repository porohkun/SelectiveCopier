namespace SelectiveCopier.Abstractions;

using Models;

/// <summary>
/// Чтение структуры папки с диска.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Полностью читает содержимое папки, без применения каких-либо фильтров.
    /// </summary>
    Task<FileSystemEntry?> ReadAsync(string rootPath, CancellationToken cancellationToken = default);
}