using System;
using Xunit;

namespace XForms.Tests
{
    // How a rebuilt page's or control's views take over the user state of the
    // views they replace (View.CarryUserState)
    public class UserStateTests
    {
        // A view with children and a state, which records the state it's given
        private class Box : View
        {
            public Box(
                object state = null,
                params View[] children)
            {
                this.State = state;
                foreach (var child in children)
                {
                    this.AddVisualChild(child);
                }
            }

            public object State
            {
                get;
                set;
            }

            public object Received
            {
                get;
                private set;
            }

            protected override object GetUserState()
            {
                return this.State;
            }

            protected override void SetUserState(
                object state)
            {
                this.Received = state;
            }

            public static void Carry(
                View oldViews,
                View newViews)
            {
                RestoreUserState(SaveUserState(oldViews), newViews);
            }
        }

        // Boxes of other types
        private sealed class Field : Box
        {
            public Field(
                object state = null)
                : base(state)
            {
            }
        }

        private sealed class Label : Box
        {
            public Label(
                object state = null)
                : base(state)
            {
            }
        }

        [Fact]
        public void TheSameViewsTakeTheStatesByPlace()
        {
            var oldA = new Field("a");
            var oldB = new Field("b");
            var newA = new Field();
            var newB = new Field();

            Box.Carry(new Box(null, oldA, oldB), new Box(null, newA, newB));

            Assert.Equal("a", newA.Received);
            Assert.Equal("b", newB.Received);
        }

        [Fact]
        public void TheRootTakesItsOwnState()
        {
            var newRoot = new Box();

            Box.Carry(new Box("root"), newRoot);

            Assert.Equal("root", newRoot.Received);
        }

        [Fact]
        public void AViewAddedAtTheStartDoesntShiftTheOthers()
        {
            var oldField = new Field("typed");
            var newField = new Field();

            Box.Carry(
                new Box(null, new Label(), oldField),
                new Box(null, new Label(), new Label(), newField));

            Assert.Equal("typed", newField.Received);
        }

        [Fact]
        public void AViewRemovedDoesntShiftTheOthers()
        {
            var newFirst = new Field();
            var newSecond = new Field();

            Box.Carry(
                new Box(null, new Label(), new Field("first"), new Field("second")),
                new Box(null, newFirst, newSecond));

            Assert.Equal("first", newFirst.Received);
            Assert.Equal("second", newSecond.Received);
        }

        [Fact]
        public void AViewAddedInTheMiddleIsMatchedByType()
        {
            var newBefore = new Field();
            var newAfter = new Field();

            Box.Carry(
                new Box(null, new Field("before"), new Label(), new Field("after")),
                new Box(null, newBefore, new Label(), new Box(), new Label(), newAfter));

            Assert.Equal("before", newBefore.Received);
            Assert.Equal("after", newAfter.Received);
        }

        [Fact]
        public void AViewOfAnotherTypeTakesNothing()
        {
            var newLabel = new Label();

            Box.Carry(new Box(null, new Field("typed")), new Box(null, newLabel));

            Assert.Null(newLabel.Received);
        }

        [Fact]
        public void ANamedViewTakesItsStateWhereverItMoved()
        {
            var oldNamed = new Field("typed") { Name = "notes" };
            var newNamed = new Field() { Name = "notes" };
            var newUnnamed = new Field();

            Box.Carry(
                new Box(null, new Box(null, oldNamed), new Box()),
                new Box(null, new Box(null, newUnnamed), new Box(null, newNamed)));

            Assert.Equal("typed", newNamed.Received);
            Assert.Null(newUnnamed.Received);
        }

        [Fact]
        public void NestedViewsTakeTheirStates()
        {
            var newInner = new Field();

            Box.Carry(
                new Box(null, new Box(null, new Label(), new Field("inner"))),
                new Box(null, new Box(null, new Label(), newInner)));

            Assert.Equal("inner", newInner.Received);
        }

        [Fact]
        public void NoStateIsGivenWhenThereIsNone()
        {
            var newField = new Field();
            newField.State = "own";

            Box.Carry(new Box(null, new Field()), new Box(null, newField));

            Assert.Null(newField.Received);
        }
    }
}
