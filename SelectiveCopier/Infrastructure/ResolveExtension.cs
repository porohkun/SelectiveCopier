namespace SelectiveCopier.Infrastructure;

using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;

public class ResolveExtension : MarkupExtension
{
    public Type Type { get; set; } = null!;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || Application.Current is not App app)
            return null!;

        return app.Services.GetRequiredService(Type);
    }
}