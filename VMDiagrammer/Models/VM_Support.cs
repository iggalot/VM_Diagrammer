using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Enum for types of supports
    /// </summary>
    public enum SupportTypes
    {
        SUPPORT_UNDEFINED   = -1,
        SUPPORT_ROLLER_X    = 0,
        SUPPORT_ROLLER_Y    = 1,
        SUPPORT_PIN         = 2,
        SUPPORT_FIXED_HOR   = 3,
        SUPPORT_FIXED_VERT  = 4
    }

    public class VM_Support : VM_Node
    {
        private SupportTypes m_SupportType = SupportTypes.SUPPORT_UNDEFINED;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x-coord insert point</param>
        /// <param name="y">y-coord insert point</param>
        /// <param name="support">type of Support <see cref="SupportTypes"/></param>
        public VM_Support(double x, double y, SupportTypes support)
        {
            this.X = x;
            this.Y = y;
            m_SupportType = support;
        }

        public new void Draw(Canvas c)
        {
            double radius = 10.0;
            double offset = radius / 2.0;

            switch (m_SupportType)
            {
                case SupportTypes.SUPPORT_UNDEFINED:
                    break;
                case SupportTypes.SUPPORT_ROLLER_X:
                    {
                        // Draw the roller "ball"
                        DrawingHelpers.DrawCircleHollow(c, this.X, this.Y,Brushes.Blue,10.0);

                        // Draw the surface line
                        VM_Node start = new VM_Node();
                        start.X = this.X - radius;
                        start.Y = this.Y + offset;

                        VM_Node end = new VM_Node();
                        end.X = this.X + radius;
                        end.Y = this.Y + offset;
                        DrawingHelpers.DrawLine(c, start, end, Brushes.Blue);
                    }
                    break;
                case SupportTypes.SUPPORT_ROLLER_Y:
                    break;
                case SupportTypes.SUPPORT_PIN:
                    {                     
                        VM_Node insert = new VM_Node(this.X,this.Y+offset);

                        VM_Node start = new VM_Node();
                        start.X = insert.X - radius;
                        start.Y = insert.Y + 2.0*offset;

                        VM_Node end = new VM_Node();
                        end.X = insert.X + radius;
                        end.Y = insert.Y + 2.0*offset;

                        VM_Node base_start = new VM_Node();
                        base_start.X = start.X - radius;
                        base_start.Y = start.Y;

                        VM_Node base_end = new VM_Node();
                        base_end.X = end.X + radius;
                        base_end.Y = end.Y;

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, start, insert, Brushes.Green);
                        DrawingHelpers.DrawLine(c, insert, end, Brushes.Green);
                        DrawingHelpers.DrawLine(c, end, start, Brushes.Green);

                        // Draw the base line
                        DrawingHelpers.DrawLine(c, base_start, base_end, Brushes.Green);



                        break;
                    }

                case SupportTypes.SUPPORT_FIXED_HOR:
                    {
                        VM_Node insert = new VM_Node(this.X, this.Y);

                        VM_Node start = new VM_Node();
                        start.X = insert.X - radius;
                        start.Y = insert.Y - 3.0 * offset;

                        VM_Node end = new VM_Node();
                        end.X = insert.X - radius;
                        end.Y = insert.Y + 3.0 * offset;

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, start, end, Brushes.Red);

                        break;
                    }
                   

                case SupportTypes.SUPPORT_FIXED_VERT:
                    break;

                default:
                    break;
            }
        }
    }
}
