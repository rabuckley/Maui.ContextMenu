using System.Windows.Input;

namespace The49.Maui.ContextMenu;

/// <summary>
/// Provides cross-platform context menu functionality as attached properties
/// </summary>
public static partial class ContextMenu
{
    /// <summary>
    /// Attached property for command executed when element is clicked
    /// </summary>
    public static readonly BindableProperty ClickCommandProperty = BindableProperty.CreateAttached(
        "ClickCommand",
        typeof(ICommand),
        typeof(VisualElement),
        null,
        propertyChanged: ClickCommandChanged);

    /// <summary>
    /// Attached property for parameter passed to ClickCommand
    /// </summary>
    public static readonly BindableProperty ClickCommandParameterProperty = BindableProperty.CreateAttached(
        "ClickCommandParameter",
        typeof(object),
        typeof(VisualElement),
        null);

    /// <summary>
    /// Attached property defining the context menu structure
    /// </summary>
    public static readonly BindableProperty MenuProperty = BindableProperty.CreateAttached(
        "Menu",
        typeof(DataTemplate),
        typeof(VisualElement),
        null,
        propertyChanged: MenuChanged);

    /// <summary>
    /// Attached property for customizing the preview shown with context menu
    /// </summary>
    public static readonly BindableProperty PreviewProperty = BindableProperty.CreateAttached(
        "Preview",
        typeof(Preview),
        typeof(VisualElement),
        null);

    /// <summary>
    /// Attached property to control whether menu shows on click instead of long press
    /// </summary>
    public static readonly BindableProperty ShowMenuOnClickProperty = BindableProperty.CreateAttached(
        "ShowMenuOnClick",
        typeof(bool),
        typeof(VisualElement),
        false,
        propertyChanged: ShowMenuOnClickChanged);

    /// <summary>
    /// Handles changes to the Menu property
    /// </summary>
    private static void MenuChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement visualElement)
        {
            return;
        }

        // Defer setup until handler is ready
        if (visualElement.Handler is null)
        {
            void UpdateMenu(object? s, EventArgs e)
            {
                MenuChanged(bindable, oldValue, newValue);
                visualElement.HandlerChanged -= UpdateMenu;
            }

            visualElement.HandlerChanged += UpdateMenu;
            return;
        }

        // Setup menu when first assigned
        if (oldValue is null && newValue is not null)
        {
            SetupMenu(visualElement);
        }

        // Cleanup when menu is removed
        if (oldValue is not null && newValue is null)
        {
            DisposeMenu(visualElement);
        }
    }

    /// <summary>
    /// Handles changes to the ClickCommand property
    /// </summary>
    private static void ClickCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement visualElement)
        {
            return;
        }

        // Defer setup until handler is ready
        if (visualElement.Handler == null)
        {
            void UpdateClickCommand(object? s, EventArgs e)
            {
                ClickCommandChanged(bindable, oldValue, newValue);
                visualElement.HandlerChanged -= UpdateClickCommand;
            }

            visualElement.HandlerChanged += UpdateClickCommand;
            return;
        }

        // Setup click command when first assigned
        if (oldValue is null && newValue is not null)
        {
            SetupClickCommand(visualElement);
        }

        // Cleanup when command is removed
        if (oldValue is not null && newValue is null)
        {
            DisposeClickCommand(visualElement);
        }
    }

    /// <summary>
    /// Handles changes to the ShowMenuOnClick property
    /// </summary>
    private static void ShowMenuOnClickChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var menu = GetMenu(bindable);

        if (menu is null)
        {
            return;
        }

        // Recreate menu with new trigger mode
        DisposeMenu(bindable);
        SetupMenu(bindable);
    }

    // Platform-specific implementations defined in separate files
    static partial void SetupMenu(BindableObject bindable);

    static partial void DisposeMenu(BindableObject bindable);

    static partial void SetupClickCommand(BindableObject bindable);

    static partial void DisposeClickCommand(BindableObject bindable);

    /// <summary>
    /// Gets the click command for a view
    /// </summary>
    public static ICommand GetClickCommand(BindableObject view)
    {
        return (ICommand)view.GetValue(ClickCommandProperty);
    }

    /// <summary>
    /// Sets the click command for a view
    /// </summary>
    public static void SetClickCommand(BindableObject view, ICommand value)
    {
        view.SetValue(ClickCommandProperty, value);
    }

    /// <summary>
    /// Gets the menu template for a view
    /// </summary>
    public static DataTemplate GetMenu(BindableObject view)
    {
        return (DataTemplate)view.GetValue(MenuProperty);
    }

    /// <summary>
    /// Sets the menu template for a view
    /// </summary>
    public static void SetMenu(BindableObject view, DataTemplate value)
    {
        view.SetValue(MenuProperty, value);
    }

    /// <summary>
    /// Gets the preview configuration for a view
    /// </summary>
    public static Preview GetPreview(BindableObject view)
    {
        return (Preview)view.GetValue(PreviewProperty);
    }

    /// <summary>
    /// Sets the preview configuration for a view
    /// </summary>
    public static void SetPreview(BindableObject view, Preview value)
    {
        view.SetValue(PreviewProperty, value);
    }

    /// <summary>
    /// Gets the click command parameter for a view
    /// </summary>
    public static object GetClickCommandParameter(BindableObject view)
    {
        return view.GetValue(ClickCommandParameterProperty);
    }

    /// <summary>
    /// Sets the click command parameter for a view
    /// </summary>
    public static void SetClickCommandParameter(BindableObject view, object value)
    {
        view.SetValue(ClickCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets whether menu shows on click for a view
    /// </summary>
    public static bool GetShowMenuOnClick(BindableObject view)
    {
        return (bool)view.GetValue(ShowMenuOnClickProperty);
    }

    /// <summary>
    /// Sets whether menu shows on click for a view
    /// </summary>
    public static void SetShowMenuOnClick(BindableObject view, bool value)
    {
        view.SetValue(ShowMenuOnClickProperty, value);
    }

    /// <summary>
    /// Executes the click command for a bindable object with fallback value
    /// </summary>
    public static void ExecuteClickCommand(BindableObject bindable, object defaultValue)
    {
        var command = GetClickCommand(bindable);
        var commandParameter = GetClickCommandParameter(bindable);

        command?.Execute(commandParameter ?? defaultValue);
    }
}