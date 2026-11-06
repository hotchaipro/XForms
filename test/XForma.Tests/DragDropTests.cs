using System.Collections.Generic;
using XForma.Input;
using XForma.Platform;
using Xunit;

namespace XForma.Tests
{
    // A drag's life, as a platform reports it or from the keyboard
    [Collection("Drag")]
    public class DragDropTests
    {
        private sealed class Box : View
        {
            public Box(
                params View[] children)
            {
                foreach (var child in children)
                {
                    this.AddVisualChild(child);
                    child.Parent = this;
                }
            }
        }

        private sealed class Target
        {
            public readonly Box View = new Box() { AllowDrop = true };
            public readonly List<string> Events = new List<string>();
            public DragOperation Accept = DragOperation.Copy;
            public DataPackage Dropped;

            public Target()
            {
                this.View.DragEnter += (sender, e) =>
                {
                    this.Events.Add("enter");
                    e.AcceptedOperation = this.Accept;
                };
                this.View.DragOver += (sender, e) => this.Events.Add("over");
                this.View.DragLeave += (sender, e) => this.Events.Add("leave");
                this.View.Drop += (sender, e) =>
                {
                    this.Events.Add("drop");
                    this.Dropped = e.Data;
                };
            }
        }

        private static Box Source(
            List<DragOperation> ended = null,
            bool cancel = false)
        {
            var source = new Box() { CanDrag = true };
            source.DragStarting += (sender, e) =>
            {
                e.Data.Text = "dragged";
                e.Cancel = cancel;
            };
            if (null != ended)
            {
                source.DropCompleted += (sender, e) => ended.Add(e.Operation);
            }

            return source;
        }

        private static void Press(
            View view,
            Key key,
            KeyModifiers modifiers = KeyModifiers.None)
        {
            Assert.True(DragDropSession.HandleKey(view, new KeyEventArgs(key, modifiers)));
        }

        [Fact]
        public void ADragStartsFromTheNearestViewThatCanBeDragged()
        {
            var inner = new Box();
            var source = Source();
            source.AddVisualChildForTest(inner);

            var data = DragDropSession.Begin(inner);

            Assert.Equal("dragged", data.Text);
            DragDropSession.End(DragOperation.None);
        }

        [Fact]
        public void ACanceledDragDoesNotStart()
        {
            Assert.Null(DragDropSession.Begin(Source(cancel: true)));
        }

        [Fact]
        public void NothingToDragStartsNothing()
        {
            Assert.Null(DragDropSession.Begin(new Box()));
        }

        [Fact]
        public void MovingOverATargetEntersItOnce()
        {
            var target = new Target();
            DragDropSession.Begin(Source());

            Assert.Equal(DragOperation.Copy, DragDropSession.Update(target.View, default, null));
            DragDropSession.Update(target.View, default, null);

            Assert.Equal(new[] { "enter", "over", "over" }, target.Events);
            DragDropSession.End(DragOperation.None);
        }

        [Fact]
        public void LeavingATargetTellsIt()
        {
            var target = new Target();
            DragDropSession.Begin(Source());
            DragDropSession.Update(target.View, default, null);

            Assert.Equal(DragOperation.None, DragDropSession.Update(null, default, null));

            Assert.Equal("leave", target.Events[target.Events.Count - 1]);
            DragDropSession.End(DragOperation.None);
        }

        [Fact]
        public void ADragOverAViewInsideATargetIsOverTheTarget()
        {
            var inner = new Box();
            var target = new Target();
            target.View.AddVisualChildForTest(inner);
            DragDropSession.Begin(Source());

            Assert.Equal(DragOperation.Copy, DragDropSession.Update(inner, default, null));

            DragDropSession.End(DragOperation.None);
        }

        [Fact]
        public void ADropGivesTheTargetTheSourcesDataAndTheSourceTheResult()
        {
            var ended = new List<DragOperation>();
            var target = new Target();
            var data = DragDropSession.Begin(Source(ended));

            var operation = DragDropSession.Drop(target.View, default, null);
            DragDropSession.End(operation);

            Assert.Same(data, target.Dropped);
            Assert.Equal(new[] { DragOperation.Copy }, ended);
        }

        [Fact]
        public void ATargetThatRefusesGetsNoDrop()
        {
            var target = new Target() { Accept = DragOperation.None };

            var operation = DragDropSession.Drop(target.View, default, new DataPackage() { Text = "from another app" });

            Assert.Equal(DragOperation.None, operation);
            Assert.DoesNotContain("drop", target.Events);
        }

        [Fact]
        public void ADragFromAnotherAppBringsItsData()
        {
            var target = new Target();
            var external = new DataPackage() { Text = "from another app" };

            DragDropSession.Drop(target.View, default, external);

            Assert.Same(external, target.Dropped);
        }

        [Fact]
        public void TheEndOfADragIsHeardOnce()
        {
            var ended = new List<DragOperation>();
            DragDropSession.Begin(Source(ended));

            DragDropSession.End(DragOperation.None);
            DragDropSession.End(DragOperation.None);

            Assert.Single(ended);
        }

        [Fact]
        public void TheKeyboardDropsOnTheTargetItMovesTo()
        {
            var ended = new List<DragOperation>();
            var source = Source(ended);
            var first = new Target();
            var second = new Target();
            var root = new Box(source, first.View, second.View);

            Press(source, Key.Space, KeyModifiers.Control);
            Assert.Contains("enter", first.Events);

            Press(source, Key.Tab);
            Assert.Contains("leave", first.Events);
            Assert.Contains("enter", second.Events);

            Press(source, Key.Enter);
            Assert.NotNull(second.Dropped);
            Assert.Null(first.Dropped);
            Assert.Equal(new[] { DragOperation.Copy }, ended);
            Assert.False(DragDropSession.IsKeyboardDrag);
            Assert.NotNull(root);
        }

        [Fact]
        public void EscapePutsAKeyboardDragBack()
        {
            var ended = new List<DragOperation>();
            var source = Source(ended);
            var target = new Target();
            _ = new Box(source, target.View);

            Press(source, Key.Space, KeyModifiers.Control);
            Press(source, Key.Escape);

            Assert.Null(target.Dropped);
            Assert.Equal(new[] { DragOperation.None }, ended);
            Assert.Contains("leave", target.Events);
        }

        [Fact]
        public void TheKeyboardSkipsTargetsThatRefuse()
        {
            var source = Source();
            var refuses = new Target() { Accept = DragOperation.None };
            var accepts = new Target();
            _ = new Box(source, refuses.View, accepts.View);

            Press(source, Key.Space, KeyModifiers.Control);
            Press(source, Key.Enter);

            Assert.NotNull(accepts.Dropped);
        }

        [Fact]
        public void OtherKeysAreLeftAloneWithoutADrag()
        {
            Assert.False(DragDropSession.HandleKey(Source(), new KeyEventArgs(Key.Tab, KeyModifiers.None)));
            Assert.False(DragDropSession.HandleKey(new Box(), new KeyEventArgs(Key.Space, KeyModifiers.Control)));
        }
    }

    internal static class ViewTestExtensions
    {
        public static void AddVisualChildForTest(
            this View parent,
            View child)
        {
            typeof(View).GetMethod("AddVisualChild", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .Invoke(parent, new object[] { child });
            child.Parent = parent;
        }
    }
}
