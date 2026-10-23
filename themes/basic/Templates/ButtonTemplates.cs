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
                Background = control.IsAlertStyle ? colors.Error : colors.Accent,
                Foreground = control.IsAlertStyle ? colors.OnError : colors.OnAccent,
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
                drawContext.FillEllipse(buttonBounds, colors.Accent);
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, Colors.White);
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

    internal sealed class ToggleButtonTemplate : RoundToggleTemplate<ToggleButton>
    {
        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 10.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        protected override bool IsChecked(ToggleButton control) => control.IsChecked;

        protected override void Render(
            ToggleButton control,
            DrawingContext drawContext)
        {
            this.RenderRound(control, drawContext, control.Icon, control.Text, LabelFormat);
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
                drawContext.FillEllipse(buttonBounds, colors.Accent);
                drawContext.DrawText(control.Text, bounds, Colors.White, LetterFormat);
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

    // An icon over a small label, shrinking as it's pressed: the menu bar's
    // buttons
    internal sealed class MenuButtonTemplate : ControlTemplate<MenuButton>
    {
        private const float PressedScale = 0.9f;

        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 10.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
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
            var foreground = (theme.Mode != ThemeMode.Light) ? theme.Colors.Foreground : Colors.White;

            float margin = Math.Min(bounds.Width, bounds.Height) * 0.1f;
            bounds = bounds.Inflate(-margin, -margin);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.75f;
            float textHeight = bounds.Height * 0.25f;
            Rect imageBounds = new Rect(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);
            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom, bounds.Width, textHeight);

            drawContext.DrawImage(control.MenuItem.Icon, imageBounds, foreground);
            drawContext.DrawText(control.MenuItem.Text, textBounds, foreground, LabelFormat);
        }
    }

    // A tab's icon and label, full size when checked
    internal sealed class TabStripItemTemplate : ControlTemplate<TabStripItem>
    {
        private const float UncheckedScale = 0.8f;

        private static readonly TextFormat LabelFormat = new TextFormat()
        {
            FontSize = 12.0f,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        protected override void OnStateChanged(
            TabStripItem control,
            TemplateContext context)
        {
            var feedback = ScaleFeedback.For(context);
            if (control.IsPressed)
            {
                feedback.To(control.IsChecked ? 0.95f : UncheckedScale - 0.05f, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
                feedback.Animator.Opacity = 1.0f;
            }
            else if (control.IsChecked)
            {
                feedback.To(1.0f, control.ActualTheme.Motion.Short, new SpringEase(EasingMode.EaseOut));
                feedback.Animator.Opacity = 1.0f;
            }
            else
            {
                feedback.To(UncheckedScale, control.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
                feedback.Animator.Opacity = (control.ActualTheme.Mode != ThemeMode.Light) ? 1.0f : 0.5f;
            }
        }

        protected override void Render(
            TabStripItem control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var theme = control.ActualTheme;

            float margin = Math.Min(bounds.Width, bounds.Height) * 0.1f;
            bounds = bounds.Inflate(-margin, -margin);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.65f;
            float textHeight = bounds.Height * 0.35f;
            Rect imageBounds = new Rect(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);
            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom, bounds.Width, bounds.Bottom - imageBounds.Bottom);

            Color foreground;
            if (theme.Mode != ThemeMode.Light)
            {
                foreground = control.IsChecked ? theme.Colors.Accent : theme.Colors.SubtleForeground;
            }
            else
            {
                foreground = Colors.White;
            }

            drawContext.DrawImage(control.Icon, imageBounds, foreground);
            drawContext.DrawText(control.Text, textBounds, foreground, LabelFormat);
        }
    }

    // A round button with a shadow
    internal sealed class FloatingButtonTemplate : ControlTemplate<FloatingButton>
    {
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
            Color foreground = control.IsSubtleStyle ? theme.Colors.Foreground : Colors.White;
            Color background = control.IsSubtleStyle
                ? ((theme.Mode != ThemeMode.Light) ? Color.FromRgb(0x33, 0x33, 0x33) : theme.Colors.Background)
                : theme.Colors.Accent;
            Color pressedBackground = control.IsSubtleStyle ? background : Colors.White;
            Color pressedForeground = control.IsSubtleStyle ? foreground : Colors.Gray;

            Size shadowPadding = new Size(bounds.Width / 10, bounds.Height / 10);
            Size shadowThinPadding = new Size(shadowPadding.Width / 2, shadowPadding.Height / 2);
            Rect buttonRect = bounds.Deflate(shadowPadding.Width, shadowPadding.Height);
            Rect shadowRect = Rect.FromLTRB(bounds.X, buttonRect.Top - shadowThinPadding.Height, buttonRect.Right + shadowThinPadding.Width, bounds.Bottom);
            Rect imageRect = buttonRect.Deflate(buttonRect.Width / 6, buttonRect.Height / 6);
            float blurAmount = Math.Min(shadowPadding.Width, shadowPadding.Height) / 1.5f;
            Color shadowColor = Colors.Gray;

            if (control.IsPressed || !control.IsEnabled)
            {
                drawContext.FillEllipseShadow(shadowRect, shadowColor, blurAmount / 2);
                drawContext.FillEllipse(buttonRect, pressedBackground);
                drawContext.DrawImage(control.Icon, imageRect, pressedForeground);
            }
            else
            {
                drawContext.FillEllipseShadow(shadowRect, shadowColor, blurAmount);
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
            var accent = control.ActualTheme.Colors.Accent;

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
