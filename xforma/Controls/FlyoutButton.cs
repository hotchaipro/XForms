using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForma.Input;
using XForma.Layouts;
using XForma.Controls;
using XForma.Primitives;

namespace XForma.Controls
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
        /// The space between neighbouring buttons on the arc they fan out on, or
        /// null (the default) for the theme's. The arc grows to fit it.
        /// </summary>
        public float? ButtonSpacing
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

            // Start the buttons out small, hidden behind this one (their
            // scale is the theme's when the flyout opens)
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
            var style = this.ActualTheme.FlyoutButtonStyle;
            var motion = this.ActualTheme.Motion;
            var nowait = this._innerButton.FlyoutAnimator.RotateTo(style.OpenRotation, motion.Medium, motion.Decelerate);
            nowait = this._innerButton.FlyoutAnimator.ScaleTo(style.OpenScale, motion.Medium, motion.Decelerate);

            for (int i = 0; i < this._layout.Children.Count - 1; i += 1)
            {
                var button = this._layout.Children[i] as FloatingButton;
                if (button == null)
                {
                    continue;
                }

                button.IsEnabled = true;
                button.IsVisible = true;

                // Start the buttons out small
                button.FlyoutAnimator.Scale = style.CollapsedButtonScale;

                // HACK: Redraw button to workaround drawing issue
                button.InvalidateVisual();

                // Scale the buttons up to full size
                nowait = button.FlyoutAnimator.ScaleTo(1.0f, motion.Medium, motion.Decelerate);

                // Spread the buttons out on an arc
                // TODO: Handle more than 3 buttons
                float angle = style.ArcCenter.Degrees;
                float step = 0;
                if (this._buttonCount > 1)
                {
                    float sweep = (((this._buttonCount & 1) == 1) ? style.ArcSweep : style.EvenArcSweep).Degrees;
                    step = sweep / (float)(this._buttonCount - 1);
                    angle = (style.ArcCenter.Degrees - (sweep / 2)) + ((float)i * step);
                }

                // Clear of this button, far enough out that neighbours on the
                // arc are the spacing apart (the circles fill their bounds)
                float distance = ((button.Bounds.Size.Width + this._innerButton.Bounds.Size.Width) / 2) + style.ButtonClearance;
                if (step > 0)
                {
                    float chord = (float)(2 * Math.Sin(Angle.FromDegrees(step / 2).Radians));
                    distance = Math.Max(distance, (button.Bounds.Size.Width + Math.Max(0, this.ButtonSpacing ?? style.ButtonSpacing)) / chord);
                }

                var point = Point.Translate(new Point(0, 0), Angle.FromDegrees(angle), -distance);
                nowait = button.FlyoutAnimator.TranslateTo(point, motion.Medium, motion.Decelerate);

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
            var style = this.ActualTheme.FlyoutButtonStyle;
            var motion = this.ActualTheme.Motion;

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
                nowait = button.FlyoutAnimator.TranslateTo(new Point(0, 0), motion.Medium, motion.Decelerate);
                this.HideWhenCollapsed(button, button.FlyoutAnimator.ScaleTo(style.CollapsedButtonScale, motion.Medium, motion.Decelerate));
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
