using System;
using System.Collections.Generic;
using VMDiagrammer.Models.Elements;

namespace VMDiagrammer.Models
{
    public class StructureStiffnessModel
    {
        private List<BeamElement> m_ElementList = new List<BeamElement>();
        private int m_Rows = 0;
        private int m_Cols = 0;

        public void AddElement(VM_Beam beam)
        {
            m_ElementList.Add(new BeamElement(beam.Start, beam.End, 1, 1, 1, 1));
        }

        public double[,] GlobalStiffnessMatrix { get; set; }

        /// <summary>
        /// Stiffness submatrices
        /// [
        ///    K_Free_Free       |       K_Free_Fixed     
        ///   -------------------------------------------
        ///    K_Fixed_Free      |       K_Fixed_Fixed    
        /// ]
        /// </summary>

        // Hold ours submatrices
        public double[,] K_Free_Free = null;
        public double[,] K_Free_Fixed = null;
        public double[,] K_Fixed_Free = null;
        public double[,] F_Fixed_Fixed = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rows">number of rows in the model</param>
        /// <param name="cols">number of cols in the model</param>
        public StructureStiffnessModel(int rows, int cols)
        {
            m_Rows = rows;
            m_Cols = cols;

            GlobalStiffnessMatrix = new double[rows, cols];
        }

        /// <summary>
        /// Assembles our model stiffness matrix
        /// </summary>
        public void Assemble()
        {
            foreach (var elem in m_ElementList)
            {
                // Row 0 -- DOFX at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.StartNode.DOF_X] += elem.StiffnessElement[0, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.StartNode.DOF_Y] += elem.StiffnessElement[0, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.StartNode.DOF_ROT] += elem.StiffnessElement[0, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.EndNode.DOF_X] += elem.StiffnessElement[0, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.EndNode.DOF_Y] += elem.StiffnessElement[0, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.EndNode.DOF_ROT] += elem.StiffnessElement[0, 5];

                // Row 1 -- DOFY at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_X] += elem.StiffnessElement[1, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_Y] += elem.StiffnessElement[1, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.StiffnessElement[1, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_X] += elem.StiffnessElement[1, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_Y] += elem.StiffnessElement[1, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.StiffnessElement[1, 5];

                // Row 2 -- DOF_ROT at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_X] += elem.StiffnessElement[2, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.StiffnessElement[2, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.StiffnessElement[2, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_X] += elem.StiffnessElement[2, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.StiffnessElement[2, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.StiffnessElement[2, 5];

                // Row 3 -- DOFX at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_X] += elem.StiffnessElement[3, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_Y] += elem.StiffnessElement[3, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_ROT] += elem.StiffnessElement[3, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_X] += elem.StiffnessElement[3, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_Y] += elem.StiffnessElement[3, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_ROT] += elem.StiffnessElement[3, 5];

                // Row 4 -- DOFY at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_X] += elem.StiffnessElement[4, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_Y] += elem.StiffnessElement[4, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.StiffnessElement[4, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_X] += elem.StiffnessElement[4, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_Y] += elem.StiffnessElement[4, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.StiffnessElement[4, 5];

                // Row  -- DOF_ROT at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_X] += elem.StiffnessElement[5, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.StiffnessElement[5, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.StiffnessElement[5, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_X] += elem.StiffnessElement[5, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.StiffnessElement[5, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.StiffnessElement[5, 5];

                //Console.WriteLine(this.ToString());
                //Console.WriteLine("=================================\n");
            }

        }

        public string ToString()
        {
            string str = "[ \n";

            for (int i = 0; i < m_Cols; i++)
            {
                for (int j = 0; j < m_Rows; j++)
                {
                    str += String.Format("{0}\t", GlobalStiffnessMatrix[i, j].ToString());
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
