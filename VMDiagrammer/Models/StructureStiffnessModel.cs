using System;
using System.Collections.Generic;
using VMDiagrammer.Helpers;
using VMDiagrammer.Models.Elements;

namespace VMDiagrammer.Models
{
    public class StructureStiffnessModel
    {
        private List<BeamElement> m_ElementList = new List<BeamElement>();
        private double?[,] m_GlobalStiffnessMatrix;
        private double?[,] m_DisplacementVector;
        private double?[,] m_DOF_Indices;

        private int m_Rows = 0;
        private int m_Cols = 0;

        public void AddElement(VM_Beam beam)
        {
            m_ElementList.Add(new BeamElement(beam.Start, beam.End, 1, 1, 1, 1));
        }

        // Contains a vector of our known and unknown nodal displacements
        public double?[,] DisplacementVector
        {
            get => m_DisplacementVector;
            set
            {
                m_DisplacementVector = value;
            }
        }

        // Contains a vector of our index numbers
        public double?[,] DOF_Indices
        {
            get => m_DOF_Indices;
            set
            {
                m_DOF_Indices = value;
            }
        }
        
        // Contains the global stiffness matrix
        public double?[,] GlobalStiffnessMatrix
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
        public double?[,] K_Free_Free = null;
        public double?[,] K_Free_Fixed = null;
        public double?[,] K_Fixed_Free = null;
        public double?[,] K_Fixed_Fixed = null;

        public double?[] Disp_Free = null;   // free degrees of freedom
        public double?[] Disp_Fixed = null;  // restrained degrees of freedom
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rows">number of rows in the model</param>
        /// <param name="cols">number of cols in the model</param>
        public StructureStiffnessModel(int rows, int cols)
        {
            m_Rows = rows;
            m_Cols = cols;

            GlobalStiffnessMatrix = new double?[rows, cols];
            DOF_Indices = new double?[rows, 1];
            DisplacementVector = new double?[rows, 1];

            Console.WriteLine(DOF_Indices.GetLength(0) + "   " + DOF_Indices.GetLength(1));
            // Populate our indices vector to unsorted values.
            for (int i = 0; i < rows; i++)
            {
                DOF_Indices[i, 0] = i;
            }




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
                DisplacementVector[elem.StartNode.DOF_IndexVector[0], 0] = elem.StartNode.DisplacementVector[0];


                // Row 1 -- DOFY at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_X] += elem.StiffnessElement[1, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_Y] += elem.StiffnessElement[1, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.StiffnessElement[1, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_X] += elem.StiffnessElement[1, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_Y] += elem.StiffnessElement[1, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.StiffnessElement[1, 5];
                DisplacementVector[elem.StartNode.DOF_IndexVector[1], 0] = elem.StartNode.DisplacementVector[1];

                // Row 2 -- DOF_ROT at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_X] += elem.StiffnessElement[2, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.StiffnessElement[2, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.StiffnessElement[2, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_X] += elem.StiffnessElement[2, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.StiffnessElement[2, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.StiffnessElement[2, 5];
                DisplacementVector[elem.StartNode.DOF_IndexVector[2], 0] = elem.StartNode.DisplacementVector[2];

                // Row 3 -- DOFX at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_X] += elem.StiffnessElement[3, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_Y] += elem.StiffnessElement[3, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_ROT] += elem.StiffnessElement[3, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_X] += elem.StiffnessElement[3, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_Y] += elem.StiffnessElement[3, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_ROT] += elem.StiffnessElement[3, 5];
                DisplacementVector[elem.EndNode.DOF_IndexVector[0], 0] = elem.EndNode.DisplacementVector[0];

                // Row 4 -- DOFY at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_X] += elem.StiffnessElement[4, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_Y] += elem.StiffnessElement[4, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.StiffnessElement[4, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_X] += elem.StiffnessElement[4, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_Y] += elem.StiffnessElement[4, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.StiffnessElement[4, 5];
                DisplacementVector[elem.EndNode.DOF_IndexVector[1], 0] = elem.EndNode.DisplacementVector[1];

                // Row  -- DOF_ROT at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_X] += elem.StiffnessElement[5, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.StiffnessElement[5, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.StiffnessElement[5, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_X] += elem.StiffnessElement[5, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.StiffnessElement[5, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.StiffnessElement[5, 5];
                DisplacementVector[elem.EndNode.DOF_IndexVector[2], 0] = elem.EndNode.DisplacementVector[2];

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
            List<double?> DOF_tobeMoved = new List<double?>(); // stores the DOF to be moved.
            
            // Copy the unrestrained rows
            for (int i = 0; i < m_Rows; i++)
            {
                if (DisplacementVector[i, 0] == 0)
                {
                    DOF_tobeMoved.Add(DOF_Indices[i, 0]);
                }
            }

            List<double?> copyDOF_tobeMoved = new List<double?>(DOF_tobeMoved); // stores the DOF to be moved.

            // Swap the rows
            for (int i = 0; i < copyDOF_tobeMoved.Count; i++)
            {
                for (int j = (int)copyDOF_tobeMoved[i]; j < m_Rows - 1; j++)
                {
                    MatrixOperations.SwapRows(ref m_GlobalStiffnessMatrix, j, j + 1);
                    MatrixOperations.SwapRows(ref m_DisplacementVector, j, j + 1);
                    MatrixOperations.SwapRows(ref m_DOF_Indices, j, j + 1);
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
                for (int j = (int)DOF_tobeMoved[i]; j < m_Rows - 1; j++)
                {
                    MatrixOperations.SwapCols(ref m_GlobalStiffnessMatrix, j, j + 1);
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

        public string PrintStiffnessSubmatrix(double?[,] arr)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            string str = "";
            str += "-----------------------------------------\n";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    str += String.Format("{0}    \t", arr[i, j].ToString());
                    if (j < cols-1)
                    {
                        str += "   ,   ";
                    }
                }

                str += "\n";
            }

            return str;
        }

        /// <summary>
        /// Populates our submatrix partitions
        /// </summary>

        public void PopulatePartition()
        {

        }


        /// <summary>
        /// Displays data of the structural stiffness model.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // The current displacement vector
            string str = "\n";
            for (int i = 0; i < m_Cols; i++)
            {
                if (DisplacementVector[i, 0] == null)
                    str += String.Format("null    \t");
                else
                    str += String.Format("{0}    \t", DisplacementVector[i, 0].ToString());
                str += "      ";
            }
            str += "\n";

            // The DOF indices
            str += "\n";
            for (int i = 0; i < m_Cols; i++)
            {
                    str += String.Format("{0}    \t", DOF_Indices[i, 0].ToString());
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
