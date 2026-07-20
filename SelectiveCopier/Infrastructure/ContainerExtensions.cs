namespace SelectiveCopier.Infrastructure;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Класс с методами расширения для регистрации в контейнере зависимостей
/// </summary>
internal static class ContainerExtensions
{
    /// <summary>
    /// Регистрирует типы, помеченные атрибутами AsSingletone, AsTransient, AsScoped
    /// </summary>
    /// <param name="services">Контейнер зависимостей, в который будут добавлены синглтоны.</param>
    /// <param name="types">Типы для регистрации</param>
    public static void AddServices(this IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            try
            {
                var attribute = type.GetCustomAttribute<ContainerRegisterAttribute>();

                if (attribute == null)
                    continue;

                if (attribute.Abstractions.Count() == 1)
                {
                    services.Add(new(attribute.Abstractions.First(), type, attribute.Lifetime));
                }
                else
                {
                    services.Add(new(type, type, attribute.Lifetime));
                    foreach (var abstraction in attribute.Abstractions)
                        services.Add(new(abstraction, type, attribute.Lifetime));
                }
            }
            catch (Exception ex)
            {
                throw new($"!!{type}:{ex.Message}", ex);
            }
        }
    }
}