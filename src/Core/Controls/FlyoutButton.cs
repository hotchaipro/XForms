using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    public class FlyoutButton : LightDismissControl
    {
        public event EventHandler Expanding;
        public event EventHandler Collapsing;

        private DockLayout _layout;
        private InnerButton _innerButton;

        private bool _isExpanded;
        private bool _isExpandingOrCollapsing;
        private int _buttonCount;

        public FlyoutButton()
        {
            this._innerButton = new InnerButton(this)
            {
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.Fill,
            };

            this._layout = new DockLayout();
            //this._layout.IsClippedToBounds = false;

            this._layout.Children.Add(
                this._innerButton,
                DockRegion.CenterOverlay);

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

            // Start the buttons out at half size
            button.Scale = 0.5f;

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

        protected internal override void OnLightDismiss()
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

            // Rotate and scale the button to the pressed position
            var nowait = this._innerButton.RotateTo(Angle.FromDegrees(90), TimeSpan.FromMilliseconds(250), new CubicEase(EasingMode.EaseOut));
            nowait = this._innerButton.ScaleTo(0.75f, TimeSpan.FromMilliseconds(250), new CubicEase(EasingMode.EaseOut));

            for (int i = 0; i < this._layout.Children.Count - 1; i += 1)
            {
                var button = this._layout.Children[i] as FloatingButton;
                if (button == null)
                {
                    continue;
                }

                button.IsEnabled = true;

                // Start the buttons out at half size
                button.Scale = 0.5f;

                // HACK: Redraw button to workaround drawing issue
                button.Invalidate();

                // Scale the buttons up to full size
                nowait = button.ScaleTo(1.0f, TimeSpan.FromMilliseconds(250), new CubicEase(EasingMode.EaseOut));

                // Spread the buttons out on an arc
                // TODO: Handle more than 3 buttons
                float angle = 45;
                if (this._buttonCount > 1)
                {
                    if ((this._buttonCount & 1) == 1)
                    {
                        angle = 0 + ((float)i * (90 / (float)(this._buttonCount - 1)));
                    }
                    else
                    {
                        angle = 15 + ((float)i * (60 / (float)(this._buttonCount - 1)));
                    }
                }
                float distance = ((button.Size.Width + this._innerButton.Size.Width) / 2) + 15;
                var point = Point.Translate(new Point(0, 0), Angle.FromDegrees(angle), -distance);
                nowait = button.TranslateTo(point, TimeSpan.FromMilliseconds(250), new CubicEase(EasingMode.EaseOut));

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

            this._isExpanded = false;

            // Unrotate and scale the button back to the collapsed state
            var nowait = this._innerButton.RotateTo(Angle.FromDegrees(0), TimeSpan.FromMilliseconds(250), new SpringEase(EasingMode.EaseIn));
            nowait = this._innerButton.ScaleTo(1.0f, TimeSpan.FromMilliseconds(250), new SpringEase(EasingMode.EaseIn));

            var animationTasks = new List<Task>();

            for (int i = 0; i < this._layout.Children.Count - 1; i += 1)
            {
                var button = this._layout.Children[i] as FloatingButton;
                if (button == null)
                {
                    continue;
                }

                button.IsEnabled = false;

                // Move and scale the buttons back to the collapsed position
                nowait = button.TranslateTo(new Point(0, 0), TimeSpan.FromMilliseconds(250), new SpringEase(EasingMode.EaseIn));
                nowait = button.ScaleTo(0.5f, TimeSpan.FromMilliseconds(250), new SpringEase(EasingMode.EaseIn));
            }
            this.Collapsing?.Invoke(this, EventArgs.Empty);

            this._isExpandingOrCollapsing = false;
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
