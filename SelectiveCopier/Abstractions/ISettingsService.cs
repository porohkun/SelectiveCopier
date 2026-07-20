namespace SelectiveCopier.Abstractions;

using Models;

public interface ISettingsService
{
    AppSettings Settings { get; }
    void Save();
}