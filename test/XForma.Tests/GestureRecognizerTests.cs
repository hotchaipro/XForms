using System;
using System.Collections.Generic;
using XForma.Input;
using Xunit;

namespace XForma.Tests
{
    // How recognizers follow a pointer, share one on a view, and take a drag
    // from a view inside theirs. Events go in as a platform delivers them.
    public class GestureRecognizerTests
    {
        private const int Finger = 1;

        private sealed class Box : View
        {
            public Box(
                params View[] children)
            {
                foreach (var child in children)
                {
                    this.AddVisualChild(child);
                    // As attaching does
                    child.Parent = this;
                }
            }
        }

        // What a delegate was told, in order
        private sealed class Log : ITapGestureDelegate, ISwipeGestureDelegate, IPanGestureDelegate, IPinchGestureDelegate
        {
            public List<string> Events { get; } = new List<string>();

            public void OnTapBegan() => this.Events.Add("tap began");

            public void OnTapEnded() => this.Events.Add("tap ended");

            public void OnTapped() => this.Events.Add("tapped");

            public bool OnSwipeBegan(int direction)
            {
                this.Events.Add($"swipe began {direction}");
                return true;
            }

            public bool OnSwipeMoved(int direction, float dx)
            {
                this.Events.Add($"swipe moved {dx}");
                return true;
            }

            public bool OnSwipeEnded(int direction, float dx)
            {
                this.Events.Add($"swipe ended {dx}");
                return true;
            }

            public bool OnSwipeCanceled(int direction)
            {
                this.Events.Add("swipe canceled");
                return true;
            }

            public void OnPanStarted() => this.Events.Add("pan started");

            public void OnPanChanged(Point translation) => this.Events.Add($"pan changed {translation.X},{translation.Y}");

            public void OnPanCompleted(Point translation) => this.Events.Add($"pan completed {translation.X},{translation.Y}");

            public void OnPanCanceled() => this.Events.Add("pan canceled");

            public void OnPinchStarted() => this.Events.Add("pinch started");

            public void OnPinchChanged(float scale, Point center) => this.Events.Add($"pinch changed {scale}");

            public void OnPinchCompleted(float scale) => this.Events.Add($"pinch completed {scale}");

            public void OnTouchBegan()
            {
            }

            public void OnTouchEnded()
            {
            }

            public void OnTouchCanceled()
            {
            }
        }

        // Follows a pointer and does what the test says, keeping what it saw
        private sealed class Probe : GestureRecognizer
        {
            public Probe(
                bool isDragGesture = false)
            {
                this.IsDrag = isDragGesture;
            }

            private bool IsDrag { get; }

            public List<PointerInputState> Seen { get; } = new List<PointerInputState>();

            public Action<Probe, PointerEventArgs> OnInput { get; set; }

            protected override bool IsDragGesture => this.IsDrag;

            public void CallRecognize() => this.Recognize();

            public void CallFail() => this.Fail();

            protected override void OnPointerInput(
                PointerEventArgs e)
            {
                this.Seen.Add(e.State);
                this.OnInput?.Invoke(this, e);
            }
        }

        private static PointerEventArgs Event(
            PointerInputState state,
            float x,
            float y = 0,
            int pointer = Finger,
            PointerDeviceType device = PointerDeviceType.Touch)
        {
            return new PointerEventArgs(pointer, device, new Point(x, y), state, state != PointerInputState.Ended, KeyModifiers.None);
        }

        private static bool Press(View view, float x = 0, float y = 0, int pointer = Finger, PointerDeviceType device = PointerDeviceType.Touch)
            => view.DispatchPointerEvent(Event(PointerInputState.Began, x, y, pointer, device));

        private static bool Move(View view, float x, float y = 0, int pointer = Finger, PointerDeviceType device = PointerDeviceType.Touch)
            => view.DispatchPointerEvent(Event(PointerInputState.Moved, x, y, pointer, device));

        private static bool Lift(View view, float x, float y = 0, int pointer = Finger, PointerDeviceType device = PointerDeviceType.Touch)
            => view.DispatchPointerEvent(Event(PointerInputState.Ended, x, y, pointer, device));

        private static bool Cancel(View view, float x = 0, float y = 0, int pointer = Finger)
            => view.DispatchPointerEvent(Event(PointerInputState.Canceled, x, y, pointer));

        // A view with a swipe and a tap, added in either order
        private static Box Row(
            Log log,
            bool isTapFirst = false,
            params View[] children)
        {
            var row = new Box(children);
            if (isTapFirst)
            {
                row.GestureRecognizers.Add(new TapGestureRecognizer(log));
                row.GestureRecognizers.Add(new SwipeGestureRecognizer(log));
            }
            else
            {
                row.GestureRecognizers.Add(new SwipeGestureRecognizer(log));
                row.GestureRecognizers.Add(new TapGestureRecognizer(log));
            }

            return row;
        }

        #region The lifecycle

        [Fact]
        public void ARecognizerIsReadyUntilPressedAndReadyAgainOnceReleased()
        {
            var probe = new Probe() { OnInput = (p, e) => { if (e.State == PointerInputState.Moved) { p.CallRecognize(); } } };
            var view = new Box();
            view.GestureRecognizers.Add(probe);

            Assert.Equal(GestureState.Ready, probe.State);
            Press(view);
            Assert.Equal(GestureState.Possible, probe.State);
            Move(view, 20);
            Assert.Equal(GestureState.Recognized, probe.State);
            Lift(view, 20);
            Assert.Equal(GestureState.Ready, probe.State);
        }

        [Fact]
        public void AFailedRecognizerGetsNoMoreInputUntilTheNextPress()
        {
            var probe = new Probe() { OnInput = (p, e) => { if (e.State == PointerInputState.Moved) { p.CallFail(); } } };
            var view = new Box();
            view.GestureRecognizers.Add(probe);

            Press(view);
            Move(view, 5);
            Assert.Equal(GestureState.Failed, probe.State);
            Move(view, 6);
            Lift(view, 6);
            Assert.Equal(GestureState.Ready, probe.State);
            Assert.Equal(new[] { PointerInputState.Began, PointerInputState.Moved }, probe.Seen);

            Press(view);
            Assert.Equal(GestureState.Possible, probe.State);
            Assert.Equal(PointerInputState.Began, probe.Seen[^1]);
        }

        [Fact]
        public void APressIsClaimedUnlessTheRecognizerFailsOnIt()
        {
            var taking = new Box();
            taking.GestureRecognizers.Add(new Probe());
            Assert.True(Press(taking, pointer: 21));

            var declining = new Box();
            declining.GestureRecognizers.Add(new Probe() { OnInput = (p, e) => p.CallFail() });
            Assert.False(Press(declining, pointer: 22));
        }

        [Fact]
        public void MovesAreClaimedOnlyOnceRecognized()
        {
            var probe = new Probe() { OnInput = (p, e) => { if ((e.State == PointerInputState.Moved) && (e.Position.X >= 10)) { p.CallRecognize(); } } };
            var view = new Box();
            view.GestureRecognizers.Add(probe);

            Press(view);
            Assert.False(Move(view, 5));
            Assert.True(Move(view, 10));
            Assert.True(Move(view, 15));
            Lift(view, 15);
        }

        [Fact]
        public void ASecondFingerIsIgnored()
        {
            var probe = new Probe();
            var view = new Box();
            view.GestureRecognizers.Add(probe);

            Press(view);
            Press(view, pointer: 2);
            Move(view, 5, pointer: 2);
            Lift(view, 5, pointer: 2);
            Assert.Equal(GestureState.Possible, probe.State);
            Assert.Equal(new[] { PointerInputState.Began }, probe.Seen);

            Lift(view, 0);
            Assert.Equal(GestureState.Ready, probe.State);
        }

        [Fact]
        public void APressForAPointerThatNeverEndedStartsOver()
        {
            var probe = new Probe() { OnInput = (p, e) => { if (e.State == PointerInputState.Moved) { p.CallFail(); } } };
            var view = new Box();
            view.GestureRecognizers.Add(probe);

            Press(view);
            Move(view, 5);
            Assert.Equal(GestureState.Failed, probe.State);

            Press(view);
            Assert.Equal(GestureState.Possible, probe.State);
        }

        [Fact]
        public void RemovingARecognizerFromItsViewEndsItsGesture()
        {
            var probe = new Probe();
            var view = new Box();
            view.GestureRecognizers.Add(probe);

            Press(view);
            view.GestureRecognizers.Remove(probe);
            Assert.Equal(GestureState.Ready, probe.State);

            view.GestureRecognizers.Add(probe);
            Press(view, pointer: 2);
            Assert.Equal(GestureState.Possible, probe.State);
        }

        #endregion

        #region Recognizers on one view

        [Fact]
        public void RecognizingCancelsTheOtherRecognizers()
        {
            var first = new Probe() { OnInput = (p, e) => { if (e.State == PointerInputState.Moved) { p.CallRecognize(); } } };
            var second = new Probe();
            var view = new Box();
            view.GestureRecognizers.Add(first);
            view.GestureRecognizers.Add(second);

            Press(view);
            Move(view, 5);

            Assert.Equal(GestureState.Recognized, first.State);
            Assert.Equal(GestureState.Ready, second.State);
            Assert.Equal(new[] { PointerInputState.Began, PointerInputState.Canceled }, second.Seen);

            // It's out until the next press
            Move(view, 6);
            Lift(view, 6);
            Assert.Equal(new[] { PointerInputState.Began, PointerInputState.Canceled }, second.Seen);
        }

        [Fact]
        public void ARecognizerAlreadyFinishedWithThePointerIsNotCanceled()
        {
            var failed = new Probe() { OnInput = (p, e) => p.CallFail() };
            var recognizing = new Probe() { OnInput = (p, e) => p.CallRecognize() };
            var view = new Box();
            view.GestureRecognizers.Add(failed);
            view.GestureRecognizers.Add(recognizing);

            Press(view);

            Assert.Equal(new[] { PointerInputState.Began }, failed.Seen);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void APressThatStaysPutIsATapAndNotASwipe(
            bool isTapFirst)
        {
            var log = new Log();
            var row = Row(log, isTapFirst);

            Press(row);
            Move(row, 4);
            Lift(row, 4);

            Assert.Equal(new[] { "tap began", "tapped", "tap ended" }, log.Events);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ASwipeCancelsTheTapBetweenTheTwoSlops(
            bool isTapFirst)
        {
            var log = new Log();
            var row = Row(log, isTapFirst);

            Press(row);
            Move(row, 12);
            Lift(row, 12);

            Assert.DoesNotContain("tapped", log.Events);
            Assert.Equal(new[] { "swipe began 1", "swipe moved 12", "swipe moved 12", "swipe ended 12" }, log.Events);
        }

        // The two used to both run: the swipe commits at 10 points, and a tap
        // accepts up to 15
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AFlickThatLiftsBeforeAnyMoveIsASwipeAndNotATap(
            bool isTapFirst)
        {
            var log = new Log();
            var row = Row(log, isTapFirst);

            Press(row);
            Lift(row, 12);

            Assert.DoesNotContain("tapped", log.Events);
            Assert.Contains("swipe ended 12", log.Events);
        }

        [Fact]
        public void AVerticalDragFailsTheSwipeAndIsNotClaimed()
        {
            var log = new Log();
            var row = Row(log);

            Press(row);
            Assert.False(Move(row, 1, 12));
            Assert.False(Move(row, 1, 40));

            Assert.DoesNotContain(log.Events, e => e.StartsWith("swipe"));

            // Past its slop, the tap is out too, and no press showed
            Assert.Empty(log.Events);
        }

        [Fact]
        public void AScrollingParentTakingThePointerEndsThePress()
        {
            var log = new Log();
            var row = Row(log);

            Press(row);
            Move(row, 0, 5);
            Cancel(row, 0, 5);

            Assert.Empty(log.Events);
        }

        [Fact]
        public void ASwipeCanceledPartWayIsToldSo()
        {
            var log = new Log();
            var row = Row(log);

            Press(row);
            Move(row, 20);
            Cancel(row, 20);

            Assert.Equal("swipe canceled", log.Events[^1]);
        }

        #endregion

        #region Swipes

        [Fact]
        public void ASwipeReportsItsDistanceFromThePress()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new SwipeGestureRecognizer(log));

            Press(view, 100);
            Assert.False(Move(view, 95));
            Assert.True(Move(view, 80));
            Assert.True(Move(view, 70));

            Assert.Equal(new[] { "swipe began -1", "swipe moved 20", "swipe moved 30" }, log.Events);
        }

        [Fact]
        public void ASwipesEventReportsLikeItsDelegate()
        {
            var phases = new List<SwipePhase>();
            var swipe = new SwipeGestureRecognizer();
            swipe.Swiping += (sender, e) => phases.Add(e.Phase);
            var view = new Box();
            view.GestureRecognizers.Add(swipe);

            Press(view);
            Move(view, 20);
            Lift(view, 30);

            Assert.Equal(new[] { SwipePhase.Began, SwipePhase.Moved, SwipePhase.Moved, SwipePhase.Ended }, phases);
        }

        [Fact]
        public void AMouseStartsADragSoonerThanATouch()
        {
            var touchLog = new Log();
            var touch = new Box();
            touch.GestureRecognizers.Add(new SwipeGestureRecognizer(touchLog));
            Press(touch);
            Assert.False(Move(touch, 7));

            var mouseLog = new Log();
            var mouse = new Box();
            mouse.GestureRecognizers.Add(new SwipeGestureRecognizer(mouseLog));
            Press(mouse, pointer: 2, device: PointerDeviceType.Mouse);
            Assert.True(Move(mouse, 7, pointer: 2, device: PointerDeviceType.Mouse));
        }

        #endregion

        #region Pans

        [Fact]
        public void APanStartsPastTheDragSlopAndReportsFromThere()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new PanGestureRecognizer(log));

            Press(view);
            Assert.False(Move(view, 5));
            Assert.True(Move(view, 12));
            Assert.True(Move(view, 20, 3));
            Lift(view, 22, 3);

            Assert.Equal(new[] { "pan started", "pan changed 8,3", "pan completed 10,3" }, log.Events);
        }

        [Fact]
        public void ShouldBeginIsAskedOnceWithHowFarThePointerMoved()
        {
            var asked = new List<Point>();
            var log = new Log();
            var pan = new PanGestureRecognizer(log) { ShouldBegin = translation => { asked.Add(translation); return true; } };
            var view = new Box();
            view.GestureRecognizers.Add(pan);

            Press(view, 100, 100);
            Move(view, 103, 100);
            Move(view, 112, 101);
            Move(view, 130, 101);

            Assert.Equal(new[] { new Point(12, 1) }, asked);
            Assert.Equal(GestureState.Recognized, pan.State);
        }

        [Fact]
        public void ShouldBeginDecliningLeavesThePointerToOthers()
        {
            var log = new Log();
            var pan = new PanGestureRecognizer(log) { ShouldBegin = translation => Math.Abs(translation.X) >= Math.Abs(translation.Y) };
            var view = new Box();
            view.GestureRecognizers.Add(pan);

            Press(view);
            Assert.False(Move(view, 1, 12));
            Assert.Equal(GestureState.Failed, pan.State);

            // Going sideways later doesn't start it
            Assert.False(Move(view, 100, 12));
            Lift(view, 100, 12);

            Assert.Empty(log.Events);
        }

        [Fact]
        public void APanReportsItsVelocityAsItLifts()
        {
            var pan = new PanGestureRecognizer(new Log());
            var view = new Box();
            view.GestureRecognizers.Add(pan);

            Press(view);
            Move(view, 20);
            Move(view, 40);
            Lift(view, 60);

            Assert.True(pan.Velocity.X > 0);
            Assert.Equal(0, pan.Velocity.Y);
        }

        #endregion

        #region Pinches

        [Fact]
        public void TwoFingersPinchAndOneIsLeftAlone()
        {
            var log = new Log();
            var pinch = new PinchGestureRecognizer(log);
            var view = new Box();
            view.GestureRecognizers.Add(pinch);

            Press(view, 0, pointer: 1);
            Assert.False(Move(view, 5, pointer: 1));
            Assert.Equal(GestureState.Possible, pinch.State);

            Press(view, 100, pointer: 2);
            Assert.Equal(GestureState.Recognized, pinch.State);
            Assert.True(Move(view, 200, pointer: 2));

            Lift(view, 200, pointer: 2);
            Lift(view, 5, pointer: 1);

            Assert.Equal(GestureState.Ready, pinch.State);
            Assert.Equal("pinch started", log.Events[0]);
            Assert.StartsWith("pinch completed", log.Events[^1]);
        }

        [Fact]
        public void ALeftoverFingerCanStartAnotherPinch()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new PinchGestureRecognizer(log));

            Press(view, 0, pointer: 1);
            Press(view, 100, pointer: 2);
            Lift(view, 100, pointer: 2);
            Press(view, 100, pointer: 3);

            Assert.Equal(2, log.Events.FindAll(e => e == "pinch started").Count);
        }

        [Fact]
        public void APinchCancelsAPanOnBothItsFingers()
        {
            var panLog = new Log();
            var pinchLog = new Log();
            var pan = new PanGestureRecognizer(panLog);
            var view = new Box();
            view.GestureRecognizers.Add(pan);
            view.GestureRecognizers.Add(new PinchGestureRecognizer(pinchLog));

            Press(view, 0, pointer: 1);
            Press(view, 100, pointer: 2);

            Assert.Equal(GestureState.Ready, pan.State);
            Assert.Contains("pinch started", pinchLog.Events);
        }

        #endregion

        #region Drags from a view inside

        [Fact]
        public void ADragOnAButtonGoesToItsAncestorsSwipe()
        {
            var buttonLog = new Log();
            var button = new Box();
            button.GestureRecognizers.Add(new TapGestureRecognizer(buttonLog));
            var rowLog = new Log();
            Row(rowLog, false, button);

            Press(button);
            Assert.False(Move(button, 5));
            Assert.True(Move(button, 20));
            Move(button, 30);
            Lift(button, 30);

            Assert.Empty(buttonLog.Events);
            Assert.Contains("swipe ended 30", rowLog.Events);
        }

        // An ancestor's tap isn't told of a press that never happened on it
        [Fact]
        public void AnAncestorsTapIsNotOfferedTheDrag()
        {
            var button = new Box();
            button.GestureRecognizers.Add(new TapGestureRecognizer(new Log()));
            var rowLog = new Log();
            Row(rowLog, false, button);

            Press(button);
            Move(button, 20);
            Lift(button, 20);

            Assert.DoesNotContain("tap began", rowLog.Events);
            Assert.DoesNotContain("tapped", rowLog.Events);
        }

        [Fact]
        public void AnAncestorWithOnlyATapLeavesTheDragAlone()
        {
            var buttonLog = new Log();
            var button = new Box();
            button.GestureRecognizers.Add(new TapGestureRecognizer(buttonLog));
            var parentLog = new Log();
            var parent = new Box(button);
            parent.GestureRecognizers.Add(new TapGestureRecognizer(parentLog));

            Press(button);
            Assert.False(Move(button, 12));
            Lift(button, 12);

            // Still within the button's tap slop
            Assert.Contains("tapped", buttonLog.Events);
            Assert.Empty(parentLog.Events);
        }

        [Fact]
        public void AVerticalDragOnAButtonIsLeftForScrolling()
        {
            var button = new Box();
            button.GestureRecognizers.Add(new TapGestureRecognizer(new Log()));
            var rowLog = new Log();
            Row(rowLog, false, button);

            Press(button);
            Assert.False(Move(button, 0, 12));
            Assert.Empty(rowLog.Events);
            Cancel(button, 0, 12);
        }

        [Fact]
        public void AFlickOnAButtonThatLiftsBeforeAnyMoveSwipesItsAncestor()
        {
            var buttonLog = new Log();
            var button = new Box();
            button.GestureRecognizers.Add(new TapGestureRecognizer(buttonLog));
            var rowLog = new Log();
            Row(rowLog, false, button);

            Press(button);
            Lift(button, 12);

            Assert.DoesNotContain("tapped", buttonLog.Events);
            Assert.Contains("swipe ended 12", rowLog.Events);
        }

        [Fact]
        public void ADragPassesAnAncestorThatDeclinesItToTheNextOne()
        {
            var button = new Box();
            button.GestureRecognizers.Add(new TapGestureRecognizer(new Log()));
            var innerLog = new Log();
            var inner = new Box(button);
            inner.GestureRecognizers.Add(new PanGestureRecognizer(innerLog) { ShouldBegin = translation => false });
            var outerLog = new Log();
            var outer = new Box(inner);
            outer.GestureRecognizers.Add(new PanGestureRecognizer(outerLog));

            Press(button);
            Assert.True(Move(button, 20));

            Assert.Empty(innerLog.Events);
            Assert.Equal(new[] { "pan started" }, outerLog.Events);
        }

        #endregion

        #region Pressed looks

        private sealed class Scroller : View
        {
            public Scroller(
                View child)
            {
                this.AddVisualChild(child);
                child.Parent = this;
            }

            internal override bool ScrollsContent => true;
        }

        [Fact]
        public void APressShowsAtOnceWhenNothingCanDragIt()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new TapGestureRecognizer(log));

            Assert.False(view.IsDragPossible);
            Press(view);

            Assert.Equal(new[] { "tap began" }, log.Events);
            Lift(view, 0);
        }

        [Fact]
        public void AMousePressShowsAtOnceEvenInAScroller()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new TapGestureRecognizer(log));
            _ = new Scroller(view);

            Press(view, device: PointerDeviceType.Mouse);

            Assert.Equal(new[] { "tap began" }, log.Events);
            Lift(view, 0, device: PointerDeviceType.Mouse);
        }

        [Fact]
        public void ADragIsPossibleInAScrollerOrOverADragGesture()
        {
            var inScroller = new Box();
            _ = new Scroller(new Box(inScroller));
            Assert.True(inScroller.IsDragPossible);

            var inRow = new Box();
            Row(new Log(), false, inRow);
            Assert.True(inRow.IsDragPossible);

            Assert.True(Row(new Log()).IsDragPossible);
        }

        [Fact]
        public void AScrollTakingATouchInAScrollerShowsNoPress()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new TapGestureRecognizer(log));
            _ = new Scroller(view);

            Press(view);
            Move(view, 0, 8);
            Cancel(view, 0, 8);

            Assert.Empty(log.Events);
        }

        [Fact]
        public void AQuickTapInAScrollerStillShowsItsPressThenTaps()
        {
            var log = new Log();
            var view = new Box();
            view.GestureRecognizers.Add(new TapGestureRecognizer(log));
            _ = new Scroller(view);

            Press(view);
            Assert.Empty(log.Events);
            Lift(view, 0);

            Assert.Equal(new[] { "tap began", "tapped", "tap ended" }, log.Events);
        }

        [Fact]
        public void APressHeldPastTheDelayShowsThenEndsOnce()
        {
            var log = new Log();
            var tap = new TapGestureRecognizer(log);
            var view = new Box();
            view.GestureRecognizers.Add(tap);
            _ = new Scroller(view);

            Press(view);
            Assert.False(tap.IsPressed);

            tap.OnPressTime();
            Assert.True(tap.IsPressed);
            Assert.Equal(new[] { "tap began" }, log.Events);

            Cancel(view, 0, 8);
            Assert.False(tap.IsPressed);
            Assert.Equal(new[] { "tap began", "tap ended" }, log.Events);
        }

        [Fact]
        public void ThePressDelayEndingAfterTheGestureDoesNothing()
        {
            var log = new Log();
            var tap = new TapGestureRecognizer(log);
            var view = new Box();
            view.GestureRecognizers.Add(tap);
            _ = new Scroller(view);

            Press(view);
            Cancel(view, 0, 8);
            tap.OnPressTime();

            Assert.Empty(log.Events);
        }

        #endregion

        #region Velocity

        [Fact]
        public void VelocityIsTheRecentMovesOverTheirTime()
        {
            var tracker = new VelocityTracker();
            tracker.Add(new Point(0, 0), 0);
            tracker.Add(new Point(10, 5), 10);
            tracker.Add(new Point(20, 10), 20);

            Assert.Equal(1000, tracker.Velocity.X, 1);
            Assert.Equal(500, tracker.Velocity.Y, 1);
        }

        [Fact]
        public void VelocityOnlyLooksBackAShortWay()
        {
            var tracker = new VelocityTracker();
            tracker.Add(new Point(0, 0), 0);
            tracker.Add(new Point(500, 0), 40);
            tracker.Add(new Point(505, 0), 80);
            tracker.Add(new Point(510, 0), 120);
            tracker.Add(new Point(515, 0), 160);

            Assert.Equal(125, tracker.Velocity.X, 1);
        }

        [Fact]
        public void AFingerThatStoppedBeforeItLiftedHasNoVelocity()
        {
            var tracker = new VelocityTracker();
            tracker.Add(new Point(0, 0), 0);
            tracker.Add(new Point(100, 0), 20);
            tracker.Add(new Point(100, 0), 90);

            Assert.Equal(Point.Zero, tracker.Velocity);
        }

        [Fact]
        public void VelocityIsCapped()
        {
            var tracker = new VelocityTracker();
            tracker.Add(new Point(0, 0), 0);
            tracker.Add(new Point(1000, 0), 1);

            Assert.Equal(8000, tracker.Velocity.X, 1);
        }

        [Fact]
        public void VelocityNeedsTwoPositionsAndSomeTime()
        {
            var tracker = new VelocityTracker();
            Assert.Equal(Point.Zero, tracker.Velocity);

            tracker.Add(new Point(5, 5), 0);
            Assert.Equal(Point.Zero, tracker.Velocity);

            tracker.Add(new Point(50, 5), 0);
            Assert.Equal(Point.Zero, tracker.Velocity);
        }

        [Fact]
        public void VelocityKeepsOnlyTheNewestPositions()
        {
            var tracker = new VelocityTracker();
            for (int i = 0; i < 40; i += 1)
            {
                tracker.Add(new Point(i, 0), i);
            }

            Assert.Equal(1000, tracker.Velocity.X, 1);

            tracker.Clear();
            Assert.Equal(Point.Zero, tracker.Velocity);
        }

        #endregion
    }
}
