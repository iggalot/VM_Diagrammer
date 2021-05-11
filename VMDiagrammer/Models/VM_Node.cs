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
            double radius = 10.0;
            double offset = radius / 2.0;

            switch (m_SupportType)
            {
                case SupportTypes.SUPPORT_ROLLER_X:
                    {
                        // Draw the roller "ball"
                        DrawingHelpers.DrawCircleHollow(c, this.X, this.Y, Brushes.Blue, 10.0);

                        // Draw the surface line
                        double startX = this.X - radius;
                        double startY = this.Y + offset;
                        double endX = this.X + radius;
                        double endY = this.Y + offset;

                        DrawingHelpers.DrawLine(c, startX, startY, endX, endY, Brushes.Blue);
                    }
                    break;
                case SupportTypes.SUPPORT_ROLLER_Y:
                    break;
                case SupportTypes.SUPPORT_PIN:
                    {
                        double insertX = this.X;
                        double insertY = this.Y + offset;
                        double startX = insertX - radius;
                        double startY = insertY+ 2.0 * offset;
                        double endX = insertX + radius;
                        double endY = insertY + 2.0 * offset;

                        double base_startX = startX - radius;
                        double base_startY = startY;

                        double base_endX = endX + radius;
                        double base_endY = endY;

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

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, startX, startY, endX, endY, Brushes.Red);

                        break;
                    }

                case SupportTypes.SUPPORT_UNDEFINED:
                case SupportTypes.SUPPORT_FIXED_VERT:
                default:
                    DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White);
                    DrawingHelpers.DrawText(c, this.X, this.Y, 0, Index.ToString());
                    break;


            }







        }
    }
}
