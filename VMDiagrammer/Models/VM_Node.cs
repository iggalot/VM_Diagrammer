using System.Windows.Controls;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x position on the canvas</param>
        /// <param name="y">y position on the canvas</param>
        public VM_Node(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The method to draw this object
        /// </summary>
        /// <param name="c"></param>
        public void Draw(Canvas c)
        {
            DrawingHelpers.DrawCircle(c, this.X, this.Y);
            DrawingHelpers.DrawText(c, this.X, this.Y, 0, Index.ToString());
        }
    }
}
