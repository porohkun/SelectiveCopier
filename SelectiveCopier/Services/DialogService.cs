namespace SelectiveCopier.Services;

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Abstractions;
using Infrastructure;
using Microsoft.Win32;

[AsSingleton(typeof(IDialogService))]
internal sealed class DialogService : IDialogService
{
    public string? BrowseFolder(string? initialPath = null)
    {
        var dialog = new OpenFolderDialog
        {
            Multiselect = false
        };

        if (!string.IsNullOrWhiteSpace(initialPath))
            dialog.InitialDirectory = initialPath;

        return dialog.ShowDialog() == true
            ? dialog.FolderName
            : null;
    }

    public void ShowInformation(string message, string caption = "SelectiveCopier") =>
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string message, string caption = "SelectiveCopier") =>
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);

    public void OpenInExplorer(string path)
    {
        if (!Directory.Exists(path))
            return;

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
        }
        catch (Exception e) when (e is Win32Exception or InvalidOperationException)
        {
            // Проводник недоступен — не повод ронять копирование, оно уже прошло успешно.
        }
    }

    public void SetClipboardText(string text)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (ExternalException)
        {
            // Буфер занят другим процессом — глотаем, отчёт пользователь всё равно увидит.
        }
    }
}