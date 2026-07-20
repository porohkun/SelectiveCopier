namespace SelectiveCopier.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Атрибут для класса, требующий его регистрации в di контейнере
/// </summary>
public abstract class ContainerRegisterAttribute : Attribute
{
    /// <summary>
    /// Текущая реализация будет привязана ко всем абстракциям, включая все базовые классы и интерфейсы
    /// </summary>
    public ContainerRegisterAttribute()
    {
        Abstractions = [];
    }

    /// <summary>
    /// Текущая реализация будет привязана к выбранным абстракциям
    /// </summary>
    /// <param name="abstractions">Набор абстракций, к которым нужно привязать текущую реализацию</param>
    public ContainerRegisterAttribute(params Type[] abstractions)
    {
        Abstractions = abstractions;
    }

    /// <summary> Абстракции </summary>
    public IEnumerable<Type> Abstractions { get; }

    /// <summary> Время жизни зарегистрированных сущностей </summary>
    public abstract ServiceLifetime Lifetime { get; }
}