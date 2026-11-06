using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    // What assistive technology sees
    public partial class View
    {
        private AccessibilityRole? _accessibilityRole;
        private string? _accessibilityLabel;

        /// <summary>
        /// What the view is to assistive technology. Controls provide a default.
        /// </summary>
        public AccessibilityRole AccessibilityRole
        {
            get
            {
                return this._accessibilityRole ?? this.DefaultAccessibilityRole;
            }

            set
            {
                this._accessibilityRole = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// What assistive technology reads for the view. Controls with text
        /// provide a default.
        /// </summary>
        public string? AccessibilityLabel
        {
            get
            {
                return this._accessibilityLabel ?? this.DefaultAccessibilityLabel;
            }

            set
            {
                this._accessibilityLabel = value;

                this.OnPropertyChanged();
            }
        }

        protected virtual AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.None;
            }
        }

        protected virtual string? DefaultAccessibilityLabel
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether the view is checked, or null when it has no checked state.
        /// </summary>
        public virtual bool? AccessibilityIsChecked
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether the view's checked state is neither checked nor unchecked
        /// (a check box's indeterminate state).
        /// </summary>
        public virtual bool AccessibilityIsMixed
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The view's value within a range (a slider's, a progress bar's), or
        /// null when it has none.
        /// </summary>
        public virtual AccessibilityRange? AccessibilityRange
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the view's value within its range, for assistive technology.
        /// Returns whether it was set: false for a view whose range only
        /// shows a value (a progress bar's).
        /// </summary>
        public virtual bool SetAccessibilityRangeValue(
            double value)
        {
            return false;
        }

        /// <summary>
        /// Whether the view (a list item, tab or radio button) is selected.
        /// </summary>
        public virtual bool AccessibilityIsSelected
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether the view (such as a picker) is expanded, or null when it has
        /// no expanded state.
        /// </summary>
        public virtual bool? AccessibilityIsExpanded
        {
            get
            {
                return null;
            }
        }

        private bool _accessibilityIsHidden;

        /// <summary>
        /// Whether assistive technology skips the view and everything in it,
        /// though it shows: content an open drawer or dialog covers, say.
        /// </summary>
        public bool AccessibilityIsHidden
        {
            get
            {
                return this._accessibilityIsHidden;
            }

            set
            {
                if (value != this._accessibilityIsHidden)
                {
                    this._accessibilityIsHidden = value;
                    this.NotifyAccessibilityStateChanged();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether assistive technology should skip the view and its
        /// descendants: it or an ancestor is hidden (or hidden from assistive
        /// technology), or scaled or faded to nothing by an animator.
        /// </summary>
        internal bool IsHiddenFromAccessibility
        {
            get
            {
                for (var view = this; null != view; view = view.Parent as View)
                {
                    if (!view._isVisible || view._accessibilityIsHidden || IsAnimatedAway(view._animator))
                    {
                        return true;
                    }

                    if (null != view._ownAnimators)
                    {
                        foreach (var animator in view._ownAnimators)
                        {
                            if (IsAnimatedAway(animator))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        private static bool IsAnimatedAway(
            Animator? animator)
        {
            const float Nothing = 0.001f;
            return (null != animator)
                && ((Math.Abs(animator.ScaleX) < Nothing) || (Math.Abs(animator.ScaleY) < Nothing) || (animator.Opacity < Nothing));
        }

        /// <summary>
        /// The value assistive technology reads after the label, such as a
        /// picker's current choice, or null.
        /// </summary>
        public virtual string? AccessibilityValue
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether assistive technology can ask the view to do the action.
        /// </summary>
        public virtual bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            switch (action)
            {
                case AccessibilityAction.Invoke:
                    return this.Focusable || (this.AccessibilityRole == AccessibilityRole.ListItem);

                case AccessibilityAction.Toggle:
                    return this.AccessibilityIsChecked.HasValue;

                case AccessibilityAction.Select:
                    return (this.AccessibilityRole == AccessibilityRole.ListItem)
                        || (this.AccessibilityRole == AccessibilityRole.TabItem)
                        || (this.AccessibilityRole == AccessibilityRole.RadioButton);

                case AccessibilityAction.Expand:
                case AccessibilityAction.Collapse:
                    return this.AccessibilityIsExpanded.HasValue;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Does an action for assistive technology. By default, each action
        /// does what a tap would (toggling, selecting); selecting something
        /// already selected does nothing. Returns whether the action was done.
        /// </summary>
        public virtual bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (!this.IsAccessibilityActionSupported(action))
            {
                return false;
            }

            // Already done
            if (((action == AccessibilityAction.Select) && this.AccessibilityIsSelected)
                || ((action == AccessibilityAction.Expand) && (this.AccessibilityIsExpanded == true))
                || ((action == AccessibilityAction.Collapse) && (this.AccessibilityIsExpanded == false)))
            {
                return true;
            }

            this.OnActivate();
            return true;
        }

        /// <summary>
        /// The text a screen reader reads for the view: its label, then its
        /// value. A list item without a label is named by the text inside it.
        /// </summary>
        internal string? AccessibilityText
        {
            get
            {
                string? label = this.AccessibilityLabel;
                if (string.IsNullOrEmpty(label) && (this.AccessibilityRole == AccessibilityRole.ListItem))
                {
                    var text = new System.Text.StringBuilder();
                    AppendAccessibilityText(this, text);
                    label = text.ToString();
                }

                // A picker may be labeled by its value
                string? value = this.AccessibilityValue;
                if (string.IsNullOrEmpty(value) || (value == label))
                {
                    return label;
                }

                return string.IsNullOrEmpty(label) ? value : label + ", " + value;
            }
        }

        private static void AppendAccessibilityText(
            View parent,
            System.Text.StringBuilder text)
        {
            var children = parent.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsVisible)
                {
                    continue;
                }

                if ((child.AccessibilityRole == AccessibilityRole.Text) && !string.IsNullOrEmpty(child.AccessibilityLabel))
                {
                    if (text.Length > 0)
                    {
                        text.Append(", ");
                    }

                    text.Append(child.AccessibilityLabel);
                }
                else
                {
                    AppendAccessibilityText(child, text);
                }
            }
        }

        /// <summary>
        /// Whether a screen reader's activate gesture does anything.
        /// </summary>
        internal bool IsAccessibilityActionable
        {
            get
            {
                return this.IsAccessibilityActionSupported(AccessibilityAction.Invoke)
                    || this.IsAccessibilityActionSupported(AccessibilityAction.Toggle)
                    || this.IsAccessibilityActionSupported(AccessibilityAction.Select);
            }
        }

        /// <summary>
        /// What a screen reader's activate gesture (a double tap) does: toggles,
        /// selects or invokes.
        /// </summary>
        internal bool PerformDefaultAccessibilityAction()
        {
            if (this.IsAccessibilityActionSupported(AccessibilityAction.Toggle))
            {
                return this.PerformAccessibilityAction(AccessibilityAction.Toggle);
            }

            if (this.IsAccessibilityActionSupported(AccessibilityAction.Select))
            {
                return this.PerformAccessibilityAction(AccessibilityAction.Select);
            }

            return this.PerformAccessibilityAction(AccessibilityAction.Invoke);
        }

        /// <summary>
        /// Raised (for the platforms) when a view's accessible state, value or
        /// label changes.
        /// </summary>
        internal static event Action<View>? AccessibilityStateChanged;

        /// <summary>
        /// Tells assistive technology that the view's checked or selected
        /// state, value or label changed.
        /// </summary>
        protected void NotifyAccessibilityStateChanged()
        {
            // NOTE: Not Renderer, which would create the native object, before
            // the view is attached (and the rest of its initializer has run)
            if (this.HasRenderer)
            {
                AccessibilityStateChanged?.Invoke(this);
            }
        }
    }
}
