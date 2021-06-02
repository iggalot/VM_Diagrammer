using System;
using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Class definition for the distributed force
    /// </summary>
    public class VM_DistributedForce : VM_BaseLoad
    {
        public VM_DistributedForce(VM_Beam beam,  double d1, double d2, double w1, double w2) : base(beam, LoadTypes.LOADTYPE_DIST_FORCE, d1, d2, w1, w2, 1.0)
        {
            // is D1 to the left of the start node?
            if (D1 < 0)
                throw new NotImplementedException("Dimension D1 = " + D1 + " cannot be less than zero!");

            // are the intensities of W1 and W2 opposite signs
            if(((W1 < 0) && (W2 > 0)) || ((W1 > 0) && (W2 < 0)))
                throw new NotImplementedException("Distributed load magnitude W1 = " + W1 + " and W2 = " + W2 + " -- load intensities cannot be opposite signs!");

            // is D2 beyond the end of the beam
            if (beam.Start.X + D2 > beam.End.X)
                throw new NotImplementedException("Dimension D2 = " + D2 + " -- dimension is beyond the end of the beam");
        }

        /// <summary>
        /// Draws the distributed load diagram
        /// </summary>
        /// <param name="c"></param>
        public override void Draw(Canvas c)
        {
            double len1 = Math.Abs(W1);
            double len2 = Math.Abs(0.5 *(W1+W2));
            double len3 = Math.Abs(W2);


            if (W1 < 0)
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, this.ArrowThickness, len1);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + 0.5 * (D1 + D2), Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, this.ArrowThickness, len2);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, this.ArrowThickness, len3);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y - len1, Beam.Start.X + D2, Beam.Start.Y - len3, Brushes.Black, 3*this.ArrowThickness);
            }
            else
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, this.ArrowThickness, len1);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + 0.5 * (D1 + D2), Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, this.ArrowThickness, len2);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, this.ArrowThickness, len3);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y + len1, Beam.Start.X + D2, Beam.Start.Y + len3, Brushes.Black, 3*this.ArrowThickness);
            }
        }

    }
}
