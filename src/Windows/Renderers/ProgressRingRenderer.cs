using System;
using XForms.Controls;
using XamlProgressRing = global::Windows.UI.Xaml.Controls.ProgressRing;

namespace XForms.Windows.Renderers
{
    public class ProgressRingRenderer : ControlRenderer, IProgressRingRenderer
    {
        private XamlProgressRing _xamlProgressRing;

        public ProgressRingRenderer(
            ProgressRing progressRing)
            : base(progressRing)
        {
            this._xamlProgressRing = new XamlProgressRing()
            {
            };

            this.SetNativeElement(this._xamlProgressRing);
        }

        public bool IsActive
        {
            get
            {
                return this._xamlProgressRing.IsActive;
            }

            set
            {
                this._xamlProgressRing.IsActive = value;
            }
        }
    }
}
