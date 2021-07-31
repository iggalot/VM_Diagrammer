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

        /// <summary>
        /// Compute the length of the beam based on its start and end nodes.
        /// </summary>
        public double Length
        {
            get => Math.Sqrt((End.X - Start.X) * (End.X - Start.X) + (End.Y - Start.Y) * (End.Y - Start.Y));
        }

        public double Area { get; set; }

        public double Inertia { get; set; }

        public double YoungsModulus { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="start">start node for the beam</param>
        /// <param name="end">end node for the beam</param>
        /// <param name="index"></param>
        /// <param name="area">cross sectional area of the beam member (normal to length)</param>
        /// <param name="youngsModulus">Young's modulus for the material</param>
        /// <param name="inertia">moment of inertia of the beam member (with respect to horizontal axis of the cross section</param>
        public VM_Beam(VM_Node start, VM_Node end, int index, double area=1, double youngsModulus=1, double inertia=1) : base(index)
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

            Area = area;
            YoungsModulus = youngsModulus;
            Inertia = inertia;

            // Set the line thickness for this object when drawn
            Thickness = 8.0;

        }
        public override string ToString()
        {
            return Index.ToString() + " -- START: " + Start.Index.ToString() + "    END: " + End.Index.ToString() + "\n";
        }

        /// <summary>
        /// Method to draw the beam object
        /// </summary>
        /// <param name="c"></param>
        public void Draw(Canvas c)
        {
            // Draw the beam object line
            DrawingHelpers.DrawLine(c, this.Start.X, this.Start.Y, this.End.X, this.End.Y, Brushes.Red, Thickness);

            // Draw the beam label
            DrawBeamLabel(c, 0.5 * (Start.X + End.X), 0.5 * (Start.Y + End.Y), 0);
        }

        /// <summary>
        /// Draws a beam label (text and graphic) for a beam.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="size"></param>
        /// <param name="pos"></param>
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
