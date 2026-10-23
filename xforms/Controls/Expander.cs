using System;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    /// <summary>
    /// A header that shows and hides the content beneath it: a tap, Enter or
    /// Space on the header expands or collapses it, and the content fades and
    /// slides in as it expands. Its theme's templates draw the expander's
    /// frame and its header.
    /// </summary>
    public class Expander : TemplatedControl
    {
        public static readonly ViewProperty<Expander, bool> IsExpandedProperty = new(nameof(IsExpanded), v => v.IsExpanded, (v, x) => v.IsExpanded = x);

        /// <summary>
        /// The header's height.
        /// </summary>
        public const float HeaderHeight = 48.0f;

        private readonly ExpanderHeader _header;
        private readonly Border _contentHost;
        private string _headerText;
        private View _content;
        private bool _isExpanded;

        public Expander()
        {
            this._header = new ExpanderHeader(this);
            this._contentHost = new Border()
            {
                Animator = new Animator(),
                Padding = new Thickness(16, 8, 16, 16),
                IsVisible = false,
            };
        }

        /// <summary>
        /// Raised as the content starts to show.
        /// </summary>
        public event EventHandler Expanding;

        /// <summary>
        /// Raised when the content is hidden.
        /// </summary>
        public event EventHandler Collapsed;

        /// <summary>
        /// The header's text, which also names the expander for assistive
        /// technology.
        /// </summary>
        public string Header
        {
            get
            {
                return this._headerText;
            }

            set
            {
                this._headerText = value;
                this._header.InvalidateVisual();
                this._header.InvalidateMeasure();
            }
        }

        /// <summary>
        /// What shows beneath the header while expanded.
        /// </summary>
        public new View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;
                this._contentHost.Child = value;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return this._isExpanded;
            }

            set
            {
                if (value == this._isExpanded)
                {
                    return;
                }

                this._isExpanded = value;
                this.OnStateChanged();
                this._header.OnExpandedChanged();
                this.OnPropertyChanged();

                var animator = this._contentHost.Animator;
                if (value)
                {
                    this.Expanding?.Invoke(this, EventArgs.Empty);
                    this._contentHost.IsVisible = true;

                    // Fades and slides in, once it's shown (not as the
                    // expander is first built)
                    if (this.IsBuilt)
                    {
                        var motion = this.ActualTheme.Motion;
                        animator.Opacity = 0;
                        animator.FadeTo(1, motion.Medium, motion.Decelerate);
                        animator.TranslateFrom(new Point(0, -12), motion.Medium, motion.Decelerate);
                    }
                }
                else
                {
                    animator.Stop();
                    animator.Opacity = 1;
                    this._contentHost.IsVisible = false;
                    this.Collapsed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // User state: whether it's expanded, when that changed since it was
        // first attached
        private bool _initialIsExpanded;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialIsExpanded = this.IsExpanded;
        }

        protected override object GetUserState()
        {
            return (this.IsExpanded != this._initialIsExpanded) ? this.IsExpanded : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is bool isExpanded)
            {
                this.IsExpanded = isExpanded;
            }
        }

        protected override View Build()
        {
            this._contentHost.IsVisible = this._isExpanded;
            this._header.Height = HeaderHeight;
            return new StackPanel()
            {
                Children = { this._header, this._contentHost },
            };
        }
    }

    /// <summary>
    /// An expander's header: its text and a chevron, toggling the expander.
    /// Its theme's template draws it.
    /// </summary>
    public sealed class ExpanderHeader : TemplatedControl, ITapGestureDelegate
    {
        private readonly Expander _expander;

        internal ExpanderHeader(
            Expander expander)
        {
            this._expander = expander;
            this.Focusable = true;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public Expander Expander
        {
            get
            {
                return this._expander;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Button;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._expander.Header;
            }
        }

        public override bool? AccessibilityIsExpanded
        {
            get
            {
                return this._expander.IsExpanded;
            }
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if ((action == AccessibilityAction.Expand) || (action == AccessibilityAction.Collapse))
            {
                this._expander.IsExpanded = (action == AccessibilityAction.Expand);
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        internal void OnExpandedChanged()
        {
            this.OnStateChanged();
            this.NotifyAccessibilityStateChanged();
        }

        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = (this.FlowDirection == FlowDirection.RightToLeft) ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            return format;
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? TextLayout.Measure(this._expander.Header ?? string.Empty, this.CreateTextFormat()).Width + 72 : availableSize.Width;
            return new Size(width, Math.Min(Expander.HeaderHeight, availableSize.Height));
        }

        private void Toggle()
        {
            if (this._expander.IsEnabled)
            {
                this._expander.IsExpanded = !this._expander.IsExpanded;
            }
        }

        protected override void OnActivate()
        {
            this.Toggle();
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.Toggle();
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }
    }
}
