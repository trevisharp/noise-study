using System;
using SignalLib;

float[] signal = new float[32];
float[] isignal = new float[32];

for (int i = 0; i < 32; i++)
    signal[i] = MathF.Cos(MathF.Tau * i / 8);

(signal, isignal) = FourrierTransform.FFT(signal, isignal);

foreach (var x in signal)
{
    var rounded = MathF.Round(x, 2);
    if (rounded == 0)
        rounded = 0f;
    var str = rounded.ToString().Replace(',','.');
    Console.Write($"{str}, ");
}