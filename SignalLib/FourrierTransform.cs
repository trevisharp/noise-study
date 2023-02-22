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
                var param = MathF.Tau * k * n / N;
                var cos = MathF.Cos(param);
                var sin = MathF.Sin(param);
                re += reSig[n] * cos + imSig[n] * sin;
                im += imSig[n] * cos - reSig[n] * sin;
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
        int N = reBuffer.Length;
        int sectionCount = N / dftThreshold;
        int div = dftThreshold;

        for (int i = 0; i < N; i++)
        {
            int index = dftThreshold * (i % sectionCount) + (i / sectionCount);
            reAux[index] = reBuffer[i];
            imAux[index] = imBuffer[i];
        }

        for (int i = 0; i < sectionCount; i++)
        {
            dft(reAux, imAux, reBuffer, imBuffer, 
                i * dftThreshold, dftThreshold, dftThreshold);
        }

        float[] temp;
        while (sectionCount > 1)
        {
            for (int s = 0; s < sectionCount; s += 2)
            {
                int start = div * s;
                int end = start + div;
                for (int i = start, j = end, k = 0; i < end; i++, j++, k++)
                {
                    var param = MathF.Tau * k / (2 * div);
                    var cos = MathF.Cos(param);
                    var sin = MathF.Sin(param);

                    float W = reBuffer[j] * cos + imBuffer[j] * sin;
                    reAux[i] = reBuffer[i] + W;
                    reAux[j] = reBuffer[i] - W;

                    W = imBuffer[j] * cos - reBuffer[j] * sin;
                    imAux[i] = imBuffer[i] + W;
                    imAux[j] = imBuffer[i] - W;
                }
            }

            div *= 2;
            sectionCount /= 2;

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
                reSum += re[i] * cos + im[i] * sin;
                imSum += im[i] * cos - re[i] * sin;
            }
            oRe[j] = reSum;
            oIm[j] = imSum;
        }
    }
}