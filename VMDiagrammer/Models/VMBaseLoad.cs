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
            W1 = w1;
            W2 = w2;
            D1 = d1;
            D2 = d2;
        }

        public virtual void Draw(Canvas c) { }
    }

    public class VM_PointForce : VMBaseLoad
    {
        public VM_PointForce(VM_Beam beam, LoadTypes load_type, double d1, double d2, double w1, double w2) : base(beam, load_type, d1, d2, w1, w2)
        {

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

        }

        public override void Draw(Canvas c)
        {
            double len1 = Math.Abs(W1);
            double len2 = Math.Abs(0.5 *(W1+W2));
            double len3 = Math.Abs(W2);


            if (W1 < 0)
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.MidPoint.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, len1);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + 0.5 * (D1 + D2), Beam.MidPoint.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, len2);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.MidPoint.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_DOWN, len3);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y - len1, Beam.Start.X + D2, Beam.Start.Y - len3, Brushes.Black);
            }
            else
            {
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D1, Beam.MidPoint.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, len1);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + 0.5 * (D1 + D2), Beam.MidPoint.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, len2);
                DrawingHelpers.DrawArrow(c, Beam.Start.X + D2, Beam.MidPoint.Y, Brushes.Black, Brushes.Black, ArrowDirections.ARROW_UP, len3);
                DrawingHelpers.DrawLine(c, Beam.Start.X + D1, Beam.Start.Y + len1, Beam.Start.X + D2, Beam.Start.Y + len3, Brushes.Black);
            }
        }

    }
}
