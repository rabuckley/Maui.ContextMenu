namespace The49.Maui.ContextMenu;

[ContentProperty(nameof(Children))]
public partial class Menu : MenuElement
{
    public static readonly BindableProperty ChildrenProperty = BindableProperty.Create(
        nameof(Children),
        typeof(IList<MenuElement>),
        typeof(Menu),
        defaultValue: new List<MenuElement>(),
        defaultBindingMode: BindingMode.OneWay);

    public IList<MenuElement> Children
    {
        get => (IList<MenuElement>)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        foreach (var item in Children)
        {
            SetInheritedBindingContext(item, BindingContext);
        }
    }
}