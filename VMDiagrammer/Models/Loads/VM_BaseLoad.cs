using System.Windows.Controls;
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

    /// <summary>
    /// A base load object for loads.
    /// </summary>
    public class VM_BaseLoad : IDrawingObjects
    {
        private LoadTypes m_LoadType = LoadTypes.LOADTYPE_UNDEFINED;
        private VM_Beam m_Beam = null;

        private double m_d1 = 0; // distance from start to start of load
        private double m_d2 = 0; // distance from start to end of load
        private double m_w1 = 0; // intensity at start of load
        private double m_w2 = 0; // intensity at end of load

        private double m_ArrowThickness = 1; // thickness of the lines for the load arrows

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

        public double ArrowThickness
        {
            get => m_ArrowThickness;
            set
            {
                m_ArrowThickness = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="beam">associated beam object</param>
        /// <param name="load_type">enum for the type of load <see cref="LoadTypes"/></param>
        /// <param name="d1">distance from start (left node) to location of load (or start of distributed load)</param>
        /// <param name="d2">distance from start (left node) to location of load (or end of distributed load)</param>
        /// <param name="w1">intensity of load at start location</param>
        /// <param name="w2">intensity of load at end location</param>
        /// <param name="thickness">thickness of the associated load arrows</param>
        public VM_BaseLoad(VM_Beam beam, LoadTypes load_type, double d1, double d2, double w1, double w2, double thickness)
        {
            Beam = beam;
            LoadType = load_type;
            ArrowThickness = thickness;

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
}
