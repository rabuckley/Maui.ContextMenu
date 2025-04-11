using Android.Views;
using static Android.Views.View;
using AView = Android.Views.View;
using Microsoft.Maui.Platform;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Util;
using Java.Lang;
using Android.Content.Res;

namespace The49.Maui.ContextMenu;

/// <summary>
/// Android-specific implementation of ContextMenu functionality
/// </summary>
public static partial class ContextMenu
{
    /// <summary>
    /// Scale factor for visual feedback during long press
    /// </summary>
    public const float LongPressScaleFactor = .95f;
    
    /// <summary>
    /// Delay before applying shrink effect during long press (ms)
    /// </summary>
    public const int LongPressShrinkDelay = 100;

    /// <summary>
    /// Resource name for preview window background color
    /// </summary>
    public const string PreviewWindowBackgroundColorResource = "AndroidContextMenuPreviewWindowBackgroundColor";
    
    /// <summary>
    /// Resource name for preview window background opacity
    /// </summary>
    public const string PreviewWindowBackgroundOpacityResource = "AndroidContextMenuPreviewWindowBackgroundOpacity";
    
    /// <summary>
    /// Resource name for context menu background color
    /// </summary>
    public const string ContextMenuBackgroundColorResource = "AndroidContextMenuContextMenuBackgroundColor";
    
    /// <summary>
    /// Resource name for context menu corner radius
    /// </summary>
    public const string ContextMenuCornerRadiusResource = "AndroidContextMenuContextMenuCornerRadius";

    /// <summary>
    /// Property to track child elements in collection views
    /// </summary>
    public static readonly BindableProperty ChildElementsProperty = BindableProperty.CreateAttached("ChildElements", typeof(List<VisualElement>), typeof(VisualElement), new List<VisualElement>());
    
    /// <summary>
    /// Registers a child element with its parent for context menu tracking
    /// </summary>
    public static void RegisterChildElement(BindableObject bindable, VisualElement element)
    {
        var views = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        views.Add(element);
        if (bindable is CollectionView collectionView)
        {
            if (collectionView.GetValue(MenuProperty) is not null)
            {
                AttachMenuToView(element, collectionView);
            }
            if (collectionView.GetValue(ClickCommandProperty) is not null)
            {
                AttachClickToView(element, collectionView);
            }
        }
    }
    
    /// <summary>
    /// Unregisters a child element from its parent and removes context menu
    /// </summary>
    public static void UnregisterChildElement(BindableObject bindable, VisualElement element)
    {
        var views = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        views.Remove(element);
        DetachMenuFromView(element);
        DetachClickFromView(element);
    }

    /// <summary>
    /// Applies an action to each child element of a bindable object
    /// </summary>
    private static void ForEachElement(BindableObject bindable, Action<VisualElement> action)
    {
        var elements = (List<VisualElement>)bindable.GetValue(ChildElementsProperty);
        foreach (var element in elements)
        {
            action(element);
        }
    }
    
    /// <summary>
    /// Android implementation of menu setup
    /// </summary>
    static partial void SetupMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            // For CollectionView, track child elements and attach menus to each cell
            collectionView.ChildAdded += ChildAddedToCollectionView;
            collectionView.ChildRemoved += ChildRemovedFromCollectionView;
            ForEachElement(collectionView, element => AttachMenuToView(element, collectionView));
        }
        else if (bindable is VisualElement visualElement)
        {
            // For regular elements, attach menu directly
            AttachMenuToView(visualElement, visualElement);
        }
    }

    private static void ChildRemovedFromCollectionView(object sender, ElementEventArgs e)
    {
        UnregisterChildElement((BindableObject)sender, (VisualElement)e.Element);
    }

    private static void ChildAddedToCollectionView(object sender, ElementEventArgs e)
    {
        RegisterChildElement((BindableObject)sender, (VisualElement)e.Element);
    }

    /// <summary>
    /// Attaches context menu functionality to an Android view
    /// </summary>
    public static void AttachMenuToView(VisualElement visualElement, BindableObject propertySource)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        var showOnClick = GetShowMenuOnClick(propertySource);
        if (showOnClick)
        {
            // Setup for click triggering
            aview.Clickable = true;
            aview.SetOnClickListener(new MenuActionListener(propertySource, visualElement, aview));
        }
        else
        {
            // Setup for long press triggering with visual feedback
            aview.LongClickable = true;
            var listener = new MenuActionListener(propertySource, visualElement, aview);
            aview.SetOnTouchListener(listener);
            aview.SetOnLongClickListener(listener);
        }
    }
    
    /// <summary>
    /// Removes context menu functionality from an Android view
    /// </summary>
    public static void DetachMenuFromView(VisualElement visualElement)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.LongClickable = false;
        aview.SetOnLongClickListener(null);
    }

    /// <summary>
    /// Attaches click command functionality to an Android view
    /// </summary>
    public static void AttachClickToView(VisualElement visualElement, BindableObject propertySource)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.Clickable = true;
        aview.SetOnClickListener(new OnClickListener(propertySource, visualElement));
    }
    
    /// <summary>
    /// Removes click command functionality from an Android view
    /// </summary>
    public static void DetachClickFromView(VisualElement visualElement)
    {
        var aview = (AView)visualElement.Handler.PlatformView;
        aview.Clickable = false;
        aview.SetOnClickListener(null);
    }

    /// <summary>
    /// Android implementation of menu disposal
    /// </summary>
    static partial void DisposeMenu(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            collectionView.ChildAdded -= ChildAddedToCollectionView;
            collectionView.ChildRemoved -= ChildRemovedFromCollectionView;
            ForEachElement(collectionView, DetachMenuFromView);
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachMenuFromView(visualElement);
        }
    }

    /// <summary>
    /// Android implementation of click command setup
    /// </summary>
    static partial void SetupClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, element => AttachClickToView(element, collectionView));
        }
        else if (bindable is VisualElement visualElement)
        {
            AttachClickToView(visualElement, visualElement);
        }
    }

    /// <summary>
    /// Android implementation of click command disposal
    /// </summary>
    static partial void DisposeClickCommand(BindableObject bindable)
    {
        if (bindable is CollectionView collectionView)
        {
            ForEachElement(collectionView, DetachClickFromView);
        }
        else if (bindable is VisualElement visualElement)
        {
            DetachClickFromView(visualElement);
        }
    }

    /// <summary>
    /// Adds a menu item to the root level of an Android menu
    /// </summary>
    public static (int, int) AddRootMenuItem(MenuElement item, IMenu amenu, int rootGroupId, int rootId)
    {
        if (item is Action action)
        {
            AddAction(action, amenu, rootGroupId, rootId++);
        }
        else if (item is MenuGroup group)
        {
            rootGroupId++;
            rootId = AddGroup(group, amenu, rootGroupId, rootId++);
            rootGroupId++;
        }
        else if (item is Menu menu)
        {
            AddSubmenu(menu, amenu, rootGroupId++, rootId++);
        }
        return (rootGroupId, rootId);
    }

    /// <summary>
    /// Converts device-independent pixels to physical pixels
    /// </summary>
    private static float DpToPixel(float dp)
    {
        return dp * ((float)Platform.CurrentActivity.Resources.DisplayMetrics.DensityDpi / (float)DisplayMetricsDensity.Default);
    }

    /// <summary>
    /// Scales a bitmap to the specified dimensions
    /// </summary>
    private static Bitmap ScaleBitmap(Bitmap targetBmp, int reqHeightInPixels, int reqWidthInPixels)
    {
        Matrix matrix = new Matrix();
        matrix.SetRectToRect(new Android.Graphics.RectF(0, 0, targetBmp.Width, targetBmp.Height), new Android.Graphics.RectF(0, 0, reqWidthInPixels, reqHeightInPixels), Matrix.ScaleToFit.Center);
        Bitmap scaledBitmap = Bitmap.CreateBitmap(targetBmp, 0, 0, targetBmp.Width, targetBmp.Height, matrix, true);
        return scaledBitmap;
    }
    
    /// <summary>
    /// Sets the icon for a menu action
    /// </summary>
    public static void SetActionIcon(IMenuItem item, Action action)
    {
        if (action.Icon != null)
        {
            var id = Platform.CurrentActivity.GetDrawableId(((IFileImageSource)action.Icon).File);
            if (id == 0)
            {
                return;
            }
            var drawable = Platform.CurrentActivity.GetDrawable(id);
            var size = (int)DpToPixel(32f);
            var bitmap = ScaleBitmap(((BitmapDrawable)drawable).Bitmap, size, size);
            var r = new BitmapDrawable(Platform.CurrentActivity.Resources, bitmap);
            item.SetIcon(r);
        }
    }
    
    /// <summary>
    /// Applies destructive styling to a menu item if needed
    /// </summary>
    public static void SetIsDestructive(IMenuItem item, Action action)
    {
        if (action.IsDestructive && action.IsEnabled)
        {
            item.SetIconTintList(new ColorStateList(new int[][]
            {
                new int[] {  }
            },
            new int[]
            {
                Microsoft.Maui.Graphics.Color.FromArgb("#F44336").ToPlatform()
            }));
        }
    }
    
    /// <summary>
    /// Adds an action to an Android menu
    /// </summary>
    public static void AddAction(Action action, IMenu amenu, int groupId, int itemId)
    {
        var item = amenu.Add(groupId, itemId, itemId, action.Title);
        item.SetEnabled(action.IsEnabled);
        item.SetVisible(action.IsVisible);
        SetActionIcon(item, action);
        SetIsDestructive(item, action);
        item.SetOnMenuItemClickListener(new MenuItemClickListener(action));
    }
    
    /// <summary>
    /// Adds a group of items to an Android menu
    /// </summary>
    public static int AddGroup(MenuGroup menuGroup, IMenu amenu, int groupId, int itemId)
    {
        foreach (var item in menuGroup.Children)
        {
            AddGroupItem(item, amenu, groupId, itemId++);
        }
        return itemId;
    }
    
    /// <summary>
    /// Adds a submenu to an Android menu
    /// </summary>
    public static void AddSubmenu(Menu menu, IMenu amenu, int groupId, int itemId)
    {
        var submenu = amenu.AddSubMenu(groupId, itemId, itemId, menu.Title);
        var rootGroupId = 0;
        var rootItemId = 0;
        foreach (var item in menu.Children)
        {
            var ids = AddRootMenuItem(item, submenu, rootGroupId, rootItemId);
            rootGroupId = ids.Item1;
            rootItemId = ids.Item2;
        }
    }

    /// <summary>
    /// Adds a menu element to a group in an Android menu
    /// </summary>
    public static void AddGroupItem(MenuElement item, IMenu amenu, int groupId, int itemId)
    {
        if (item is Action action)
        {
            AddAction(action, amenu, groupId, itemId);
        }
        else if (item is MenuGroup group)
        {
            throw new InvalidOperationException("Nested ContextMenu groups is not supported");
        }
        else if (item is Menu menu)
        {
            AddGroupMenu(menu, amenu, groupId, itemId);
        }
    }
    
    /// <summary>
    /// Adds a submenu to a group in an Android menu
    /// </summary>
    public static void AddGroupMenu(Menu menu, IMenu amenu, int groupId, int itemId)
    {
        var submenu = amenu.AddSubMenu(groupId, itemId, itemId, menu.Title);
        var rootGroupId = 0;
        var rootItemId = 0;
        foreach (var item in menu.Children)
        {
            var ids = AddRootMenuItem(item, submenu, rootGroupId, rootItemId);
            rootGroupId = ids.Item1;
            rootItemId = ids.Item2;
        }
    }
}

/// <summary>
/// Provides visual feedback during long press by shrinking the view
/// </summary>
internal class ShrinkContextMenuTarget : Java.Lang.Object, IRunnable
{
    private AView _target;

    public ShrinkContextMenuTarget(AView target)
    {
        _target = target;
    }
    
    /// <summary>
    /// Animates the view to slightly shrink, providing visual feedback
    /// </summary>
    public void Run()
    {
        _target.Animate()
            .ScaleX(ContextMenu.LongPressScaleFactor)
            .ScaleY(ContextMenu.LongPressScaleFactor)
            .SetDuration(ViewConfiguration.LongPressTimeout - ContextMenu.LongPressShrinkDelay)
            .Start();
    }
}

/// <summary>
/// Listener for menu item clicks that executes the associated command
/// </summary>
public class MenuItemClickListener : Java.Lang.Object, IMenuItemOnMenuItemClickListener
{
    private readonly Action _action;

    public MenuItemClickListener(Action action)
    {
        _action = action;
    }

    /// <summary>
    /// Handles menu item click by executing the action's command
    /// </summary>
    public bool OnMenuItemClick(IMenuItem item)
    {
        _action.Command?.Execute(_action.CommandParameter);
        return true;
    }
}

/// <summary>
/// Listener for element clicks that executes the associated command
/// </summary>
public class OnClickListener : Java.Lang.Object, IOnClickListener
{
    private BindableObject _propertyOwner;
    private BindableObject _contextOwner;

    public OnClickListener(BindableObject propertyOnwer, BindableObject contextOwner) : base()
    {
        _propertyOwner = propertyOnwer;
        _contextOwner = contextOwner;
    }
    
    /// <summary>
    /// Handles element click by executing the command
    /// </summary>
    public void OnClick(AView v)
    {
        ContextMenu.ExecuteClickCommand(_propertyOwner, _contextOwner.BindingContext);
    }
}

/// <summary>
/// Listener that manages context menu actions including long press and click
/// </summary>
public class MenuActionListener : Java.Lang.Object, IOnLongClickListener, IOnClickListener, IOnTouchListener
{
    private BindableObject _propertyOwner;
    private BindableObject _contextOwner;
    private ShrinkContextMenuTarget _shrink;

    public MenuActionListener(BindableObject propertyOnwer, BindableObject contextOwner, AView target) : base()
    {
        _propertyOwner = propertyOnwer;
        _contextOwner = contextOwner;
        _shrink = new ShrinkContextMenuTarget(target);
    }

    /// <summary>
    /// Shows menu on click when configured for click activation
    /// </summary>
    public void OnClick(AView v)
    {
        ShowMenu(v);
    }

    /// <summary>
    /// Shows menu with preview on long press
    /// </summary>
    public bool OnLongClick(AView v)
    {
        // Cancel any ongoing animations and reset scale
        v.Animate().Cancel();
        v.ScaleX = 1f;
        v.ScaleY = 1f;
        ShowMenuWithPreview(v);
        return true;
    }

    /// <summary>
    /// Manages touch feedback for long press
    /// </summary>
    public bool OnTouch(AView v, MotionEvent e)
    {
        if (!v.LongClickable)
        {
            return false;
        }
        switch (e.Action)
        {
            case MotionEventActions.Down:
                // Start shrink animation after delay
                v.Handler.PostDelayed(_shrink, 100);
                break;
            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                // Cancel shrink animation if touch released
                v.Handler.RemoveCallbacks(_shrink);
                break;
        }
        return false;
    }

    /// <summary>
    /// Populates an Android menu with items from the context menu
    /// </summary>
    private void PopulateMenu(IMenu amenu)
    {
        var menuTemplate = ContextMenu.GetMenu(_propertyOwner);
        var content = menuTemplate.CreateContent();

        if (content is Menu menu)
        {
            // Ensure menu items have access to the binding context
            BindableObject.SetInheritedBindingContext(menu, _contextOwner.BindingContext);

            var rootGroupId = 0;
            var rootId = 0;

            // Add each menu item
            foreach (var item in menu.Children)
            {
                var ids = ContextMenu.AddRootMenuItem(item, amenu, rootGroupId, rootId);
                rootGroupId = ids.Item1;
                rootId = ids.Item2;
            }
#if ANDROID28_0_OR_GREATER
            amenu.SetGroupDividerEnabled(true);
#endif
        }
    }

    /// <summary>
    /// Shows the context menu without preview
    /// </summary>
    public void ShowMenu(AView aview)
    {
        var menu = new ContextMenuPopup(aview, true);
        PopulateMenu(menu.Menu);
        menu.Show(0, 0);
    }

    /// <summary>
    /// Shows the context menu with preview window
    /// </summary>
    public void ShowMenuWithPreview(AView aview)
    {
        ContextMenuWindow w = null;
        int offsetX = 0;
        int offsetY = 0;
        if (_propertyOwner is VisualElement visualElement)
        {
            var preview = ContextMenu.GetPreview(visualElement);
            w = new ContextMenuWindow(Platform.CurrentActivity, aview, preview);

            if (preview != null)
            {
                // Adjust for preview padding to align menu properly
                offsetX = (int)preview.Padding.Left;
                offsetY = -(int)preview.Padding.Bottom;
            }
        }

        PopulateMenu(w.Menu);
        w.Show(ViewUtils.DpToPx(offsetX), ViewUtils.DpToPx(offsetY));
    }
}
