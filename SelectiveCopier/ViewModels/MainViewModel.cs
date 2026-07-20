namespace SelectiveCopier.ViewModels;

using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Abstractions;
using Infrastructure;
using Models;

[AsSingleton]
public sealed class MainViewModel : BindableBase
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ITreeBuilderService _treeBuilderService;
    private readonly ISearchService _searchService;
    private readonly ICopyService _copyService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IRestoreSelectionService _restoreSelectionService;

    /// <summary>
    /// Полный, нефильтрованный снимок папки-источника.
    /// </summary>
    private FileSystemEntry? _cache;

    /// <summary>
    /// Путь, для которого прочитан <see cref="_cache"/>.
    /// </summary>
    private string? _cachedPath;

    private string _sourcePath = string.Empty;

    private string _targetPath;

    private string _fileFilter;

    private bool _isFileFilterValid = true;

    private string _searchText = string.Empty;

    private bool _isBusy;

    public MainViewModel(
        IFileSystemService fileSystemService,
        ITreeBuilderService treeBuilderService,
        ISearchService searchService,
        ICopyService copyService,
        IDialogService dialogService,
        ISettingsService settingsService,
        IRestoreSelectionService restoreSelectionService)
    {
        _fileSystemService = fileSystemService;
        _treeBuilderService = treeBuilderService;
        _searchService = searchService;
        _copyService = copyService;
        _dialogService = dialogService;
        _settingsService = settingsService;
        _restoreSelectionService = restoreSelectionService;

        var settings = settingsService.Settings;

        RecentSources = settings.RecentSources;
        _targetPath = settings.TargetPath;
        _fileFilter = settings.FileFilter;
        _isFileFilterValid = TryCompileFilter(_fileFilter, out _);

        BrowseSourceCommand = new(BrowseSource);
        BrowseTargetCommand = new(BrowseTarget);
        RefreshCommand = new(RefreshAsync, () => !IsBusy);
        CopyCommand = new(CopyAsync, CanCopy);
        RestoreSelectionCommand = new(RestoreSelectionAsync, CanRestoreSelection);
    }

    /// <summary>
    /// Предыдущие папки-источники. Пополняется только по кнопке «Копировать».
    /// </summary>
    public ObservableCollection<string> RecentSources { get; }

    /// <summary>
    /// Отображаемые узлы верхнего уровня.
    /// </summary>
    public ObservableCollection<FileSystemNodeViewModel> Nodes { get; } = [];

    /// <summary>
    /// Путь-источник. Его изменение перечитывает дерево с диска.
    /// </summary>
    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (!Set(ref _sourcePath, value))
                return;

            _ = ReloadFromDiskAsync();
        }
    }

    /// <summary>
    /// Путь-цель. Сохраняется в настройках сразу.
    /// </summary>
    public string TargetPath
    {
        get => _targetPath;
        set
        {
            if (!Set(ref _targetPath, value))
                return;

            _settingsService.Settings.TargetPath = value;
            CopyCommand.RaiseCanExecuteChanged();
            RestoreSelectionCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Регулярка-фильтр для имён файлов. Дерево при её изменении НЕ перестраивается —
    /// только проверяется валидность. Перестройка идёт по кнопке «Перечитать».
    /// </summary>
    public string FileFilter
    {
        get => _fileFilter;
        set
        {
            if (!Set(ref _fileFilter, value))
                return;

            IsFileFilterValid = TryCompileFilter(value, out _);

            if (IsFileFilterValid)
                _settingsService.Settings.FileFilter = value;
        }
    }

    /// <summary>
    /// Валидна ли текущая регулярка. Невалидная подсвечивается в поле ввода.
    /// </summary>
    public bool IsFileFilterValid
    {
        get => _isFileFilterValid;
        private set => Set(ref _isFileFilterValid, value);
    }

    /// <summary>
    /// Строка поиска. Меняет только видимость узлов, состав коллекции и чекбоксы не трогает.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (!Set(ref _searchText, value))
                return;

            _searchService.Apply(Nodes, value);
        }
    }

    /// <summary>
    /// Идёт длительная операция — интерфейс блокируется.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!Set(ref _isBusy, value))
                return;

            RefreshCommand.RaiseCanExecuteChanged();
            CopyCommand.RaiseCanExecuteChanged();
            RestoreSelectionCommand.RaiseCanExecuteChanged();
        }
    }

    public RelayCommand BrowseSourceCommand { get; }

    public RelayCommand BrowseTargetCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand CopyCommand { get; }

    public AsyncRelayCommand RestoreSelectionCommand { get; }

    private static bool TryCompileFilter(string? pattern, out Regex? regex)
    {
        regex = null;

        if (string.IsNullOrWhiteSpace(pattern))
            return true;

        try
        {
            regex = new(pattern, RegexOptions.IgnoreCase);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private void BrowseSource()
    {
        var path = _dialogService.BrowseFolder(SourcePath);

        if (path is not null)
            SourcePath = path;
    }

    private void BrowseTarget()
    {
        var path = _dialogService.BrowseFolder(TargetPath);

        if (path is not null)
            TargetPath = path;
    }

    /// <summary>
    /// Кнопка «Перечитать». Если путь не менялся — диск не трогаем, перестраиваем из кэша.
    /// </summary>
    private async Task RefreshAsync()
    {
        if (!string.Equals(_cachedPath, SourcePath, StringComparison.OrdinalIgnoreCase))
        {
            await ReloadFromDiskAsync();
            return;
        }

        RebuildTree();
    }

    /// <summary>
    /// Читает папку-источник с диска в кэш и перестраивает дерево.
    /// </summary>
    private async Task ReloadFromDiskAsync()
    {
        var path = SourcePath;

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            _cache = null;
            _cachedPath = null;
            Nodes.Clear();
            CopyCommand.RaiseCanExecuteChanged();
            return;
        }

        IsBusy = true;

        try
        {
            _cache = await _fileSystemService.ReadAsync(path);
            _cachedPath = path;
            RebuildTree();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _cache = null;
            _cachedPath = null;
            Nodes.Clear();
            _dialogService.ShowError($"Не удалось прочитать папку:{Environment.NewLine}{e.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Перестраивает отображаемое дерево из кэша, применяя фильтр и текущий поиск.
    /// </summary>
    private void RebuildTree()
    {
        // Невалидная регулярка сюда не доходит: дерево остаётся прежним до её исправления.
        if (!TryCompileFilter(FileFilter, out var regex))
            return;

        Nodes.Clear();

        foreach (var node in _treeBuilderService.Build(_cache, regex))
            Nodes.Add(node);

        _searchService.Apply(Nodes, SearchText);
        CopyCommand.RaiseCanExecuteChanged();
    }

    private bool CanCopy() =>
        !IsBusy
        && !string.IsNullOrWhiteSpace(SourcePath)
        && !string.IsNullOrWhiteSpace(TargetPath)
        && Nodes.Count > 0;

    private async Task CopyAsync()
    {
        var source = SourcePath;
        var target = TargetPath;

        var hasSelection = Nodes
            .SelectMany(node => node.EnumerateSelfAndDescendants())
            .Any(node => !node.IsDirectory && node.IsChecked == true);

        if (!hasSelection)
        {
            _dialogService.ShowInformation("Не выбрано ни одного файла.");
            return;
        }

        IsBusy = true;

        try
        {
            var copied = await _copyService.CopyAsync(source, target, Nodes);

            AddRecentSource(source);
            _dialogService.ShowInformation($"Скопировано файлов: {copied}.");
            _dialogService.OpenInExplorer(target);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _dialogService.ShowError($"Ошибка копирования:{Environment.NewLine}{e.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Добавляет источник в список недавних. Вызывается только после успешного копирования.
    /// </summary>
    private void AddRecentSource(string path)
    {
        var existing = RecentSources.FirstOrDefault(item => string.Equals(item, path, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
            RecentSources.Remove(existing);

        RecentSources.Insert(0, path);

        while (RecentSources.Count > AppSettings.MaxRecentSources)
            RecentSources.RemoveAt(RecentSources.Count - 1);

        _settingsService.Save();
    }

    /// <summary>
    /// Кнопка активна, только если цель существует и в ней есть хоть один файл.
    /// </summary>
    private bool CanRestoreSelection()
    {
        if (IsBusy || Nodes.Count == 0 || string.IsNullOrWhiteSpace(TargetPath) || !Directory.Exists(TargetPath))
            return false;

        try
        {
            return Directory.EnumerateFiles(TargetPath, "*", SearchOption.AllDirectories).Any();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private async Task RestoreSelectionAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _restoreSelectionService.RestoreAsync(TargetPath, [.. Nodes]);

            var message = $"Восстановлено файлов: {result.Restored} из {result.TotalInTarget}.";

            if (result.NotFound.Count > 0)
            {
                _dialogService.SetClipboardText(string.Join(Environment.NewLine, result.NotFound));

                message += $"{Environment.NewLine}{Environment.NewLine}"
                           + $"Не найдено в дереве: {result.NotFound.Count}."
                           + $"{Environment.NewLine}Пути скопированы в буфер обмена.";
            }

            _dialogService.ShowInformation(message);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _dialogService.ShowError($"Не удалось прочитать папку-цель:{Environment.NewLine}{e.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}