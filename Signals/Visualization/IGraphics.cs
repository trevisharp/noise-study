using System.Drawing;

namespace Signals.Visualization;

public interface IGraphics
{
    void DrawLine(PointF p, PointF q, float penWidth, Color color);

    void FillRectangle(RectangleF rect, Color color);
}