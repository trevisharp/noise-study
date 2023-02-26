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

    public void Dispose()
    {
        if (disposed)
            return;
        
        disposed = true;
        pool.Return(this.real);
        pool.Return(this.imag);
    }

    private static ArrayPool<float> pool;
    
    private static void initPool()
    {
        if (pool != null)
            return;
        
        pool = ArrayPool<float>.Create(1024 * 1024, 1024);
    }
    
    public static Signal Empty(int N = 1024)
    {
        initPool();

        float[] real = pool.Rent(1024);
        float[] imag = pool.Rent(1024);

        return new Signal(real, imag);
    }
}
