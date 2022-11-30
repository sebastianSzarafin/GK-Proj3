using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Proj3
{
    class Chromatic
    {
        static int canvasWidth = 350, canvasHeight = 350; 
        static int offset = 10;
        List<(int i, Point3D p)> waves;

        public Chromatic()
        {
            waves = new List<(int i, Point3D p)>();
        }

        public void SetBorder(Canvas canvas)
        {
            foreach(var wave in waves)
            {
                double x = wave.p.X / (wave.p.X + wave.p.Y + wave.p.Z);
                double y = wave.p.Y / (wave.p.X + wave.p.Y + wave.p.Z);
                double z = 1 - x - y;

                int xp = (int)(x * canvasWidth) + offset;
                int yp = (int)(y * canvasHeight) + offset;
                yp = transform(yp, (int)canvasHeight / 2, Math.Abs(yp - (int)(canvasHeight / 2)));

                Ellipse ellipse = new Ellipse();
                ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, (byte)(x * 255), (byte)(y * 255), (byte)(z * 255)));
                ellipse.Width = 5;
                ellipse.Height = 5;
                canvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, xp);
                Canvas.SetTop(ellipse, yp);
            }

            int transform(int y, int OY, int diff) => y < OY ? OY + diff : OY - diff;
        }
        public void Initialize(Canvas canvas)
        {
            GetWaves();
            SetBorder(canvas);
        }
        void GetWaves()
        {
            string[] reader = File.ReadAllLines("CIE-XYZ.txt");
            foreach(string line in reader)
            {
                string[] parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                waves.Add((
                    Convert.ToInt32(parts[0]),
                    new Point3D(
                        Convert.ToDouble(parts[1], CultureInfo.InvariantCulture),
                        Convert.ToDouble(parts[2], CultureInfo.InvariantCulture),
                        Convert.ToDouble(parts[3], CultureInfo.InvariantCulture))));
            }
        }
    }
}
