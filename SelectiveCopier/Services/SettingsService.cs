namespace SelectiveCopier.Services;

using System.ComponentModel;
using System.IO;
using System.Text.Json;
using Abstractions;
using Infrastructure;
using Models;

/// <summary>
/// Заглушка. У тебя есть своя реализация — этот файл можно просто удалить.
/// </summary>
[AsSingleton(typeof(ISettingsService))]
internal sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path = Path.Combine(Consts.AppDataPath, "settings.json");

    public SettingsService()
    {
        Settings = Load();
        Settings.PropertyChanged += OnSettingsChanged;
    }

    public AppSettings Settings { get; }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(Settings, Options));
        }
        catch (IOException)
        {
        }
    }

    private AppSettings Load()
    {
        try
        {
            return File.Exists(_path)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path)) ?? new()
                : new();
        }
        catch (Exception e) when (e is IOException or JsonException)
        {
            return new();
        }
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e) => Save();
}