using System;
using System.Collections.Generic;
using VMDiagrammer.Helpers;
using VMDiagrammer.Models.Elements;

namespace VMDiagrammer.Models
{
    public class StructureStiffnessModel
    {
        private List<BeamElement> m_ElementList = new List<BeamElement>();
        private double[,] m_GlobalStiffnessMatrix;
        private int m_Rows = 0;
        private int m_Cols = 0;

        public void AddElement(VM_Beam beam)
        {
            m_ElementList.Add(new BeamElement(beam.Start, beam.End, 1, 1, 1, 1));
        }

        // Contains a vector of our known and unknown nodal displacements
        public double?[] DisplacementVector { get; set; }

        // Contains a vector of our index numbers
        public int[] DOF_Indices { get; set; }
        
        // Contains the global stiffness matrix
        public double[,] GlobalStiffnessMatrix
        {
            get => m_GlobalStiffnessMatrix;
            set
            {
                m_GlobalStiffnessMatrix = value;
            }
        }

        public int Rows
        {
            get => m_Rows;
            set
            {
                m_Rows = value;
            }
        }

        public int Cols
        {
            get => m_Cols;
            set
            {
                m_Cols = value;
            }
        }

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

            DOF_Indices = new int[rows];

            // Populate our indices vector to unsorted values.
            for (int i = 0; i < rows; i++)
            {
                DOF_Indices[i] = i;
            }

            DisplacementVector = new double?[rows];

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
                DisplacementVector[elem.StartNode.DOF_IndexVector[0]] = elem.StartNode.DisplacementVector[0];


                // Row 1 -- DOFY at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_X] += elem.StiffnessElement[1, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_Y] += elem.StiffnessElement[1, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.StiffnessElement[1, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_X] += elem.StiffnessElement[1, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_Y] += elem.StiffnessElement[1, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.StiffnessElement[1, 5];
                DisplacementVector[elem.StartNode.DOF_IndexVector[1]] = elem.StartNode.DisplacementVector[1];

                // Row 2 -- DOF_ROT at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_X] += elem.StiffnessElement[2, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.StiffnessElement[2, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.StiffnessElement[2, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_X] += elem.StiffnessElement[2, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.StiffnessElement[2, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.StiffnessElement[2, 5];
                DisplacementVector[elem.StartNode.DOF_IndexVector[2]] = elem.StartNode.DisplacementVector[2];

                // Row 3 -- DOFX at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_X] += elem.StiffnessElement[3, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_Y] += elem.StiffnessElement[3, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_ROT] += elem.StiffnessElement[3, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_X] += elem.StiffnessElement[3, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_Y] += elem.StiffnessElement[3, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_ROT] += elem.StiffnessElement[3, 5];
                DisplacementVector[elem.EndNode.DOF_IndexVector[0]] = elem.EndNode.DisplacementVector[0];

                // Row 4 -- DOFY at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_X] += elem.StiffnessElement[4, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_Y] += elem.StiffnessElement[4, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.StiffnessElement[4, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_X] += elem.StiffnessElement[4, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_Y] += elem.StiffnessElement[4, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.StiffnessElement[4, 5];
                DisplacementVector[elem.EndNode.DOF_IndexVector[1]] = elem.EndNode.DisplacementVector[1];

                // Row  -- DOF_ROT at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_X] += elem.StiffnessElement[5, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.StiffnessElement[5, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.StiffnessElement[5, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_X] += elem.StiffnessElement[5, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.StiffnessElement[5, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.StiffnessElement[5, 5];
                DisplacementVector[elem.EndNode.DOF_IndexVector[2]] = elem.EndNode.DisplacementVector[2];

                //Console.WriteLine(this.ToString());
                //Console.WriteLine("=================================\n");
            }
        }

        /// <summary>
        /// Function that rearranges rows and columns of an array matrix.
        /// Used for grouping degrees of freedom 
        /// </summary>
        public void GroupFixedFree()
        {
            List<int> DOF_tobeMoved = new List<int>(); // stores the DOF to be moved.

            int[] tempIndex = new int[m_Cols];
            double[] tempDispVector = new double[m_Cols];

            int current_row_index = 0;
            
            // Copy the unrestrained rows
            for (int i = 0; i < m_Rows; i++)
            {
                if (DisplacementVector[i] == 0)
                {
                    DOF_tobeMoved.Add(DOF_Indices[i]);
                }
            }

            List<int> copyDOF_tobeMoved = new List<int>(DOF_tobeMoved); // stores the DOF to be moved.

            // Swap the rows
            for (int i = 0; i < copyDOF_tobeMoved.Count; i++)
            {
                for (int j = copyDOF_tobeMoved[i]; j < m_Rows - 1; j++)
                {
                    MatrixOperations.SwapRows(ref m_GlobalStiffnessMatrix, m_Rows, m_Cols, j, j + 1);
                }

                for (int k = 0; k < copyDOF_tobeMoved.Count; k++)
                {
                    if (k == i)
                        copyDOF_tobeMoved[k] = m_Rows - 1; // move it to the end
                    else
                        copyDOF_tobeMoved[k] = copyDOF_tobeMoved[k] - 1; // move all the others up one.
                }
            }

            // Swap the columns using the original list of dofs
            for (int i = 0; i < DOF_tobeMoved.Count; i++)
            {
                for (int j = DOF_tobeMoved[i]; j < m_Rows - 1; j++)
                {
                    MatrixOperations.SwapCols(ref m_GlobalStiffnessMatrix, m_Rows, m_Cols, j, j + 1);
                }

                for (int k = 0; k < DOF_tobeMoved.Count; k++)
                {
                    if (k == i)
                        DOF_tobeMoved[k] = m_Rows - 1; // move it to the end
                    else
                        DOF_tobeMoved[k] = DOF_tobeMoved[k] - 1; // move all the others up one.
                }
            }


        }

        public override string ToString()
        {
            // The current displacement vector
            string str = "\n";
            for (int i = 0; i < m_Cols; i++)
            {
                if (DisplacementVector[i] == null)
                    str += String.Format("null    \t");
                else
                    str += String.Format("{0}    \t", DisplacementVector[i].ToString());
                str += "      ";
            }
            str += "\n";

            // The DOF indices
            str += "\n";
            for (int i = 0; i < m_Cols; i++)
            {
                    str += String.Format("{0}    \t", DOF_Indices[i].ToString());
                str += "      ";
            }
            str += "\n";

            str += "[ \n";
            str += "--------------------------------------------------------------------------\n";

            for (int i = 0; i < m_Rows; i++)
            {
                for (int j = 0; j < m_Cols; j++)
                {
                    str += String.Format("{0}    \t", GlobalStiffnessMatrix[i, j].ToString());
                    if (j < m_Cols)
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
