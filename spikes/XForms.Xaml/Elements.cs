using System;
using System.Collections.Generic;
using System.Windows.Input;
using XForms.Controls;
using XForms.Input;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Xaml
{
    // The XAML elements the spike adds. Grid, StackPanel and Border are the
    // framework's own (phase 2 of docs/alignment.md); the spike first had its
    // own, built on a public-API workaround.

    /// <summary>
    /// Where a template puts its templated parent's content.
    /// </summary>
    public class ContentPresenter : UserControl
    {


        public View Presented
        {
            get
            {
                return this.Content;
            }

            set
            {
                this.Content = value;
            }
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            drawContext.Clear(Colors.Transparent);
        }
    }

    /// <summary>
    /// A button whose look comes from a template; clicking it runs a command.
    /// </summary>
    /// <remarks>
    /// GAP: No visual states (pressed, pointer over). WORKAROUND: the button
    /// dims while pressed, using the opacity of the template's root.
    /// </remarks>
    public class Button : UserControl, ITapGestureDelegate
    {


        private View _templateRoot;

        public Button()
        {
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public object ButtonContent { get; set; }

        public ControlTemplate Template { get; set; }

        public Color Background { get; set; } = Colors.Transparent;

        public Color Foreground { get; set; } = Colors.Black;

        public ICommand Command { get; set; }

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
                return this.ButtonContent as string;
            }
        }

        /// <summary>
        /// Builds the button's look from its template, once its properties are set.
        /// </summary>
        public void ApplyTemplate()
        {
            this._templateRoot = this.Template?.Build(this);
            this.Content = this._templateRoot;
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            drawContext.Clear(Colors.Transparent);
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            if (null != this._templateRoot)
            {
                this._templateRoot.Animator.Opacity = 0.7f;
            }
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            if (null != this._templateRoot)
            {
                this._templateRoot.Animator.Opacity = 1.0f;
            }
        }

        void ITapGestureDelegate.OnTapped()
        {
            if ((null != this.Command) && this.Command.CanExecute(null))
            {
                this.Command.Execute(null);
            }
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
            if (null != this._templateRoot)
            {
                this._templateRoot.Animator.Opacity = 1.0f;
            }
        }
    }
}
