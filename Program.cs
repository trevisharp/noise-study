ApplicationConfiguration.Initialize();

Bitmap bmp = null;
Graphics g = null;

var form = new Form();
form.WindowState = FormWindowState.Maximized;
form.FormBorderStyle =  FormBorderStyle.None;

var pb = new PictureBox();
form.Controls.Add(pb);

form.Load += delegate
{
    bmp = new Bitmap(pb.Width, pb.Height);
    g = Graphics.FromImage(bmp);
    g.Clear(Color.White);
    pb.Image = bmp;
};

Application.Run(form);