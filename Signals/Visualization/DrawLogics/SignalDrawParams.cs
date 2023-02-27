using System.Drawing;

namespace Signals.Visualization.DrawLogics;

public class SignalDrawParams : DrawParams
{
    public SignalDrawParams(RectangleF rect)
        => this.Rectangle = rect;

    public RectangleF Rectangle { get; set; }
    public float[] Real { get; set; }
    public float[] Imag { get; set; }

    public static implicit operator SignalDrawParams(RectangleF rect)
        => new SignalDrawParams(rect);
}