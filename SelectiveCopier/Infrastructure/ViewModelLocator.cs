namespace SelectiveCopier.Infrastructure;

using System.ComponentModel;
using System.Windows;

/// <summary>
/// Класс, предоставляющий локатор моделей представления для использования с паттерном MVVM в WPF.
/// </summary>
internal static class ViewModelLocator
{
    /// <summary>
    /// Свойство зависимости для привязки типа модели представления к элементу управления.
    /// </summary>
    public static DependencyProperty ViewModelProperty =
        DependencyProperty.RegisterAttached("ViewModel", typeof(Type), typeof(ViewModelLocator), new(typeof(object), ViewModelChanged));

    /// <summary>
    /// Получает тип модели представления для указанного объекта.
    /// </summary>
    /// <param name="obj">Элемент управления.</param>
    /// <returns>Тип модели представления.</returns>
    public static Type GetViewModel(DependencyObject obj)
    {
        return (Type)obj.GetValue(ViewModelProperty);
    }

    /// <summary>
    /// Устанавливает тип модели представления для указанного объекта.
    /// </summary>
    /// <param name="obj">Элемент управления.</param>
    /// <param name="value">Тип модели представления.</param>
    public static void SetViewModel(DependencyObject obj, Type value)
    {
        obj.SetValue(ViewModelProperty, value);
    }

    private static void ViewModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(obj) || e.NewValue is not Type viewModelType)
            return;

        var viewModel = App.Current!.Services.GetService(viewModelType);
        Bind(obj, viewModel);
    }

    private static void Bind(object view, object? viewModel)
    {
        if (view is FrameworkElement frameworkElement)
        {
            frameworkElement.DataContext = viewModel;
        }
    }
}