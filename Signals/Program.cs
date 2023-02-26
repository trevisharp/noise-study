using System;
using Signals;

Console.WriteLine("Aplicação Teste");

var signal = Signal.Empty();
signal.FFT();
signal.IFFT();

Console.WriteLine("Teste completo");