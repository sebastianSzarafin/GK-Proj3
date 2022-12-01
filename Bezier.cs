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
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Proj3
{
    internal class Bezier
    {
        static int canvasWidth = 350, canvasHeight = 350;
        static int offset = 20;
        public int bezierPoints = 0;
        Vertex start, end;
        List<Vertex> points;
        List<Edge> edges;

        public Bezier() 
        {
            start = new Vertex(new Point(offset, canvasHeight / 2), Colors.Black, Colors.Green);
            end = new Vertex(new Point(canvasWidth - offset, canvasHeight / 2), Colors.Black, Colors.Green);
            /*edges = new List<Edge>() { new Edge(start, end) };
            start.e1 = edges[0];
            end.e1 = edges[0];*/
            edges = new List<Edge>() { Vertex.ConnectVertices(start, end) };
            points = new List<Vertex>(); 
        }

        public void Initialize(Canvas canvas)
        {
            canvas.Children.Add(start);
            canvas.Children.Add(end);
            canvas.Children.Add(edges[0]);
        }
        public void ResetCurve(Canvas canvas)
        {
            /*canvas.Children.RemoveRange(1, canvas.Children.Count - 1);
            edges.Clear();
            points.Clear();
            start = new Vertex(new Point(offset, canvasHeight / 2), Colors.Black, Colors.Green);
            end = new Vertex(new Point(canvasWidth - offset, canvasHeight / 2), Colors.Black, Colors.Green);
            if(bezierPoints == 0)
            {
                edges = new List<Edge>() { new Edge(start, end) };
                start.e1 = edges[0];
                end.e1 = edges[0];
            }
            else
            {
                double step = (end.position.X - start.position.X) / (bezierPoints + 1);
                double y = 50, dir = 1;
                for (double i = start.position.X + step; i < end.position.X; i += step)
                {
                    points.Add(new Vertex(new Point(i, start.position.Y + y * dir), Colors.Blue, Colors.Gray));
                    dir *= -1;
                }
                edges.Add(new Edge(start, points[0]));
                start.e1 = edges[0];
                points[0].e1 = edges[0];
                for (int i = 0; i < points.Count - 1; i++) edges.Add(new Edge(points[i], points[i + 1]));
                for(int i = 1; i < points.Count - 1; i++)
                {
                    points[i].
                }
                edges.Add(new Edge(points[points.Count - 1], end));
                end.e1 = edges[edges.Count - 1];
                points[points.Count - 1].e2 = edges[edges.Count - 1];
            }

            foreach (Edge e in edges) canvas.Children.Add(e);
            foreach (Vertex p in points) canvas.Children.Add(p);
            canvas.Children.Add(start);
            canvas.Children.Add(end);*/

            canvas.Children.RemoveRange(1, canvas.Children.Count - 1);
            edges.Clear();
            points.Clear();
            start = new Vertex(new Point(offset, canvasHeight / 2), Colors.Black, Colors.Green);
            end = new Vertex(new Point(canvasWidth - offset, canvasHeight / 2), Colors.Black, Colors.Green);
            
            if (bezierPoints == 0) edges = new List<Edge>() { Vertex.ConnectVertices(start, end) };
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
            if (IsMouseCaptured)
            {
                position = e.GetPosition(this);
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
    public class Edge :UIElement
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
            drawingContext.DrawLine(new Pen(Brushes.Blue, 2), p1.position, p2.position);
        }
    }
}
