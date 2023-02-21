using System;
using System.Linq;

namespace NoiseStudio;

public static class FastFourrierTransform
{
    public static (float[] rsignal, float[] isignal) FFT(
        float[] rsignal, float[] isignal
    )
    {
        float[] outRsignal = new float[1];
        float[] outIsignal = new float[1];


        return (outRsignal, outIsignal);
    }

    private static float[] fft(float[] re)
    {
        int N = re.Length;
        if (N <= 32)
            return dft(re);
        
        int M = N / 2;
        float[] output = new float[re.Length];

        var even = re.Where((x, i) => i % 2 == 0).ToArray();
        var odd = re.Where((x, i) => i % 2 == 1).ToArray();

        var ftEven = fft(even);
        var ftOdd = fft(odd);

        for (int k = 0; k < M; k++)
        {
            output[k] = ftEven[k] + ftOdd[k] * MathF.Cos(MathF.Tau * k / N);
            output[k + M] = ftEven[k] - ftOdd[k] * MathF.Cos(MathF.Tau * k / N);
        }

        return output;
    }

    private static float[] dft(float[] re)
    {
        return re;
    }
}