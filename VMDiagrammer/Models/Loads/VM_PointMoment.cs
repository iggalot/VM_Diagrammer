using System;
using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Class definition for the point moment
    /// </summary>
    public class VM_PointMoment : VM_BaseLoad
    {
        private ArrowDirections m_ArrowDirection = ArrowDirections.ARROW_COUNTERCLOCKWISE;

        public ArrowDirections Sense
        {
            get
            {
                return m_ArrowDirection;
            }
            set
            {
                m_ArrowDirection = value;
            }
        }

        public VM_PointMoment(VM_Beam beam, double d1, double d2, double w1, double w2, ArrowDirections dir) : base(beam, LoadTypes.LOADTYPE_CONC_MOMENT, d1, d2, w1, w2, 3.0)
        {
            if (D1 != D2)
                throw new NotImplementedException("D1 = " + D1 + " and D2 = " + D2 + " -- dimensions muse be the same for a point moment");
            if (W1 != W2)
                throw new NotImplementedException("W1 = " + W1 + " and W2 = " + W2 + " -- intensities muse be the same for a point moment");

            Sense = dir;
        }

        public override void Draw(Canvas c)
        {
            // Draw a dot at the center of the arrow
            DrawingHelpers.DrawCircle(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, 4, 1);

            // Draw the arrow
            DrawingHelpers.DrawCircularArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Transparent, Brushes.Black, Sense);
        }

    }
}
