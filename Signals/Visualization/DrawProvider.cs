using System;

namespace Signals.Visualization;

using DrawLogics;

public class DrawProvider
{
    private static DrawProvider provider = new DrawProvider();
    public static DrawProvider Current => provider;
    private DrawProvider()
    {
        this.signalDrawLogic = new DefaultSignalDrawLogic();
    }

    private IDrawLogic signalDrawLogic;
    public IDrawLogic SignalDrawLogic => signalDrawLogic;
    public void SetSignalDrawLogic(IDrawLogic logic)
    {
        if (logic == null)
            throw new NullReferenceException("The logic must be not null");
        
        this.signalDrawLogic = logic;
    }
}