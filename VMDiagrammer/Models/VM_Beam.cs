using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Typical data for a beam object in our Model
    /// </summary>
    public class VM_Beam : BaseVMObject, IDrawingObjects
    {
        private VM_Node m_Start;   // start node for the beam
        private VM_Node m_End;     // end node for the beam
        private VM_Node m_MidPoint; // midpoint of our beam element

        /// <summary>
        /// public accessor for the start node
        /// </summary>
        public VM_Node Start
        {
            get => m_Start;
            set
            {
                m_Start = value;
            }
        }

        /// <summary>
        /// public accessor for the end node
        /// </summary>
        public VM_Node End
        {
            get => m_End;
            set
            {
                m_End = value;
            }
        }

        public VM_Node MidPoint
        {
            get => m_MidPoint;
            set
            {
                m_MidPoint = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="start">start node for the beam</param>
        /// <param name="end">end node for the beam</param>
        public VM_Beam(VM_Node start, VM_Node end)
        {
            Start = start;
            End = end;
            MidPoint = new VM_Node(0.5 * (Start.X + End.X), 0.5 * (Start.Y + End.Y));
        }

        /// <summary>
        /// Method to draw the beam object
        /// </summary>
        /// <param name="c"></param>
        public void Draw(Canvas c)
        {
            DrawingHelpers.DrawLine(c, this.Start.X, this.Start.Y, this.End.X, this.End.Y, Brushes.Red);
            DrawingHelpers.DrawText(c, 0.5 * (this.Start.X + this.End.X), 0.5 * (this.Start.Y + this.End.Y), 0, this.Index.ToString());
        }
    }
}
