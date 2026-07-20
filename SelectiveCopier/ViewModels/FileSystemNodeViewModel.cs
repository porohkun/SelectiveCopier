using System.Collections.ObjectModel;

using SelectiveCopier.Infrastructure;

namespace SelectiveCopier.ViewModels;

/// <summary>
/// Узел дерева файловой системы.
/// </summary>
/// <remarks>
/// <see cref="IsChecked"/> отражает РЕАЛЬНОЕ содержимое папки (<see cref="Children"/>),
/// а не отфильтрованное строкой поиска отображение. <see cref="IsVisible"/> влияет только на показ
/// и никогда не участвует в расчёте состояния чекбокса.
/// </remarks>
public sealed class FileSystemNodeViewModel : BindableBase
{
    private bool? _isChecked = false;

    public FileSystemNodeViewModel(string name, string fullPath, bool isDirectory, FileSystemNodeViewModel? parent)
    {
        Name = name;
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Parent = parent;
    }

    /// <summary>
    /// Имя файла с расширением либо имя папки.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Полный путь на диске.
    /// </summary>
    public string FullPath { get; }

    public bool IsDirectory { get; }

    public FileSystemNodeViewModel? Parent { get; }

    /// <summary>
    /// Реальные дочерние узлы — то, что осталось после фильтра-регулярки.
    /// Строка поиска на состав этой коллекции не влияет.
    /// </summary>
    public ObservableCollection<FileSystemNodeViewModel> Children { get; } = [];

    /// <summary>
    /// Состояние чекбокса. <see langword="null"/> — неопределённое: выбрана часть содержимого.
    /// Сеттер предназначен для действий пользователя: распространяет значение вниз и пересчитывает предков.
    /// </summary>
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            // Клик по чекбоксу, находящемуся в неопределённом состоянии, трактуем как «снять всё».
            var effective = value ?? false;

            if (_isChecked == effective)
                return;

            _isChecked = effective;
            RaisePropertyChanged();

            SetSubtree(effective);
            Parent?.RefreshCheckStateFromChildren();
        }
    }

    /// <summary>
    /// Видимость узла по строке поиска. На <see cref="IsChecked"/> не влияет.
    /// </summary>
    public bool IsVisible
    {
        get;
        set => Set(ref field, value);
    } = true;

    public bool IsExpanded
    {
        get;
        set => Set(ref field, value);
    }

    /// <summary>
    /// Пересчитывает состояние по реальным дочерним узлам и рекурсивно поднимает изменение к предкам.
    /// </summary>
    public void RefreshCheckStateFromChildren()
    {
        if (!IsDirectory || Children.Count == 0)
            return;

        var checkedCount = 0;
        var uncheckedCount = 0;

        foreach (var child in Children)
        {
            switch (child._isChecked)
            {
                case true:
                    checkedCount++;
                    break;

                case false:
                    uncheckedCount++;
                    break;
            }
        }

        bool? state = checkedCount == Children.Count
            ? true
            : uncheckedCount == Children.Count
                ? false
                : null;

        if (_isChecked != state)
        {
            _isChecked = state;
            RaisePropertyChanged(nameof(IsChecked));
        }

        Parent?.RefreshCheckStateFromChildren();
    }

    /// <summary>
    /// Перечисляет узел и всех его потомков.
    /// </summary>
    public IEnumerable<FileSystemNodeViewModel> EnumerateSelfAndDescendants()
    {
        yield return this;

        foreach (var descendant in Children.SelectMany(child => child.EnumerateSelfAndDescendants()))
            yield return descendant;
    }

    /// <summary>
    /// Жёстко проставляет значение всему поддереву без обратного всплытия.
    /// </summary>
    private void SetSubtree(bool value)
    {
        foreach (var child in Children)
        {
            if (child._isChecked != value)
            {
                child._isChecked = value;
                child.RaisePropertyChanged(nameof(IsChecked));
            }

            child.SetSubtree(value);
        }
    }
}
