using System;
using SignalLib;

int N = 64;

float[] signal = new float[N];
float[] isignal = new float[N];
signal[1] = 5;
FourrierTransform.FFT(signal, isignal);

Console.WriteLine("FFT: ");
foreach (var x in signal)
{
    var rounded = MathF.Round(x, 1);
    if (rounded == 0)
        rounded = 0f;
    var str = rounded.ToString().Replace(',','.');
    Console.Write($"{str}, ");
}
Console.WriteLine();

signal = new float[N];
isignal = new float[N];
signal[1] = 5;
(signal, isignal) = FourrierTransform.DFT(signal, isignal);

Console.WriteLine("DFT: ");
foreach (var x in signal)
{
    var rounded = MathF.Round(x, 1);
    if (rounded == 0)
        rounded = 0f;
    var str = rounded.ToString().Replace(',','.');
    Console.Write($"{str}, ");
}