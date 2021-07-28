using System;
using VMDiagrammer.Helpers;

namespace VMDiagrammer.Models.Elements
{

    public class BeamElement2D
    {
        private VM_Node m_Start = null;
        private VM_Node m_End = null;

        private double m_Area = 1.0;
        private double m_Length = 1.0;
        private double m_Inertia = 1.0;
        private double m_YoungsModulus = 1.0;
        private double m_Angle = 0;  // angle of member from start node to end node measured in radians

        private double[,] m_TransformationMatrix = null;


        // Stiffness Element for this beam
        private double[,] StiffnessElement = null;

        public double[,] TransformedGlobalStiffnessElement = null;

        public VM_Node StartNode
        {
            get => m_Start;
            set
            {
                m_Start = value;
            }
        }

        public VM_Node EndNode
        {
            get => m_End;
            set
            {
                m_Start = value;
            }
        }

        public double A
        {
            get => m_Area;
            set
            {
                m_Area = value;
            }
        }

        public double L
        {
            get => m_Length;
            set
            {
                m_Length = value;
            }
        }

        public double I
        {
            get => m_Inertia;
            set
            {
                m_Inertia = value;
            }
        }

        public double E
        {
            get => m_YoungsModulus;
            set
            {
                m_YoungsModulus = value;            }
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public BeamElement2D(VM_Node start, VM_Node end, double area, double length, double young, double inertia)
        {
            m_Start = start;
            m_End = end;
            m_Angle = Math.Atan((end.Y - start.Y) / (end.X - start.X));


            A = area;
            L = length;
            E = young;
            I = inertia;

            m_TransformationMatrix = new double[,]
            {
                // Degrees of freedom u1, v1, theta1, u2, v2, theta2
                // u - axial
                // v - sheear
                // theta - rotation
                // start_u                  start_v             start_theta              end_u                 end_v                 end_theta
                { Math.Cos(m_Angle)     ,   Math.Sin(m_Angle),      0,          0                     ,                   0,             0  },
                { -1 * Math.Sin(m_Angle),   Math.Cos(m_Angle),      0,          0                     ,                   0      ,       0  },
                {         0             ,   0                ,      1,          0                     ,                   0      ,       0  },
                {         0             ,   0                ,      0,          Math.Cos(m_Angle)     ,   Math.Sin(m_Angle)      ,       0  },
                {         0             ,   0                ,      0,          -1 * Math.Sin(m_Angle),   Math.Cos(m_Angle)      ,       0  },
                {         0             ,   0                ,      0,          0                     ,                   0      ,       1  }
            };

            //Console.WriteLine("----------- Transform -----------");
            //Console.WriteLine(MatrixOperations.Display(m_TransformationMatrix));

            StiffnessElement = new double[,]
            {
                // Degrees of freedom u1, v1, theta1, u2, v2, theta2
                // u - axial
                // v - sheear
                // theta - rotation
                // start_u          start_v             start_theta         end_u             end_v             end_theta
                { A*E / L   ,                   0,                 0,        -A*E / L,                   0,               0 },
                { 0         ,  12.0*E*I / (L*L*L),   6.0*E*I / (L*L),              0 , -12.0*E*I / (L*L*L),   6*E*I / (L*L) },
                { 0         ,     6.0*E*I / (L*L),     4.0*E*I / (L),              0 ,    -6.0*E*I / (L*L),   2.0*E*I / (L) },
                { -A*E / L  ,                   0,                 0,         A*E / L,                   0,               0 },
                { 0         , -12.0*E*I / (L*L*L),  -6.0*E*I / (L*L),              0 ,  12.0*E*I / (L*L*L),  -6*E*I / (L*L) },
                { 0         ,     6.0*E*I / (L*L),     2.0*E*I / (L),              0 ,    -6.0*E*I / (L*L),   4.0*E*I / (L) }
            };

            TransformedGlobalStiffnessElement = MatrixOperations.MatrixProduct(StiffnessElement, m_TransformationMatrix);
            if(!MatrixOperations.IsSymmetric(m_TransformationMatrix))
                Console.WriteLine("m_Transformation is not symmetric -- S: " + start.Index + " E: " + end.Index);
            if (!MatrixOperations.IsSymmetric(StiffnessElement))
                Console.WriteLine("StiffnessElement is not symmetric -- S: " + start.Index + " E: " + end.Index);
            if (!MatrixOperations.IsSymmetric(TransformedGlobalStiffnessElement))
                Console.WriteLine("TransformedGlobalStiffnessElement is not symmetric -- S: " + start.Index + " E: " + end.Index);
        }

        public override string ToString()
        {
            string str = "[ \n";

            
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    str += String.Format("{0}\t", StiffnessElement[i,j].ToString());
                    if (j < 5)
                    {
                        str += "   ,   ";
                    }
                }

                str += "\n";
            }

            str += " ]";
            return str;
        }
    }
}
