using System;
using UIKit;
using XForms.Controls;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    public abstract class ControlRenderer<TNativeElement> : ViewRenderer<TNativeElement>, IControlRenderer
        where TNativeElement : UIView
    {
        protected ControlRenderer(
            Control control)
            : base(control)
        {
        }
    }
}
