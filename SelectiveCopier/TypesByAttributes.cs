namespace SelectiveCopier;

/// <summary>
/// Кэш типов, собранных TypeHarvester'ом
/// Здесь только заглушка, чтобы студия не ругалась
/// </summary>
internal static partial class TypesByAttributes
{
    /// <summary>
    /// Возвращает собранные типы по атрибуту
    /// </summary>
    /// <typeparam name="TAttribute">Атрибут</typeparam>
    internal static partial IEnumerable<Type> Get<TAttribute>();

    /// <summary>
    /// Возвращает собранные типы по одному из атрибутов
    /// </summary>
    /// <typeparam name="TAttribute1">Атрибут 1</typeparam>
    /// <typeparam name="TAttribute2">Атрибут 2</typeparam>
    internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2>();

    /// <summary>
    /// Возвращает собранные типы по одному из атрибутов
    /// </summary>
    /// <typeparam name="TAttribute1">Атрибут 1</typeparam>
    /// <typeparam name="TAttribute2">Атрибут 2</typeparam>
    /// <typeparam name="TAttribute3">Атрибут 3</typeparam>
    internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2, TAttribute3>();

    /// <summary>
    /// Возвращает собранные типы по одному из атрибутов
    /// </summary>
    /// <param name="attributeTypes">Список атрибутов</param>
    internal static partial IEnumerable<Type> Get(params Type[] attributeTypes);
}