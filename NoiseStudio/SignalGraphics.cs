using System.Drawing;

using Signals.Visualization;

public class SignalGraphics : IGraphics
{
    public Graphics Graphics { get; set; }

    public void DrawLine(PointF p, PointF q, float penWidth, Color color)
    {
        Pen pen = new Pen(color, penWidth);
        Graphics.DrawLine(pen, p, q);
    }

    public void FillRectangle(RectangleF rect, Color color)
    {
        Brush brush = new SolidBrush(color);
        Graphics.FillRectangle(brush, rect);
    }
}