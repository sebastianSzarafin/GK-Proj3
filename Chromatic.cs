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
        static int offset = 20;
        List<(int i, Point3D p)> waves;
        List<(int i, Point p)> wavesOnCanvas;


        public Chromatic()
        {
            waves = new List<(int i, Point3D p)>();
            wavesOnCanvas = new List<(int i, Point p)>();
        }

        public void SetBorder(Canvas canvas)
        {
            foreach(var wave in waves)
            {
                double X = wave.p.X / (wave.p.X + wave.p.Y + wave.p.Z);
                double Y = wave.p.Y / (wave.p.X + wave.p.Y + wave.p.Z);
                double Z = 1 - X - Y;

                int xp = (int)(X * canvasWidth) + offset;
                int yp = (int)(Y * canvasHeight) + offset;
                yp = transform(yp, (int)canvasHeight / 2, Math.Abs(yp - (int)(canvasHeight / 2)));

                Ellipse ellipse = new Ellipse();
                ellipse.Fill = new SolidColorBrush(CIEtoRGB(X, Y, Z));
                ellipse.Width = 5;
                ellipse.Height = 5;
                canvas.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, xp);
                Canvas.SetTop(ellipse, yp);
                Canvas.SetZIndex(ellipse, 2);

                wavesOnCanvas.Add((wave.i, new Point(xp, yp)));
            }

            int transform(int y, int OY, int diff) => y < OY ? OY + diff : OY - diff;
            Color CIEtoRGB(double X, double Y, double Z)
            {
                byte R = adj(3.2404542 * X - 1.5371385 * Y - 0.4985314 * Z);
                byte G = adj(-0.9692660 * X + 1.8760108 * Y + 0.0415560 * Z);
                byte B = adj(0.0556434 * X - 0.2040259 * Y + 1.0572252 * Z);



                return Color.FromRgb(R, G, B);

                byte adj(double C)
                {
                    if (Math.Abs(C) < 0.0031308)
                    {
                        return (byte)(Math.Min(12.92 * C * 255, 255));
                    }
                    return (byte)(Math.Min((1.055 * Math.Pow(C, 0.41666) - 0.055) * 255, 255));
                }
            }
        }
        public void Initialize(Canvas canvas)
        {
            GetWaves();
            SetBorder(canvas);
            ProcessImage(canvas);
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
        void ProcessImage(Canvas canvas)
        {
            Point left = wavesOnCanvas.MinBy(w => w.p.X).p;
            Point right = wavesOnCanvas.MaxBy(w => w.p.X).p;
            Point top = wavesOnCanvas.MaxBy(w => w.p.Y).p;
            Point bottom = wavesOnCanvas.MinBy(w => w.p.Y).p;

            /*Image chromaticDiagram = new Image
            {
                Width = right.X - left.X + 10,
                Height = top.Y - bottom.Y + 40,
                Source = new BitmapImage(new Uri("//Mac/Home/Documents/SEM5/GK/Proj3/Proj3/bin/Debug/net6.0-windows/chromaticDiagram1.png", UriKind.Absolute))
            };
            chromaticDiagram.RenderTransform = new RotateTransform(3);
            canvas.Children.Add(chromaticDiagram);
            Canvas.SetLeft(chromaticDiagram, left.X);
            Canvas.SetTop(chromaticDiagram, bottom.Y - 30);*/

            Image chromaticDiagram = new Image
            {
                Width = right.X - left.X + 20,
                Height = top.Y - bottom.Y + 20,
                Source = new BitmapImage(new Uri("//Mac/Home/Documents/SEM5/GK/Proj3/Proj3/bin/Debug/net6.0-windows/chromaticDiagram1.png", UriKind.Absolute))
            };
            chromaticDiagram.RenderTransform = new RotateTransform(3.5);
            canvas.Children.Add(chromaticDiagram);
            Canvas.SetLeft(chromaticDiagram, left.X + 3);
            Canvas.SetTop(chromaticDiagram, bottom.Y - 18);
        }
    }
}
