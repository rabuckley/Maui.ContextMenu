using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using UIKit;

namespace The49.Maui.ContextMenu;

/// <summary>
/// iOS-specific implementation of ContextMenu functionality
/// </summary>
public partial class ContextMenu
{
    /// <summary>
    /// Shared delegate for all context menu interactions
    /// </summary>
    static IUIContextMenuInteractionDelegate _delegate = new ContextMenuInteractionDelegate();
    
    /// <summary>
    /// Property to track iOS context menu interaction
    /// </summary>
    public static readonly BindableProperty InteractionProperty = BindableProperty.CreateAttached("Interaction", typeof(IUIInteraction), typeof(VisualElement), null);
    
    /// <summary>
    /// Property to track tap gesture recognizer for click commands
    /// </summary>
    public static readonly BindableProperty TapGestureRecognizerProperty = BindableProperty.CreateAttached("TapGestureRecognizer", typeof(UITapGestureRecognizer), typeof(VisualElement), null);

    /// <summary>
    /// iOS implementation of menu setup
    /// </summary>
    static partial void SetupMenu(BindableObject bindable)
    {
        if (bindable is VisualElement ve)
        {
            // Special handling for UIButtons using native menu system
            if (ve.Handler.PlatformView is UIButton button)
            {
                AttachControlMenu(button, ve);
            }
        }
        if (bindable is CollectionView collectionView)
        {
            // Collection view support handled in CollectionViewDelegator
        }
        else if (bindable is VisualElement visualElement)
        {
            // Only attach interaction for long-press mode
            if (!GetShowMenuOnClick(visualElement))
            {
                AttachInteraction(visualElement);
            }
        }
    }

    /// <summary>
    /// iOS implementation of menu disposal
    /// </summary>
    static partial void DisposeMenu(BindableObject bindable)
    {
        if (bindable is VisualElement visualElement)
        {
            DetachInteraction(visualElement);
        }
    }

    /// <summary>
    /// iOS implementation of click command setup
    /// </summary>
    static partial void SetupClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            // Collection view support handled in CollectionViewDelegator
        }
        else if (bindable is VisualElement visualElement)
        {
            // Skip if already setup
            if (bindable.GetValue(TapGestureRecognizerProperty) != null)
            {
                return;
            }
            
            // Add tap gesture recognizer to handle clicks
            var pview = (UIView)visualElement.Handler.PlatformView;
            var gestureRecognizer = new UITapGestureRecognizer(() => ExecuteClickCommand(visualElement, visualElement));
            pview.AddGestureRecognizer(gestureRecognizer);
            visualElement.SetValue(TapGestureRecognizerProperty, gestureRecognizer);
        }
    }

    /// <summary>
    /// iOS implementation of click command disposal
    /// </summary>
    static partial void DisposeClickCommand(BindableObject bindable)
    {
        if (bindable is VisualElement visualElement)
        {
            var pview = (UIView)visualElement.Handler.PlatformView;
            var gestureRecognizer = (UITapGestureRecognizer)visualElement.GetValue(TapGestureRecognizerProperty);
            if (gestureRecognizer != null)
            {
                pview.RemoveGestureRecognizer(gestureRecognizer);
                visualElement.SetValue(TapGestureRecognizerProperty, null);
            }
        }
    }
    
    /// <summary>
    /// Attaches a context menu interaction to an iOS view
    /// </summary>
    static void AttachInteraction(VisualElement visualElement)
    {
        DetachInteraction(visualElement);
        visualElement.Dispatcher.Dispatch(() =>
        {
            var pview = (UIView)visualElement.Handler.PlatformView;
            var interaction = new UIContextMenuInteraction(_delegate);
            pview.UserInteractionEnabled = true;
            pview.AddInteraction(interaction);
            visualElement.SetValue(InteractionProperty, interaction);
        });
    }
    
    /// <summary>
    /// Removes a context menu interaction from an iOS view
    /// </summary>
    static void DetachInteraction(VisualElement visualElement)
    {
        var interaction = (IUIInteraction)visualElement.GetValue(InteractionProperty);
        if (interaction != null)
        {
            var pview = (UIView)visualElement.Handler.PlatformView;
            pview.RemoveInteraction(interaction);
            visualElement.SetValue(InteractionProperty, null);
        }
    }
    
    /// <summary>
    /// Attaches a context menu to a UIButton using native menu system
    /// </summary>
    static void AttachControlMenu(UIButton button, VisualElement visualElement)
    {
        var menuTemplate = GetMenu(visualElement);
        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            BindableObject.SetInheritedBindingContext(menu, visualElement.BindingContext);
            button.Menu = CreateMenu(menu);
            button.ShowsMenuAsPrimaryAction = GetShowMenuOnClick(visualElement);
        }
    }
    
    /// <summary>
    /// Creates a UIMenu from a Menu model
    /// </summary>
    public static UIMenu CreateMenu(Menu menu)
    {
        UIMenuElement[] children = new UIMenuElement[menu.Children.Count];
        var i = 0;
        foreach (var item in menu.Children)
        {
            children[i++] = CreateMenuItem(item);
        }
        if (!string.IsNullOrEmpty(menu.Title))
        {
            return UIMenu.Create(menu.Title, children);
        }
        return UIMenu.Create(children);
    }
    
    /// <summary>
    /// Creates a UIMenuElement from a MenuElement
    /// </summary>
    static UIMenuElement CreateMenuItem(MenuElement item)
    {
        if (item is Action action)
        {
            return CreateAction(action);
        }
        else if (item is MenuGroup group)
        {
            return CreateGroup(group);
        }
        else if (item is Menu menu)
        {
            return CreateSubMenu(menu);
        }
        return null;
    }

    /// <summary>
    /// Creates a UIAction from an Action model
    /// </summary>
    static UIMenuElement CreateAction(Action action)
    {
        var a = UIAction.Create(action.Title, GetActionImage(action), null, delegate
        {
            action.Command?.Execute(action.CommandParameter);
        });

        // Apply subtitle if specified
        if (!string.IsNullOrEmpty(action.SubTitle))
        {
            a.DiscoverabilityTitle = action.SubTitle;
        }

        // Apply appropriate attributes based on state
        if (!action.IsVisible)
        {
            a.Attributes = UIMenuElementAttributes.Hidden;
        }
        else if (action.IsDestructive)
        {
            a.Attributes = UIMenuElementAttributes.Destructive;
        }
        else if (!action.IsEnabled)
        {
            a.Attributes = UIMenuElementAttributes.Disabled;
        }

        return a;
    }
    
    /// <summary>
    /// Creates a UIMenu for group display from a MenuGroup
    /// </summary>
    static UIMenuElement CreateGroup(MenuGroup menuGroup)
    {
        UIMenuElement[] children = new UIMenuElement[menuGroup.Children.Count];
        var i = 0;
        foreach (var item in menuGroup.Children)
        {
            children[i++] = CreateMenuItem(item);
        }

        // DisplayInline makes the group appear without nesting
        if (!string.IsNullOrEmpty(menuGroup.Title))
        {
            return UIMenu.Create(menuGroup.Title, null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, children);
        }

        return UIMenu.Create("", null, UIMenuIdentifier.None, UIMenuOptions.DisplayInline, children);
    }
    
    /// <summary>
    /// Creates a UIMenu from a Menu for use as a submenu
    /// </summary>
    static UIMenuElement CreateSubMenu(Menu menu)
    {
        UIMenuElement[] children = new UIMenuElement[menu.Children.Count];
        var i = 0;
        foreach (var item in menu.Children)
        {
            children[i++] = CreateMenuItem(item);
        }
        if (string.IsNullOrEmpty(menu.Title))
        {
            throw new InvalidOperationException("Cannot create a submenu without a title");
        }
        var a = UIMenu.Create(menu.Title, null, UIMenuIdentifier.None, default(UIMenuOptions), children);

        return a;
    }
    
    /// <summary>
    /// Gets the appropriate UIImage for an action
    /// </summary>
    static UIImage? GetActionImage(Action action)
    {
        // First try system icon (SF Symbols)
        if (action.SystemIcon != null)
        {
            return UIImage.GetSystemImage(action.SystemIcon);
        }
        
        // Then try custom icon
        if (action.Icon == null)
        {
            return null;
        }
        if (action.Icon is IFileImageSource fileImageSource)
        {
            var filename = fileImageSource.File;
            return File.Exists(filename)
                        ? UIImage.FromFile(filename)
                        : UIImage.FromBundle(filename);
        }
        return null;
    }
    
    /// <summary>
    /// Creates a UITargetedPreview for displaying the preview
    /// </summary>
    public static UITargetedPreview CreateTargetedPreview(UIView target, UIContextMenuConfiguration configuration, object bindingContext, VisualElement visualElement)
    {
        var preview = ContextMenu.GetPreview(visualElement);

        if (preview == null)
        {
            return null;
        }

        // Configure preview appearance
        var parameters = new UIPreviewParameters();
        if (preview.VisiblePath != null)
        {
            // Apply path-based preview shape
            var bounds = target.Bounds.ToRectangle();
            bounds = new Rect(
                bounds.X + preview.Padding.Left,
                bounds.Y + preview.Padding.Top,
                bounds.Width - preview.Padding.HorizontalThickness,
                bounds.Height - preview.Padding.VerticalThickness
            );
            parameters.VisiblePath = preview.VisiblePath.PathForBounds(bounds).AsUIBezierPath();
        }
        parameters.BackgroundColor = (preview.BackgroundColor ?? Colors.Transparent).ToPlatform();

        if (preview.PreviewTemplate == null)
        {
            // Use default preview (the target itself)
            return new UITargetedPreview(target, parameters);
        }
        else
        {
            // Create custom preview from template
            var inst = (VisualElement)preview.PreviewTemplate.CreateContent();
            BindableObject.SetInheritedBindingContext(inst, bindingContext);

            // Size preview to match target
            inst.Arrange(target.Bounds.ToRectangle());
            inst.HeightRequest = target.Bounds.Height;
            inst.WidthRequest = target.Bounds.Width;

            return new UITargetedPreview(inst.ToPlatform(visualElement.Handler.MauiContext), parameters, new UIPreviewTarget(target, new CGPoint(target.Bounds.GetMidX(), target.Bounds.GetMidY())));
        }
    }
}

/// <summary>
/// Delegate for handling iOS context menu interactions
/// </summary>
public class ContextMenuInteractionDelegate : UIKit.UIContextMenuInteractionDelegate
{
    /// <summary>
    /// Creates the menu configuration when user activates context menu
    /// </summary>
    public override UIContextMenuConfiguration GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
    {
        if (interaction.View is MauiView view)
        {
            var menuTemplate = ContextMenu.GetMenu((VisualElement)view.View);
            var content = menuTemplate.CreateContent();

            if (content is Menu menu)
            {
                BindableObject.SetInheritedBindingContext(menu, ((VisualElement)view.View).BindingContext);
                return UIContextMenuConfiguration.Create(null, null, action =>
                {
                    return ContextMenu.CreateMenu(menu);
                });
            }
        }

        return null;
    }
    
    /// <summary>
    /// Creates the preview for highlighting during menu display
    /// </summary>
    public override UITargetedPreview GetHighlightPreview(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration, INSCopying identifier)
    {
        return GetPreviewForHighlightingMenu(interaction, configuration);
    }

    /// <summary>
    /// Creates the preview for the context menu
    /// </summary>
    public override UITargetedPreview GetPreviewForHighlightingMenu(UIContextMenuInteraction interaction, UIContextMenuConfiguration configuration)
    {
        if (interaction.View is MauiView view)
        {
            return ContextMenu.CreateTargetedPreview(
                interaction.View, 
                configuration, 
                ((VisualElement)view.View).BindingContext, 
                (VisualElement)view.View
            );
        }
        return null;
    }
}
