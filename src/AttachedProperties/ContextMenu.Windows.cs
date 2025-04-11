using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace The49.Maui.ContextMenu;

/// <summary>
/// Windows-specific implementation of ContextMenu functionality
/// </summary>
public static partial class ContextMenu
{
    /// <summary>
    /// Property to track Windows flyout for context menu
    /// </summary>
    public static readonly BindableProperty FlyoutProperty = BindableProperty.CreateAttached("Flyout", typeof(MenuFlyout), typeof(VisualElement), null);

    /// <summary>
    /// Property to track Right Tapped event handler
    /// </summary>
    public static readonly BindableProperty RightTappedHandlerProperty = BindableProperty.CreateAttached("RightTappedHandler", typeof(RightTappedEventHandler), typeof(VisualElement), null);

    /// <summary>
    /// Property to track Click event handler
    /// </summary>
    public static readonly BindableProperty ClickHandlerProperty = BindableProperty.CreateAttached("ClickHandler", typeof(PointerEventHandler), typeof(VisualElement), null);

    /// <summary>
    /// Windows implementation of menu setup
    /// </summary>
    static partial void SetupMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            // Currently not implemented for CollectionView
            // Would need to handle each item in the collection
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachMenuToView(visualElement, visualElement);
        }
    }

    /// <summary>
    /// Windows implementation of menu disposal
    /// </summary>
    static partial void DisposeMenu(BindableObject bindable)
    {
        if (bindable is CollectionView)
        {
            // Not yet implemented
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachMenuFromView(visualElement);
        }
    }

    /// <summary>
    /// Windows implementation of click command setup
    /// </summary>
    static partial void SetupClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView)
        {
            // Not yet implemented
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachClickToView(visualElement, visualElement);
        }
    }

    /// <summary>
    /// Windows implementation of click command disposal
    /// </summary>
    static partial void DisposeClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView)
        {
            // Not yet implemented
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachClickFromView(visualElement);
        }
    }

    /// <summary>
    /// Attaches a context menu to a Windows view
    /// </summary>
    private static void AttachMenuToView(VisualElement visualElement, BindableObject propertySource)
    {
        var showOnClick = GetShowMenuOnClick(propertySource);
        var platformView = visualElement.Handler.PlatformView as FrameworkElement;

        if (platformView == null)
            return;

        // Create the MenuFlyout
        var menuFlyout = CreateMenuFlyout(propertySource, visualElement);
        visualElement.SetValue(FlyoutProperty, menuFlyout);

        if (showOnClick)
        {
            // Attach click handler
            var clickHandler = new PointerEventHandler((sender, args) =>
            {
                if (args.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse &&
                    args.GetCurrentPoint(platformView).Properties.IsLeftButtonPressed)
                {
                    ShowFlyout(sender as FrameworkElement, visualElement, menuFlyout);
                    args.Handled = true;
                }
            });

            platformView.PointerPressed += clickHandler;
            visualElement.SetValue(ClickHandlerProperty, clickHandler);
        }
        else
        {
            // Attach right click handler
            var rightTappedHandler = new RightTappedEventHandler((sender, args) =>
            {
                ShowFlyout(sender as FrameworkElement, visualElement, menuFlyout);
                args.Handled = true;
            });

            platformView.RightTapped += rightTappedHandler;
            visualElement.SetValue(RightTappedHandlerProperty, rightTappedHandler);
        }
    }

    /// <summary>
    /// Shows the context menu flyout at the appropriate position
    /// </summary>
    private static void ShowFlyout(FrameworkElement element, VisualElement visualElement, MenuFlyout flyout)
    {
        // Update menu items to ensure they have the latest binding context
        UpdateMenuItems(flyout, visualElement);

        // Show flyout
        flyout.ShowAt(element, new Point(0, 0));
    }

    /// <summary>
    /// Updates menu items with the current binding context
    /// </summary>
    private static void UpdateMenuItems(MenuFlyout flyout, VisualElement contextOwner)
    {
        // Clear existing items
        flyout.Items.Clear();

        // Recreate menu items with current binding context
        var menuTemplate = GetMenu(contextOwner);
        if (menuTemplate == null)
            return;

        var content = menuTemplate.CreateContent();
        if (content is Menu menu)
        {
            BindableObject.SetInheritedBindingContext(menu, contextOwner.BindingContext);

            // Add all children
            foreach (var item in menu.Children)
            {
                AddMenuElement(item, flyout.Items);
            }
        }
    }

    /// <summary>
    /// Removes a context menu from a Windows view
    /// </summary>
    private static void DetachMenuFromView(VisualElement visualElement)
    {
        var platformView = visualElement.Handler.PlatformView as FrameworkElement;
        if (platformView == null)
            return;

        // Remove right tapped handler if exists
        var rightTappedHandler = visualElement.GetValue(RightTappedHandlerProperty) as RightTappedEventHandler;
        if (rightTappedHandler != null)
        {
            platformView.RightTapped -= rightTappedHandler;
            visualElement.SetValue(RightTappedHandlerProperty, null);
        }

        // Remove click handler if exists
        var clickHandler = visualElement.GetValue(ClickHandlerProperty) as PointerEventHandler;
        if (clickHandler != null)
        {
            platformView.PointerPressed -= clickHandler;
            visualElement.SetValue(ClickHandlerProperty, null);
        }

        // Clear flyout
        visualElement.SetValue(FlyoutProperty, null);
    }

    /// <summary>
    /// Creates a MenuFlyout for a context menu
    /// </summary>
    private static MenuFlyout CreateMenuFlyout(BindableObject propertySource, VisualElement contextOwner)
    {
        var flyout = new MenuFlyout();

        // We'll populate the flyout when it's shown to ensure up-to-date binding context
        return flyout;
    }

    /// <summary>
    /// Adds a menu element to a flyout item collection
    /// </summary>
    private static void AddMenuElement(MenuElement element, IList<MenuFlyoutItemBase> items)
    {
        if (element is Action action)
        {
            AddAction(action, items);
        }
        else if (element is Menu submenu)
        {
            AddSubmenu(submenu, items);
        }
        else if (element is MenuGroup group)
        {
            AddGroup(group, items);
        }
    }

    /// <summary>
    /// Adds an action to a flyout item collection
    /// </summary>
    private static void AddAction(Action action, IList<MenuFlyoutItemBase> items)
    {
        if (!action.IsVisible)
            return;

        var item = new MenuFlyoutItem
        {
            Text = action.Title,
            IsEnabled = action.IsEnabled
        };

        // Set icon if available
        if (action.Icon != null && action.Icon is FileImageSource fileImageSource)
        {
            // For simplicity, we'll use a font icon for demonstration
            // In a real implementation, you'd load the image from the source
            item.Icon = new SymbolIcon(Symbol.Document);
        }

        // Apply destructive styling if needed
        if (action.IsDestructive)
        {
            item.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 244, 67, 54)); // Red color
        }

        // Set command
        item.Click += (sender, e) =>
        {
            action.Command?.Execute(action.CommandParameter);
        };

        items.Add(item);
    }

    /// <summary>
    /// Adds a submenu to a flyout item collection
    /// </summary>
    private static void AddSubmenu(Menu menu, IList<MenuFlyoutItemBase> items)
    {
        if (string.IsNullOrEmpty(menu.Title))
        {
            // Can't create a submenu without a title in WinUI
            foreach (var child in menu.Children)
            {
                AddMenuElement(child, items);
            }
            return;
        }

        var submenuItem = new MenuFlyoutSubItem
        {
            Text = menu.Title
        };

        foreach (var child in menu.Children)
        {
            AddMenuElement(child, submenuItem.Items);
        }

        items.Add(submenuItem);
    }

    /// <summary>
    /// Adds a group to a flyout item collection (using separator)
    /// </summary>
    private static void AddGroup(MenuGroup group, IList<MenuFlyoutItemBase> items)
    {
        // Add a separator if there are already items and this group has a title
        if (items.Count > 0 && !string.IsNullOrEmpty(group.Title))
        {
            items.Add(new MenuFlyoutSeparator());
        }

        // Add a header if the group has a title
        if (!string.IsNullOrEmpty(group.Title))
        {
            items.Add(new MenuFlyoutSeparator());
            items.Add(new MenuFlyoutItem { Text = group.Title, IsEnabled = false });
        }

        foreach (var child in group.Children)
        {
            AddMenuElement(child, items);
        }
    }

    /// <summary>
    /// Attaches a click handler to a Windows view
    /// </summary>
    private static void AttachClickToView(VisualElement visualElement, BindableObject propertySource)
    {
        var platformView = visualElement.Handler.PlatformView as FrameworkElement;
        if (platformView == null)
            return;

        var clickHandler = new PointerEventHandler((sender, args) =>
        {
            if (args.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse &&
                args.GetCurrentPoint(platformView).Properties.IsLeftButtonPressed)
            {
                ExecuteClickCommand(propertySource, visualElement);
                args.Handled = true;
            }
        });

        platformView.PointerPressed += clickHandler;
        visualElement.SetValue(ClickHandlerProperty, clickHandler);
    }

    /// <summary>
    /// Removes a click handler from a Windows view
    /// </summary>
    private static void DetachClickFromView(VisualElement visualElement)
    {
        var platformView = visualElement.Handler.PlatformView as FrameworkElement;
        if (platformView == null)
            return;

        var clickHandler = visualElement.GetValue(ClickHandlerProperty) as PointerEventHandler;
        if (clickHandler != null)
        {
            platformView.PointerPressed -= clickHandler;
            visualElement.SetValue(ClickHandlerProperty, null);
        }
    }
}