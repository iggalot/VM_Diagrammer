using System;
using System.Windows.Controls;
using System.Windows.Media;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;

namespace VMDiagrammer.Models
{
    public enum LoadTypes
    {
        LOADTYPE_UNDEFINED = -1,
        LOADTYPE_CONC_FORCE = 0,
        LOADTYPE_CONC_MOMENT = 1,
        LOADTYPE_DIST_FORCE = 2,
        LOADTYPE_DIST_MOMENT = 3
    }
    public class VMBaseLoad : IDrawingObjects
    {
        private LoadTypes m_LoadType = LoadTypes.LOADTYPE_UNDEFINED;
        private VM_Beam m_Beam = null;

        private double m_d1 = 0; // distance from start to start of load
        private double m_d2 = 0; // distance from start to end of load
        private double m_w1 = 0; // intensity at start of load
        private double m_w2 = 0; // intensity at end of load

        public double D1
        {
            get => m_d1;
            set
            {
                m_d1 = value;
            }
        }
        public double D2
        {
            get => m_d2;
            set
            {
                m_d2 = value;
            }
        }

        public double W1
        {
            get => m_w1;
            set
            {
                m_w1 = value;
            }
        }

        public double W2
        {
            get => m_w2;
            set
            {
                m_w2 = value;
            }
        }

        public VM_Beam Beam
        {
            get => m_Beam;
            set
            {
                m_Beam = value;
            }
        }

        public LoadTypes LoadType
        {
            get => m_LoadType;
            set
            {
                m_LoadType = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="beam">the beam object this load is assigned to <see cref="VM_Beam"/></param>
        /// <param name="load_type">the enum of the load type <see cref="LoadTypes"/></param>
        /// <param name="d1">the load's start distance from the start node</param>
        /// <param name="d2">the load's end distance from the start node</param>
        /// <param name="w1">the intensity at the start of the load</param>
        /// <param name="w2">the intensity at the end of the load</param>
        public VMBaseLoad(VM_Beam beam, LoadTypes load_type, double d1, double d2, double w1, double w2)
        {
            Beam = beam;
            LoadType = load_type;             
            
            // if the start is to the left of the end
            if(d1 < d2)
            {
                W1 = w1;
                W2 = w2;
                D1 = d1;
                D2 = d2;
            }
            // otherwise reverse their order
            else
            {
                W1 = w2;
                W2 = w1;
                D1 = d2;
                D2 = d1;
            }

        }

        public virtual void Draw(Canvas c) { }
    }

    public class VM_PointForce : VMBaseLoad
    {
        public VM_PointForce(VM_Beam beam, LoadTypes load_type, double d1, double d2, double w1, double w2) : base(beam, load_type, d1, d2, w1, w2)
        {
            if (d1 != d2)
                throw new NotImplementedException("D1 = " + D1 + " and D2 = " + D2 + " -- dimensions must be the same for a point load");
        }

        public override void Draw(Canvas c)
        {
            if (W1 < 0)
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN);
            else
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP);
        }
    }

    public class VM_DistributedForce : VMBaseLoad
    {
        public VM_DistributedForce(VM_Beam beam, LoadTypes load_type, double d1, double d2, double w1, double w2) : base(beam, load_type, d1, d2, w1, w2)
        {
            // are the intensities of opposite sign?  Throw an error for now.  Loads must be same signs or zero in one term.
            if(d1 < 0)
                throw new NotImplementedException("Dimension D1 = " + D1 + " cannot be less than zero");
            if (((w1 < 0) && (w2 > 0) || (w1 > 0) && w2 < 0))
                throw new NotImplementedException("Distributed Load magnitude W1 = " + W1 + " and W2 = " + W2 + " -- intensities cannot be opposite signs!");
            if (beam.Start.X + D2 > beam.End.X)
                throw new NotImplementedException("Dimension D2 = " + D2 + " is beyond the end of the beam");
        }
        /// <summary>
        /// Draws a distributed load as three arrows with a line connecting the first and third arrow
        /// </summary>
        /// <param name="c"></param>
        public override void Draw(Canvas c)
        {
            // Draw three arrows for now, one at start of load, one at middle of load and one at end of load
            double scale_factor;
            if ((W1 == 0) || (W2 == 0))
                scale_factor = 2.0;
            else
                scale_factor = Math.Max(W1, W2) / Math.Min(W1, W2);

            // Cap the scaling factor to a reasonable limit;
            if (scale_factor > 5.0)
                scale_factor = 5.0;

            if (W1 < 0)
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, (W1 / W1) * scale_factor);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + (0.5 * (D1 + D2)), Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, (0.5 * (W1 + W2) / W1)*scale_factor);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, (W2 / W1) * scale_factor);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y - 20, Beam.Start.X + D2, Beam.End.Y - 20 * scale_factor, Brushes.Black);
            }

            else
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, (W1 / W1) * scale_factor);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + (0.5 * (D1 + D2)), Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, (0.5 * (W1 + W2) / W1) * scale_factor);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, (W2 / W1) * scale_factor);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y + 20, Beam.Start.X + D2, Beam.End.Y + 20 * scale_factor, Brushes.Black);
            }
        }
    }
}
