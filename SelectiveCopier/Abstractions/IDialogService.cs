namespace SelectiveCopier.Abstractions;

/// <summary>
/// Диалоги приложения.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Показывает диалог выбора папки.
    /// </summary>
    /// <returns>Выбранный путь либо <see langword="null"/>, если отменено.</returns>
    string? BrowseFolder(string? initialPath = null);

    void ShowInformation(string message, string caption = "SelectiveCopier");

    void ShowError(string message, string caption = "SelectiveCopier");

    /// <summary>
    /// Открывает папку в проводнике.
    /// </summary>
    void OpenInExplorer(string path);

    /// <summary>
    /// Копирует текст в буфер обмена.
    /// </summary>
    void SetClipboardText(string text);
}