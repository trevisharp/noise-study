using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

namespace Signals.Internal;

internal static class FourrierTransform
{
    private const int dftThreshold = 32;
    private static float[] reAux = null;
    private static float[] imAux = null;
    private static float[] cosBuffer = null;
    private static float[] sinBuffer = null;

    internal static (float[] rsignal, float[] isignal) DFT(
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
    
    internal static (float[] rsingal, float[] isingal) RFFT(float[] signal)
    {
        throw new NotImplementedException();
    }
    internal static (float[] rsingal, float[] isingal) IFFT(float[] rsignal, float[] isignal)
    {
        throw new NotImplementedException();
    }

    internal static (float[] rsignal, float[] isignal) FFT(float[] rsignal, float[] isignal)
    {
        if (rsignal.Length != isignal.Length)
            throw new Exception("Real and Imaginary Signal must have the same size.");
        
        if (!testPowerOfTwo(rsignal.Length))
            throw new Exception("Signals must have a power size of 2.");

        initAuxBuffers(rsignal.Length);

        return fft(rsignal, reAux, isignal, imAux);
    }

    private static bool testPowerOfTwo(int N)
    {
        do
        {
            N /= 2;
            if (N % 2 == 1 && N > 1)
                return false;
        } while (N > 0);
        return true;
    }
    
    private static void initAuxBuffers(int size)
    {
        if (reAux == null)
        {
            rentAuxBuffers(size);
            return;
        }

        if (reAux.Length < size)
        {
            ArrayPool<float>.Shared.Return(reAux);
            ArrayPool<float>.Shared.Return(imAux);
            rentAuxBuffers(size);
        }
    }

    private static void rentAuxBuffers(int size)
    {
        reAux = ArrayPool<float>.Shared.Rent(size);
        imAux = ArrayPool<float>.Shared.Rent(size);
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

        evenOddFragmentation(N, dftThreshold, sectionCount, reBuffer, imBuffer, reAux, imAux);
        
        var cosBuffer = getCosBuffer(dftThreshold);
        var sinBuffer = getSinBuffer(cosBuffer);

        fracDFT(reAux, imAux, reBuffer, imBuffer, cosBuffer, sinBuffer, sectionCount);

        mergeDFTresults(sectionCount, dftThreshold, reBuffer, imBuffer, reAux, imAux);

        return (reBuffer, imBuffer);
    }
    private static void evenOddFragmentation(
        int N, int division, int sectionCount,
        float[] reBuffer, float[] imBuffer,
        float[] reAux, float[] imAux
    )
    {
        int[] coefs = getFFTCoefs(N, division);
        for (int i = 0; i < N; i++)
        {
            int sec = (i / dftThreshold);
            int idSec = (i % dftThreshold);
            int index = sectionCount * idSec + coefs[sec];
            reAux[i] = reBuffer[index];
            imAux[i] = imBuffer[index];
        }
    }

    private static void fracDFT(
        float[] reAux, float[] imAux,
        float[] reBuffer, float[] imBuffer,
        float[] cosBuffer, float[] sinBuffer,
        int sectionCount)
    {
        if (Environment.ProcessorCount > 1)
            parallelFracDFT(
                reAux, imAux, reBuffer, imBuffer, 
                cosBuffer, sinBuffer, sectionCount
            );
        else
            sequentialFracDFT(
                reAux, imAux, reBuffer, imBuffer, 
                cosBuffer, sinBuffer, sectionCount
            );
    }

    private static void sequentialFracDFT(
        float[] reAux, float[] imAux,
        float[] reBuffer, float[] imBuffer,
        float[] cosBuffer, float[] sinBuffer,
        int sectionCount)
    {
        for (int i = 0; i < sectionCount; i++)
        {
            dft(
                reAux, imAux, reBuffer, imBuffer, 
                cosBuffer, sinBuffer,
                i * dftThreshold, dftThreshold
            );
        }
    }

    private static void parallelFracDFT(
        float[] reAux, float[] imAux,
        float[] reBuffer, float[] imBuffer,
        float[] cosBuffer, float[] sinBuffer,
        int sectionCount)
    {
        Parallel.For(0, sectionCount, i =>
        {
            dft(
                reAux, imAux, reBuffer, imBuffer, 
                cosBuffer, sinBuffer,
                i * dftThreshold, dftThreshold
            );
        });
    }

    private static unsafe void dft(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        if (AdvSimd.IsSupported)
            smidDFT(re, im, oRe, oIm, cosBuffer, sinBuffer, offset, N);
        else if (Sse42.IsSupported)
            sse42DFT(re, im, oRe, oIm, cosBuffer, sinBuffer, offset, N);
        else if (Sse41.IsSupported)
            sse41DFT(re, im, oRe, oIm, cosBuffer, sinBuffer, offset, N);
        else if (Avx2.IsSupported)
            avxDFT(re, im, oRe, oIm, cosBuffer, sinBuffer, offset, N);
        else if (Sse3.IsSupported)
            sse3DFT(re, im, oRe, oIm, cosBuffer, sinBuffer, offset, N);
        else
            slowDFT(re, im, oRe, oIm, cosBuffer, sinBuffer, offset, N);
    }

    private static unsafe void slowDFT(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        fixed (float* 
            rep = re, imp = im, 
            orep = oRe, oimp = oIm,
            cosp = cosBuffer, sinp = sinBuffer
        )
        {
            float* tcosp = cosp, tsinp = sinp;
            float* torep = orep + offset, toimp = oimp + offset;
            float* endTorep = torep + N;

            for (; torep < endTorep; torep++, toimp++)
            {
                float reSum = 0f;
                float imSum = 0f;
                float* trep = rep + offset, timp = imp + offset;
                float* endTrep = trep + N;

                for (; trep < endTrep; trep++, timp++, tcosp++, tsinp++)
                {
                    var cos = *tcosp;
                    var sin = *tsinp;

                    var crrRe = *trep;
                    var crrIm = *timp;
                    reSum += crrRe * cos + crrIm * sin;
                    imSum += crrIm * cos - crrRe * sin;
                }
                *torep = reSum;
                *toimp = imSum;
            }
        }
    }

    private static unsafe void sse42DFT(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        fixed (float* 
            rep = re, imp = im, 
            orep = oRe, oimp = oIm,
            cosp = cosBuffer, sinp = sinBuffer
        )
        {
            float* tcosp = cosp, tsinp = sinp;
            float* torep = orep + offset, toimp = oimp + offset;
            float* endTorep = torep + N;
            float sum = 0;
            float* sumPointer = &sum;

            for (; torep < endTorep; torep++, toimp++)
            {
                float reSum = 0f;
                float imSum = 0f;
                float* trep = rep + offset, timp = imp + offset;
                float* endTrep = trep + N;

                for (; trep < endTrep; tcosp += 4, tsinp += 4, trep += 4, timp += 4)
                {
                    var cos = Sse42.LoadVector128(tcosp);
                    var sin = Sse42.LoadVector128(tsinp);
                    var rev = Sse42.LoadVector128(trep);
                    var imv = Sse42.LoadVector128(timp);

                    var m1 = Sse42.Multiply(cos, rev);
                    var m2 = Sse42.Multiply(sin, imv);
                    var m3 = Sse42.Add(m1, m2);

                    Sse42.StoreScalar(sumPointer, m3);
                    reSum += sum;

                    m1 = Sse42.Multiply(imv, cos);
                    m2 = Sse42.Multiply(rev, sin);
                    m3 = Sse42.Subtract(m1, m2);

                    Sse42.StoreScalar(sumPointer, m3);
                    imSum += sum;
                }
                *torep = reSum;
                *toimp = imSum;
            }
        }
    }

    private static unsafe void sse41DFT(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        fixed (float* 
            rep = re, imp = im, 
            orep = oRe, oimp = oIm,
            cosp = cosBuffer, sinp = sinBuffer
        )
        {
            float* tcosp = cosp, tsinp = sinp;
            float* torep = orep + offset, toimp = oimp + offset;
            float* endTorep = torep + N;
            float sum = 0;
            float* sumPointer = &sum;

            for (; torep < endTorep; torep++, toimp++)
            {
                float reSum = 0f;
                float imSum = 0f;
                float* trep = rep + offset, timp = imp + offset;
                float* endTrep = trep + N;

                for (; trep < endTrep; tcosp += 4, tsinp += 4, trep += 4, timp += 4)
                {
                    var cos = Sse41.LoadVector128(tcosp);
                    var sin = Sse41.LoadVector128(tsinp);
                    var rev = Sse41.LoadVector128(trep);
                    var imv = Sse41.LoadVector128(timp);

                    var m1 = Sse41.Multiply(cos, rev);
                    var m2 = Sse41.Multiply(sin, imv);
                    var m3 = Sse41.Add(m1, m2);

                    Sse41.StoreScalar(sumPointer, m3);
                    reSum += sum;

                    m1 = Sse41.Multiply(imv, cos);
                    m2 = Sse41.Multiply(rev, sin);
                    m3 = Sse41.Subtract(m1, m2);

                    Sse41.StoreScalar(sumPointer, m3);
                    imSum += sum;
                }
                *torep = reSum;
                *toimp = imSum;
            }
        }
    }

    private static unsafe void sse3DFT(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        fixed (float* 
            rep = re, imp = im, 
            orep = oRe, oimp = oIm,
            cosp = cosBuffer, sinp = sinBuffer
        )
        {
            float* tcosp = cosp, tsinp = sinp;
            float* torep = orep + offset, toimp = oimp + offset;
            float* endTorep = torep + N;
            float sum = 0;
            float* sumPointer = &sum;

            for (; torep < endTorep; torep++, toimp++)
            {
                float reSum = 0f;
                float imSum = 0f;
                float* trep = rep + offset, timp = imp + offset;
                float* endTrep = trep + N;

                for (; trep < endTrep; tcosp += 4, tsinp += 4, trep += 4, timp += 4)
                {
                    var cos = Sse3.LoadVector128(tcosp);
                    var sin = Sse3.LoadVector128(tsinp);
                    var rev = Sse3.LoadVector128(trep);
                    var imv = Sse3.LoadVector128(timp);

                    var m1 = Sse3.Multiply(cos, rev);
                    var m2 = Sse3.Multiply(sin, imv);
                    var m3 = Sse3.Add(m1, m2);

                    Sse3.StoreScalar(sumPointer, m3);
                    reSum += sum;

                    m1 = Sse3.Multiply(imv, cos);
                    m2 = Sse3.Multiply(rev, sin);
                    m3 = Sse3.Subtract(m1, m2);

                    Sse3.StoreScalar(sumPointer, m3);
                    imSum += sum;
                }
                *torep = reSum;
                *toimp = imSum;
            }
        }
    }

    private static unsafe void avxDFT(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        fixed (float* 
            rep = re, imp = im, 
            orep = oRe, oimp = oIm,
            cosp = cosBuffer, sinp = sinBuffer
        )
        {
            float* tcosp = cosp, tsinp = sinp;
            float* torep = orep + offset, toimp = oimp + offset;
            float* endTorep = torep + N;
            float sum = 0;
            float* sumPointer = &sum;

            for (; torep < endTorep; torep++, toimp++)
            {
                float reSum = 0f;
                float imSum = 0f;
                float* trep = rep + offset, timp = imp + offset;
                float* endTrep = trep + N;

                for (; trep < endTrep; tcosp += 4, tsinp += 4, trep += 4, timp += 4)
                {
                    var cos = Avx2.LoadVector128(tcosp);
                    var sin = Avx2.LoadVector128(tsinp);
                    var rev = Avx2.LoadVector128(trep);
                    var imv = Avx2.LoadVector128(timp);

                    var m1 = Avx2.Multiply(cos, rev);
                    var m2 = Avx2.Multiply(sin, imv);
                    var m3 = Avx2.Add(m1, m2);

                    Avx2.StoreScalar(sumPointer, m3);
                    reSum += sum;

                    m1 = Avx2.Multiply(imv, cos);
                    m2 = Avx2.Multiply(rev, sin);
                    m3 = Avx2.Subtract(m1, m2);

                    Avx2.StoreScalar(sumPointer, m3);
                    imSum += sum;
                }
                *torep = reSum;
                *toimp = imSum;
            }
        }
    }

    private static unsafe void smidDFT(
        float[] re, float[] im,
        float[] oRe, float[] oIm,
        float[] cosBuffer, float[] sinBuffer,
        int offset, int N
    )
    {
        fixed (float* 
            rep = re, imp = im, 
            orep = oRe, oimp = oIm,
            cosp = cosBuffer, sinp = sinBuffer
        )
        {
            float* tcosp = cosp, tsinp = sinp;
            float* torep = orep + offset, toimp = oimp + offset;
            float* endTorep = torep + N;
            float* sumPointer = stackalloc float[4];

            for (; torep < endTorep; torep++, toimp++)
            {
                float reSum = 0f;
                float imSum = 0f;
                float* trep = rep + offset, timp = imp + offset;
                float* endTrep = trep + N;

                for (; trep < endTrep; tcosp += 4, tsinp += 4, trep += 4, timp += 4)
                {
                    var cos = AdvSimd.LoadVector128(tcosp);
                    var sin = AdvSimd.LoadVector128(tsinp);
                    var rev = AdvSimd.LoadVector128(trep);
                    var imv = AdvSimd.LoadVector128(timp);

                    var m1 = AdvSimd.Multiply(cos, rev);
                    var m2 = AdvSimd.Multiply(sin, imv);
                    var m3 = AdvSimd.Add(m1, m2);

                    AdvSimd.Store(sumPointer, m3);
                    reSum += sumPointer[3] + sumPointer[2] + sumPointer[1] + sumPointer[0];

                    m1 = AdvSimd.Multiply(imv, cos);
                    m2 = AdvSimd.Multiply(rev, sin);
                    m3 = AdvSimd.Subtract(m1, m2);

                    AdvSimd.Store(sumPointer, m3);
                    imSum += sumPointer[3] + sumPointer[2] + sumPointer[1] + sumPointer[0];
                }
                *torep = reSum;
                *toimp = imSum;
            }
        }
    }

    private static void mergeDFTresults(
        int sectionCount, int div,
        float[] reBuffer, float[] imBuffer,
        float[] reAux, float[] imAux
    )
    {
        int swapCount = 0;
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
        
        if (swapCount % 2 == 1)
            swapSignals(reBuffer, reAux, imBuffer, imAux);
    }

    private static void swapSignals(
        float[] reSource,
        float[] reTarget,
        float[] imSource,
        float[] imTarget
    )
    {
        copySignal(reSource, reTarget);
        copySignal(imSource, imTarget);
    }

    private static void copySignal(float[] source, float[] target)
        => Buffer.BlockCopy(source, 0, target, 0, 4 * target.Length);

    private static int[] getFFTCoefs(int N, int div)
    {
        int size = N / div;
        int[] coefs = new int[size];
        int[] buffer = new int[size];
        for (int i = 0; i < size; i++)
            coefs[i] = i;
        
        recursiveEvenOddSplit(coefs, buffer, 0, size);
        return coefs;
    }

    private static void recursiveEvenOddSplit(int[] input, int[] output, int offset, int size)
    {
        if (size == 1)
            return;
        
        evenOddSplit(input, output, offset, size);
        recursiveEvenOddSplit(input, output, offset, size / 2);
        recursiveEvenOddSplit(input, output, offset + size / 2, size / 2);
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

    private static float[] getCosBuffer(int N)
    {   
        if (cosBuffer != null)
            return cosBuffer;
        
        var cosArr = new float[N * N];

        for (int j = 0; j < N; j++)
        {
            for (int i = 0; i < N; i++)
                cosArr[i + j * N] = MathF.Cos(MathF.Tau * i * j / N);
        }
        
        cosBuffer = cosArr;
        return cosArr;
    }

    private static float[] getSinBuffer(float[] cosBuffer)
    {
        if (sinBuffer != null)
            return sinBuffer;
        
        var sinArr = new float[cosBuffer.Length];

        for (int i = 0; i < cosBuffer.Length; i++)
        {
            var cos = cosBuffer[i];
            sinArr[i] = MathF.Sqrt(1 - cos * cos);
        }

        sinBuffer = sinArr;
        return sinArr;
    }
}
