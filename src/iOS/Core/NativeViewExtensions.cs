using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XForms.iOS
{
    internal static class NativeViewExtensions
    {
        private static readonly NSString PropertiesKey = new NSString("xforms.props");

        private enum AssociationPolicy
        {
            Assign = 0,
            RetainNonAtomic = 1,
            CopyNonAtomic = 3,
            Retain = 01401,
            Copy = 01403,
        }

        [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.dylib")]
        private static extern void objc_setAssociatedObject(
            IntPtr pointer, IntPtr key,
            IntPtr value, AssociationPolicy policy);

        [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.dylib")]
        private static extern IntPtr objc_getAssociatedObject(
            IntPtr pointer, IntPtr key);

        private static T GetProperty<T>(
            this UIView view,
            NSString propertyKey)
            where T : NSObject
        {
            var pointer = objc_getAssociatedObject(
                view.Handle,
                propertyKey.Handle);

            return ObjCRuntime.Runtime.GetNSObject<T>(pointer);
        }

        private static void SetProperty<T>(
            this UIView view,
            NSString propertyKey,
            T value,
            AssociationPolicy policy)
            where T : NSObject
        {
            objc_setAssociatedObject(
                view.Handle,
                propertyKey.Handle,
                value.Handle,
                policy);
        }

        internal static NativeViewExtendedProperties LayoutProperties(
            this UIView view)
        {
            var properties = GetProperty<NativeViewExtendedProperties>(view, PropertiesKey);
            if (null == properties)
            {
                properties = new NativeViewExtendedProperties();
                SetProperty<NativeViewExtendedProperties>(view, PropertiesKey, properties, AssociationPolicy.RetainNonAtomic);
            }

            return properties;
        }

        internal static NativeLayout.LayoutParams LayoutParameters(
            this UIView view)
        {
            return view.LayoutProperties()?.LayoutParameters;
        }

        public static void RequestLayout(
            this UIView view)
        {
            RequestLayoutIfNeeded(view);
        }

        public static void RequestLayoutIfNeeded(
            this UIView view)
        {
            var properties = view.LayoutProperties();
            if (!properties.IsLayoutRequested)
            {
                view.SetNeedsLayout();
                properties.IsLayoutRequested = true;
            }

            var parent = view.Superview;
            if (null != parent)
            {
                parent.RequestLayoutIfNeeded();
            }
        }

        public static void Measure(
            this UIView view,
            MeasureSpec widthMeasureSpec,
            MeasureSpec heightMeasureSpec)
        {
            var nativeLayout = view as NativeLayout;
            if (null != nativeLayout)
            {
                nativeLayout.Measure(widthMeasureSpec, heightMeasureSpec);
            }
            else
            {
                var containerSize = new CGSize()
                {
                    Width = widthMeasureSpec.Size,
                    Height = heightMeasureSpec.Size,
                };

                var measuredSize = view.SizeThatFits(containerSize);

                //var dimensionX = GetDefaultSize(measuredSize.Width, widthMeasureSpec);
                //var dimensionY = GetDefaultSize(measuredSize.Height, heightMeasureSpec);
                var dimensionX = new MeasuredDimension() { Size = (float)measuredSize.Width };
                var dimensionY = new MeasuredDimension() { Size = (float)measuredSize.Height };

                view.SetMeasuredSize(dimensionX, dimensionY);
            }
        }

        //private static MeasuredDimension GetDefaultSize(
  //          nfloat size,
  //          MeasureSpec measureSpec)
        //{
  //          MeasuredDimension result = new MeasuredDimension();
        //	var specMode = measureSpec.Mode;
        //	var specSize = measureSpec.Size;
        //	switch (specMode)
        //	{
        //		case MeasureMode.Unspecified:
        //			result.Size = (float)size;
        //			break;
        //		case MeasureMode.AtMost:
        //		case MeasureMode.Exactly:
        //			result.Size = specSize;
        //			break;
  //              default:
  //                  result.Size = (float)size;
  //                  break;
        //	}
        //	return result;
        //}

        public static void SetMeasuredSize(
            this UIView view,
            MeasuredDimension dimensionX,
            MeasuredDimension dimensionY)
        {
            var layoutProperties = view.LayoutProperties();
            layoutProperties.MeasuredSize = new MeasuredSize(dimensionX, dimensionY);
        }

        public static void Layout(
            this UIView view,
            float left,
            float top,
            float right,
            float bottom)
        {
            var properties = view.LayoutProperties();
            properties.IsLayoutRequested = false;
            
            var nativeLayout = view as NativeLayout;
            if (null != nativeLayout)
            {
                nativeLayout.Layout(left, top, right, bottom);
            }
            else
            {
                view.Frame = new CGRect(left, top, right - left, bottom - top);
            }
        }
    }

    internal class NativeViewExtendedProperties : NSObject
    {
        public Thickness Padding
        {
            get;
            set;
        }

        public NativeLayout.LayoutParams LayoutParameters
        {
            get;
            set;
        }

        public MeasuredSize MeasuredSize
        {
            get;
            internal set;
        }

        internal bool IsLayoutRequested
        {
            get;
            set;
        }
    }

    internal enum MeasureMode
    {
        Unspecified = 0,
        Exactly = 1,
        AtMost = 2,
    }

    internal struct MeasureSpec
    {
        public const MeasureMode Unspecified = MeasureMode.Unspecified;
        public const MeasureMode Exactly = MeasureMode.Exactly;
        public const MeasureMode AtMost = MeasureMode.AtMost;

        public static MeasureSpec MakeMeasureSpec(
            float size,
            MeasureMode mode)
        {
            return new MeasureSpec(size, mode);
        }

        private MeasureSpec(
            float size,
            MeasureMode mode)
        {
            this.Size = size;
            this.Mode = mode;
        }

        public MeasureMode Mode
        {
            get;
        }

        public float Size
        {
            get;
        }
    }

    [Flags]
    internal enum MeasuredStateFlags
    {
        None = 0,
        TooSmall = 1,
    }

    internal struct MeasuredDimension
    {
        public float Size
        {
            get;
            set;
        }

        internal MeasuredStateFlags State
        {
            get;
            set;
        }
    }

    internal struct MeasuredSize
    {
        public MeasuredSize(
            MeasuredDimension width,
            MeasuredDimension height)
        {
            this.Width = width.Size;
            this.Height = height.Size;
            this.WidthState = width.State;
            this.HeightState = height.State;
        }

        public MeasuredSize(
            float width,
            float height)
        {
            this.Width = width;
            this.Height = height;
            this.WidthState = MeasuredStateFlags.None;
            this.HeightState = MeasuredStateFlags.None;
        }

        public float Width { get; }

        public float Height { get; }

        internal MeasuredStateFlags WidthState { get; }

        internal MeasuredStateFlags HeightState { get; }
    }
}
