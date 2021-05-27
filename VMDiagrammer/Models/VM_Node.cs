using System;
using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Data for a typical Node in our model
    /// </summary>
    public class VM_Node : BaseVMObject, IDrawingObjects
    {
        public const double DEFAULT_NODE_RADIUS = 15;

        /// <summary>
        /// Private members
        /// </summary>
        private double m_X;   // x-coordinate
        private double m_Y;   // y-coordinate
        private SupportTypes m_SupportType;  // type of support as an ENUM

        /// <summary>
        /// Accessor for the X coordinate of our node
        /// </summary>
        public double X
        {
            get => m_X;
            set { m_X = value; }
        }

        /// <summary>
        /// Accessor for the Y coordinate of our node
        /// </summary>
        public double Y
        {
            get => m_Y;
            set { m_Y = value; }
        }

        public SupportTypes SupportType
        {
            get => m_SupportType;
            set { m_SupportType = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x position on the canvas</param>
        /// <param name="y">y position on the canvas</param>
        public VM_Node(double x, double y, SupportTypes support = SupportTypes.SUPPORT_UNDEFINED)
        {
            X = x;
            Y = y;

            SupportType = support;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public VM_Node()
        {

        }

        /// <summary>
        /// The method to draw this object
        /// </summary>
        /// <param name="c"></param>
        public void Draw(Canvas c)
        {
            double radius = 15.0;
            double offset = radius / 2.0;

            switch (m_SupportType)
            {
                case SupportTypes.SUPPORT_ROLLER_X:
                    {
                        // Draw the node icon
                        DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS);

                        // Draw the roller "ball"
                        DrawingHelpers.DrawCircleHollow(c, this.X, this.Y+2.0 * offset, Brushes.Blue, radius);

                        // Draw the surface line
                        double startX = this.X - radius;
                        double startY = this.Y + offset;
                        double endX = this.X + radius;
                        double endY = this.Y + offset;

                        DrawingHelpers.DrawLine(c, startX, startY + 2.0 * offset, endX, endY + 2.0 * offset, Brushes.Blue);
                    }
                    break;
                case SupportTypes.SUPPORT_ROLLER_Y:
                    break;
                case SupportTypes.SUPPORT_PIN:
                    {
                        double insertX = this.X;
                        double insertY = this.Y + offset;
                        double startX = insertX - 0.75 * radius;
                        double startY = insertY+ 2.0 * offset;
                        double endX = insertX + 0.75 * radius;
                        double endY = insertY + 2.0 * offset;

                        double base_startX = startX - 0.5 * radius;
                        double base_startY = startY;

                        double base_endX = endX + 0.52 * radius;
                        double base_endY = endY;

                        // Draw the node icon
                        DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS);

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, startX, startY, insertX, insertY, Brushes.Green);
                        DrawingHelpers.DrawLine(c, insertX, insertY, endX, endY, Brushes.Green);
                        DrawingHelpers.DrawLine(c, endX, endY, startX, startY, Brushes.Green);

                        // Draw the base line
                        DrawingHelpers.DrawLine(c, base_startX, base_startY, base_endX, base_endY, Brushes.Green);

                        break;
                    }

                case SupportTypes.SUPPORT_FIXED_HOR:
                    {
                        double insertX = this.X;
                        double insertY = this.Y;
                        double startX = insertX - radius;
                        double startY = insertY - 3.0 * offset;
                        double endX = insertX - radius;
                        double endY = insertY + 3.0 * offset;

                        // Draw the node icon
                        DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS);

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, startX, startY, endX, endY, Brushes.Red);

                        break;
                    }

                case SupportTypes.SUPPORT_UNDEFINED:
                    // Default node
                    DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS);
                    break;
                case SupportTypes.SUPPORT_FIXED_VERT:
                default:
                    {
                        throw new NotImplementedException("Support type drawing ability not defined for support type: " + m_SupportType);
                    }
            }

            // Draw the node label
            DrawNodeLabel(c, this.X, this.Y, 0);
        }

        private void DrawNodeLabel(Canvas c, double x, double y, double z, double size=DrawingHelpers.DEFAULT_TEXT_HEIGHT, TextPositions pos=TextPositions.TEXT_ABOVE)
        {
            double xpos = x;
            double ypos = y;
            double zpos = z;

            switch (pos)
            {
                case TextPositions.TEXT_ABOVE:
                    ypos -= 2.5*size;
                    break;
                case TextPositions.TEXT_BELOW:
                    ypos += 0.5 * size -4;
                    break;
                case TextPositions.TEXT_LEFT:
                case TextPositions.TEXT_RIGHT:
                default:
                    throw new NotImplementedException("Invalid text position, " + pos + " detected in DrawText function");
            }

            // Draw the node icon
            DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS);
            
            // Draw the node label text
            DrawingHelpers.DrawText(c, xpos, ypos, zpos, Index.ToString(), Brushes.Black, DrawingHelpers.DEFAULT_TEXT_HEIGHT);

            // Draw the node label icon
            DrawingHelpers.DrawCircleHollow(c, xpos+3, ypos+8, Brushes.Black, 1.5 * DrawingHelpers.DEFAULT_TEXT_HEIGHT);
        }
    }
}
