using System;
using System.Collections.Generic;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class CommandBarCommandCollection
    {
        private List<UICommand> _primaryCommands;
        private List<UICommand> _secondaryCommands;

        public CommandBarCommandCollection()
        {
            this._primaryCommands = new List<UICommand>();
            this._secondaryCommands = new List<UICommand>();
        }

        public List<UICommand> PrimaryCommands
        {
            get
            {
                return this._primaryCommands;
            }
        }

        public List<UICommand> SecondaryCommands
        {
            get
            {
                return this._secondaryCommands;
            }
        }
    }

    public class CommandBar : ContentControl
    {
        private DockPanel _buttonsLayout;
        private CommandBarButtonCollection _primaryButtons;
        private CommandBarButtonCollection _secondaryButtons;

        public CommandBar()
        {
            this._buttonsLayout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this._primaryButtons = new CommandBarButtonCollection(this, Dock.Left);
            this._secondaryButtons = new CommandBarButtonCollection(this, Dock.Right);

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;

            this.Content = this._buttonsLayout;
        }

        // Buttons side by side, each a cell this wide plus its share of the
        // spacing (half on each side of its icon and label, so a tap between
        // two lands on the nearer). Without room for the spacing across the
        // bar, they go back to side by side.

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.InvalidateMeasure();
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            this.FitButtons(availableSize.Width - this._buttonsLayout.Margin.Left - this._buttonsLayout.Margin.Right);
            return base.MeasureOverride(availableSize);
        }

        // Sizes the buttons for the width: spaced if the visible ones fit
        private void FitButtons(
            float width)
        {
            int count = 0;
            foreach (var button in this._primaryButtons.Buttons)
            {
                count += button.IsVisible ? 1 : 0;
            }

            foreach (var button in this._secondaryButtons.Buttons)
            {
                count += button.IsVisible ? 1 : 0;
            }

            var style = this.ActualTheme.CommandBarStyle;
            float spacing = Math.Max(0, style.ButtonSpacing);
            if (!float.IsInfinity(width) && ((count * (style.ButtonWidth + spacing)) > width))
            {
                spacing = 0;
            }

            float buttonWidth = style.ButtonWidth + spacing;
            float buttonHeight = style.ButtonHeight;
            foreach (var buttons in new[] { this._primaryButtons.Buttons, this._secondaryButtons.Buttons })
            {
                foreach (var button in buttons)
                {
                    if ((button.Width != buttonWidth) || (button.Height != buttonHeight))
                    {
                        button.Width = buttonWidth;
                        button.Height = buttonHeight;
                    }
                }
            }
        }

        internal DockPanel ButtonsLayout
        {
            get
            {
                return this._buttonsLayout;
            }
        }

        internal virtual void SetCommands(
            CommandBarCommandCollection commands)
        {
            if (null == commands)
            {
                if ((this._primaryButtons.Count == 0) && (this._secondaryButtons.Count == 0))
                {
                    return;
                }

                throw new NotSupportedException();
            }

            if ((this._primaryButtons.Count != 0) || (this._secondaryButtons.Count != 0))
            {
                throw new NotImplementedException();
            }

            foreach (var command in commands.PrimaryCommands)
            {
                this._primaryButtons.Add(new CommandBarButton()
                {
                    Text = command.Text,
                    Icon = command.Icon,
                    Command = command.Command,
                });
            }

            foreach (var command in commands.SecondaryCommands)
            {
                this._secondaryButtons.Add(new CommandBarButton()
                {
                    Text = command.Text,
                    Icon = command.Icon,
                    Command = command.Command,
                });
            }
        }

        private object _commandParameter;

        /// <summary>
        /// The value the bar's buttons pass to their commands, such as the list
        /// item the bar acts on.
        /// </summary>
        public object CommandParameter
        {
            get
            {
                return this._commandParameter;
            }

            set
            {
                this._commandParameter = value;

                foreach (var child in this._buttonsLayout.Children)
                {
                    if (child is CommandBarButton button)
                    {
                        button.CommandParameter = value;
                    }
                }
            }
        }
    }
}
