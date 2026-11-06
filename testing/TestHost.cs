using System;
using System.Collections.Generic;
using System.Reflection;
using XForma.Platform;
using XForma.Primitives;

namespace XForma.Testing
{
    /// <summary>
    /// Runs an app's views in a unit test, with no device or window: on a platform
    /// that shows nothing, but measures and arranges as a real one does. Find views
    /// by their <see cref="View.Name"/>, which stays the same however the page's
    /// laid out.
    /// </summary>
    /// <example>
    /// <code>
    /// TestHost.Start(new App());
    /// var page = new MainPage();
    /// TestHost.LayOut(page, 400, 800);
    /// var button = TestHost.Find&lt;Button&gt;(page, "save");
    /// </code>
    /// </example>
    public static class TestHost
    {
        /// <summary>
        /// Makes the app the running one. Tests that share a process run one at a
        /// time, since an app is process-wide.
        /// </summary>
        public static T Start<T>(
            T application)
            where T : Application
        {
            ArgumentNullException.ThrowIfNull(application);
            var platform = DispatchProxy.Create<IPlatform, PlatformProxy>();
            application.Start(platform, null);
            return application;
        }

        /// <summary>
        /// Measures and arranges the view at the size.
        /// </summary>
        public static void LayOut(
            View view,
            float width,
            float height)
        {
            ArgumentNullException.ThrowIfNull(view);
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
        }

        /// <summary>
        /// The view with the name inside the view (or the view itself), or an
        /// exception if there's none.
        /// </summary>
        public static T Find<T>(
            View root,
            string name)
            where T : View
        {
            return TryFind(root, name) as T ?? throw new InvalidOperationException($"No {typeof(T).Name} named '{name}'.");
        }

        private static View? TryFind(
            View view,
            string name)
        {
            if (view.Name == name)
            {
                return view;
            }

            foreach (var child in view.DrawingOrderChildren)
            {
                var found = TryFind(child, name);
                if (null != found)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Where the view is in its parent.
        /// </summary>
        public static Rect FrameOf(
            View view)
        {
            ArgumentNullException.ThrowIfNull(view);
            return new Rect(view.FrameOrigin.X, view.FrameOrigin.Y, view.FrameSize.Width, view.FrameSize.Height);
        }

        private static object? Default(
            Type type)
        {
            return type.IsValueType && (type != typeof(void)) ? Activator.CreateInstance(type) : null;
        }

        private static readonly MethodInfo CreateProxy = typeof(DispatchProxy).GetMethod(nameof(DispatchProxy.Create), 2, Type.EmptyTypes)!;

        // A platform that creates a renderer of whatever kind it's asked for
        public class PlatformProxy : DispatchProxy
        {
            protected override object? Invoke(
                MethodInfo? targetMethod,
                object?[]? args)
            {
                var type = targetMethod!.ReturnType;
                if (targetMethod.Name.StartsWith("Create", StringComparison.Ordinal) && type.IsInterface)
                {
                    bool isApplication = typeof(IApplicationRenderer).IsAssignableFrom(type);
                    var proxy = CreateProxy.MakeGenericMethod(type, typeof(RendererProxy)).Invoke(null, null)!;
                    var renderer = (RendererProxy)proxy;
                    renderer.Element = isApplication ? null : args?[0] as Element;
                    return proxy;
                }

                return Default(type);
            }
        }

        public class RendererProxy : DispatchProxy
        {
            private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>();

            internal Element? Element;

            protected override object? Invoke(
                MethodInfo? targetMethod,
                object?[]? args)
            {
                string name = targetMethod!.Name;
                switch (name)
                {
                    case "get_Element":
                        return this.Element;

                    case "get_NativeElement":
                        return this;

                    case "get_SystemTextScale":
                        return 1.0f;

                    case "MeasureNative":
                        return ((this.Element is View measured) && !(this.Element is TextBlock) && !(this.Element is Image)) ? measured.MeasureContent((Size)args![0]!) : Size.Empty;

                    case "ArrangeNative":
                        (this.Element as View)?.LayoutContent(((Rect)args![0]!).Size);
                        return null;
                }

                // Properties remember what they're given
                if (name.StartsWith("set_", StringComparison.Ordinal))
                {
                    this._values[name.Substring(4)] = args![0];
                    return null;
                }

                if (name.StartsWith("get_", StringComparison.Ordinal))
                {
                    return this._values.TryGetValue(name.Substring(4), out var value) ? value : Default(targetMethod.ReturnType);
                }

                return Default(targetMethod.ReturnType);
            }
        }
    }
}
