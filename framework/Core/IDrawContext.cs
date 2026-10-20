using System;

namespace XForms
{
    public interface IDrawContext
    {
        void Clear(
            Color color);

        void DrawRectangle(
            Rectangle rectangle,
            Color color,
            float strokeWidth = 1.0f);

        void FillRectangle(
            Rectangle rectangle,
            Color color);

        void DrawEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f);

        void FillEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color);

        void FillEllipse(
            Rectangle rectangle,
            Color color);

        void FillEllipseShadow(
            Rectangle rectangle,
            Color color,
            float blurAmount = 8.0f);

        void DrawImage(
            Bitmap bitmap,
            Rectangle destinationRectangle,
            Color tint);

        /// <summary>
        /// Draws a straight line with round ends.
        /// </summary>
        void DrawLine(
            Point start,
            Point end,
            Color color,
            float strokeWidth = 1.0f);

        /// <summary>
        /// Draws part of a circle with round ends, clockwise from the start
        /// angle (in degrees, with 0 at the top).
        /// </summary>
        void DrawArc(
            Point center,
            float radius,
            float startDegrees,
            float sweepDegrees,
            Color color,
            float strokeWidth = 1.0f);

        void DrawText(
            string text,
            Rectangle bounds,
            Color color,
            TextFormat textFormat);
    }
}
