namespace SelectiveCopier.Behaviors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

/// <summary>
/// Обновление источника для <see cref="ComboBox.Text"/> редактируемого <see cref="ComboBox"/>.
/// </summary>
/// <remarks>
/// <see cref="ComboBox.Text"/> биндится штатно с <c>UpdateSourceTrigger=Explicit</c>, а бихейвер
/// решает, когда протолкнуть значение: по потере фокуса, по Enter и при выборе из списка.
/// Посимвольное обновление не годится — чтение папки слишком дорогое.
/// </remarks>
public static class EditableComboBoxBehavior
{
    public static readonly DependencyProperty UpdateSourceOnSelectionProperty =
        DependencyProperty.RegisterAttached(
            "UpdateSourceOnSelection",
            typeof(bool),
            typeof(EditableComboBoxBehavior),
            new(false, UpdateSourceOnSelectionChanged));

    public static bool GetUpdateSourceOnSelection(DependencyObject obj) =>
        (bool)obj.GetValue(UpdateSourceOnSelectionProperty);

    public static void SetUpdateSourceOnSelection(DependencyObject obj, bool value) =>
        obj.SetValue(UpdateSourceOnSelectionProperty, value);

    private static void UpdateSourceOnSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not ComboBox comboBox)
            return;

        comboBox.LostFocus -= OnLostFocus;
        comboBox.SelectionChanged -= OnSelectionChanged;
        comboBox.KeyDown -= OnKeyDown;

        if (e.NewValue is not true)
            return;

        comboBox.LostFocus += OnLostFocus;
        comboBox.SelectionChanged += OnSelectionChanged;
        comboBox.KeyDown += OnKeyDown;
    }

    private static void OnLostFocus(object sender, RoutedEventArgs e) => Push((ComboBox)sender);

    private static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Push((ComboBox)sender);
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (ComboBox)sender;

        // Событие всплывает и от внутреннего TextBox шаблона — реагируем только на выбор в списке.
        if (!ReferenceEquals(e.OriginalSource, comboBox) || e.AddedItems.Count == 0)
            return;

        // ComboBox.Text обновляется асинхронно относительно SelectionChanged, поэтому
        // проталкиваем источник после того, как WPF допишет текст.
        comboBox.Dispatcher.BeginInvoke(() => Push(comboBox), DispatcherPriority.Input);
    }

    private static void Push(ComboBox comboBox) =>
        comboBox.GetBindingExpression(ComboBox.TextProperty)?.UpdateSource();
}