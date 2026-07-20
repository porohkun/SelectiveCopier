namespace SelectiveCopier.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Атрибут для класса, требующий его регистрации в di контейнере как скоупед
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AsScopedAttribute : ContainerRegisterAttribute
{
    /// <inheritdoc/>
    public AsScopedAttribute()
    {
    }

    /// <inheritdoc/>
    public AsScopedAttribute(params Type[] abstractions)
        : base(abstractions)
    {
    }

    /// <inheritdoc/>
    public override ServiceLifetime Lifetime => ServiceLifetime.Scoped;
}