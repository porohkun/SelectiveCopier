namespace SelectiveCopier.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Атрибут для класса, требующий его регистрации в di контейнере как синглтон
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AsSingletonAttribute : ContainerRegisterAttribute
{
    /// <inheritdoc/>
    public AsSingletonAttribute()
    {
    }

    /// <inheritdoc/>
    public AsSingletonAttribute(params Type[] abstractions)
        : base(abstractions)
    {
    }

    /// <inheritdoc/>
    public override ServiceLifetime Lifetime => ServiceLifetime.Singleton;
}