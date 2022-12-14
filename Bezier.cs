using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Proj3
{
    internal class Bezier
    {
        static List<(int i, Point3D p)> chromaticWaves = new List<(int i, Point3D p)>();
        static Ellipse chromaticPoint = new Ellipse() { Width = 10, Height = 10, Stroke = Brushes.Black, Fill = Brushes.Red };
        const int canvasWidth = 350, canvasHeight = 350;
        const int offset = 20, wavesCount = 341;
        const double step = 0.002;
        public int bezierPoints = 0;
        Vertex start, end;
        List<Vertex> points;
        List<Edge> edges;
        List<Rectangle> curve;
        Point[] Fcurve;
        Rectangle chromaticPointColorRectangle;
        public (double X, double Y, double Z) integral
        {
            get
            {
                double X = 0, Y = 0, Z = 0;
                for (int i = 0; i < curve.Count - 1; i++)
                {
                    if (Fcurve[i] == default(Point) || Fcurve[i + 1] == default(Point)) continue;

                    int id = (int)Fcurve[i].X;
                    double dx = Fcurve[i + 1].X - Fcurve[i].X;
                    X += Fcurve[i].Y * chromaticWaves[id].p.X * dx;
                    Y += Fcurve[i].Y * chromaticWaves[id].p.Y * dx;
                    Z += Fcurve[i].Y * chromaticWaves[id].p.Z * dx;
                }
                return (X, Y, Z);
            }
        }

        public Bezier(Rectangle _chromaticPointColorRectangle) 
        {
            start = new Vertex(new Point(offset, canvasHeight / 2), Colors.Black, Colors.Green);
            end = new Vertex(new Point(canvasWidth - offset, canvasHeight / 2), Colors.Black, Colors.Green);
            edges = new List<Edge>();
            points = new List<Vertex>();
            curve = new List<Rectangle>();
            chromaticPointColorRectangle = _chromaticPointColorRectangle;
            Fcurve = new Point[500];
        }

        public void Initialize(Canvas bezierCanvas, List<(int i, Point3D p)> waves, Canvas chromaticCanvas)
        {
            bezierCanvas.Children.Add(start);
            bezierCanvas.Children.Add(end);
            for (double t = 0; t <= 1; t += step)
            {
                Rectangle r = new Rectangle() { Width = 1, Height = 2, Fill = Brushes.Black };
                Canvas.SetLeft(r, start.position.X + t * (end.position.X - start.position.X));
                Canvas.SetTop(r, start.position.Y);
                curve.Add(r);
                bezierCanvas.Children.Add(r);
            }
            chromaticWaves = waves;
            chromaticCanvas.Children.Add(chromaticPoint);
            DrawCurve(bezierCanvas);
            DrawChromaticPoint();
            Canvas.SetZIndex(chromaticPoint, 2);
        }
        public void ResetCurve(Canvas canvas)
        {
            canvas.Children.RemoveRange(1, canvas.Children.Count - 1);
            edges.Clear();
            points.Clear();
            start = new Vertex(new Point(offset, canvasHeight / 2), Colors.Black, Colors.Green);
            end = new Vertex(new Point(canvasWidth - offset, canvasHeight / 2), Colors.Black, Colors.Green);
            
            if (bezierPoints == 0) edges = new List<Edge>();
            else
            {
                double step = (end.position.X - start.position.X) / (bezierPoints + 1);
                double y = 50, dir = 1;
                for (double i = start.position.X + step; Math.Ceiling(i) < end.position.X; i += step)
                {
                    points.Add(new Vertex(new Point(i, start.position.Y + y * dir), Colors.Blue, Colors.Gray));
                    dir *= -1;
                }
                edges.Add(Vertex.ConnectVertices(points[0], start));
                for (int i = 1; i < points.Count; i++)
                {
                    if (i == 1) edges.Add(Vertex.ConnectVertices(points[i], points[i - 1]));
                    if (i + 1 < points.Count) edges.Add(Vertex.ConnectVertices(points[i], points[i + 1]));

                }
                edges.Add(Vertex.ConnectVertices(points[points.Count - 1], end));
            }

            foreach (Edge e in edges) canvas.Children.Add(e);
            foreach (Vertex p in points) canvas.Children.Add(p);
            canvas.Children.Add(start);
            canvas.Children.Add(end);

            DrawCurve(canvas);
        }
        public void DrawCurve(Canvas canvas)
        {
            for (int i = 0; i < Fcurve.Length; i++) Fcurve[i] = default(Point);

            int n = 2 + bezierPoints - 1;
            double[] binoms = new double[n + 1];
            for (int k = 0; k < n + 1; k++) binoms[k] = binomCoefficient(n, k);
            List<Vertex> curvePoints = new List<Vertex>(points);
            curvePoints.Insert(0, start);
            curvePoints.Insert(curvePoints.Count, end);
            double t = 0;
            foreach (Rectangle r in curve)
            {
                canvas.Children.Remove(r);

                int i = 0;
                double valX = 0, valY = 0;
                foreach(Vertex p in curvePoints)
                {
                    valX += binoms[i] * Math.Pow(1 - t, n - i) * Math.Pow(t, i) * p.position.X;
                    valY += binoms[i] * Math.Pow(1 - t, n - i) * Math.Pow(t, i) * p.position.Y;
                    i++;
                }

                Canvas.SetLeft(r, valX);
                Canvas.SetTop(r, valY);
                canvas.Children.Add(r);

                double pX = Math.Min(valX / canvasWidth, 1) * 340, pY = canvasHeight - valY;
                Fcurve[(int)pX].X = pX; Fcurve[(int)pX].Y = pY;

                t += step;
            }

            double binomCoefficient(double n, double k)
            {
                if (k > n) { return 0; }
                if (n == k) { return 1; }
                if (k > n - k) { k = n - k; }
                double c = 1;
                for (double i = 1; i <= k; i++)
                {
                    c *= n--;
                    c /= i;
                }
                return c;
            }
        }
        public void DrawChromaticPoint()
        {
            (double X, double Y, double Z) i = integral;
            double X = i.X / (i.X + i.Y + i.Z);
            double Y = i.Y / (i.X + i.Y + i.Z);
            double Z = i.Z / (i.X + i.Y + i.Z);

            int xp = (int)(X * canvasWidth) + offset;
            int yp = (int)(Y * canvasHeight) + offset;
            yp = transform(yp, canvasHeight / 2, Math.Abs(yp - canvasHeight / 2));

            Canvas.SetLeft(chromaticPoint, xp);
            Canvas.SetTop(chromaticPoint, yp);

            chromaticPointColorRectangle.Fill = new SolidColorBrush(CIEtoRGB(X, Y, Z));

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
    }

    public class Vertex : UIElement
    {
        public static readonly uint radius = 4;
        public readonly SolidColorBrush standardBrush;
        public readonly SolidColorBrush highlightedBrush;

        public Point position;
        private SolidColorBrush currentBrush;
        private Pen currentPen;
        public Edge? e1, e2;

        public Vertex(Point _position, Color sc, Color hc)
        {
            position = _position;
            standardBrush = new SolidColorBrush(sc);
            highlightedBrush = new SolidColorBrush(hc);
            currentBrush = standardBrush;
            currentPen = new Pen(standardBrush, radius);
            Canvas.SetZIndex(this, 2);
        }

        public static Edge ConnectVertices(Vertex v1, Vertex v2)
        {            
            Edge e = new Edge(v1, v2);
            if (v1.e1 == null)
            {
                v1.e1 = e;
                if (v2.e1 == null) v2.e1 = e;
                else v2.e2 = e;
            }
            else
            {
                v1.e2 = e;
                if (v2.e1 == null) v2.e1 = e;
                else v2.e2 = e;
            }
            return e;
        }

        public void Highlight()
        {
            currentBrush = highlightedBrush;
            currentPen = new Pen(highlightedBrush, radius);
            InvalidateVisual();
        }

        public void DeHighlight()
        {
            currentBrush = standardBrush;
            currentPen = new Pen(standardBrush, radius);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(currentBrush, currentPen, position, radius, radius);
            if (e1 != null) e1.InvalidateVisual();
            if (e2 != null) e2.InvalidateVisual();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            CaptureMouse();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Canvas parentCanvas = (Canvas)this.VisualParent;
            if (IsMouseCaptured)
            {
                position.X = Math.Min(Math.Max(e.GetPosition(this).X, radius), parentCanvas.ActualWidth - radius);
                position.Y = Math.Min(Math.Max(e.GetPosition(this).Y, radius), parentCanvas.ActualHeight - radius);
                InvalidateVisual();
            }
        }

        protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsMouseDirectlyOver)
            {
                Highlight();
            }
            else
            {
                DeHighlight();
            }
        }
    }
    public class Edge : UIElement
    {
        Vertex p1, p2;
        public Vertex P1 { get => p1; }
        public Vertex P2 { get => p2; }
        public Edge(Vertex _p1, Vertex _p2)
        {            
            p1 = _p1;
            p2 = _p2;
            Canvas.SetZIndex(this, 1);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(new Pen(Brushes.Blue, 1), p1.position, p2.position);
        }
    }
}
