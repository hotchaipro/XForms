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

        void DrawText(
            string text,
            Rectangle bounds,
            Color color,
            TextFormat textFormat);
    }
}
