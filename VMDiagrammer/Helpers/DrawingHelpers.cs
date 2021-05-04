using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VMDiagrammer.Models;

namespace VMDiagrammer.Helpers
{
    /// <summary>
    /// A class for drawing shapes onto a WPF canvas
    /// </summary>
    public class DrawingHelpers
    {
        /// <summary>
        /// Draws a basic circle (ellipse) on a WPF canvas
        /// </summary>
        /// <param name="c">the WPF canvas object</param>
        /// <param name="x">the upper left x-coordinate for a bounding box around the node</param>
        /// <param name="y">the upper left y-coordinate for a bounding box around the node</param>
        /// <returns></returns>
        public static Shape DrawCircle(Canvas c, double x, double y, Brush fill, Brush stroke, double radius=30.0)
        {
            // Draw circle node
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = fill;
            myEllipse.Stroke = stroke;
            myEllipse.StrokeThickness = 2.0;

            myEllipse.Width = radius;
            myEllipse.Height = radius;

            Canvas.SetLeft(myEllipse, x - myEllipse.Width / 2.0);
            Canvas.SetTop(myEllipse, y - myEllipse.Height / 2.0);

            c.Children.Add(myEllipse);

            return myEllipse;
        }

        public static Shape DrawCircleHollow(Canvas c, double x, double y, Brush stroke, double radius = 30.0)
        {
            return DrawCircle(c, x, y, Brushes.Transparent, stroke, radius);
        }

        /// <summary>
        /// Draws a basic line object on a WPF canvas
        /// </summary>
        /// <param name="c">the WPF canvas object</param>
        /// <param name="start">the starting VM_Node</param>
        /// <param name="end">the ending VM_Node</param>
        /// <returns></returns>
        public static Shape DrawLine(Canvas c, VM_Node start, VM_Node end, Brush stroke)
        {
            Line myLine = new Line();
            myLine.Stroke = stroke;
            myLine.StrokeThickness = 2.0;
            myLine.X1 = start.X;
            myLine.Y1 = start.Y;
            myLine.X2 = end.X;
            myLine.Y2 = end.Y;

            c.Children.Add(myLine);

            return myLine;
        }

        public static void DrawText(Canvas c, double x, double y, double z, string str)
        {
            if (string.IsNullOrEmpty(str))
                return;
            // Draw text
            TextBlock textBlock = new TextBlock();
            textBlock.Text = str;
            textBlock.FontSize = 24.0;
            textBlock.Foreground = Brushes.Green;

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);

            c.Children.Add(textBlock);
        }
    }
}
