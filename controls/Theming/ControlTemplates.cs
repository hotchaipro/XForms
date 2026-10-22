using System;
using System.Collections.Immutable;

namespace XForms.Controls
{
    /// <summary>
    /// A theme's templates, by control type: the theme extension that a theme
    /// package (such as XForms.Themes.Simple) fills, and controls read through
    /// <see cref="ThemeTemplates"/>.
    /// </summary>
    public sealed record ControlTemplates
    {
        public static ControlTemplates Empty { get; } = new ControlTemplates();

        private ImmutableDictionary<Type, ControlTemplate> Templates { get; init; } = ImmutableDictionary<Type, ControlTemplate>.Empty;

        /// <summary>
        /// The template for the control type, or for its nearest base type
        /// that has one (a subclass of a control looks like it); or null.
        /// </summary>
        public ControlTemplate Get(
            Type controlType)
        {
            for (var type = controlType; null != type; type = type.BaseType)
            {
                if (this.Templates.TryGetValue(type, out var template))
                {
                    return template;
                }
            }

            return null;
        }

        /// <summary>
        /// The templates, with this one for its control type (replacing any).
        /// </summary>
        public ControlTemplates With(
            ControlTemplate template)
        {
            ArgumentNullException.ThrowIfNull(template);
            return this with { Templates = this.Templates.SetItem(template.ControlType, template) };
        }

        public bool Equals(
            ControlTemplates other)
        {
            return (null != other) && (this.Templates == other.Templates);
        }

        public override int GetHashCode()
        {
            return this.Templates.GetHashCode();
        }
    }

    /// <summary>
    /// The controls' templates in a theme: <c>theme.GetTemplate(type)</c> and
    /// <c>theme.WithTemplate(template)</c>, as for a view and its descendants:
    /// <c>view.Theme = theme.WithTemplate(new MyButtonTemplate())</c>.
    /// </summary>
    public static class ThemeTemplates
    {
        extension(Theme theme)
        {
            /// <summary>
            /// The theme's template for the control type (or its nearest base
            /// type), or null if the theme has none.
            /// </summary>
            public ControlTemplate GetTemplate(
                Type controlType)
            {
                return theme.GetExtension<ControlTemplates>()?.Get(controlType);
            }

            /// <summary>
            /// The theme, with the template for its control type.
            /// </summary>
            public Theme WithTemplate(
                ControlTemplate template)
            {
                var templates = theme.GetExtension<ControlTemplates>() ?? ControlTemplates.Empty;
                return theme.WithExtension(templates.With(template));
            }
        }
    }
}
