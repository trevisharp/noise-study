using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using NoiseStudio;

List<(RectangleF rect, Signal s)> list = new List<(RectangleF rect, Signal s)>();
int wind = 0;

ApplicationConfiguration.Initialize();

Bitmap bmp = null;
Graphics g = null;

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
        x.s.Draw(new RectangleF(
            x.rect.X,
            x.rect.Y + wind,
            x.rect.Width,
            x.rect.Height
        ), g);
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
    // var s = Signal.Cos();
    // add(s);
    // add(s.Fourrier());
}