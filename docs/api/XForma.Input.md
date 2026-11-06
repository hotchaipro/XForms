# XForma.Input

## HoldingEventArgs

| Member | |
|---|---|
| `HoldingEventArgs(XForma.Point position, XForma.Input.HoldingState state) -> void` |  |
| `HoldingState.get -> XForma.Input.HoldingState` |  |
| `Position.get -> XForma.Point` |  |

## HoldingState

| Member | |
|---|---|
| `Canceled = 2 -> XForma.Input.HoldingState` | The pointer moved away or was canceled after holding. |
| `Completed = 1 -> XForma.Input.HoldingState` | The pointer was released after holding. |
| `Started = 0 -> XForma.Input.HoldingState` | The pointer has been held down long enough. |

## IPanGestureDelegate

| Member | |
|---|---|
| `OnPanCanceled() -> void` |  |
| `OnPanChanged(XForma.Point translation) -> void` | The pointer moved; the translation is from where the pan started. |
| `OnPanCompleted(XForma.Point translation) -> void` |  |
| `OnPanStarted() -> void` |  |

## IPinchGestureDelegate

| Member | |
|---|---|
| `OnPinchChanged(float scale, XForma.Point center) -> void` | The fingers moved: the scale is their distance over the distance when the pinch started, around the point midway between them. |
| `OnPinchCompleted(float scale) -> void` |  |
| `OnPinchStarted() -> void` |  |

## ISwipeGestureDelegate

| Member | |
|---|---|
| `OnSwipeBegan(int direction) -> bool` |  |
| `OnSwipeCanceled(int direction) -> bool` |  |
| `OnSwipeEnded(int direction, float dx) -> bool` |  |
| `OnSwipeMoved(int direction, float dx) -> bool` |  |

## ITapGestureDelegate

| Member | |
|---|---|
| `OnTapBegan() -> void` |  |
| `OnTapEnded() -> void` |  |
| `OnTapped() -> void` |  |

## PanGestureRecognizer

Recognizes a drag. Once the pointer has moved past the drag slop, the pan takes the pointer from scrolling parents, unless `ShouldBegin` declines it.

| Member | |
|---|---|
| `IsDragGesture.get -> bool` |  |
| `OnPointerInput(XForma.PointerEventArgs e) -> void` |  |
| `PanGestureRecognizer(XForma.Input.IPanGestureDelegate gestureDelegate) -> void` |  |
| `ShouldBegin.get -> System.Func<XForma.Point, bool>` | Asked once, as the pointer passes the drag slop, with how far it has moved: false leaves the pointer to others, such as a scrolling parent (a pan that only goes sideways declines a drag that went mostly up or down). Null always begins. |
| `ShouldBegin.set -> void` | Asked once, as the pointer passes the drag slop, with how far it has moved: false leaves the pointer to others, such as a scrolling parent (a pan that only goes sideways declines a drag that went mostly up or down). Null always begins. |
| `Velocity.get -> XForma.Point` | How fast the pan was moving as it lifted, in points a second: for a delegate deciding, in OnPanCompleted, whether it was a flick. |

## PinchGestureRecognizer

Recognizes two fingers moving apart or together on the view. A single finger is left to scroll as usual.

| Member | |
|---|---|
| `OnDetached(XForma.View view) -> void` |  |
| `OnPointerInput(XForma.PointerEventArgs e) -> void` |  |
| `PinchGestureRecognizer(XForma.Input.IPinchGestureDelegate gestureDelegate) -> void` |  |
| `TracksMultiplePointers.get -> bool` |  |

## SwipeGestureRecognizer

A sideways swipe. A touch that moves the drag slop from where it pressed is a swipe if it went mostly sideways (and the gesture then claims the pointer from a scrolling parent), or fails if it went mostly up or down (the parent scrolls). A quick swipe that lifts before any move arrives is still a swipe. Follow one with `Swiping`, on any view, or with a delegate. Add a `TapGestureRecognizer` too for taps: a swipe cancels it.

| Member | |
|---|---|
| `IsDragGesture.get -> bool` |  |
| `OnPointerInput(XForma.PointerEventArgs touch) -> void` |  |
| `SwipeGestureRecognizer() -> void` |  |
| `SwipeGestureRecognizer(XForma.Input.ISwipeGestureDelegate swipeDelegate) -> void` |  |
| `Swiping -> System.EventHandler<XForma.Input.SwipingEventArgs>` | A sideways swipe began, moved, ended or was canceled. |
| `Velocity.get -> float` | How fast the swipe was moving its way as it lifted, in points a second (negative going back): for a delegate deciding, in OnSwipeEnded, whether a short swipe was a flick. |

## SwipePhase

Where a swipe is: begun (the finger has moved far enough sideways), moving, released, or canceled.

| Member | |
|---|---|
| `Began = 0 -> XForma.Input.SwipePhase` |  |
| `Canceled = 3 -> XForma.Input.SwipePhase` |  |
| `Ended = 2 -> XForma.Input.SwipePhase` |  |
| `Moved = 1 -> XForma.Input.SwipePhase` |  |

## SwipingEventArgs

A sideways swipe as it moves, for something that follows the finger (such as `FollowSwipe`).

| Member | |
|---|---|
| `Direction.get -> int` | Which way the swipe started: 1 to the right, -1 to the left. |
| `Distance.get -> float` | How far the finger has moved that way from where it pressed (0 if it's gone back past it). |
| `Phase.get -> XForma.Input.SwipePhase` |  |
| `SwipingEventArgs(XForma.Input.SwipePhase phase, int direction, float distance, float velocity) -> void` |  |
| `Velocity.get -> float` | How fast the finger was moving that way as it was released, in points a second (negative going back); 0 until then. |

## TapGestureRecognizer

Recognizes taps (a press and release without moving), double taps (a second tap soon after a first, close to it) and holds (a press held without moving). A control handles them through its `ITapGestureDelegate`, which also tells it when a press begins and ends (for pressed feedback); an app handles the events. A touch that may yet become a drag (`IsDragPossible`) begins its press a moment later, or as it lifts, so a swipe or a scroll shows no pressed look. Activating the view (Enter, Space, a screen reader) taps it, so a view with a tap recognizer is focusable.

| Member | |
|---|---|
| `DoubleTapped -> System.EventHandler<XForma.Input.TappedEventArgs>` | Raised for a second tap soon after a first, close to it. |
| `Holding -> System.EventHandler<XForma.Input.HoldingEventArgs>` | Raised when a pointer is held down on the view without moving (`Started`), and again when it's released or moves away. A held pointer doesn't also tap. |
| `IsPressed.get -> bool` | Whether a press that may yet be a tap is down on the view (for pressed feedback). |
| `OnAttached(XForma.View view) -> void` |  |
| `OnDetached(XForma.View view) -> void` |  |
| `OnPointerInput(XForma.PointerEventArgs touch) -> void` |  |
| `TapGestureRecognizer() -> void` |  |
| `TapGestureRecognizer(XForma.Input.ITapGestureDelegate gestureDelegate) -> void` |  |
| `Tapped -> System.EventHandler<XForma.Input.TappedEventArgs>` | Raised when a pointer is pressed and released over the view without moving, or the view is activated. |

## TappedEventArgs

| Member | |
|---|---|
| `DeviceType.get -> XForma.PointerDeviceType` |  |
| `Position.get -> XForma.Point` | Where the tap was, relative to the view. |
| `TappedEventArgs(XForma.Point position, XForma.PointerDeviceType deviceType) -> void` |  |
