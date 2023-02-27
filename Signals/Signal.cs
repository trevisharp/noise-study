using System;
using System.Buffers;

namespace Signals;

using Internal;

public class Signal : IDisposable
{
    private float[] real;
    private float[] imag;
    private bool disposed = false;

    private Signal(float[] real, float[] imag)
    {
        this.real = real;
        this.imag = imag;
    }
    
    ~Signal()
    {
        if (disposed)
            return;
        
        Dispose();
    }

    public Signal FFT()
    {
        FourrierTransform.FFT(this.real, this.imag);
        return this;
    }
    
    public Signal IFFT()
    {
        FourrierTransform.IFFT(this.real, this.imag);
        return this;
    }

    public Signal Clone()
    {
        float[] realCopy = new float[this.real.Length];
        float[] imagCopy = new float[this.imag.Length];

        Buffer.BlockCopy(this.real, 0, realCopy, 0, 4 * this.real.Length);
        Buffer.BlockCopy(this.imag, 0, imagCopy, 0, 4 * this.imag.Length);

        return new Signal(realCopy, imagCopy);
    }

    public void Dispose()
    {
        if (disposed)
            return;
        
        disposed = true;
        pool.Return(this.real);
        pool.Return(this.imag);
    }

    public static implicit operator Signal(float[] data)
        => FromArray(data);
    
    public static implicit operator Signal(Func<int, float> func)
    {
        float[] real = new float[1024];

        for (int i = 0; i < 1024; i++)
            real[i] = func(i);
        
        return real;
    }

    public static Signal operator +(Signal s1, Signal s2)
    {
        
    }

    private static ArrayPool<float> pool;
    
    private static void initPool()
    {
        if (pool != null)
            return;
        
        pool = ArrayPool<float>.Create(1024 * 1024, 1024);
    }
    
    /// <summary>
    /// Create a empty signal with all values equals to zero
    /// </summary>
    /// <param name="N">Size of created signal, must be bigger than 0</param>
    /// <returns>The signal object</returns>
    public static Signal Empty(int N = 1024)
    {
        if (N < 1)
            throw new InvalidOperationException(
                "The vector size must be bigger than 0"
            );


        initPool();

        float[] real = pool.Rent(1024);
        float[] imag = pool.Rent(1024);

        return new Signal(real, imag);
    }

    /// <summary>
    /// Create a signal from real and imaginary vector
    /// </summary>
    /// <param name="real">Real signal</param>
    /// <param name="imag">Imaginary signal, optional</param>
    /// <returns>The signal with real and imaginary part</returns>
    public static Signal FromArray(float[] real, float[] imag = null)
    {
        if (real == null)
            throw new NullReferenceException($"Real vector must be not null.");

        if (imag == null)
            imag = new float[real.Length];

        return new Signal(real, imag);
    }

    /// <summary>
    /// Create a Sin wave signal of size N with a period
    /// </summary>
    /// <param name="N">Size, must be bigger than 0</param>
    /// <param name="period">Period os sin wave, must be bigger than 0</param>
    /// <returns>Object os signal</returns>
    public static Signal Sin(int N, float period)
    {
        if (N < 1)
            throw new InvalidOperationException(
                "The vector size must be bigger than 0"
            );
        
        if (period > 0f)
            throw new InvalidOperationException(
                "The period must be bigger than 0"
            );

        float[] real = new float[N];

        for (int i = 0; i < N; i++)
        {
            var param = MathF.Tau * i / period;
            real[i] = MathF.Sin(param);
        }

        return real;
    }

    /// <summary>
    /// Create a Cos wave signal of size N with a period
    /// </summary>
    /// <param name="N">Size, must be bigger than 0</param>
    /// <param name="period">Period os cos wave, must be bigger than 0</param>
    /// <returns>Object os signal</returns>
    public static Signal Cos(int N, float period)
    {
        if (N < 1)
            throw new InvalidOperationException(
                "The vector size must be bigger than 0"
            );
        
        if (period > 0f)
            throw new InvalidOperationException(
                "The period must be bigger than 0"
            );

        
        float[] real = new float[N];

        for (int i = 0; i < N; i++)
        {
            var param = MathF.Tau * i / period;
            real[i] = MathF.Cos(param);
        }

        return real;
    }
}
