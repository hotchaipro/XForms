using System;
using UIKit;

namespace XForms.iOS
{
    internal class NativePage : UIView
    {
        public NativePage()
        {
        }

        public override void LayoutSubviews()
        {
            int childCount = this.Subviews.Length;
            for (int i = 0; i < childCount; i += 1)
            {
                var child = this.Subviews[i];
                child.Frame = this.Frame;
            }
        }
    }
}
