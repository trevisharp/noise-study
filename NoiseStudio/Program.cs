using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Signals;

List<(RectangleF rect, Signal s)> list = new List<(RectangleF rect, Signal s)>();

ApplicationConfiguration.Initialize();

Bitmap bmp = null;
Graphics g = null;
SignalGraphics sg = null;

var form = new Form();
form.WindowState = FormWindowState.Maximized;
form.FormBorderStyle =  FormBorderStyle.None;

var tm = new Timer();
tm.Interval = 20;

var pb = new PictureBox();
pb.Dock = DockStyle.Fill;
form.Controls.Add(pb);

form.Load += delegate
{
    bmp = new Bitmap(pb.Width, pb.Height);
    g = Graphics.FromImage(bmp);
    sg = new SignalGraphics()
    {
        Graphics = g
    };
    g.Clear(Color.White);
    pb.Image = bmp;
    tm.Start();
    load();
};

form.KeyDown += (o, e) =>
{
    if (e.KeyCode == Keys.Escape)
        Application.Exit();
};

tm.Tick += delegate
{
    g.Clear(Color.White);
    foreach (var x in list)
    {
        var signal = x.s;
        x.s.Draw(sg, x.rect);
    }
    pb.Refresh();
};

Application.Run(form);

void add(Signal s)
{
    var rect = new RectangleF(
        5, 5 + list.Count * (5 + 300),
        pb.Width - 10, 300
    );
    list.Add((rect, s));
}

void load()
{
    var s = Signal.Sin(1024, 8);
    var r = Signal.Cos(1024, 16);
    var t = s + r;
    add(t);

    var fft = t.Clone().FFT();
    add(fft);

    var original = fft.Clone().IFFT();
    add(original);
}