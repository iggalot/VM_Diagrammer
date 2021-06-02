using System;
using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Class definition for the point force
    /// </summary>
    public class VM_PointForce : VM_BaseLoad
    {
        public VM_PointForce(VM_Beam beam, double d1, double d2, double w1, double w2) : base(beam, LoadTypes.LOADTYPE_CONC_FORCE, d1, d2, w1, w2, 3.0)
        {
            if (D1 != D2)
                throw new NotImplementedException("D1 = " + D1 + " and D2 = " + D2 + " -- dimensions muse be the same for a point force");
            if (W1 != W2)
                throw new NotImplementedException("W1 = " + W1 + " and W2 = " + W2 + " -- intensities muse be the same for a point force");

        }

        public override void Draw(Canvas c)
        {
            double len1 = Math.Abs(W1);

            if (W1 < 0)
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Blue, Brushes.Blue, ArrowDirections.ARROW_DOWN, this.ArrowThickness, len1 );
            else
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Blue, Brushes.Blue, ArrowDirections.ARROW_UP, this.ArrowThickness, len1);
        }

    }
}
