namespace SelectiveCopier.Services;

using Abstractions;
using Infrastructure;
using ViewModels;

[AsSingleton(typeof(ISearchService))]
internal sealed class SearchService : ISearchService
{
    public void Apply(IEnumerable<FileSystemNodeViewModel> nodes, string? searchText)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(searchText);

        foreach (var node in nodes)
            ApplyToNode(node, hasSearch ? searchText! : null);
    }

    /// <summary>
    /// Возвращает <see langword="true"/>, если узел остаётся видимым.
    /// </summary>
    private static bool ApplyToNode(FileSystemNodeViewModel node, string? searchText)
    {
        if (!node.IsDirectory)
        {
            // Поиск применяется только к файлам: обычный Contains без учёта регистра.
            var visible = searchText is null
                          || node.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);

            node.IsVisible = visible;
            return visible;
        }

        var hasVisibleChild = false;

        foreach (var child in node.Children)
        {
            // Считаем именно так, а не через Any: обойти нужно все узлы, чтобы проставить видимость каждому.
            if (ApplyToNode(child, searchText))
                hasVisibleChild = true;
        }

        // Пустые папки не считаются содержимым — скрываем рекурсивно опустевшие ветки целиком.
        node.IsVisible = hasVisibleChild;
        return hasVisibleChild;
    }
}