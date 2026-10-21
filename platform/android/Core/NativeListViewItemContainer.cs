using System;
using Android.Animation;
using Android.Views.Animations;
using XForms.Primitives;

namespace XForms.Android
{
    /// <summary>
    /// Hosts a list view item container in a list row. When the item's height
    /// changes while it is shown (for example when its command bar opens), the
    /// row animates to the new height and clips its content, so the rows below
    /// slide away to uncover what is underneath.
    /// </summary>
    internal sealed class NativeListViewItemContainer : global::Android.Widget.FrameLayout
    {
        private const long ResizeDurationMilliseconds = 250;

        private ListViewItem _itemContainer;
        private object _item;
        private object _measuredItem;
        private int _displayedHeight = -1;
        private int _targetHeight = -1;
        private ValueAnimator _resizeAnimator;
        private readonly global::Android.Graphics.Paint _gapPaint = new global::Android.Graphics.Paint();
        private global::Android.Graphics.Bitmap _openedSnapshot;
        private bool _isOpen;
        private bool _isClosing;

        internal NativeListViewItemContainer(
            global::Android.Content.Context context)
            : base(context)
        {
            // Show the content's revealed part only
            this.SetClipChildren(true);
        }

        internal ListViewItem ItemContainer
        {
            get
            {
                return this._itemContainer;
            }

            set
            {
                if (value != this._itemContainer)
                {
                    this._itemContainer = value;

                    this.RemoveAllViews();
                    if (null != value)
                    {
                        this.AddView((global::Android.Views.View)value.Renderer.NativeElement);
                    }
                }
            }
        }

        /// <summary>
        /// The list item this row currently shows. Rows are recycled, so a change
        /// of item snaps to the new height rather than animating.
        /// </summary>
        internal object Item
        {
            get
            {
                return this._item;
            }

            set
            {
                this._item = value;
            }
        }

        protected override void OnMeasure(
            int widthMeasureSpec,
            int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int contentHeight = this.MeasuredHeight;

            if ((this._displayedHeight < 0) || (!Equals(this._measuredItem, this._item)) || (!this.IsAttachedToWindow))
            {
                // A new item (or a row that isn't on screen yet) takes its height immediately
                this.StopResizing();
                this.DiscardOpenedSnapshot();
                this._isOpen = false;
                this._isClosing = false;
                this._measuredItem = this._item;
                this._displayedHeight = contentHeight;
                this._targetHeight = contentHeight;
            }
            else if (contentHeight > this._targetHeight)
            {
                // Opening: the rows below slide away to uncover the new content
                this._isOpen = true;
                this._isClosing = false;
                this.AnimateHeight(contentHeight);
            }
            else if (contentHeight < this._targetHeight)
            {
                // Closing: the content has already lost what it hid (such as the
                // command bar), so show the opened row while the rows below slide
                // back up over it
                this._isOpen = false;
                this._isClosing = (null != this._openedSnapshot);
                this.AnimateHeight(contentHeight);
            }

            this.SetMeasuredDimension(this.MeasuredWidth, this._displayedHeight);
        }

        protected override void OnLayout(
            bool changed,
            int left,
            int top,
            int right,
            int bottom)
        {
            // Lay the content out at its full height; the row clips whatever
            // has not been revealed yet
            for (int i = 0; i < this.ChildCount; i += 1)
            {
                var child = this.GetChildAt(i);
                child.Layout(
                    this.PaddingLeft,
                    this.PaddingTop,
                    this.PaddingLeft + child.MeasuredWidth,
                    this.PaddingTop + child.MeasuredHeight);
            }
        }

        protected override void DispatchDraw(
            global::Android.Graphics.Canvas canvas)
        {
            if ((this._isClosing) && (null != this._openedSnapshot))
            {
                canvas.DrawBitmap(this._openedSnapshot, 0, 0, null);
                return;
            }

            // Without a snapshot, the content may be shorter than a closing row;
            // fill the gap with the item background rather than the list's
            var backgroundColor = XForms.Application.Current?.Theme?.Colors.Background;
            if ((this.ChildCount > 0) && (backgroundColor.HasValue))
            {
                int contentBottom = this.GetChildAt(0).Bottom;
                if (contentBottom < this.Height)
                {
                    this._gapPaint.Color = backgroundColor.Value.ToAndroidColor();
                    canvas.DrawRect(0, contentBottom, this.Width, this.Height, this._gapPaint);
                }
            }

            base.DispatchDraw(canvas);

            // Keep a picture of the fully open row for when it closes
            if ((this._isOpen) && (null == this._resizeAnimator))
            {
                this.CaptureOpenedSnapshot(backgroundColor);
            }
        }

        private void CaptureOpenedSnapshot(
            Color? backgroundColor)
        {
            if ((this.ChildCount == 0) || (this.Width <= 0) || (this.Height <= 0))
            {
                return;
            }

            var snapshot = this._openedSnapshot;
            if ((null == snapshot) || (snapshot.Width != this.Width) || (snapshot.Height != this.Height))
            {
                this.DiscardOpenedSnapshot();
                snapshot = global::Android.Graphics.Bitmap.CreateBitmap(this.Width, this.Height, global::Android.Graphics.Bitmap.Config.Argb8888);
                this._openedSnapshot = snapshot;
            }

            using (var snapshotCanvas = new global::Android.Graphics.Canvas(snapshot))
            {
                snapshotCanvas.DrawColor(
                    backgroundColor?.ToAndroidColor() ?? global::Android.Graphics.Color.Transparent,
                    global::Android.Graphics.PorterDuff.Mode.Src);

                this.GetChildAt(0).Draw(snapshotCanvas);
            }
        }

        private void DiscardOpenedSnapshot()
        {
            var snapshot = this._openedSnapshot;
            if (null != snapshot)
            {
                this._openedSnapshot = null;
                snapshot.Recycle();
                snapshot.Dispose();
            }
        }

        private void AnimateHeight(
            int targetHeight)
        {
            this.StopResizing();

            this._targetHeight = targetHeight;

            var animator = ValueAnimator.OfInt(this._displayedHeight, targetHeight);
            animator.SetDuration(ResizeDurationMilliseconds);
            animator.SetInterpolator(new DecelerateInterpolator());
            animator.Update += this.ResizeAnimator_Update;
            animator.AnimationEnd += this.ResizeAnimator_AnimationEnd;
            animator.Start();

            this._resizeAnimator = animator;
        }

        private void ResizeAnimator_AnimationEnd(
            object sender,
            EventArgs e)
        {
            if (sender != this._resizeAnimator)
            {
                return;
            }

            this.StopResizing();
            this.FinishClosing();

            // Redraw now that the row has settled (and capture it, if open)
            this.Invalidate();
        }

        private void FinishClosing()
        {
            if (this._isClosing)
            {
                this._isClosing = false;
                this.DiscardOpenedSnapshot();
            }
        }

        private void ResizeAnimator_Update(
            object sender,
            ValueAnimator.AnimatorUpdateEventArgs e)
        {
            this._displayedHeight = (int)e.Animation.AnimatedValue;
            this.RequestLayout();
        }

        private void StopResizing()
        {
            var animator = this._resizeAnimator;
            if (null != animator)
            {
                this._resizeAnimator = null;
                animator.Update -= this.ResizeAnimator_Update;
                animator.AnimationEnd -= this.ResizeAnimator_AnimationEnd;
                animator.Cancel();
            }
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            // Settle on the final height so a reattached row isn't left mid-animation
            if (null != this._resizeAnimator)
            {
                this.StopResizing();
                this._displayedHeight = this._targetHeight;
            }

            this.FinishClosing();
        }
    }
}
