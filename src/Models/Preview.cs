namespace The49.Maui.ContextMenu;

public partial class Preview : Element
{
    public static readonly BindableProperty PreviewTemplateProperty = BindableProperty.Create(
        nameof(PreviewTemplate),
        typeof(DataTemplate),
        typeof(Preview),
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty VisiblePathProperty = BindableProperty.Create(
        nameof(VisiblePath),
        typeof(IShape),
        typeof(Preview),
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
        nameof(BackgroundColor),
        typeof(Color),
        typeof(Preview),
        defaultBindingMode: BindingMode.OneWay);

    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(
        nameof(Padding),
        typeof(Thickness),
        typeof(Preview),
        defaultBindingMode: BindingMode.OneWay);

    public DataTemplate PreviewTemplate
    {
        get => (DataTemplate)GetValue(PreviewTemplateProperty);
        set => SetValue(PreviewTemplateProperty, value);
    }

    public IShape VisiblePath
    {
        get => (IShape)GetValue(VisiblePathProperty);
        set => SetValue(VisiblePathProperty, value);
    }

    public Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }
}