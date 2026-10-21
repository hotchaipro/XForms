using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;
using XForms.Toolkit;

namespace XForms.Test
{
    /// <summary>
    /// Animations on the compositor: looping translation, rotation and fades,
    /// a spring, a reveal, nested animators (the app's and a control's own),
    /// and an interrupted animation reporting it didn't finish.
    /// </summary>
    public class AnimationPage : Page
    {
        private static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

        private readonly TextBlock _status;

        public AnimationPage()
        {
            var layout = new StackPanel()
            {
                Spacing = 16,
                Padding = new Thickness(20),
            };

            // Back and forth, eased, forever
            var slider = new Box("Slide", 0xFF2196F3) { Animator = new() };
            slider.Animator.TranslateTo(new Point(200, 0), Second, new CubicEase(EasingMode.EaseInOut), loop: true, autoReverse: true);
            layout.Children.Add(slider);

            var row = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 24 };
            layout.Children.Add(row);

            // A spin around the center, linear, forever
            var spinner = new Box("Spin", 0xFF4CAF50) { Animator = new() };
            spinner.Animator.RotateFrom(Angle.Zero, Angle.FromDegrees(360), TimeSpan.FromSeconds(2), loop: true);
            row.Children.Add(spinner);

            // A fade in and out, forever
            var fader = new Box("Fade", 0xFF9C27B0) { Animator = new() };
            fader.Animator.FadeTo(0.15f, Second, loop: true, autoReverse: true);
            row.Children.Add(fader);

            // Nested: the app's animator spins it, the control's own pulses it
            var nested = new PulsingBox("Both", 0xFFFF9800) { Animator = new() };
            nested.Animator.RotateFrom(Angle.Zero, Angle.FromDegrees(-360), TimeSpan.FromSeconds(3), loop: true);
            row.Children.Add(nested);

            row.Children.Add(new ProgressRing() { VerticalAlignment = VerticalAlignment.Center });

            var row2 = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 24 };
            layout.Children.Add(row2);

            // A spring: tap to grow from half size, overshooting
            var spring = new Box("Tap: spring", 0xFFE91E63) { Animator = new() };
            Gestures.Tap(spring).Tapped += async (sender, e) =>
            {
                this._status.Text = "Spring...";
                bool finished = await spring.Animator.ScaleFrom(0.5f, TimeSpan.FromMilliseconds(1500), new SpringEase(EasingMode.EaseOut) { Oscillations = 2 });
                this._status.Text = $"Spring finished: {finished}";
            };
            row2.Children.Add(spring);

            // A reveal: tap to uncover from the left edge
            var reveal = new Box("Tap: reveal", 0xFF009688) { Animator = new() };
            Gestures.Tap(reveal).Tapped += async (sender, e) =>
            {
                this._status.Text = "Reveal...";
                bool finished = await reveal.Animator.RevealFrom(new Rect(0, 0, 0, reveal.Bounds.Height), TimeSpan.FromMilliseconds(1500), new CubicEase(EasingMode.EaseOut));
                this._status.Text = $"Reveal finished: {finished}; clip {reveal.Animator.Clip?.ToString() ?? "none"}";
            };
            row2.Children.Add(reveal);

            // Interrupt: a slow move, replaced by another before it finishes
            var mover = new Box("Tap: move", 0xFF795548) { Animator = new() };
            Gestures.Tap(mover).Tapped += async (sender, e) =>
            {
                var first = mover.Animator.TranslateTo(new Point(0, 60), TimeSpan.FromSeconds(2));
                await System.Threading.Tasks.Task.Delay(300);
                var second = mover.Animator.TranslateTo(Point.Zero, TimeSpan.FromMilliseconds(400), new CubicEase(EasingMode.EaseOut));
                bool firstFinished = await first;
                bool secondFinished = await second;
                this._status.Text = $"First move finished: {firstFinished}; second: {secondFinished}";
            };
            row2.Children.Add(mover);

            this._status = new TextBlock() { Text = "Tap the boxes" };
            layout.Children.Add(this._status);

            // Layout transitions: tap a chip to move it to the front; others slide
            var chips = new WrapPanel()
            {
                ChildTransitions = new LayoutTransition(),
                ItemSpacing = 8,
                LineSpacing = 8,
            };
            for (int i = 1; i <= 6; i += 1)
            {
                chips.Children.Add(this.CreateChip(chips, i));
            }

            var add = new Box("Tap: add", 0xFF607D8B) { Width = 120, Height = 40 };
            int next = 7;
            Gestures.Tap(add).Tapped += (sender, e) => chips.Children.Add(this.CreateChip(chips, next++));
            layout.Children.Add(add);
            layout.Children.Add(chips);

            // A frame callback: counts the frames the display shows each second
            var frames = new TextBlock() { Text = "Frames per second: ..." };
            layout.Children.Add(frames);
            int count = 0;
            TimeSpan start = TimeSpan.Zero;
            void OnFrame(TimeSpan time)
            {
                if (start == TimeSpan.Zero)
                {
                    start = time;
                }

                count += 1;
                if (time - start >= TimeSpan.FromSeconds(1))
                {
                    frames.Text = $"Frames per second: {count / (time - start).TotalSeconds:0}";
                    count = 0;
                    start = time;
                }

                if (null != this.Parent)
                {
                    this.Application.RequestAnimationFrame(OnFrame);
                }
            }

            this.Attached += (sender, e) => this.Application.RequestAnimationFrame(OnFrame);

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            this.Content = page;
        }

        private View CreateChip(
            WrapPanel chips,
            int number)
        {
            var chip = new Box($"Chip {number}", 0xFF3F51B5) { Width = 80, Height = 40 };
            Gestures.Tap(chip).Tapped += (sender, e) =>
            {
                chips.Children.Remove(chip);
                chips.Children.Insert(0, chip);
            };
            return chip;
        }

        // A labeled, colored box
        private class Box : UserControl
        {
            private readonly string _label;
            private readonly Color _color;

            public Box(
                string label,
                uint color)
            {
                this._label = label;
                this._color = Color.FromArgb(color);
                this.Width = 90;
                this.Height = 90;
                this.HorizontalAlignment = HorizontalAlignment.Left;
            }

            public override Color BackgroundColor => Colors.Transparent;

            protected override void Render(
                DrawingContext context)
            {
                context.Clear(Colors.Transparent);
                context.FillRoundedRectangle(new Rect(0, 0, this.Bounds.Width, this.Bounds.Height), 12, new SolidColorBrush(this._color));

                using (var text = new TextLayout(this._label, new TextFormat() { FontSize = 13, HorizontalAlignment = HorizontalAlignment.Center }, this.Bounds.Width))
                {
                    context.DrawText(text, new Point(0, (this.Bounds.Height - text.Size.Height) / 2), Colors.White);
                }
            }
        }

        // A box with its own animator, inside the app's
        private sealed class PulsingBox : Box
        {
            public PulsingBox(
                string label,
                uint color)
                : base(label, color)
            {
                var own = this.CreateOwnAnimator();
                own.ScaleTo(0.7f, TimeSpan.FromMilliseconds(600), new CubicEase(EasingMode.EaseInOut), loop: true, autoReverse: true);
            }
        }
    }
}
