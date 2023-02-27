using System;
using System.Linq;
using System.Drawing;

namespace Signals.Visualization.DrawLogics;

public class DefaultSignalDrawLogic : IDrawLogic
{
    public void Draw(IGraphics g, DrawParams parameters)
    {
        RectangleF rect = RectangleF.Empty;
        float[] real = null;
        float[] imag = null;
        
        if (parameters is SignalDrawParams sdp)
        {
            rect = sdp.Rectangle;
            real = sdp.Real;
            imag = sdp.Imag;
        }
        else
        {
            throw new InvalidOperationException(
                "The draw of a Signal must recive a Rectangle, Real " + 
                "and Imaginary array in a SignalDrawParams object."
            );
        }

        g.FillRectangle(rect, Color.White);

        float max = real.Union(imag).Max();
        float min = real.Union(imag).Min();
        float twid = 1024f * rect.Width / real.Length;
        float thei = rect.Height / (max - min);

        var rpts = real
            .Select((x, i) =>
            {
                float t = i / 1024f;
                return new PointF(
                    rect.X + t * twid,
                    rect.Y + rect.Height - (x - min) * thei
                );
            })
            .ToArray();
            
        var ipts = imag
            .Select((x, i) =>
            {
                float t = i / 1024f;
                return new PointF(
                    rect.X + t * twid,
                    rect.Y + rect.Height - (x - min) * thei
                );
            })
            .ToArray();
        
        for (int i = 0; i < rpts.Length - 1; i++)
        {
            g.DrawLine(rpts[i], rpts[i + 1], 2f, Color.Blue);
            g.DrawLine(ipts[i], ipts[i + 1], 2f, Color.Red);
        }
    }
}