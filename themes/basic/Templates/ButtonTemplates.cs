using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    // The Basic look of the buttons: filled, flat, with a quick shrink as
    // they're pressed

    internal sealed class ButtonTemplate : ControlTemplate<Button>
    {
        private const float PressedScale = 0.9f;

        protected override void OnStateChanged(
            Button control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
            }
        }

        // The theme's button style, over its tokens
        private static ButtonStyle StyleFor(
            Button control)
        {
            var theme = control.ActualTheme;
            var colors = theme.Colors;
            var defaults = new ButtonStyle()
            {
                Background = control.IsAlertStyle ? colors.Error : AccentColors.Fill(theme),
                Foreground = control.IsAlertStyle ? colors.OnError : AccentColors.OnFill(theme),
                CornerRadius = theme.Shapes.SmallCornerRadius,
                TextStyle = theme.Typography.Label,
            };

            return (theme.GetExtension<ButtonStyle>() ?? new ButtonStyle()).Over(defaults);
        }

        protected override void Render(
            Button control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var style = StyleFor(control);
            var format = style.TextStyle.ToTextFormat();
            format.HorizontalAlignment = HorizontalAlignment.Center;
            format.VerticalAlignment = VerticalAlignment.Center;

            drawContext.Clear(Colors.Transparent);
            if (control.IsEnabled)
            {
                drawContext.FillRoundedRectangle(bounds, style.CornerRadius.Value, new SolidColorBrush(style.Background.Value));
                drawContext.DrawText(control.Text, bounds, style.Foreground.Value, format);
            }
            else
            {
                // A disabled button is its text alone, dimmed
                drawContext.DrawText(control.Text, bounds, control.ActualTheme.Colors.Disabled, format);
            }
        }
    }

    // A round icon (or letter) that grows when checked, with a label beneath:
    // ToggleButton, RadioButton and the days of the week
    internal abstract class RoundToggleTemplate<TControl> : ControlTemplate<TControl>
        where TControl : TemplatedControl
    {
        protected const float UncheckedScale = 0.8f;
        protected const float PressedScaleDelta = 0.05f;

        protected abstract bool IsChecked(TControl control);

        protected virtual float PressedScale(
            TControl control)
        {
            return UncheckedScale - PressedScaleDelta;
        }

        protected override void OnStateChanged(
            TControl control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(this.PressedScale(control), TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else if (this.IsChecked(control))
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.To(UncheckedScale, control.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseOut));
            }
        }

        // The circle, its icon and the label beneath
        protected void RenderRound(
            TControl control,
            DrawingContext drawContext,
            Bitmap icon,
            string text,
            TextFormat format)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var colors = control.ActualTheme.Colors;

            float textHeight = bounds.Height / 4;
            Rect textBounds = new Rect(bounds.X, bounds.Bottom - textHeight, bounds.Width, textHeight);

            float buttonDimension = Math.Min(bounds.Width, bounds.Height - textHeight);
            float buttonRadius = buttonDimension / 2;
            Rect buttonBounds = new Rect(
                bounds.Center.X - buttonRadius,
                bounds.Center.Y - (textHeight / 2) - buttonRadius,
                buttonDimension,
                buttonDimension);

            float imageDimension = buttonDimension / 2;
            Rect imageBounds = new Rect(
                buttonBounds.Center.X - (imageDimension / 2),
                buttonBounds.Center.Y - (imageDimension / 2),
                imageDimension,
                imageDimension);

            if (control.IsPressed)
            {
                drawContext.FillEllipse(buttonBounds, colors.SubtleForeground);
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, Colors.White);
                }
            }
            else if (this.IsChecked(control))
            {
                drawContext.FillEllipse(buttonBounds, AccentColors.Fill(control.ActualTheme));
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, AccentColors.OnFill(control.ActualTheme));
                }
            }
            else
            {
                drawContext.DrawEllipse(buttonBounds.Center, buttonRadius - 1.0f, buttonRadius - 1.0f, colors.SubtleForeground, strokeWidth: 1.0f);
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, colors.SubtleForeground);
                }
            }

            drawContext.DrawText(text, textBounds, colors.SubtleForeground, format);
        }
    }

    // A pill with the icon left of the label: outlined, and filled with the
    // accent while checked; it shrinks a little as it's pressed
    internal sealed class ToggleButtonTemplate : ControlTemplate<ToggleButton>
    {
        private const float PressedScale = 0.95f;
        private const float IconSize = 24;
        private const float IconLabelGap = 6;
        private const float BorderWidth = 1;
        private static readonly Thickness Padding = new Thickness(16, 8);

        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 15.0f,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        protected override void OnStateChanged(
            ToggleButton control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new SpringEase(EasingMode.EaseOut));
            }
        }

        protected override Size? Measure(
            ToggleButton control,
            Size availableSize)
        {
            var text = TextLayout.Measure(control.Text ?? string.Empty, LabelFormat);
            float width = Padding.Left + Padding.Right + (float)Math.Ceiling(text.Width);
            if (null != control.Icon)
            {
                width += IconSize + (string.IsNullOrEmpty(control.Text) ? 0 : IconLabelGap);
            }

            float height = Padding.Top + Padding.Bottom + Math.Max(IconSize, (float)Math.Ceiling(text.Height));
            return new Size(width, height);
        }

        protected override void Render(
            ToggleButton control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var colors = control.ActualTheme.Colors;
            float radius = Math.Min(20, bounds.Height / 2);

            Color foreground;
            if (control.IsChecked)
            {
                drawContext.FillRoundedRectangle(bounds, radius, colors.StrongAccent);
                foreground = colors.OnStrongAccent;
            }
            else
            {
                float inset = BorderWidth / 2;
                drawContext.DrawRoundedRectangle(bounds.Deflate(inset, inset), radius - inset, colors.Outline, BorderWidth);
                foreground = colors.Foreground;
            }

            // The icon and label, centered together
            var text = TextLayout.Measure(control.Text ?? string.Empty, LabelFormat);
            bool hasIcon = null != control.Icon;
            float contentWidth = (float)Math.Ceiling(text.Width) + (hasIcon ? IconSize + (string.IsNullOrEmpty(control.Text) ? 0 : IconLabelGap) : 0);
            float x = bounds.Center.X - (contentWidth / 2);
            if (hasIcon)
            {
                drawContext.DrawImage(control.Icon, new Rect(x, bounds.Center.Y - (IconSize / 2), IconSize, IconSize), foreground);
                x += IconSize + IconLabelGap;
            }

            drawContext.DrawText(control.Text, new Rect(x, 0, Math.Max(0, bounds.Right - x), bounds.Height), foreground, LabelFormat);
        }
    }

    internal sealed class RadioButtonTemplate : RoundToggleTemplate<RadioButton>
    {
        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 10.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        protected override bool IsChecked(RadioButton control) => control.IsChecked;

        // A checked radio button shrinks a little from its full size
        protected override float PressedScale(
            RadioButton control)
        {
            return (control.IsChecked ? 1.0f : UncheckedScale) - PressedScaleDelta;
        }

        protected override void Render(
            RadioButton control,
            DrawingContext drawContext)
        {
            this.RenderRound(control, drawContext, control.RadioButtonItem.Icon, control.RadioButtonItem.Text, LabelFormat);
        }
    }

    internal sealed class DayOfWeekToggleButtonTemplate : RoundToggleTemplate<DayOfWeekToggleButton>
    {
        private static readonly TextFormat LetterFormat = new TextFormat()
        {
            FontSize = 20.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        protected override bool IsChecked(DayOfWeekToggleButton control) => control.IsChecked;

        // The day's letter in the circle
        protected override void Render(
            DayOfWeekToggleButton control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var colors = control.ActualTheme.Colors;

            float buttonDimension = Math.Min(bounds.Width, bounds.Height);
            float buttonRadius = buttonDimension / 2;
            Rect buttonBounds = new Rect(
                bounds.Center.X - buttonRadius,
                bounds.Center.Y - buttonRadius,
                buttonDimension,
                buttonDimension);

            if (control.IsPressed)
            {
                drawContext.FillEllipse(buttonBounds, colors.SubtleForeground);
                drawContext.DrawText(control.Text, bounds, Colors.White, LetterFormat);
            }
            else if (control.IsChecked)
            {
                drawContext.FillEllipse(buttonBounds, AccentColors.Fill(control.ActualTheme));
                drawContext.DrawText(control.Text, bounds, AccentColors.OnFill(control.ActualTheme), LetterFormat);
            }
            else
            {
                drawContext.DrawEllipse(buttonBounds.Center, buttonRadius - 1.0f, buttonRadius - 1.0f, colors.SubtleForeground, strokeWidth: 1.0f);
                drawContext.DrawText(control.Text, bounds, colors.SubtleForeground, LetterFormat);
            }
        }
    }

    internal sealed class ImageToggleButtonTemplate : ControlTemplate<ImageToggleButton>
    {
        protected override void Render(
            ImageToggleButton control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            Rect iconBounds = bounds;
            if (control.IconSize.HasValue)
            {
                Size iconSize = control.IconSize.Value;
                iconBounds = new Rect(
                    bounds.Left + ((bounds.Width - iconSize.Width) / 2),
                    bounds.Top + ((bounds.Height - iconSize.Height) / 2),
                    iconSize.Width,
                    iconSize.Height);
            }

            drawContext.DrawImage(control.IsChecked ? control.CheckedIcon : control.UncheckedIcon, iconBounds, control.Tint);
        }
    }

    // An icon over a small label, in the accent (or gray, for a subtle item),
    // shrinking as it's pressed: the menu bar's buttons
    internal sealed class MenuButtonTemplate : ControlTemplate<MenuButton>
    {
        private const float PressedScale = 0.9f;
        private const float MaxIconSize = 28;
        private const float IconLabelGap = 2;

        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 11.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };

        protected override void OnStateChanged(
            MenuButton control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
            }
        }

        protected override void Render(
            MenuButton control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var theme = control.ActualTheme;
            var foreground = control.MenuItem.IsSubtleStyle
                ? theme.Colors.SubtleForeground
                : TabStripTemplate.SelectedColor(theme);

            // A fixed size icon over the label, centered together: a taller
            // bar has more room around them, not bigger icons
            float textHeight = (float)Math.Ceiling(LabelFormat.FontSize * 1.25f);
            float iconSize = Math.Min(MaxIconSize, Math.Min(bounds.Width, bounds.Height - textHeight - IconLabelGap));
            float top = (bounds.Height - (iconSize + IconLabelGap + textHeight)) / 2;
            Rect imageBounds = new Rect(bounds.Center.X - (iconSize / 2), top, iconSize, iconSize);
            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom + IconLabelGap, bounds.Width, textHeight);

            drawContext.DrawImage(control.MenuItem.Icon, imageBounds, foreground);
            drawContext.DrawText(control.MenuItem.Text, textBounds, foreground, LabelFormat);
        }
    }

    // A tab's icon over its label: gray, and the selection's color (with a
    // bolder, larger label) when checked; it shrinks a little as it's pressed
    internal sealed class TabStripItemTemplate : ControlTemplate<TabStripItem>
    {
        private const float PressedScale = 0.92f;
        private const float MaxIconSize = 28;
        private const float IconLabelGap = 2;

        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 12.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };

        private static readonly TextFormat CheckedLabelFormat = new TextFormat()
        {
            FontSize = 13.0f,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
        };

        protected override void OnStateChanged(
            TabStripItem control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new SpringEase(EasingMode.EaseOut));
            }
        }

        private static Color Foreground(
            TabStripItem control)
        {
            var theme = control.ActualTheme;
            if (control.IsChecked)
            {
                return TabStripTemplate.SelectedColor(theme);
            }

            switch (theme.Mode)
            {
                case ThemeMode.Light: return Color.FromRgb(0x6b, 0x6b, 0x6b);
                case ThemeMode.Dark: return Color.FromRgb(0x99, 0x99, 0x99);
                default: return theme.Colors.Foreground;
            }
        }

        protected override void Render(
            TabStripItem control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var format = control.IsChecked ? CheckedLabelFormat : LabelFormat;
            var foreground = Foreground(control);

            // The icon and label, centered together
            float textHeight = (float)Math.Ceiling(format.FontSize * 1.25f);
            float iconSize = Math.Min(MaxIconSize, Math.Min(bounds.Width, bounds.Height - textHeight - IconLabelGap));
            float top = bounds.Y + ((bounds.Height - (iconSize + IconLabelGap + textHeight)) / 2);
            Rect imageBounds = new Rect(bounds.Center.X - (iconSize / 2), top, iconSize, iconSize);
            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom + IconLabelGap, bounds.Width, textHeight);

            drawContext.DrawImage(control.Icon, imageBounds, foreground);
            drawContext.DrawText(control.Text, textBounds, foreground, format);
        }
    }

    // A round button with a soft shadow beneath it: the circle fills the
    // button's bounds, and the shadow is outside them
    internal sealed class FloatingButtonTemplate : ControlTemplate<FloatingButton>
    {
        // CSS's 0 3px 8px rgba(0, 0, 0, 0.3)
        private static readonly Shadow ButtonShadow = new Shadow()
        {
            Color = Color.FromArgb(0x4d, 0, 0, 0),
            Offset = new Point(0, 3),
            Blur = 8,
            Shape = ShadowShape.Ellipse,
        };

        // The icon's share of the circle's diameter
        private const float IconScale = 0.47f;

        protected override void Apply(
            FloatingButton control,
            TemplateContext context)
        {
            control.Shadow = ButtonShadow;
        }

        protected override void Unapply(
            FloatingButton control,
            TemplateContext context)
        {
            if (control.Shadow == ButtonShadow)
            {
                control.Shadow = null;
            }
        }

        protected override void Render(
            FloatingButton control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            if ((bounds.Height <= 0) || (bounds.Width <= 0))
            {
                return;
            }

            var theme = control.ActualTheme;
            Color foreground = control.IsSubtleStyle ? theme.Colors.Foreground : theme.Colors.OnStrongAccent;
            Color background = control.IsSubtleStyle
                ? ((theme.Mode != ThemeMode.Light) ? Color.FromRgb(0x33, 0x33, 0x33) : theme.Colors.Background)
                : theme.Colors.StrongAccent;
            Color pressedBackground = control.IsSubtleStyle ? background : Colors.White;
            Color pressedForeground = control.IsSubtleStyle ? foreground : Colors.Gray;

            float diameter = Math.Min(bounds.Width, bounds.Height);
            Rect buttonRect = new Rect(bounds.Center.X - (diameter / 2), bounds.Center.Y - (diameter / 2), diameter, diameter);
            float iconInset = diameter * (1 - IconScale) / 2;
            Rect imageRect = buttonRect.Deflate(iconInset, iconInset);

            if (control.IsPressed || !control.IsEnabled)
            {
                drawContext.FillEllipse(buttonRect, pressedBackground);
                drawContext.DrawImage(control.Icon, imageRect, pressedForeground);
            }
            else
            {
                drawContext.FillEllipse(buttonRect, background);
                drawContext.DrawImage(control.Icon, imageRect, foreground);
            }
        }
    }

    // A command bar's icon and label, in the accent color
    internal sealed class CommandBarButtonTemplate : ControlTemplate<CommandBarButton>
    {
        private const float PressedScale = 0.9f;

        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 9.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        protected override void OnStateChanged(
            CommandBarButton control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new SpringEase(EasingMode.EaseOut));
            }
        }

        protected override void Render(
            CommandBarButton control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var accent = AccentColors.Text(control.ActualTheme);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.65f;
            float textHeight = bounds.Height * 0.35f;
            Rect imageBounds = new Rect(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            drawContext.DrawImage(control.Icon, imageBounds, accent);

            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom, bounds.Width, bounds.Bottom - imageBounds.Bottom);
            drawContext.DrawText(control.Text, textBounds, accent, LabelFormat);
        }
    }
}
