using System;
using XForms.Primitives;

namespace XForms
{
    /// <summary>
    /// The bindable properties of <see cref="View"/>, such as
    /// <c>View.IsVisibleProperty</c>, for <see cref="ViewBindingExtensions"/>.
    /// </summary>
    public static class ViewProperties
    {
        private static readonly ViewProperty<View, bool> IsVisiblePropertyField = new(nameof(View.IsVisible), v => v.IsVisible, (v, x) => v.IsVisible = x);
        private static readonly ViewProperty<View, bool> IsHitTestVisiblePropertyField = new(nameof(View.IsHitTestVisible), v => v.IsHitTestVisible, (v, x) => v.IsHitTestVisible = x);
        private static readonly ViewProperty<View, float> WidthPropertyField = new(nameof(View.Width), v => v.Width, (v, x) => v.Width = x);
        private static readonly ViewProperty<View, float> HeightPropertyField = new(nameof(View.Height), v => v.Height, (v, x) => v.Height = x);
        private static readonly ViewProperty<View, Thickness> MarginPropertyField = new(nameof(View.Margin), v => v.Margin, (v, x) => v.Margin = x);
        private static readonly ViewProperty<View, string> AccessibilityLabelPropertyField = new(nameof(View.AccessibilityLabel), v => v.AccessibilityLabel, (v, x) => v.AccessibilityLabel = x);

        extension(View)
        {
            public static ViewProperty<View, bool> IsVisibleProperty => IsVisiblePropertyField;
            public static ViewProperty<View, bool> IsHitTestVisibleProperty => IsHitTestVisiblePropertyField;
            public static ViewProperty<View, float> WidthProperty => WidthPropertyField;
            public static ViewProperty<View, float> HeightProperty => HeightPropertyField;
            public static ViewProperty<View, Thickness> MarginProperty => MarginPropertyField;
            public static ViewProperty<View, string> AccessibilityLabelProperty => AccessibilityLabelPropertyField;
        }
    }

    /// <summary>
    /// The bindable properties of <see cref="Image"/>, such as
    /// <c>Image.SourceProperty</c>, for <see cref="ViewBindingExtensions"/>.
    /// </summary>
    public static class ImageProperties
    {
        private static readonly ViewProperty<Image, ImageSource> SourcePropertyField = new(nameof(Image.Source), v => v.Source, (v, x) => v.Source = x);
        private static readonly ViewProperty<Image, Color> TintPropertyField = new(nameof(Image.Tint), v => v.Tint, (v, x) => v.Tint = x);

        extension(Image)
        {
            public static ViewProperty<Image, ImageSource> SourceProperty => SourcePropertyField;
            public static ViewProperty<Image, Color> TintProperty => TintPropertyField;
        }
    }

    /// <summary>
    /// The bindable properties of <see cref="TextBlock"/>, such as
    /// <c>TextBlock.TextProperty</c>, for <see cref="ViewBindingExtensions"/>.
    /// </summary>
    public static class TextBlockProperties
    {
        private static readonly ViewProperty<TextBlock, string> TextPropertyField = new(nameof(TextBlock.Text), v => v.Text, (v, x) => v.Text = x);
        private static readonly ViewProperty<TextBlock, Color> ForegroundPropertyField = new(nameof(TextBlock.Foreground), v => v.Foreground, (v, x) => v.Foreground = x);
        private static readonly ViewProperty<TextBlock, float> FontSizePropertyField = new(nameof(TextBlock.FontSize), v => v.FontSize, (v, x) => v.FontSize = x);
        private static readonly ViewProperty<TextBlock, FontWeight> FontWeightPropertyField = new(nameof(TextBlock.FontWeight), v => v.FontWeight, (v, x) => v.FontWeight = x);
        private static readonly ViewProperty<TextBlock, bool> StrikethroughPropertyField = new(nameof(TextBlock.Strikethrough), v => v.Strikethrough, (v, x) => v.Strikethrough = x);

        extension(TextBlock)
        {
            public static ViewProperty<TextBlock, string> TextProperty => TextPropertyField;
            public static ViewProperty<TextBlock, Color> ForegroundProperty => ForegroundPropertyField;
            public static ViewProperty<TextBlock, float> FontSizeProperty => FontSizePropertyField;
            public static ViewProperty<TextBlock, FontWeight> FontWeightProperty => FontWeightPropertyField;
            public static ViewProperty<TextBlock, bool> StrikethroughProperty => StrikethroughPropertyField;
        }
    }

    /// <summary>
    /// The bindable properties of <see cref="TextBox"/>, such as
    /// <c>TextBox.TextProperty</c>, for <see cref="ViewBindingExtensions"/>.
    /// </summary>
    public static class TextBoxProperties
    {
        private static readonly ViewProperty<TextBox, string> TextPropertyField = new(nameof(TextBox.Text), v => v.Text, (v, x) => v.Text = x);
        private static readonly ViewProperty<TextBox, string> PlaceholderTextPropertyField = new(nameof(TextBox.PlaceholderText), v => v.PlaceholderText, (v, x) => v.PlaceholderText = x);

        extension(TextBox)
        {
            public static ViewProperty<TextBox, string> TextProperty => TextPropertyField;
            public static ViewProperty<TextBox, string> PlaceholderTextProperty => PlaceholderTextPropertyField;
        }
    }
}
