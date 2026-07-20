namespace SelectiveCopier.Models;

/// <summary>
/// Результат восстановления выбора из папки-цели.
/// </summary>
/// <param name="Restored">Сколько файлов из цели удалось сопоставить с деревом.</param>
/// <param name="NotFound">Относительные пути файлов, которые есть в цели, но которых нет в дереве.</param>
public readonly record struct RestoreResult(int Restored, IReadOnlyList<string> NotFound)
{
    public int TotalInTarget => Restored + NotFound.Count;
}