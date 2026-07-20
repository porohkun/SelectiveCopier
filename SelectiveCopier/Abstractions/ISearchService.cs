namespace SelectiveCopier.Abstractions;

using ViewModels;

/// <summary>
/// Применение строки поиска к дереву.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Проставляет <see cref="FileSystemNodeViewModel.IsVisible"/> по строке поиска.
    /// Состояние чекбоксов не изменяется.
    /// </summary>
    void Apply(IEnumerable<FileSystemNodeViewModel> nodes, string? searchText);
}