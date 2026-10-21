using System;
using XForms.Input;

namespace XForms.Test
{
    /// <summary>
    /// Tap handling for the test pages' views.
    /// </summary>
    internal static class Gestures
    {
        /// <summary>
        /// Adds a tap recognizer to the view and returns it, for its events.
        /// </summary>
        public static TapGestureRecognizer Tap(
            View view)
        {
            var recognizer = new TapGestureRecognizer();
            view.GestureRecognizers.Add(recognizer);
            return recognizer;
        }
    }
}
