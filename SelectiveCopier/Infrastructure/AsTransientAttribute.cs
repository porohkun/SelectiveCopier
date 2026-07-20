namespace SelectiveCopier.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Атрибут для класса, требующий его регистрации в di контейнере как транзиент
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AsTransientAttribute : ContainerRegisterAttribute
{
    /// <inheritdoc/>
    public AsTransientAttribute()
    {
    }

    /// <inheritdoc/>
    public AsTransientAttribute(params Type[] abstractions)
        : base(abstractions)
    {
    }

    /// <inheritdoc/>
    public override ServiceLifetime Lifetime => ServiceLifetime.Transient;
}