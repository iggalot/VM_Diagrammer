using System.Windows.Controls;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Typical data for a beam object in our Model
    /// </summary>
    public class VM_Beam : IDrawingObjects
    {
        private VM_Node m_Start;   // start node for the beam
        private VM_Node m_End;     // end node for the beam

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="start">start node for the beam</param>
        /// <param name="end">end node for the beam</param>
        public VM_Beam(VM_Node start, VM_Node end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Method to draw the beam object
        /// </summary>
        /// <param name="c"></param>
        public override void Draw(Canvas c)
        {
            DrawingHelpers.DrawLine(c, this.Start, this.End);
        }
    }
}
