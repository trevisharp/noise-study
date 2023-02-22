using System;
using SignalLib;

int N = 65536;

DateTime dt;
TimeSpan span;

float[] signal = new float[N];
float[] isignal = new float[N];
signal[1] = 5;

dt = DateTime.Now;
for (int t = 0; t < 100; t++)
{
    (signal, isignal) = FourrierTransform.FFT(signal, isignal);
}
span = DateTime.Now - dt;
Console.WriteLine(span.TotalMilliseconds / 100);

void testFFT()
{
    float[] signal = new float[N];
    float[] isignal = new float[N];
    signal[1] = 5;
    (signal, isignal) = FourrierTransform.FFT(signal, isignal);

    foreach (var x in signal)
    {
        var rounded = MathF.Round(x, 2);
        if (rounded == 0)
            rounded = 0f;
        var str = rounded.ToString().Replace(',','.');
        Console.Write($"{str}, ");
    }
}

void testDFT()
{
    float[] signal = new float[N];
    float[] isignal = new float[N];
    signal[1] = 5;
    (signal, isignal) = FourrierTransform.DFT(signal, isignal);

    foreach (var x in signal)
    {
        var rounded = MathF.Round(x, 2);
        if (rounded == 0)
            rounded = 0f;
        var str = rounded.ToString().Replace(',','.');
        Console.Write($"{str}, ");
    }
}