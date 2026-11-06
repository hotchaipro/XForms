using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using XForma.Controls;
using Xunit;

namespace XForma.Tests
{
    // Failures reach the app, views don't outlive their use, and paths that
    // used to throw do what they say
    public class RobustnessTests
    {
        [Fact]
        public void AReportedExceptionIsLogged()
        {
            var logged = new List<LogEventArgs>();
            EventHandler<LogEventArgs> handler = (sender, e) => logged.Add(e);
            FrameworkLog.MessageLogged += handler;
            try
            {
                var exception = new InvalidOperationException("broken");

                bool handled = Application.ReportException(exception);

                Assert.False(handled);
                var message = Assert.Single(logged, (e) => ReferenceEquals(exception, e.Exception));
                Assert.Equal(LogLevel.Error, message.Level);
            }
            finally
            {
                FrameworkLog.MessageLogged -= handler;
            }
        }

        [Fact]
        public void AnExceptionHeardTwiceIsReportedOnce()
        {
            int count = 0;
            var exception = new InvalidOperationException("broken");
            EventHandler<LogEventArgs> handler = (sender, e) => count += ReferenceEquals(exception, e.Exception) ? 1 : 0;
            FrameworkLog.MessageLogged += handler;
            try
            {
                Application.ReportException(exception);
                Application.ReportException(exception);

                Assert.Equal(1, count);
            }
            finally
            {
                FrameworkLog.MessageLogged -= handler;
            }
        }

        [Fact]
        public void ABrokenLogListenerDoesNotThrow()
        {
            EventHandler<LogEventArgs> handler = (sender, e) => throw new InvalidOperationException("listener");
            FrameworkLog.MessageLogged += handler;
            try
            {
                FrameworkLog.Write(LogLevel.Warning, "message");
            }
            finally
            {
                FrameworkLog.MessageLogged -= handler;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference MenuButtonFor(
            MenuItem item)
        {
            return new WeakReference(new MenuButton(new MenuBar(), item));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RadioButtonFor(
            RadioButtonItem item)
        {
            return new WeakReference(new RadioButton(new RadioButtons(), item));
        }

        private static void Collect()
        {
            for (int i = 0; i < 3; i += 1)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [Fact]
        public void AMenuItemDoesNotKeepItsButtonAlive()
        {
            var item = new MenuItem();
            var button = MenuButtonFor(item);

            Collect();

            Assert.False(button.IsAlive);

            // The next change detaches, and nothing's left to fail
            item.IsVisible = !item.IsVisible;
        }

        [Fact]
        public void ARadioButtonItemDoesNotKeepItsButtonAlive()
        {
            var item = new RadioButtonItem();
            var button = RadioButtonFor(item);

            Collect();

            Assert.False(button.IsAlive);
            item.Text = "changed";
        }

        [Fact]
        public void AMenuButtonStillFollowsItsItem()
        {
            var item = new MenuItem() { IsVisible = true };
            var button = new MenuButton(new MenuBar(), item);

            item.IsVisible = false;

            Assert.False(button.IsVisible);
            GC.KeepAlive(button);
        }

        [Fact]
        public void ACommandBarsCommandsCanBeReplaced()
        {
            var bar = new CommandBar();
            var first = new CommandBarCommandCollection();
            first.PrimaryCommands.Add(new UICommand() { Text = "one" });
            first.SecondaryCommands.Add(new UICommand() { Text = "two" });
            var second = new CommandBarCommandCollection();
            second.PrimaryCommands.Add(new UICommand() { Text = "three" });

            bar.CommandParameter = "item";
            bar.SetCommands(first);
            bar.SetCommands(second);

            var button = Assert.Single(bar.PrimaryButtons);
            Assert.Equal("three", button.Text);
            Assert.Equal("item", button.CommandParameter);
            Assert.Empty(bar.SecondaryButtons);
            Assert.Single(bar.ButtonsLayout.Children);
        }

        [Fact]
        public void ACommandBarsCommandsCanBeRemoved()
        {
            var bar = new CommandBar();
            var commands = new CommandBarCommandCollection();
            commands.PrimaryCommands.Add(new UICommand() { Text = "one" });

            bar.SetCommands(commands);
            bar.SetCommands(null);

            Assert.Empty(bar.PrimaryButtons);
            Assert.Empty(bar.ButtonsLayout.Children);
        }

        [Fact]
        public void AFailingRefreshHandlerEndsTheRefresh()
        {
            var container = new RefreshContainer();
            container.RefreshRequested += (sender, e) => throw new InvalidOperationException("broken");

            container.RequestRefresh();

            Assert.False(container.IsRefreshing);
        }
    }
}
