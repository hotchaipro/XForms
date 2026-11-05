using System;

namespace XForms.Controls
{
    /// <summary>
    /// Says that a numeric constant on a control is behavior rather than look,
    /// and why, so XF0003 lets it stay in the control: how far a swipe has to
    /// go, or the width where a layout changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class BehaviorConstantAttribute : Attribute
    {
        public BehaviorConstantAttribute(
            string reason)
        {
            this.Reason = reason;
        }

        public string Reason
        {
            get;
        }
    }
}
