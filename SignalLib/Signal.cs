// using System;
// using System.Linq;
// using System.Drawing;
// using System.Threading.Tasks;

// namespace NoiseStudio;

// public class Signal
// {
//     private float[] real = null;
//     private float[] img = null;

//     public Signal()
//         => this.real = new float[10_000];

//     public Signal(float[] data)
//         => this.real = data;
    
//     public void Draw(RectangleF rect, Graphics g)
//     {
//         g.DrawRectangle(Pens.Blue, rect.X, rect.Y, rect.Width, rect.Height);

//         float max = real.Max();
//         float min = real.Min();
//         float twid = 1000 * rect.Width / this.real.Length;
//         float thei = rect.Height / (max - min);

//         var pts = real
//             .Select((x, i) =>
//             {
//                 float t = i / 1000f;
//                 return new PointF(
//                     rect.X + t * twid,
//                     rect.Y + rect.Height - (x - min) * thei
//                 );
//             })
//             .ToArray();

//         g.DrawLines(Pens.Red, pts);
//     }

//     // public Signal Fourrier()
//     // {
//     //     float[] fourrier = new float[40_000];

//     //     Parallel.For(0, fourrier.Length, k =>
//     //     {
//     //         int N = data.Length;
//     //         float value = 0;

//     //         for (int n = 0; n < N; n++)
//     //         {
//     //             value += data[n] * MathF.Cos(
//     //                 MathF.Tau * k * n / N
//     //             );
//     //         }

//     //         fourrier[k] = value;
//     //     });

//     //     return new Signal(fourrier);
//     // }

//     // public Signal Integral()
//     // {
//     //     float[] integral = new float[10_000];

//     //     float sum = 0;
//     //     for (int i = 0; i < integral.Length; i++)
//     //     {
//     //         sum += data[i] / 1000f;
//     //         integral[i] = sum;
//     //     }

//     //     return new Signal(integral);
//     // }

//     // public static Signal RandomNoise()
//     // {
//     //     var rand = Random.Shared;
//     //     Signal noise = new Signal();

//     //     for (int i = 0; i < noise.data.Length; i++)
//     //         noise.data[i] = 2 * rand.NextSingle() - 1;

//     //     return noise;
//     // }

//     // public static Signal WhiteNoise()
//     // {
//     //     var rand = Random.Shared;
//     //     Signal noise = new Signal();

//     //     for (int i = 0; i < noise.data.Length; i++)
//     //     {
//     //         float value = 0f;
//     //         for (int j = 0; j < 20; j++)
//     //             value += 2 * rand.NextSingle() - 1;
//     //         noise.data[i] = value / 20;
//     //     }

//     //     return noise;
//     // }

//     public static Signal Cos()
//     {
//         Signal noise = new Signal();

//         for (int i = 0; i < noise.real.Length; i++)
//             noise.real[i] = MathF.Cos(
//                 MathF.Tau * i / 1000f
//             );

//         return noise;
//     }
// }