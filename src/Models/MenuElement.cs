namespace The49.Maui.ContextMenu;

public abstract partial class MenuElement : Element
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(MenuElement),
        defaultBindingMode: BindingMode.OneWay);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}