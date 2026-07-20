namespace SelectiveCopier;

using System.ComponentModel;
using System.Windows;
using Abstractions;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Views;

public partial class App : Application
{
    public App()
    {
        Services = ConfigureServices();

        InitializeComponent();
    }

    public new static App? Current => LicenseManager.UsageMode != LicenseUsageMode.Designtime && Application.Current is App app
        ? app
        : null;

    public IServiceProvider Services { get; }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(services);
        services.AddServices(TypesByAttributes.Get<AsSingletonAttribute>());

        // Чтобы AppSettings можно было резолвить прямо из XAML через ResolveExtension.
        services.AddSingleton(provider => provider.GetRequiredService<ISettingsService>().Settings);

        return services.BuildServiceProvider();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var settings = Services.GetRequiredService<ISettingsService>().Settings;
        Current!.Resources["Settings"] = settings;
        var mainView = Services.GetRequiredService<MainView>();
        mainView?.Show();
    }
}