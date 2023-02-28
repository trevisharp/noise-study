using System;
using System.Buffers;
using System.Drawing;
using System.Threading.Tasks;

namespace Signals;

using Internal;
using Visualization;
using Visualization.DrawLogics;

public class Signal : IDisposable
{
    private float[] real;
    private float[] imag;
    private bool disposed = false;
    private bool needReturn = true;

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

    /// <summary>
    /// Draw a signal in any plataform
    /// </summary>
    /// <param name="g">A class that implements IGraphics
    /// which indicates how the drawing should be done </param>
    /// <param name="rect">The rect which indicates where 
    /// the drawing should be done</param>
    public void Draw(IGraphics g, RectangleF rect)
    {
        SignalDrawParams parameters = rect;
        parameters.Real = this.real;
        parameters.Imag = this.imag;

        DrawProvider.Current.SignalDrawLogic.Draw(g, parameters);
    }

    /// <summary>
    /// Apply Fast Fourier Transform in this signal
    /// </summary>
    /// <returns>This signal</returns>
    public Signal FFT()
    {
        FourrierTransform.FFT(this.real, this.imag);
        return this;
    }
    
    /// <summary>
    /// Apply Fast Fourier Transform in this signal asynchronously
    /// </summary>
    /// <returns>This signal</returns>
    public async Task<Signal> FFTAsync()
    {
        await Task.Run(() => FourrierTransform.FFT(this.real, this.imag));
        return this;
    }

    /// <summary>
    /// Apply Inverse Fast Fourier Transform in this signal
    /// </summary>
    /// <returns>This signal</returns>
    public Signal IFFT()
    {
        FourrierTransform.IFFT(this.real, this.imag);
        return this;
    }

    /// <summary>
    /// Apply Inverse Fast Fourier Transform in this signal
    /// </summary>
    /// <returns>This signal</returns>
    public async Task<Signal> IFFTAsync()
    {
        await Task.Run(() => FourrierTransform.IFFT(this.real, this.imag));
        return this;
    }

    /// <summary>
    /// Add other signal in this signal
    /// </summary>
    /// <param name="s">The other signal</param>
    /// <returns>This signal</returns>
    public Signal Add(Signal s)
    {
        SignalOperations.Add(this.real, this.imag, s.real, s.imag);
        return this;
    }
    
    /// <summary>
    /// Subtract other signal in this signal
    /// </summary>
    /// <param name="s">The other signal</param>
    /// <returns>This signal</returns>
    public Signal Sub(Signal s)
    {
        SignalOperations.Sub(this.real, this.imag, s.real, s.imag);
        return this;
    }
    
    /// <summary>
    /// Add other signal in this signal, asynchronously
    /// </summary>
    /// <param name="s">The other signal</param>
    /// <returns>This signal</returns>
    public async Task<Signal> AddAsync(Signal s)
    {
        await Task.Run(() => SignalOperations.Add(
            this.real, this.imag, s.real, s.imag)
        );
        return this;
    }

    public Signal Magnitude()
    {
        for (int i = 0; i < this.real.Length; i++)
        {
            var real = this.real[i];
            var imag = this.imag[i];

            this.imag[i] = 0;
            this.real[i] = MathF.Sqrt(real * real + imag * imag);
        }

        return this;
    }

    public Signal ToSpectrum()
    {
        throw new NotImplementedException();
    }

    public Signal Integrate()
    {
        throw new NotImplementedException();
    }

    public Signal Derivate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Copy this signal to other new signal
    /// </summary>
    /// <returns>The new signal</returns>
    public Signal Clone()
    {
        float[] realCopy = rent(this.real.Length);
        float[] imagCopy = rent(this.imag.Length);

        Buffer.BlockCopy(this.real, 0, realCopy, 0, 4 * this.real.Length);
        Buffer.BlockCopy(this.imag, 0, imagCopy, 0, 4 * this.imag.Length);

        return new Signal(realCopy, imagCopy);
    }

    public void Dispose()
    {
        if (disposed)
            return;
        
        disposed = true;

        if (!needReturn)
            return;

        pool.Return(this.real);
        pool.Return(this.imag);
    }

    public static implicit operator Signal(float[] data)
        => FromArray(data);
    
    public static implicit operator Signal(Func<int, float> func)
    {
        float[] real = rent(1024);

        for (int i = 0; i < 1024; i++)
            real[i] = func(i);
        
        return real;
    }

    public static Signal operator +(Signal s1, Signal s2)
    {
        var newSignal = s1.Clone();
        newSignal.Add(s2);
        return newSignal;
    }

    public static Signal operator -(Signal s1, Signal s2)
    {
        var newSignal = s1.Clone();
        newSignal.Sub(s2);
        return newSignal;
    }

    private static ArrayPool<float> pool;
    
    private static void initPool()
    {
        if (pool != null)
            return;
        
        pool = ArrayPool<float>.Create(1024 * 1024, 1024);
    }

    private static float[] rent(int N)
    {
        initPool();
        return pool.Rent(N);
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

        float[] real = rent(1024);
        float[] imag = rent(1024);

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

        var newSignal = new Signal(real, imag);
        newSignal.needReturn = false;

        return newSignal;
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
        
        if (period < float.Epsilon)
            throw new InvalidOperationException(
                "The period must be bigger than 0"
            );
        
        float[] real = rent(N);
        float[] imag = rent(N);

        for (int i = 0; i < N; i++)
        {
            var param = MathF.Tau * i / period;
            real[i] = MathF.Sin(param);
        }

        return new Signal(real, imag);
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
        
        if (period < float.Epsilon)
            throw new InvalidOperationException(
                "The period must be bigger than 0"
            );
        
        float[] real = rent(N);
        float[] imag = rent(N);

        for (int i = 0; i < N; i++)
        {
            var param = MathF.Tau * i / period;
            real[i] = MathF.Cos(param);
        }

        return new Signal(real, imag);
    }

    /// <summary>
    /// Generate a Dirac function, zero for all points excepts in alfa point
    /// </summary>
    /// <param name="N">The size of signal</param>
    /// <param name="alfa">The point where the signal is 1</param>
    /// <returns>Object of signal</returns>
    public static Signal Dirac(int N, int alfa)
    {
        if (N < 1)
            throw new InvalidOperationException(
                "The vector size must be bigger than 0"
            );
        
        if (alfa < 1)
            throw new InvalidOperationException(
                "The period must be bigger than 0"
            );
        
        float[] real = rent(N);
        float[] imag = rent(N);
        
        real[alfa] = 1;

        return real;
    }

    public static Signal GaussianNoise(int N)
    {
        if (N < 1)
            throw new InvalidOperationException(
                "The vector size must be bigger than 0"
            );
        
        float[] real = rent(N);
        float[] imag = rent(N);

        byte[] data = new byte[10 * N];
        Random.Shared.NextBytes(data);

        for (int i = 0; i < N; i++)
        {
            float sum = 0;
            for (int j = 0; j < 10; j++)
                sum += data[10 * i + j] - 128;
            sum /= 10 * 128;
            real[i] = sum;
        }

        return real;
    }
}
