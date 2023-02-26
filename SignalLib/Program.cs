using System;
using Signals.Internal;

int N = 1024;

float[] signal = new float[N];
float[] isignal = new float[N];

for (int i = 0; i < N; i++)
{
    signal[i] = N * (5 + .5f * (2 * Random.Shared.NextSingle() - 1));
    isignal[i] = N * (5 + .5f * (2 * Random.Shared.NextSingle() - 1));
}

FourrierTransform.IFFT(signal, isignal);

foreach (var x in signal)
{
    var rounded = MathF.Round(x, 2);
    if (rounded == 0)
        rounded = 0f;
    var str = rounded.ToString().Replace(',','.');
    Console.Write($"{str}, ");
}

return;

// DateTime dt;
// TimeSpan span;

// float[] signal = new float[N];
// float[] isignal = new float[N];
// signal[1] = 5;

// dt = DateTime.Now;
// for (int t = 0; t < 1000; t++)
// {
//     FourrierTransform.FFT(signal, isignal);
// }
// span = DateTime.Now - dt;
// Console.WriteLine(span.TotalMilliseconds / 1000);

// void testIDFT()
// {
//     float[] signal = new float[N];
//     float[] isignal = new float[N];
//     signal[1] = 5;
//     (signal, isignal) = FourrierTransform.DFT(signal, isignal);

//     (signal, isignal) = FourrierTransform.IDFT(signal, isignal);

//     foreach (var x in signal)
//     {
//         var rounded = MathF.Round(x, 2);
//         if (rounded == 0)
//             rounded = 0f;
//         var str = rounded.ToString().Replace(',','.');
//         Console.Write($"{str}, ");
//     }
// }

// void testIFFT()
// {
//     float[] signal = new float[N];
//     float[] isignal = new float[N];
//     signal[1] = 5;
//     FourrierTransform.FFT(signal, isignal);

//     // (signal, isignal) = FourrierTransform.IDFT(signal, isignal);

//     foreach (var x in signal)
//     {
//         var rounded = MathF.Round(x, 2);
//         if (rounded == 0)
//             rounded = 0f;
//         var str = rounded.ToString().Replace(',','.');
//         Console.Write($"{str}, ");
//     }
// }


// void testFFT()
// {
//     float[] signal = new float[N];
//     float[] isignal = new float[N];
//     signal[1] = 5;
//     FourrierTransform.FFT(signal, isignal);

//     foreach (var x in signal)
//     {
//         var rounded = MathF.Round(x, 2);
//         if (rounded == 0)
//             rounded = 0f;
//         var str = rounded.ToString().Replace(',','.');
//         Console.Write($"{str}, ");
//     }
// }

// void testDFT()
// {
//     float[] signal = new float[N];
//     float[] isignal = new float[N];
//     signal[1] = 5;
//     (signal, isignal) = FourrierTransform.DFT(signal, isignal);

//     foreach (var x in signal)
//     {
//         var rounded = MathF.Round(x, 2);
//         if (rounded == 0)
//             rounded = 0f;
//         var str = rounded.ToString().Replace(',','.');
//         Console.Write($"{str}, ");
//     }
// }