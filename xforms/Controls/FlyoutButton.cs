using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Input;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class FlyoutButton : LightDismissControl
    {
        public event EventHandler Expanding;
        public event EventHandler Collapsing;

        private DockPanel _layout;
        private InnerButton _innerButton;

        private bool _isExpanded;
        private bool _isExpandingOrCollapsing;
        private int _buttonCount;

        public FlyoutButton()
        {
            this._innerButton = new InnerButton(this)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this._layout = new DockPanel();
            //this._layout.IsClippedToBounds = false;

            this._layout.Children.Add(
                this._innerButton,
                Dock.CenterOverlay);

            this.Content = this._layout;
        }

        public Bitmap Icon
        {
            get
            {
                return this._innerButton.Icon;
            }

            set
            {
                this._innerButton.Icon = value;
            }
        }

        private bool _isSubtleStyle = true;

        /// <summary>
        /// Whether the button is in the subtle style while its flyout is
        /// closed (the default), or the accent color, as an action button.
        /// Open, it's always subtle.
        /// </summary>
        /// <summary>
        /// The space between neighbouring buttons on the arc they fan out on:
        /// 12 by default. The arc grows to fit it.
        /// </summary>
        public float ButtonSpacing
        {
            get;
            set;
        } = 12;

        /// <summary>
        /// The least distance from this button's center to the buttons' (the
        /// arc's radius): 0 (the default) for as close as the spacing allows.
        /// </summary>
        public float MinimumRadius
        {
            get;
            set;
        }

        public bool IsSubtleStyle
        {
            get
            {
                return this._isSubtleStyle;
            }

            set
            {
                this._isSubtleStyle = value;
                if (!this._isExpanded)
                {
                    this._innerButton.IsSubtleStyle = value;
                }

                this.OnPropertyChanged();
            }
        }

        private bool IsExpanded
        {
            get
            {
                return this._isExpanded;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        public void AddButton(
            FloatingButton button)
        {
            if (null == button)
            {
                throw new ArgumentNullException(nameof(button));
            }

            // Start the buttons out at half size, hidden behind this one
            button.FlyoutAnimator.Scale = 0.5f;
            button.IsVisible = false;

            this._layout.Children.Insert(0, button);

            button.Clicked += Button_Clicked;

            this._buttonCount += 1;
        }

        private void Button_Clicked(
            object sender,
            EventArgs e)
        {
            this.CollapseFlyout();
        }

        protected override void OnLightDismiss()
        {
            this.CollapseFlyout();
        }

        private void ExpandFlyout()
        {
            if (this._isExpandingOrCollapsing)
            {
                return;
            }

            this._isExpandingOrCollapsing = true;

            this.IsLightDismissEnabled = true;
            this._innerButton.IsSubtleStyle = true;

            // Rotate and scale the button to the pressed position
            var nowait = this._innerButton.FlyoutAnimator.RotateTo(Angle.FromDegrees(90), this.ActualTheme.Motion.Medium, new CubicEase(EasingMode.EaseOut));
            nowait = this._innerButton.FlyoutAnimator.ScaleTo(0.75f, this.ActualTheme.Motion.Medium, new CubicEase(EasingMode.EaseOut));

            for (int i = 0; i < this._layout.Children.Count - 1; i += 1)
            {
                var button = this._layout.Children[i] as FloatingButton;
                if (button == null)
                {
                    continue;
                }

                button.IsEnabled = true;
                button.IsVisible = true;

                // Start the buttons out at half size
                button.FlyoutAnimator.Scale = 0.5f;

                // HACK: Redraw button to workaround drawing issue
                button.InvalidateVisual();

                // Scale the buttons up to full size
                nowait = button.FlyoutAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Medium, new CubicEase(EasingMode.EaseOut));

                // Spread the buttons out on an arc
                // TODO: Handle more than 3 buttons
                float angle = 45;
                float step = 0;
                if (this._buttonCount > 1)
                {
                    if ((this._buttonCount & 1) == 1)
                    {
                        step = 90 / (float)(this._buttonCount - 1);
                        angle = 0 + ((float)i * step);
                    }
                    else
                    {
                        step = 60 / (float)(this._buttonCount - 1);
                        angle = 15 + ((float)i * step);
                    }
                }

                // Clear of this button, far enough out that neighbours on the
                // arc are the spacing apart (the circles fill their bounds),
                // and at least the minimum radius
                float distance = ((button.Bounds.Size.Width + this._innerButton.Bounds.Size.Width) / 2) + 15;
                if (step > 0)
                {
                    float chord = (float)(2 * Math.Sin(Angle.FromDegrees(step / 2).Radians));
                    distance = Math.Max(distance, (button.Bounds.Size.Width + Math.Max(0, this.ButtonSpacing)) / chord);
                }

                distance = Math.Max(distance, this.MinimumRadius);
                var point = Point.Translate(new Point(0, 0), Angle.FromDegrees(angle), -distance);
                nowait = button.FlyoutAnimator.TranslateTo(point, this.ActualTheme.Motion.Medium, new CubicEase(EasingMode.EaseOut));

                // CONSIDER: Morph the color from white to accent
                //this._innerButton.Animate(String.Empty, (value) => { this._innerButton.BackgroundColor = Color.FromRgb(0, 0, 0); }, length: AnimationLengthMs, easing: Easing.CubicOut);
            }

            this.Expanding?.Invoke(this, EventArgs.Empty);

            this._isExpandingOrCollapsing = false;

            this._isExpanded = true;
        }

        private void CollapseFlyout()
        {
            if (this._isExpandingOrCollapsing)
            {
                return;
            }

            this._isExpandingOrCollapsing = true;

            this.IsLightDismissEnabled = false;
            this._innerButton.IsSubtleStyle = this._isSubtleStyle;

            this._isExpanded = false;

            // The button is back in its collapsed state at once (no pressed
            // look, no growing back), as the buttons retract
            this._innerButton.FlyoutAnimator.Rotation = Angle.FromDegrees(0);
            this._innerButton.FlyoutAnimator.Scale = 1.0f;
            AnimationHandle nowait;

            var animationTasks = new List<Task>();

            for (int i = 0; i < this._layout.Children.Count - 1; i += 1)
            {
                var button = this._layout.Children[i] as FloatingButton;
                if (button == null)
                {
                    continue;
                }

                button.IsEnabled = false;

                // Move and scale the buttons back to the collapsed position,
                // then hide them (unless the flyout opened again meanwhile)
                nowait = button.FlyoutAnimator.TranslateTo(new Point(0, 0), this.ActualTheme.Motion.Medium, new CubicEase(EasingMode.EaseOut));
                this.HideWhenCollapsed(button, button.FlyoutAnimator.ScaleTo(0.5f, this.ActualTheme.Motion.Medium, new CubicEase(EasingMode.EaseOut)));
            }
            this.Collapsing?.Invoke(this, EventArgs.Empty);

            this._isExpandingOrCollapsing = false;
        }

        private async void HideWhenCollapsed(
            FloatingButton button,
            AnimationHandle collapsing)
        {
            await collapsing;
            if (!this._isExpanded)
            {
                button.IsVisible = false;
            }
        }

        private sealed class InnerButton : FloatingButton, ITapGestureDelegate
        {
            private FlyoutButton _outerButton;

            public InnerButton(
                FlyoutButton outerButton)
            {
                if (null == outerButton)
                {
                    throw new ArgumentNullException(nameof(outerButton));
                }

                this._outerButton = outerButton;
                this.IsSubtleStyle = true;
            }

            // Closing the flyout shows no pressed look
            internal override bool ShowsPressedLook
            {
                get
                {
                    return !this._outerButton.IsExpanded;
                }
            }

            // Labeled by the flyout button's label
            protected override string DefaultAccessibilityLabel
            {
                get
                {
                    return this._outerButton.AccessibilityLabel;
                }
            }

            protected override void OnTapBegan()
            {
                base.OnTapBegan();

                if (this._outerButton.IsExpanded)
                {
                    this._outerButton.CollapseFlyout();
                }
                else
                {
                    this._outerButton.ExpandFlyout();
                }
            }
        }
    }
}
