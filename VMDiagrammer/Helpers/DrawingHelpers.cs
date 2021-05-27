using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VMDiagrammer.Models;

namespace VMDiagrammer.Helpers
{

    public enum ArrowDirections
    {
        ARROW_UP = 0,
        ARROW_RIGHT = 1,
        ARROW_DOWN = 2,
        ARROW_LEFT = 3
    }

    public enum TextPositions
    {
        TEXT_ABOVE = 0,
        TEXT_BELOW = 1,
        TEXT_LEFT = 2,
        TEXT_RIGHT = 3
    }
    /// <summary>
    /// A class for drawing shapes onto a WPF canvas
    /// </summary>
    public class DrawingHelpers
    {

        public const double DEFAULT_ARROW_SHAFTLENGTH = 20;
        public const double DEFAULT_ARROW_HEADLENGTH = 8;
        public const double DEFAULT_ARROW_THICKNESS = 3;

        public const double DEFAULT_TEXT_HEIGHT = 12.0;


        /// <summary>
        /// Draws a basic circle (ellipse) on a WPF canvas
        /// </summary>
        /// <param name="c">the WPF canvas object</param>
        /// <param name="x">the upper left x-coordinate for a bounding box around the node</param>
        /// <param name="y">the upper left y-coordinate for a bounding box around the node</param>
        /// <returns></returns>
        public static Shape DrawCircle(Canvas c, double x, double y, Brush fill, Brush stroke, double radius)
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

        public static Shape DrawCircleHollow(Canvas c, double x, double y, Brush stroke, double radius)
        {
            return DrawCircle(c, x, y, Brushes.Transparent, stroke, radius);
        }

        /// <summary>
        /// Draws a basic line object on a WPF canvas
        /// </summary>
        /// <param name="c">the WPF canvas object</param>
        /// <param name="ex">end point x-coord</param>
        /// <param name="ey">end point y-coord</param>
        /// <param name="sx">start point x_coord</param>
        /// <param name="sy">start point y-coord</param>
        /// <param name="stroke">color of the line object as a <see cref="Brush"/></param>
        /// <returns>the Shape object</returns>
        public static Shape DrawLine(Canvas c, double sx, double sy, double ex, double ey, Brush stroke, double thickness = 2.0)
        {
            Line myLine = new Line();
            myLine.Stroke = stroke;
            myLine.StrokeThickness = thickness;
            myLine.X1 = sx;
            myLine.Y1 = sy;
            myLine.X2 = ex;
            myLine.Y2 = ey;

            c.Children.Add(myLine);

            return myLine;
        }

        public static void DrawText(Canvas c, double x, double y, double z, string str, Brush brush, double size)
        {
            double xpos = x;
            double ypos = y;
            double zpos = z;

            if (string.IsNullOrEmpty(str))
                return;
            // Draw text
            TextBlock textBlock = new TextBlock();
            textBlock.Text = str;
            textBlock.FontSize = size;
            textBlock.Foreground = brush;

            Canvas.SetLeft(textBlock, xpos);
            Canvas.SetTop(textBlock, ypos);

            c.Children.Add(textBlock);
        }

        public static void DrawArrow(Canvas c, double x, double y, Brush fill, Brush stroke, ArrowDirections dir, double thickness, double shaft_len=DEFAULT_ARROW_SHAFTLENGTH, double head_len = DEFAULT_ARROW_HEADLENGTH)
        {
            switch (dir)    
            {
                case ArrowDirections.ARROW_DOWN:
                    DrawArrowDown(c, x, y, fill, stroke, thickness, shaft_len, head_len);
                    break;
                case ArrowDirections.ARROW_UP:
                    DrawArrowUp(c, x, y, fill, stroke, thickness, shaft_len, head_len);
                    break;
                case ArrowDirections.ARROW_RIGHT:
                case ArrowDirections.ARROW_LEFT:
                default:
                    throw new NotImplementedException("draw function not defined for arrow of direction = " + dir);
                    break;
            }
        }

        public static void DrawArrowDown(Canvas c, double x, double y, Brush fill, Brush stroke, double thickness, double shaft_len, double head_len)
        {
            DrawLine(c, x, y, x, y - shaft_len, stroke, thickness);
            DrawLine(c, x, y, x - head_len, y - head_len, stroke, thickness);
            DrawLine(c, x, y, x + head_len, y - head_len, stroke, thickness);
        }

        public static void DrawArrowUp(Canvas c, double x, double y, Brush fill, Brush stroke, double thickness, double shaft_len, double head_len)
        {
            DrawLine(c, x, y, x, y + shaft_len, stroke, thickness);
            DrawLine(c, x, y, x - head_len, y + head_len, stroke, thickness);
            DrawLine(c, x, y, x + head_len, y + head_len, stroke, thickness);
        }
    }
}
