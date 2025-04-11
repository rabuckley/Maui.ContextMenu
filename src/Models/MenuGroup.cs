namespace The49.Maui.ContextMenu;

[ContentProperty(nameof(Children))]
public partial class MenuGroup : MenuElement
{
    public static readonly BindableProperty ChildrenProperty = BindableProperty.Create(
        nameof(Children),
        typeof(IList<MenuElement>),
        typeof(MenuGroup),
        defaultValue: new List<MenuElement>(),
        defaultBindingMode: BindingMode.OneWay);

    public IList<MenuElement> Children
    {
        get => (IList<MenuElement>)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }
}