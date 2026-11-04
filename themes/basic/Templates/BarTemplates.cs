using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    // The menu bar: on the page's background, below a hairline
    internal sealed class MenuBarTemplate : ControlTemplate<MenuBar>
    {
        protected override Color? Background(
            MenuBar control)
        {
            return control.ActualTheme.Colors.Background;
        }

        protected override void Apply(
            MenuBar control,
            TemplateContext context)
        {
            var hairline = Hairline(control.ActualTheme, VerticalAlignment.Top);
            context.AddPart(hairline);
            context.State = hairline;
        }

        protected override void Unapply(
            MenuBar control,
            TemplateContext context)
        {
            if (context.State is Border hairline)
            {
                context.RemovePart(hairline);
            }
        }

        protected override void OnThemeChanged(
            MenuBar control,
            TemplateContext context)
        {
            if (context.State is Border hairline)
            {
                hairline.Background = control.ActualTheme.Colors.Outline;
            }
        }

        // A bar's edge: a line in the outline color across it
        internal static Border Hairline(
            Theme theme,
            VerticalAlignment edge)
        {
            return new Border()
            {
                Height = 1,
                Background = theme.Colors.Outline,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = edge,
                IsHitTestVisible = false,
            };
        }
    }

    // The tab strip: on the page's background, above a hairline, with an
    // underline beneath the checked tab that slides to the next with its page
    internal sealed class TabStripTemplate : ControlTemplate<TabStrip>
    {
        private const float UnderlineHeight = 3;
        private const float HairlineHeight = 1;

        private static readonly EasingFunction SlideEasing = new CubicEase(EasingMode.EaseOut);

        // Each strip's parts, for arranging them (which has no context)
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TabStrip, Parts> AllParts = new System.Runtime.CompilerServices.ConditionalWeakTable<TabStrip, Parts>();

        private sealed class Parts
        {
            public Border Underline;
            public Border Hairline;

            // The tabs' grid: where its first cell is, and how wide each is
            public float CellLeft;
            public float CellWidth;

            // Where the underline is (or is sliding to), in tabs
            public float Position = float.NaN;
        }

        internal static Color SelectedColor(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Light) ? theme.Colors.StrongAccent : theme.Colors.Accent;
        }


        protected override Color? Background(
            TabStrip control)
        {
            return control.ActualTheme.Colors.Background;
        }

        protected override void Apply(
            TabStrip control,
            TemplateContext context)
        {
            var underline = new Border()
            {
                Animator = new Animator(),
                Height = UnderlineHeight,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = SelectedColor(control.ActualTheme),
                IsHitTestVisible = false,
                IsVisible = false,
            };
            var hairline = MenuBarTemplate.Hairline(control.ActualTheme, VerticalAlignment.Bottom);
            context.AddPart(hairline);
            context.AddPart(underline);
            var parts = new Parts() { Underline = underline, Hairline = hairline };
            context.State = parts;
            AllParts.AddOrUpdate(control, parts);
        }

        protected override void Unapply(
            TabStrip control,
            TemplateContext context)
        {
            if (context.State is Parts parts)
            {
                context.RemovePart(parts.Underline);
                context.RemovePart(parts.Hairline);
            }

            AllParts.Remove(control);
        }

        protected override void OnThemeChanged(
            TabStrip control,
            TemplateContext context)
        {
            if (context.State is Parts parts)
            {
                parts.Underline.Background = SelectedColor(control.ActualTheme);
                parts.Hairline.Background = control.ActualTheme.Colors.Outline;
            }
        }

        // The underline sits at the bottom of the first of the tabs' cells,
        // above the hairline (the default arrangement places it there, by its
        // size, margin and alignment); it moves by translation, so it can slide
        protected override bool Arrange(
            TabStrip control,
            Size finalSize)
        {
            if (!AllParts.TryGetValue(control, out var parts))
            {
                return false;
            }

            var items = control.Items;
            if (items.Count == 0)
            {
                parts.Underline.IsVisible = false;
                return false;
            }

            // NOTE: The tabs' grid stretches to its MaxWidth, centered
            float gridWidth = finalSize.Width;
            if ((items[0].Parent is View grid) && !float.IsNaN(grid.MaxWidth))
            {
                gridWidth = Math.Min(gridWidth, grid.MaxWidth);
            }

            float cellWidth = gridWidth / items.Count;
            float cellLeft = (finalSize.Width - gridWidth) / 2;
            if ((cellWidth != parts.CellWidth) || (cellLeft != parts.CellLeft))
            {
                parts.CellWidth = cellWidth;
                parts.CellLeft = cellLeft;
                parts.Position = float.NaN;
                parts.Underline.Width = cellWidth;
                parts.Underline.Margin = new Thickness(cellLeft, 0, 0, HairlineHeight);
            }

            this.MoveUnderline(control, parts);
            return false;
        }

        protected override void OnStateChanged(
            TabStrip control,
            TemplateContext context)
        {
            if (context.State is Parts parts)
            {
                parts.Underline.Background = SelectedColor(control.ActualTheme);
                this.MoveUnderline(control, parts);
            }
        }

        // Follows a gesture's switch directly; otherwise slides to the tab, in
        // the share of the transition left, as the page does
        private void MoveUnderline(
            TabStrip control,
            Parts parts)
        {
            if ((parts.CellWidth <= 0) || (control.SelectedIndex < 0))
            {
                parts.Underline.IsVisible = false;
                return;
            }

            parts.Underline.IsVisible = true;
            float position = control.SelectionPosition;
            var target = new Point(position * parts.CellWidth, 0);
            var animator = parts.Underline.Animator;
            if (float.IsNaN(parts.Position) || control.IsSwitching)
            {
                parts.Position = position;
                animator.Translation = target;
            }
            else if (position != parts.Position)
            {
                float distance = Math.Min(1, Math.Abs(position - parts.Position));
                parts.Position = position;
                animator.TranslateTo(target, TimeSpan.FromTicks((long)(control.TransitionDuration.Ticks * distance)), SlideEasing);
            }
        }
    }

    // An opaque row, so the swipe commands beneath show only when it moves
    internal sealed class CommandListViewItemTemplate : ControlTemplate<CommandListViewItem>
    {
        protected override Color? Background(
            CommandListViewItem control)
        {
            return control.ActualTheme.Colors.Background;
        }
    }

    // Content that shrinks a little as it's pressed
    internal sealed class HyperlinkButtonTemplate : ControlTemplate<HyperlinkButton>
    {
        private const float PressedScale = 0.9f;

        private sealed class Feedback
        {
            public Animator Animator;
            public bool IsPressed;
        }

        // The content scales on a layer of its own
        protected override void Apply(
            HyperlinkButton control,
            TemplateContext context)
        {
            context.State = new Feedback() { Animator = context.CreateOwnAnimator() };
        }

        protected override void OnStateChanged(
            HyperlinkButton control,
            TemplateContext context)
        {
            var feedback = (Feedback)context.State;
            if (control.IsPressed == feedback.IsPressed)
            {
                return;
            }

            feedback.IsPressed = control.IsPressed;
            if (control.IsPressed)
            {
                feedback.Animator.ScaleTo(PressedScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                feedback.Animator.ScaleTo(1.0f, control.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
            }
        }
    }
}
