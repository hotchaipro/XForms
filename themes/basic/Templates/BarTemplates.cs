using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    // The Basic look of the bars: in the accent color in light mode, and on
    // the surface color otherwise
    internal sealed class MenuBarTemplate : ControlTemplate<MenuBar>
    {
        protected override Color? Background(
            MenuBar control)
        {
            return BarBackground(control.ActualTheme);
        }

        internal static Color BarBackground(
            Theme theme)
        {
            return (theme.Mode != ThemeMode.Light) ? theme.Colors.Surface : theme.Colors.Accent;
        }
    }

    internal sealed class TabStripTemplate : ControlTemplate<TabStrip>
    {
        protected override Color? Background(
            TabStrip control)
        {
            return MenuBarTemplate.BarBackground(control.ActualTheme);
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
