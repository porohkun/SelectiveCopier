namespace SelectiveCopier.Infrastructure;

using System.Reflection;

public static class Consts
{
    public static Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

    public static string AppDataPath =>
#if DEBUG
        AppPath;
#else
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SelectiveCopier");
#endif

    public static string AppPath => AppDomain.CurrentDomain.BaseDirectory;
}