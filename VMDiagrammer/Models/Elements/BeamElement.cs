using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMDiagrammer.Models.Elements
{
    public class BeamElement
    {
        private VM_Node m_Start = null;
        private VM_Node m_End = null;

        private double m_Area = 1.0;
        private double m_Length = 1.0;
        private double m_Inertia = 1.0;
        private double m_YoungsModulus = 1.0;

        // Stiffness Element for this beam
        public double[,] StiffnessElement = null;

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
        public BeamElement(VM_Node start, VM_Node end, double area, double length, double young, double inertia)
        {
            m_Start = start;
            m_End = end;
            A = area;
            L = length;
            E = young;
            I = inertia;
            
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
                { 0         ,     6.0*E*I / (L*L),     2.0*E*I / (L),              0 ,    -6.0*E*I / (L*L),   4.0*E*I / (L) },
            };
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
