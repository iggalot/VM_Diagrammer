using System;
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
            // If the start node is to the right of the end node, reverse their order in this data object
            if(start.X < end.X)
            {
                Start = start;
                End = end;
            } else
            {
                Start = end;
                End = start;
            }

            MidPoint = new VM_Node(0.5 * (Start.X + End.X), 0.5 * (Start.Y + End.Y));
        }

        /// <summary>
        /// Method to draw the beam object
        /// </summary>
        /// <param name="c"></param>
        public void Draw(Canvas c)
        {
            // Draw the beam object line
            DrawingHelpers.DrawLine(c, this.Start.X, this.Start.Y, this.End.X, this.End.Y, Brushes.Red);

            // Draw the beam label
            DrawBeamLabel(c, this.MidPoint.X, this.MidPoint.Y, 0);
        }

        private void DrawBeamLabel(Canvas c, double x, double y, double z, double size = DrawingHelpers.DEFAULT_TEXT_HEIGHT, TextPositions pos = TextPositions.TEXT_BELOW)
        {
            double xpos = x-3;
            double ypos = y;
            double zpos = z;

            double offset = 15;
            Brush color = Brushes.Green;

            switch (pos)
            {
                case TextPositions.TEXT_ABOVE:
                    ypos -= 2.5 * size;
                    break;
                case TextPositions.TEXT_BELOW:
                    ypos += 0.5 * size + 2;
                    break;
                case TextPositions.TEXT_LEFT:
                case TextPositions.TEXT_RIGHT:
                default:
                    throw new NotImplementedException("Invalid text position, " + pos + " detected in DrawText function");
            }

            // Draw the text
            DrawingHelpers.DrawText(c, xpos, ypos, 0, this.Index.ToString(), color, DrawingHelpers.DEFAULT_TEXT_HEIGHT);

            // Draw the box around the text
            DrawingHelpers.DrawLine(c, xpos - 0.25 * offset, ypos, xpos + 1.25 * offset, ypos, color);
            DrawingHelpers.DrawLine(c, xpos + 1.25 * offset, ypos, xpos + 1.25 * offset, ypos + offset, color);
            DrawingHelpers.DrawLine(c, xpos + 1.25 * offset, ypos + offset, xpos - 0.25 * offset, ypos + offset, color);
            DrawingHelpers.DrawLine(c, xpos - 0.25 * offset, ypos + offset, xpos - 0.25 * offset, ypos, color);

            //DrawingHelpers.DrawCircleHollow(c, xpos + 3, ypos + 8, Brushes.Black, 1.5 * DrawingHelpers.DEFAULT_TEXT_HEIGHT);
        }
    }
}
