namespace SelectiveCopier.Models;

using System.Collections.ObjectModel;
using Infrastructure;

public class AppSettings : BindableBase
{
    public const int MaxRecentSources = 10;

    public double MainViewWidth
    {
        get;
        set => Set(ref field, value);
    } = 900;

    public double MainViewHeight
    {
        get;
        set => Set(ref field, value);
    } = 640;

    /// <summary>
    /// Последние использованные папки-источники. Пополняется только по кнопке «Копировать». Максимум <see cref="MaxRecentSources"/>.
    /// </summary>
    public ObservableCollection<string> RecentSources
    {
        get;
        set => SetObservableCollection(ref field, value);
    } = [];

    /// <summary>
    /// Последняя папка-цель.
    /// </summary>
    public string TargetPath
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Последняя использованная регулярка-фильтр.
    /// </summary>
    public string FileFilter
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;
}