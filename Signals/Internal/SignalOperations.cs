using System;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;

namespace Signals.Internal;

internal static class SignalOperations
{
    private const int splitThreshold = 128;

    internal static void Add(
        float[] s1Real, float[] s1Imag,
        float[] s2Real, float[] s2Imag
    )
    {
        add(s1Real, s2Real);
        add(s1Imag, s2Imag);
    }

    internal static void Sub(
        float[] s1Real, float[] s1Imag,
        float[] s2Real, float[] s2Imag
    )
    {
        sub(s1Real, s2Real);
        sub(s1Imag, s2Imag);
    }

    internal static void Integrate(
        float[] real, float[] imag
    )
    {
        integrate(real);
        integrate(imag);
    }

    private static void add(float[] source, float[] target)
    {
        if (source == null || target == null)
            return;
        
        int len = source.Length < target.Length 
            ? source.Length : target.Length;
        
        if (len < splitThreshold || Environment.ProcessorCount < 2)
            addSequential(source, target, len);
        else addParallel(source, target, len);
    }
    
    private static void sub(float[] source, float[] target)
    {
        if (source == null || target == null)
            return;
        
        int len = source.Length < target.Length 
            ? source.Length : target.Length;
        
        if (len < splitThreshold || Environment.ProcessorCount < 2)
            subSequential(source, target, len);
        else subParallel(source, target, len);
    }

    private static void addSequential(float[] source, float[] target, int len)
    {
        for (int i = 0; i < len; i += splitThreshold)
            add(source, target, i * splitThreshold, splitThreshold);
    }

    private static void subSequential(float[] source, float[] target, int len)
    {
        for (int i = 0; i < len; i += splitThreshold)
            sub(source, target, i * splitThreshold, splitThreshold);
    }

    private static void addParallel(float[] source, float[] target, int len)
    {
        Parallel.For(0, len / splitThreshold, i =>
        {
            add(source, target, i * splitThreshold, splitThreshold);
        });
    }

    private static void subParallel(float[] source, float[] target, int len)
    {
        Parallel.For(0, len / splitThreshold, i =>
        {
            sub(source, target, i * splitThreshold, splitThreshold);
        });
    }

    private static void add(float[] source, float[] target, int offset, int len)
    {
        if (offset + len > source.Length)
            len = source.Length - offset;
        
        if (offset + len > target.Length)
            len = target.Length - offset;
        
        if (AdvSimd.IsSupported)
            smidAdd(source, target, offset, len);
        else if (Sse42.IsSupported)
            sse42Add(source, target, offset, len);
        else if (Sse41.IsSupported)
            sse41Add(source, target, offset, len);
        else if (Avx2.IsSupported)
            avxAdd(source, target, offset, len);
        else if (Sse3.IsSupported)
            sse3Add(source, target, offset, len);
        else slowAdd(source, target, offset, len);
    }

    private static void sub(float[] source, float[] target, int offset, int len)
    {
        if (offset + len > source.Length)
            len = source.Length - offset;
        
        if (offset + len > target.Length)
            len = target.Length - offset;
        
        if (AdvSimd.IsSupported)
            smidSub(source, target, offset, len);
        else if (Sse42.IsSupported)
            sse42Sub(source, target, offset, len);
        else if (Sse41.IsSupported)
            sse41Sub(source, target, offset, len);
        else if (Avx2.IsSupported)
            avxSub(source, target, offset, len);
        else if (Sse3.IsSupported)
            sse3Sub(source, target, offset, len);
        else slowSub(source, target, offset, len);
    }

    private static unsafe void sse42Add(
        float[] source, float[] target, 
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Sse42.LoadVector128(sp);
                var tv = Sse42.LoadVector128(tp);
                var result = Sse42.Add(sv, tv);
                Sse42.Store(sp, result);
            }
        }
    }

    private static unsafe void sse41Add(
        float[] source, float[] target, 
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Sse41.LoadVector128(sp);
                var tv = Sse41.LoadVector128(tp);
                var result = Sse41.Add(sv, tv);
                Sse41.Store(sp, result);
            }
        }
    }

    private static unsafe void sse3Add(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Ssse3.LoadVector128(sp);
                var tv = Ssse3.LoadVector128(tp);
                var result = Ssse3.Add(sv, tv);
                Ssse3.Store(sp, result);
            }
        }
    }

    private static unsafe void avxAdd(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Avx2.LoadVector128(sp);
                var tv = Avx2.LoadVector128(tp);
                var result = Avx2.Add(sv, tv);
                Avx2.Store(sp, result);
            }
        }
    }

    private static unsafe void smidAdd(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = AdvSimd.LoadVector128(sp);
                var tv = AdvSimd.LoadVector128(tp);
                var result = AdvSimd.Add(sv, tv);
                AdvSimd.Store(sp, result);
            }
        }
    }
    
    private static unsafe void slowAdd(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp++, tp++)
                *sp += *tp;
        }
    }

        private static unsafe void sse42Sub(
        float[] source, float[] target, 
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Sse42.LoadVector128(sp);
                var tv = Sse42.LoadVector128(tp);
                var result = Sse42.Subtract(sv, tv);
                Sse42.Store(sp, result);
            }
        }
    }

    private static unsafe void sse41Sub(
        float[] source, float[] target, 
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Sse41.LoadVector128(sp);
                var tv = Sse41.LoadVector128(tp);
                var result = Sse41.Subtract(sv, tv);
                Sse41.Store(sp, result);
            }
        }
    }

    private static unsafe void sse3Sub(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Ssse3.LoadVector128(sp);
                var tv = Ssse3.LoadVector128(tp);
                var result = Ssse3.Subtract(sv, tv);
                Ssse3.Store(sp, result);
            }
        }
    }

    private static unsafe void avxSub(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = Avx2.LoadVector128(sp);
                var tv = Avx2.LoadVector128(tp);
                var result = Avx2.Subtract(sv, tv);
                Avx2.Store(sp, result);
            }
        }
    }

    private static unsafe void smidSub(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp += 4, tp += 4)
            {
                var sv = AdvSimd.LoadVector128(sp);
                var tv = AdvSimd.LoadVector128(tp);
                var result = AdvSimd.Subtract(sv, tv);
                AdvSimd.Store(sp, result);
            }
        }
    }
    
    private static unsafe void slowSub(
        float[] source, float[] target,
        int offset, int len
    )
    {
        fixed (float* 
            sourcePointer = source,
            targetPointer = target
        )
        {
            var sp = sourcePointer + offset;
            var tp = targetPointer + offset;
            var end = sp + len;
            for (; sp < end; sp++, tp++)
                *sp -= *tp;
        }
    }

    private static unsafe void integrate(float[] data)
    {
        var temp = stackalloc float[3];
        fixed (float* sg = data)
        {
            float* p = sg;
            float* end = p + data.Length;

            int i = 2;
            temp[0] = (data[0] + 4 * data[1] + data[2]) / 3f;
            temp[1] = (data[1] + 4 * data[2] + data[3]) / 3f;
            p += 2;

            for (; p < end - 2; p++)
            {
                float fa = *p;

                float fm = *(p+1);

                float fb = *(p+2);

                temp[i] = (fa + 4 * fm + fb) / 3f;
                i = (i + 1) % 3;

                *p = temp[i] + *(p - 1);
            }

            i = (i + 1) % 3;
            *(end - 2) = temp[i] + *(end - 3);

            i = (i + 1) % 3;
            *(end - 1) = temp[i] + *(end - 2);
            
            i = (i + 1) % 3;
            *end = temp[i] + *(end - 1);
        }
    }
}