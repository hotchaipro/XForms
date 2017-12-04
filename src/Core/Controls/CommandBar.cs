using System;
using System.Collections.Generic;
using XForms.Layouts;

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
        private DockLayout _buttonsLayout;
        private CommandBarButtonCollection _primaryButtons;
        private CommandBarButtonCollection _secondaryButtons;

        public CommandBar()
        {
            this._buttonsLayout = new DockLayout()
            {
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.Fill,
            };

            this._primaryButtons = new CommandBarButtonCollection(this, DockRegion.Left);
            this._secondaryButtons = new CommandBarButtonCollection(this, DockRegion.Right);

            this.HorizontalAlignment = LayoutAlignment.Fill;
            this.VerticalAlignment = LayoutAlignment.Start;

            this.Content = this._buttonsLayout;
        }

        internal DockLayout ButtonsLayout
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

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            // Propagate binding context to child buttons
            foreach (var child in this._buttonsLayout.Children)
            {
                child.BindingContext = this.BindingContext;
            }
        }
    }
}
