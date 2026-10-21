using System;
using XForms.Controls;
using XForms.Input;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Pointer, gesture, wheel, focus and key input: taps, double taps and
    /// holds, a pan, a pinch, the wheel and keys, with a log of the last events.
    /// </summary>
    public class InputPage : Page
    {
        private readonly TextBlock _log;
        private readonly TextBlock _gestures;
        private int _taps, _doubleTaps;
        private string _holding = "none";

        public InputPage()
        {
            var layout = new StackPanel()
            {
                Spacing = 12,
                Padding = new Thickness(16),
            };

            this._log = new TextBlock() { Text = "Last event: none", FontSize = 13 };
            layout.Children.Add(this._log);

            // Taps, double taps and holds
            var tapBox = Box("Tap, double tap or hold", Color.FromArgb(0xFF2196F3));
            var tapGesture = Gestures.Tap(tapBox);
            tapGesture.Tapped += (s, e) => { this._taps += 1; this.UpdateGestures(); };
            tapGesture.DoubleTapped += (s, e) => { this._doubleTaps += 1; this.UpdateGestures(); };
            tapGesture.Holding += (s, e) => { this._holding = e.HoldingState.ToString(); this.UpdateGestures(); };
            tapBox.PointerPressed += this.Log;
            tapBox.PointerReleased += this.Log;
            layout.Children.Add(tapBox);

            this._gestures = new TextBlock() { FontSize = 13 };
            this.UpdateGestures();
            layout.Children.Add(this._gestures);

            // Pan and pinch, through each box's own animator
            var row = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 12 };
            var panBox = new PanBox(this);
            row.Children.Add(panBox);
            var pinchBox = new PinchBox(this);
            row.Children.Add(pinchBox);
            layout.Children.Add(row);

            // Keys and focus
            var keyBox = new KeyBox(this);
            layout.Children.Add(keyBox);

            // The wheel
            var wheelBox = Box("Turn the wheel here", Color.FromArgb(0xFF795548));
            float wheelTotal = 0;
            wheelBox.PointerWheelChanged += (s, e) =>
            {
                wheelTotal += e.WheelDelta;
                ((TextBlock)wheelBox.Child).Text = $"Wheel total {wheelTotal:0}";
                e.Handled = true;
            };
            layout.Children.Add(wheelBox);

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            this.Content = page;
        }

        internal void Log(
            object sender,
            PointerEventArgs e)
        {
            this._log.Text = $"Last event: {e.State} pointer {e.PointerId} ({e.DeviceType}) at {e.Position.X:0},{e.Position.Y:0} {e.KeyModifiers}";
        }

        internal void LogText(
            string text)
        {
            this._log.Text = "Last event: " + text;
        }

        private void UpdateGestures()
        {
            this._gestures.Text = $"Taps {this._taps}, double taps {this._doubleTaps}, holding {this._holding}";
        }

        private static Border Box(
            string text,
            Color color)
        {
            return new Border()
            {
                Background = color,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new TextBlock()
                {
                    Text = text,
                    Foreground = Colors.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
        }

        // Moves with the pointer while panned, then springs back
        private sealed class PanBox : Border, IPanGestureDelegate
        {
            private readonly InputPage _page;
            private readonly Animator _ownAnimator;

            public PanBox(InputPage page)
            {
                this._page = page;
                this._ownAnimator = this.CreateOwnAnimator();
                this.Background = Color.FromArgb(0xFF4CAF50);
                this.Width = 120;
                this.Height = 120;
                this.Child = new TextBlock() { Text = "Pan me", Foreground = Colors.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                this.GestureRecognizers.Add(new PanGestureRecognizer(this));
            }

            public void OnPanStarted() => this._page.LogText("pan started");

            public void OnPanChanged(Point translation)
            {
                this._ownAnimator.Translation = translation;
                this._page.LogText($"pan {translation.X:0},{translation.Y:0}");
            }

            public void OnPanCompleted(Point translation)
            {
                this._page.LogText($"pan completed at {translation.X:0},{translation.Y:0}");
                this._ownAnimator.TranslateTo(Point.Zero, TimeSpan.FromMilliseconds(200), new CubicEase(EasingMode.EaseOut));
            }

            public void OnPanCanceled() => this._ownAnimator.Translation = Point.Zero;

            public void OnTouchBegan() { }

            public void OnTouchEnded() { }

            public void OnTouchCanceled() { }
        }

        // Scales with two fingers
        private sealed class PinchBox : Border, IPinchGestureDelegate
        {
            private readonly InputPage _page;
            private readonly Animator _ownAnimator;

            public PinchBox(InputPage page)
            {
                this._page = page;
                this._ownAnimator = this.CreateOwnAnimator();
                this.Background = Color.FromArgb(0xFFFF9800);
                this.Width = 120;
                this.Height = 120;
                this.Child = new TextBlock() { Text = "Pinch me", Foreground = Colors.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                this.GestureRecognizers.Add(new PinchGestureRecognizer(this));
            }

            public void OnPinchStarted() => this._page.LogText("pinch started");

            public void OnPinchChanged(float scale, Point center)
            {
                this._ownAnimator.Scale = Math.Max(0.25f, Math.Min(4, scale));
                this._page.LogText($"pinch scale {scale:0.00}");
            }

            public void OnPinchCompleted(float scale)
            {
                this._page.LogText($"pinch completed at {scale:0.00}");
                this._ownAnimator.ScaleTo(1, TimeSpan.FromMilliseconds(200), new CubicEase(EasingMode.EaseOut));
            }

            public void OnTouchBegan() { }

            public void OnTouchEnded() { }

            public void OnTouchCanceled() { }
        }

        // Takes focus when tapped and shows the keys it gets
        private sealed class KeyBox : Border
        {
            private readonly InputPage _page;
            private readonly TextBlock _text;

            public KeyBox(InputPage page)
            {
                this._page = page;
                this.Height = 60;
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.BorderThickness = new Thickness(2);
                this.Focusable = true;
                this._text = new TextBlock() { Text = "Tap here, then press keys", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                this.Child = this._text;
                Gestures.Tap(this).Tapped += (s, e) => this.Focus();
                this.UpdateBorder();
            }

            protected override void OnGotFocus() => this.UpdateBorder();

            protected override void OnLostFocus() => this.UpdateBorder();

            private void UpdateBorder()
            {
                this.BorderBrush = this.IsFocused ? Color.FromArgb(0xFF2196F3) : Color.FromArgb(0xFFCCCCCC);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                this._text.Text = $"Key down: {e.Key} {e.KeyModifiers}";
                e.Handled = (e.Key != Key.Tab);
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                this._page.LogText($"key up {e.Key}");
            }
        }
    }
}
