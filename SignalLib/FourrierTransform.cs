using System;
using System.Threading.Tasks;

namespace SignalLib;

public static class FourrierTransform
{
    private const int dftThreshold = 32;
    private static float[] reAux = null;
    private static float[] imAux = null;

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
        if (reAux == null || reAux.Length < rsignal.Length)
        {
            reAux = new float[rsignal.Length];
            imAux = new float[rsignal.Length];
        }
        
        return fft(rsignal, isignal, reAux, imAux);
    }

    private static (float[], float[]) fft(
        float[] reBuffer,
        float[] reAux,
        float[] imBuffer,
        float[] imAux
    )
    {
        int N = reBuffer.Length;
        int sectionCount = N / dftThreshold;
        int div = dftThreshold;
        int swapCount = 0;

        int[] coefs = getFFTCoefs(N, div);

        for (int i = 0; i < N; i++)
        {
            int sec = (i / dftThreshold);
            int idSec = (i % dftThreshold);
            int index = sectionCount * idSec + coefs[sec];
            reAux[i] = reBuffer[index];
            imAux[i] = imBuffer[index];
        }

        Parallel.For(0, sectionCount, i =>
        {
            dft(reAux, imAux, reBuffer, imBuffer, 
                i * dftThreshold, dftThreshold, dftThreshold);
        });

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
                    var sin = MathF.Sqrt(1 - cos * cos);

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
            swapCount++;

            temp = reBuffer;
            reBuffer = reAux;
            reAux = temp;
            
            temp = imBuffer;
            imBuffer = imAux;
            imAux = temp;
        }

        return (reBuffer, imBuffer);
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
                var sin = MathF.Sqrt(1 - cos * cos);
                reSum += re[i] * cos + im[i] * sin;
                imSum += im[i] * cos - re[i] * sin;
            }
            oRe[j] = reSum;
            oIm[j] = imSum;
        }
    }

    private static int[] getFFTCoefs(int N, int div)
    {
        int size = N / div;
        int[] coefs = new int[size];
        int[] buffer = new int[size];
        for (int i = 0; i < size; i++)
            coefs[i] = i;
        
        recEvenOddSplit(coefs, buffer, 0, size);
        return coefs;
    }

    private static void recEvenOddSplit(int[] input, int[] output, int offset, int size)
    {
        if (size == 1)
            return;
        
        evenOddSplit(input, output, offset, size);
        recEvenOddSplit(input, output, offset, size / 2);
        recEvenOddSplit(input, output, offset + size / 2, size / 2);
    }

    private static void evenOddSplit(int[] data, int[] buff, int offset, int size)
    {
        int end = offset + size;
        for (int i = offset, j = offset, k = offset + size / 2; i < end; i += 2, j++, k++)
        {
            buff[j] = data[i];
            buff[k] = data[i + 1];
        }

        end = offset + size;
        for (int i = offset; i < end; i++)
            data[i] = buff[i];
    }
}