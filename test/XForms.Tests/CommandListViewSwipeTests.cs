using System;
using System.Collections.Generic;
using System.Windows.Input;
using XForms.Controls;
using XForms.Input;
using Xunit;

namespace XForms.Tests
{
    // A committed swipe acts on the item it was let go on, whatever its row
    // shows by the time the animation ends
    public class CommandListViewSwipeTests
    {
        private sealed class Recording : ICommand
        {
            public List<object> Items { get; } = new List<object>();

            public event EventHandler CanExecuteChanged
            {
                add
                {
                }

                remove
                {
                }
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter) => this.Items.Add(parameter);
        }

        private static (CommandListView List, CommandListViewItemContainer Row, Recording Command) Swiped(
            object item)
        {
            var command = new Recording();
            var list = new CommandListView();
            list.SwipeCommands.PrimaryCommands.Add(new UICommand() { Command = command, Text = "done" });

            var row = (CommandListViewItemContainer)list.CreateItemContainer();
            row.Bind(list, item);
            ((ISwipeGestureDelegate)row).OnSwipeBegan(1);
            return (list, row, command);
        }

        [Fact]
        public void TheCommandRunsOnTheSwipedItem()
        {
            var (_, row, command) = Swiped("first");

            row.CommitSwipe()();

            Assert.Equal(new object[] { "first" }, command.Items);
        }

        [Fact]
        public void ARowReboundBeforeTheCommitStillActsOnTheSwipedItem()
        {
            var (list, row, command) = Swiped("first");
            var commit = row.CommitSwipe();

            row.Recycle();
            row.Bind(list, "second");
            commit();

            Assert.Equal(new object[] { "first" }, command.Items);
        }

        [Fact]
        public void ANewSwipeBeforeTheCommitKeepsTheFirstCommand()
        {
            var (_, row, command) = Swiped("first");
            var commit = row.CommitSwipe();

            ((ISwipeGestureDelegate)row).OnSwipeBegan(-1);
            commit();

            Assert.Equal(new object[] { "first" }, command.Items);
        }
    }
}
