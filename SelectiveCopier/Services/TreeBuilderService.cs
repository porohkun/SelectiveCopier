namespace SelectiveCopier.Services;

using System.Text.RegularExpressions;
using Abstractions;
using Infrastructure;
using Models;
using ViewModels;

[AsSingleton(typeof(ITreeBuilderService))]
internal sealed class TreeBuilderService : ITreeBuilderService
{
    public IReadOnlyList<FileSystemNodeViewModel> Build(FileSystemEntry? root, Regex? fileFilter)
    {
        if (root is null)
            return [];

        var nodes = new List<FileSystemNodeViewModel>();

        foreach (var child in Sort(root.Children))
        {
            var node = BuildNode(child, fileFilter, null);

            if (node is not null)
                nodes.Add(node);
        }

        return nodes;
    }

    /// <summary>
    /// Строит узел. Возвращает <see langword="null"/>, если узел не проходит фильтр
    /// либо является пустой (в том числе рекурсивно пустой) папкой.
    /// </summary>
    private static FileSystemNodeViewModel? BuildNode(FileSystemEntry entry, Regex? fileFilter, FileSystemNodeViewModel? parent)
    {
        if (!entry.IsDirectory)
        {
            return fileFilter is null || fileFilter.IsMatch(entry.Name)
                ? new(entry.Name, entry.FullPath, false, parent)
                : null;
        }

        var node = new FileSystemNodeViewModel(entry.Name, entry.FullPath, true, parent);

        foreach (var child in Sort(entry.Children))
        {
            var childNode = BuildNode(child, fileFilter, node);

            if (childNode is not null)
                node.Children.Add(childNode);
        }

        // Папка без содержимого — как изначально пустая, так и опустевшая после фильтра
        // (включая ту, где остались только пустые папки) — не отображается.
        return node.Children.Count == 0
            ? null
            : node;
    }

    /// <summary>
    /// Папки всегда выше файлов, внутри группы — по имени.
    /// </summary>
    private static IEnumerable<FileSystemEntry> Sort(IReadOnlyList<FileSystemEntry> entries) =>
        entries
            .OrderByDescending(entry => entry.IsDirectory)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase);
}