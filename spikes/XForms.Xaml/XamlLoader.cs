using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;
using XForms.Controls;
using XForms.Layouts;

namespace XForms.Xaml
{
    /// <summary>
    /// The result of loading XAML: the root view, named elements and
    /// storyboards.
    /// </summary>
    public sealed class XamlView
    {
        internal XamlView(View root, Dictionary<string, View> names, Dictionary<string, Storyboard> storyboards)
        {
            this.Root = root;
            this.Names = names;
            this.Storyboards = storyboards;
        }

        public View Root { get; }

        public IReadOnlyDictionary<string, View> Names { get; }

        public IReadOnlyDictionary<string, Storyboard> Storyboards { get; }
    }

    /// <summary>
    /// A template: XAML that builds a templated control's look.
    /// </summary>
    public sealed class ControlTemplate
    {
        private readonly XamlLoader _loader;
        private readonly XElement _content;

        internal ControlTemplate(XamlLoader loader, XElement content)
        {
            this._loader = loader;
            this._content = content;
        }

        public View Build(Button templatedParent)
        {
            return this._loader.CreateView(this._content, templatedParent, isTemplateRoot: true);
        }
    }

    /// <summary>
    /// Property values applied to elements of a type, before their own.
    /// </summary>
    public sealed class Style
    {
        public string TargetType { get; set; }

        public List<KeyValuePair<string, string>> Setters { get; } = new List<KeyValuePair<string, string>>();
    }

    /// <summary>
    /// Animations started together.
    /// </summary>
    /// <remarks>
    /// GAP: No opacity (fade) animation, and no frame callback.
    /// WORKAROUND: opacity animates on the UI thread, stepped by a timer.
    /// Translation uses the Animator's TranslateTo, on the compositor. The
    /// loader gives each target an Animator when it creates it.
    /// </remarks>
    public sealed class Storyboard
    {
        internal List<(string Target, string Property, float From, float To, TimeSpan Duration)> Animations { get; } = new();

        internal Dictionary<string, View> Names { get; set; }

        public void Begin()
        {
            foreach (var animation in this.Animations)
            {
                if (!this.Names.TryGetValue(animation.Target, out var view))
                {
                    continue;
                }

                switch (animation.Property)
                {
                    case "Opacity":
                        StepOpacity(view, animation.From, animation.To, animation.Duration);
                        break;

                    case "TranslateY":
                        view.Animator.Translation = new Point(view.Animator.Translation.X, animation.From);
                        _ = view.Animator.TranslateTo(new Point(view.Animator.Translation.X, animation.To), animation.Duration, new CubicEase(EasingMode.EaseOut));
                        break;

                    case "TranslateX":
                        view.Animator.Translation = new Point(animation.From, view.Animator.Translation.Y);
                        _ = view.Animator.TranslateTo(new Point(animation.To, view.Animator.Translation.Y), animation.Duration, new CubicEase(EasingMode.EaseOut));
                        break;
                }
            }
        }

        private static void StepOpacity(View view, float from, float to, TimeSpan duration)
        {
            var application = Application.Current;
            var start = DateTime.UtcNow;
            view.Animator.Opacity = from;

            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((state) =>
            {
                double t = Math.Min(1, (DateTime.UtcNow - start).TotalMilliseconds / duration.TotalMilliseconds);
                application.Dispatcher.Post(() => view.Animator.Opacity = (float)(from + ((to - from) * t)));
                if (t >= 1)
                {
                    timer.Dispose();
                }
            }, null, 0, 16);
        }
    }

    /// <summary>
    /// Builds XForms views from a small XAML subset, at run time.
    /// </summary>
    /// <remarks>
    /// Supported: Grid, StackPanel, Border, TextBlock, TextBox, Button (with a
    /// ControlTemplate and ContentPresenter), Style (by key or implicit, by
    /// target type), resources, {Binding}, {StaticResource}, {TemplateBinding},
    /// and Storyboards of opacity and translation. Bindings use reflection,
    /// the spike's own choice; the framework doesn't need it.
    /// </remarks>
    public sealed class XamlLoader
    {
        private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

        private readonly Dictionary<string, object> _resources = new Dictionary<string, object>();
        private readonly Dictionary<string, View> _names = new Dictionary<string, View>();
        private readonly Dictionary<string, Storyboard> _storyboards = new Dictionary<string, Storyboard>();
        private readonly HashSet<string> _animatedNames = new HashSet<string>();
        private readonly object _dataContext;

        private XamlLoader(object dataContext)
        {
            this._dataContext = dataContext;
        }

        public static XamlView Load(string xaml, object dataContext)
        {
            var loader = new XamlLoader(dataContext);
            var root = XElement.Parse(xaml);

            View content = null;
            foreach (var child in root.Elements())
            {
                if (child.Name.LocalName.EndsWith(".Resources", StringComparison.Ordinal))
                {
                    loader.LoadResources(child);
                }
                else
                {
                    content = loader.CreateView(child, null);
                }
            }

            foreach (var storyboard in loader._storyboards.Values)
            {
                storyboard.Names = loader._names;
            }

            return new XamlView(content, loader._names, loader._storyboards);
        }

        private void LoadResources(XElement resources)
        {
            foreach (var element in resources.Elements())
            {
                string key = (string)element.Attribute(X + "Key");
                switch (element.Name.LocalName)
                {
                    case "Style":
                        {
                            var style = new Style() { TargetType = (string)element.Attribute("TargetType") };
                            foreach (var setter in element.Elements())
                            {
                                style.Setters.Add(new KeyValuePair<string, string>((string)setter.Attribute("Property"), (string)setter.Attribute("Value")));
                            }

                            // A style without a key applies to every element of its type
                            this._resources[key ?? ("implicit:" + style.TargetType)] = style;
                            break;
                        }

                    case "ControlTemplate":
                        this._resources[key] = new ControlTemplate(this, ElementOnly(element));
                        break;

                    case "Storyboard":
                        {
                            var storyboard = new Storyboard();
                            foreach (var animation in element.Elements())
                            {
                                storyboard.Animations.Add((
                                    this.NoteAnimated((string)animation.Attribute("TargetName")),
                                    (string)animation.Attribute("Property"),
                                    ParseFloat((string)animation.Attribute("From")),
                                    ParseFloat((string)animation.Attribute("To")),
                                    TimeSpan.Parse((string)animation.Attribute("Duration"), CultureInfo.InvariantCulture)));
                            }

                            this._storyboards[key] = storyboard;
                            break;
                        }

                    case "Color":
                        this._resources[key] = Color.FromString(element.Value.Trim());
                        break;
                }
            }
        }

        private static XElement ElementOnly(XElement parent)
        {
            foreach (var child in parent.Elements())
            {
                return child;
            }

            return null;
        }

        private string NoteAnimated(string name)
        {
            this._animatedNames.Add(name);
            return name;
        }

        internal View CreateView(XElement element, Button templatedParent, bool isTemplateRoot = false)
        {
            object target;
            string type = element.Name.LocalName;

            // A view that animates gets its Animator when it's created: a
            // storyboard's target, a view with an Opacity, or a template's root
            // (which the templated button dims while pressed)
            string elementName = (string)element.Attribute(X + "Name");
            bool animates = isTemplateRoot
                || (null != element.Attribute("Opacity"))
                || ((null != elementName) && this._animatedNames.Contains(elementName));
            var animator = animates ? new Animator() : null;

            switch (type)
            {
                case "Grid":
                    {
                        var grid = new Grid() { Animator = animator };
                        string rows = (string)element.Attribute("RowDefinitions");
                        string columns = (string)element.Attribute("ColumnDefinitions");
                        if (null != rows) grid.RowDefinitions.AddRange(rows);
                        if (null != columns) grid.ColumnDefinitions.AddRange(columns);
                        target = grid;
                        break;
                    }

                case "StackPanel":
                    target = new StackPanel() { Animator = animator };
                    break;

                case "Border":
                    target = new Border() { Animator = animator };
                    break;

                case "TextBlock":
                    target = new TextBlock() { Animator = animator, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };
                    break;

                case "TextBox":
                    target = new TextBox() { Animator = animator, HorizontalAlignment = HorizontalAlignment.Stretch };
                    break;

                case "Button":
                    target = new Button() { Animator = animator };
                    break;

                case "ContentPresenter":
                    target = new ContentPresenter() { Animator = animator };
                    break;

                default:
                    throw new NotSupportedException("Unsupported element: " + type);
            }

            var view = (View)target;

            // Styles first (implicit by type, or by key), then the element's own values
            if (this._resources.TryGetValue("implicit:" + type, out var implicitStyle))
            {
                this.ApplyStyle((Style)implicitStyle, target, templatedParent);
            }

            string styleValue = (string)element.Attribute("Style");
            if ((null != styleValue) && (this.Resolve(styleValue, templatedParent) is Style keyedStyle))
            {
                this.ApplyStyle(keyedStyle, target, templatedParent);
            }

            foreach (var attribute in element.Attributes())
            {
                if (attribute.Name == X + "Name")
                {
                    this._names[attribute.Value] = view;
                    continue;
                }

                string name = attribute.Name.LocalName;
                if ((name == "Style") || (name == "RowDefinitions") || (name == "ColumnDefinitions") || attribute.IsNamespaceDeclaration)
                {
                    continue;
                }

                this.SetValue(target, name, attribute.Value, templatedParent);
            }

            // Children
            foreach (var child in element.Elements())
            {
                if (child.Name.LocalName.Contains('.'))
                {
                    continue;
                }

                var childView = this.CreateView(child, templatedParent);
                switch (target)
                {
                    case Grid grid:
                        // Grid.Row and Grid.Column become the child's typed cell
                        grid.Children.Add(
                            childView,
                            (int)ToFloat((string)child.Attribute("Grid.Row") ?? "0"),
                            (int)ToFloat((string)child.Attribute("Grid.Column") ?? "0"),
                            (int)ToFloat((string)child.Attribute("Grid.RowSpan") ?? "1"),
                            (int)ToFloat((string)child.Attribute("Grid.ColumnSpan") ?? "1"));
                        break;

                    case StackPanel panel:
                        panel.Children.Add(childView);
                        break;

                    case Border border:
                        border.Child = childView;
                        break;

                    case Button button:
                        button.ButtonContent = childView;
                        break;
                }
            }

            // Text content, such as <Button>Greet</Button>
            if ((target is Button textButton) && (null == textButton.ButtonContent) && !element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
            {
                textButton.ButtonContent = element.Value.Trim();
            }

            // A content presenter shows its templated parent's content
            if ((target is ContentPresenter presenter) && (null != templatedParent))
            {
                presenter.Presented = templatedParent.ButtonContent as View
                    ?? new TextBlock()
                    {
                        Text = templatedParent.ButtonContent?.ToString(),
                        Foreground = templatedParent.Foreground,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
            }

            if (target is Button templated)
            {
                templated.ApplyTemplate();
            }

            return view;
        }

        private void ApplyStyle(Style style, object target, Button templatedParent)
        {
            foreach (var setter in style.Setters)
            {
                this.SetValue(target, setter.Key, setter.Value, templatedParent);
            }
        }

        private object Resolve(string text, Button templatedParent)
        {
            text = text.Trim();
            if (text.StartsWith("{StaticResource", StringComparison.Ordinal))
            {
                string key = text.Substring("{StaticResource".Length).TrimEnd('}').Trim();
                return this._resources.TryGetValue(key, out var value) ? value : null;
            }

            if (text.StartsWith("{TemplateBinding", StringComparison.Ordinal) && (null != templatedParent))
            {
                string property = text.Substring("{TemplateBinding".Length).TrimEnd('}').Trim();
                switch (property)
                {
                    case "Background": return templatedParent.Background;
                    case "Foreground": return templatedParent.Foreground;
                }
            }

            return text;
        }

        private void SetValue(object target, string name, string text, Button templatedParent)
        {
            // Bindings to the data context
            if (text.StartsWith("{Binding", StringComparison.Ordinal))
            {
                this.Bind(target, name, text);
                return;
            }

            object value = this.Resolve(text, templatedParent);
            Assign(target, name, value);
        }

        private static void Assign(object target, string name, object value)
        {
            var view = (View)target;

            switch (name)
            {
                case "Margin": view.Margin = ToThickness(value); return;
                case "Width": view.Width = ToFloat(value); return;
                case "Height": view.Height = ToFloat(value); return;
                case "HorizontalAlignment": view.HorizontalAlignment = Enum.Parse<HorizontalAlignment>(value.ToString()); return;
                case "VerticalAlignment": view.VerticalAlignment = Enum.Parse<VerticalAlignment>(value.ToString()); return;
                case "Opacity": view.Animator.Opacity = ToFloat(value); return;
                case "Grid.Row":
                case "Grid.Column":
                case "Grid.RowSpan":
                case "Grid.ColumnSpan":
                    // Applied when the child is added to its grid
                    return;
            }

            switch (target)
            {
                case StackPanel panel when name == "Orientation":
                    panel.Orientation = Enum.Parse<Orientation>(value.ToString());
                    return;

                case StackPanel panel when name == "Spacing":
                    panel.Spacing = ToFloat(value);
                    return;

                case Border border:
                    switch (name)
                    {
                        case "Background": border.Background = ToColor(value); return;
                        case "BorderBrush": border.BorderBrush = ToColor(value); return;
                        case "BorderThickness": border.BorderThickness = ToThickness(value); return;
                        case "CornerRadius": border.CornerRadius = ToFloat(value); return;
                        case "Padding": border.Padding = ToThickness(value); return;
                    }
                    break;

                case TextBlock textView:
                    switch (name)
                    {
                        case "Text": textView.Text = value?.ToString(); return;
                        case "FontSize": textView.FontSize = ToFloat(value); return;
                        case "FontWeight": textView.FontWeight = Enum.Parse<FontWeight>(value.ToString(), true); return;
                        case "Foreground": textView.Foreground = ToColor(value); return;
                    }
                    break;

                case TextBox textEntry:
                    switch (name)
                    {
                        case "Text": textEntry.Text = value?.ToString(); return;
                        case "PlaceholderText": textEntry.PlaceholderText = value?.ToString(); return;
                    }
                    break;

                case Button button:
                    switch (name)
                    {
                        case "Content": button.ButtonContent = value; return;
                        case "Template": button.Template = value as ControlTemplate; return;
                        case "Background": button.Background = ToColor(value); return;
                        case "Foreground": button.Foreground = ToColor(value); return;
                        case "Command": button.Command = value as ICommand; return;
                    }
                    break;
            }

            throw new NotSupportedException($"Unsupported property: {target.GetType().Name}.{name}");
        }

        /// <summary>
        /// {Binding Path} or {Binding Path=Name, Mode=TwoWay}, by reflection on the data context.
        /// </summary>
        /// <remarks>
        /// TwoWay bindings listen to the view's PropertyChanged (phase 3 made
        /// views observable, and TextBox reports typing).
        /// </remarks>
        private void Bind(object target, string name, string text)
        {
            string inner = text.Substring("{Binding".Length).TrimEnd('}').Trim();
            string path = inner;
            bool isTwoWay = false;
            foreach (var part in inner.Split(','))
            {
                var pair = part.Split('=');
                if (pair.Length == 2 && pair[0].Trim() == "Path") path = pair[1].Trim();
                else if (pair.Length == 2 && pair[0].Trim() == "Mode") isTwoWay = (pair[1].Trim() == "TwoWay");
                else if (pair.Length == 1) path = pair[0].Trim();
            }

            var source = this._dataContext;
            var property = source?.GetType().GetProperty(path, BindingFlags.Public | BindingFlags.Instance);
            if (null == property)
            {
                return;
            }

            void Update() => Assign(target, name, property.GetValue(source));
            Update();

            var targetProperty = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (isTwoWay && (target is INotifyPropertyChanged view) && (null != targetProperty) && property.CanWrite)
            {
                view.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == name)
                    {
                        var value = targetProperty.GetValue(target);
                        if (!Equals(value, property.GetValue(source)))
                        {
                            property.SetValue(source, value);
                        }
                    }
                };
            }

            if (source is INotifyPropertyChanged observable)
            {
                observable.PropertyChanged += (sender, e) =>
                {
                    if ((e.PropertyName == path) || string.IsNullOrEmpty(e.PropertyName))
                    {
                        Update();
                    }
                };
            }
        }

        private static float ParseFloat(string text) => float.Parse(text, CultureInfo.InvariantCulture);

        private static float ToFloat(object value) => value is float f ? f : ParseFloat(value.ToString());

        private static Color ToColor(object value) => value is Color color ? color : Color.FromString(value.ToString());

        private static Thickness ToThickness(object value)
        {
            if (value is Thickness thickness)
            {
                return thickness;
            }

            var parts = value.ToString().Split(',');
            if (parts.Length == 4)
            {
                return new Thickness(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3]));
            }

            if (parts.Length == 2)
            {
                return new Thickness(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[0]), ParseFloat(parts[1]));
            }

            return new Thickness(ParseFloat(parts[0]));
        }
    }
}
