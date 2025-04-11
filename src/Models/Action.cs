using System.Windows.Input;

namespace The49.Maui.ContextMenu;

/// <summary>
/// Represents a menu action that can be executed by the user
/// </summary>
public class Action : MenuElement
{
    /// <summary>
    /// Command to execute when the action is triggered
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(Action));

    /// <summary>
    /// Gets or sets the command to execute
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Parameter to pass to the command when executed
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(Action));

    /// <summary>
    /// Gets or sets the parameter for the command
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Custom icon for the action (platform-specific handling)
    /// </summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(Action));

    /// <summary>
    /// Gets or sets the icon image
    /// </summary>
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// System icon name (primarily for iOS SF Symbols)
    /// </summary>
    public static readonly BindableProperty SystemIconProperty = BindableProperty.Create(
        nameof(SystemIcon),
        typeof(string),
        typeof(Action));

    /// <summary>
    /// Gets or sets the system icon name
    /// </summary>
    public string SystemIcon
    {
        get => (string)GetValue(SystemIconProperty);
        set => SetValue(SystemIconProperty, value);
    }

    /// <summary>
    /// Whether the action is enabled
    /// </summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled),
        typeof(bool),
        typeof(Action),
        defaultValue: true);

    /// <summary>
    /// Gets or sets whether the action is enabled
    /// </summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// Whether the action is visible in the menu
    /// </summary>
    public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(
        nameof(IsVisible),
        typeof(bool),
        typeof(Action),
        defaultValue: true);

    /// <summary>
    /// Gets or sets whether the action is visible
    /// </summary>
    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Whether the action represents a destructive operation (shown in red)
    /// </summary>
    public static readonly BindableProperty IsDestructiveProperty = BindableProperty.Create(
        nameof(IsDestructive),
        typeof(bool),
        typeof(Action));

    /// <summary>
    /// Gets or sets whether the action is destructive
    /// </summary>
    public bool IsDestructive
    {
        get => (bool)GetValue(IsDestructiveProperty);
        set => SetValue(IsDestructiveProperty, value);
    }

    /// <summary>
    /// Subtitle shown below the main title (platform-specific support)
    /// </summary>
    public static readonly BindableProperty SubTitleProperty = BindableProperty.Create(
        nameof(SubTitle),
        typeof(string),
        typeof(Action));

    /// <summary>
    /// Gets or sets the subtitle text
    /// </summary>
    public string SubTitle
    {
        get => (string)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }
}