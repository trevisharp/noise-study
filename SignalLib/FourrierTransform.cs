using System;
using System.Linq;
using System.Collections.Generic;

namespace SignalLib;

public static class FourrierTransform
{
    private const int dftThreshold = 32;

    public static (float[] rsignal, float[] isignal) DFT(
        float[] reSig, float[] imSig)
    {
        if (reSig.Length != imSig.Length)
            throw new Exception("Real and Imaginary Signal have different sizes");
        int N = reSig.Length;

        float[] ouReSig = new float[N];
        float[] ouImSig = new float[N];

        for (int k = 0; k < N; k++)
        {
            float re = 0f;
            float im = 0f;
            for (int n = 0; n < N; n++)
            {
                re += reSig[n] * MathF.Cos(MathF.Tau * k * n / N)
                    + imSig[n] * MathF.Sin(MathF.Tau * k * n / N);

                im += imSig[n] * MathF.Cos(MathF.Tau * k * n / N)
                    - reSig[n] * MathF.Sin(MathF.Tau * k * n / N);
            }
            ouReSig[k] = re;  
            ouImSig[k] = im;
        }

        return (ouReSig, ouImSig);
    }

    public static (float[] rsignal, float[] isignal) FFT(
        float[] rsignal, float[] isignal
    )
    {
        float[] reAux = new float[rsignal.Length];
        float[] imAux = new float[isignal.Length];

        fft(rsignal, isignal, reAux, imAux);

        return (rsignal, isignal);
    }

    private static void fft(
        float[] reBuffer,
        float[] reAux,
        float[] imBuffer,
        float[] imAux
    )
    {
        float[] temp;
        int N = reBuffer.Length;
        int sectionCount = N / dftThreshold;

        for (int i = 0; i < N; i++)
        {
            int index = dftThreshold * (i % sectionCount) + (i / sectionCount);
            reAux[index] = reBuffer[i];
            imAux[index] = imBuffer[i];
        }

        for (int i = 0; i < sectionCount; i++)
        {
            dft(reAux, imAux, reBuffer, imBuffer, 
                dftThreshold * i, dftThreshold, N);
        }

        while (sectionCount > 1)
        {
            temp = reBuffer;
            reBuffer = reAux;
            reAux = temp;
            temp = imBuffer;
            imBuffer = imAux;
            imAux = temp;
        }   
    }

    private static void dft(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        int offset, int size, int N
    )
    {
        int end = offset + size;
        for (int k = 0, j = offset; j < end; j++, k++)
        {
            float reSum = 0f;
            float imSum = 0f;
            for (int i = offset, n = 0; i < end; i++, n++)
            {
                var param = MathF.Tau * k * n / N;
                var cos = MathF.Cos(param);
                var sin = MathF.Sin(param);
                reSum += re[n] * cos + im[n] * sin;
                imSum += im[n] * cos - re[n] * sin;
            }
            oRe[j] = reSum;
            oIm[j] = imSum;
        }
    }
}