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

        public VMBaseLoad(VM_Beam beam, LoadTypes load_type, double d1, double d2, double w1, double w2)
        {
            Beam = beam;
            LoadType = load_type;

            // if the start of distributed load is to the left of the end
            if (d1 < d2)
            {
                W1 = w1;
                W2 = w2;
                D1 = d1;
                D2 = d2;
            }
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
        public VM_PointForce(VM_Beam beam, double d1, double d2, double w1, double w2) : base(beam, LoadTypes.LOADTYPE_CONC_FORCE, d1, d2, w1, w2)
        {
            if (D1 != D2)
                throw new NotImplementedException("D1 = " + D1 + " and D2 = " + D2 + " -- dimensions muse be the same for a point force");
            if (W1 != W2)
                throw new NotImplementedException("W1 = " + W1 + " and W2 = " + W2 + " -- intensities muse be the same for a point force");

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
        public VM_DistributedForce(VM_Beam beam,  double d1, double d2, double w1, double w2) : base(beam, LoadTypes.LOADTYPE_DIST_FORCE, d1, d2, w1, w2)
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

        public override void Draw(Canvas c)
        {
            double len1 = Math.Abs(W1);
            double len2 = Math.Abs(0.5 *(W1+W2));
            double len3 = Math.Abs(W2);


            if (W1 < 0)
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, len1);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + 0.5 * (D1 + D2), Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, len2);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, len3);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y - len1, Beam.Start.X + D2, Beam.Start.Y - len3, Brushes.Black);
            }
            else
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, len1);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + 0.5 * (D1 + D2), Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, len2);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.Start.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, len3);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y + len1, Beam.Start.X + D2, Beam.Start.Y + len3, Brushes.Black);
            }
        }

    }
}
