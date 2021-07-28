using System;
using System.Collections.Generic;
using System.Linq;
using VMDiagrammer.Helpers;
using VMDiagrammer.Models.Elements;

namespace VMDiagrammer.Models
{
    public class StructureStiffnessModel
    {
        private List<BeamElement2D> m_ElementList = new List<BeamElement2D>();
        private double[,] m_GlobalStiffnessMatrix;
        private double?[,] m_DisplacementVector;
        private double?[,] m_LoadVector;
        private double[,] m_DOF_Indices;

        private int numRestrainedDOF = 0;
        private int numUnrestrainedDOF = 0;

        private int m_Rows = 0;
        private int m_Cols = 0;

        public void AddElement(VM_Beam beam)
        {
            m_ElementList.Add(new BeamElement2D(beam.Start, beam.End, 1, 1, 1, 1));
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
        public double[,] DOF_Indices
        {
            get => m_DOF_Indices;
            set
            {
                m_DOF_Indices = value;
            }
        }


        // Contains a vector of our forces.
        public double?[,] LoadVector
        {
            get => m_LoadVector;
            set
            {
                m_LoadVector = value;
            }
        }

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
        public double[,] K_Free_Free = null;
        public double[,] K_Free_Fixed = null;
        public double[,] K_Fixed_Free = null;
        public double[,] K_Fixed_Fixed = null;

        public double[,] invK_Free_Free = null;

        public double[,] Disp_Free = null;   // displacements for free degrees of freedom
        public double[,] Disp_Fixed = null;  // displacements for restrained degrees of freedom

        public double[,] Load_Free = null;   // nodal loads for free degrees of freedom 
        public double[,] Load_Fixed = null;  // nodal loads for fixed degrees of freedom  (support reactions)

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rows">number of rows in the model</param>
        /// <param name="cols">number of cols in the model</param>
        public StructureStiffnessModel(int rows, int cols)
        {
            m_Rows = rows;
            m_Cols = cols;

            // Need to zero out all the values of Global Stiffness matrix since we used a double[]
            GlobalStiffnessMatrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    GlobalStiffnessMatrix[i, j] = 0.0;
                }
            }

            DOF_Indices = new double[rows, 1];
            DisplacementVector = new double?[rows, 1];
            LoadVector = new double?[rows, 1];

            // Populate our indices vector to unsorted values.
            for (int i = 0; i < rows; i++)
            {
                DOF_Indices[i, 0] = i;
            }
        }

        /// <summary>
        /// Assembles our model stiffness matrix
        /// </summary>
        public void AssembleStiffnessMatrix()
        {
            foreach (var elem in m_ElementList)
            {
                // Row 0 -- DOFX at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.StartNode.DOF_X] += elem.TransformedGlobalStiffnessElement[0, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.StartNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[0, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.StartNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[0, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.EndNode.DOF_X] += elem.TransformedGlobalStiffnessElement[0, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.EndNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[0, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_X, elem.EndNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[0, 5];

                // Row 1 -- DOFY at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_X] += elem.TransformedGlobalStiffnessElement[1, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[1, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[1, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_X] += elem.TransformedGlobalStiffnessElement[1, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[1, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[1, 5];

                // Row 2 -- DOF_ROT at start
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_X] += elem.TransformedGlobalStiffnessElement[2, 0];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[2, 1];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[2, 2];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_X] += elem.TransformedGlobalStiffnessElement[2, 3];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[2, 4];
                GlobalStiffnessMatrix[elem.StartNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[2, 5];

                // Row 3 -- DOFX at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_X] += elem.TransformedGlobalStiffnessElement[3, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[3, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.StartNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[3, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_X] += elem.TransformedGlobalStiffnessElement[3, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[3, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_X, elem.EndNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[3, 5];

                // Row 4 -- DOFY at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_X] += elem.TransformedGlobalStiffnessElement[4, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[4, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.StartNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[4, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_X] += elem.TransformedGlobalStiffnessElement[4, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[4, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_Y, elem.EndNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[4, 5];

                // Row  -- DOF_ROT at end
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_X] += elem.TransformedGlobalStiffnessElement[5, 0];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[5, 1];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.StartNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[5, 2];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_X] += elem.TransformedGlobalStiffnessElement[5, 3];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_Y] += elem.TransformedGlobalStiffnessElement[5, 4];
                GlobalStiffnessMatrix[elem.EndNode.DOF_ROT, elem.EndNode.DOF_ROT] += elem.TransformedGlobalStiffnessElement[5, 5];

                //Console.WriteLine(this.ToString());
                //Console.WriteLine("=================================\n");

                if(!MatrixOperations.IsSymmetric(GlobalStiffnessMatrix))
                {
                    Console.WriteLine("S: " + elem.StartNode.Index.ToString() + "   E: " + elem.EndNode.Index.ToString() + " is not symmetric!");
                }
            }
        }

        /// <summary>
        /// Assembles our displacement vector
        /// </summary>
        protected void AssembleDisplacementVector()
        {
            foreach (var elem in m_ElementList)
            {
                // Row 0 -- DOFX at start
                DisplacementVector[elem.StartNode.DOF_IndexVector[0], 0] = elem.StartNode.DisplacementVector[0, 0];

                // Row 1 -- DOFY at start
                DisplacementVector[elem.StartNode.DOF_IndexVector[1], 0] = elem.StartNode.DisplacementVector[1, 0];

                // Row 2 -- DOF_ROT at start
                DisplacementVector[elem.StartNode.DOF_IndexVector[2], 0] = elem.StartNode.DisplacementVector[2, 0];

                // Row 3 -- DOFX at end
                DisplacementVector[elem.EndNode.DOF_IndexVector[0], 0] = elem.EndNode.DisplacementVector[0, 0];

                // Row 4 -- DOFY at end
                DisplacementVector[elem.EndNode.DOF_IndexVector[1], 0] = elem.EndNode.DisplacementVector[1, 0];

                // Row  -- DOF_ROT at end
                DisplacementVector[elem.EndNode.DOF_IndexVector[2], 0] = elem.EndNode.DisplacementVector[2, 0];

                //Console.WriteLine(this.ToString());
                //Console.WriteLine("=================================\n");
            }
        }

        /// <summary>
        /// Assembles our load vector
        /// </summary>
        protected void AssembleLoadVector()
        {
            for (int i = 0; i < this.Rows; i++)
            {
                if (DisplacementVector[i, 0] == 0)
                    LoadVector[i, 0] = null;
                else
                    if (LoadVector[i,0] == null)
                        LoadVector[i, 0] = 0;
            }


            //foreach (var elem in m_ElementList)
            //{
            //    // Row 0 -- DOFX at start
            //    LoadVector[elem.StartNode.DOF_IndexVector[0], 0] = elem.StartNode.ForceVector[0, 0];

            //    // Row 1 -- DOFY at start
            //    LoadVector[elem.StartNode.DOF_IndexVector[1], 0] = elem.StartNode.ForceVector[1, 0];

            //    // Row 2 -- DOF_ROT at start
            //    LoadVector[elem.StartNode.DOF_IndexVector[2], 0] = elem.StartNode.ForceVector[2, 0];

            //    // Row 3 -- DOFX at end
            //    LoadVector[elem.EndNode.DOF_IndexVector[0], 0] = elem.EndNode.ForceVector[0, 0];

            //    // Row 4 -- DOFY at end
            //    LoadVector[elem.EndNode.DOF_IndexVector[1], 0] = elem.EndNode.ForceVector[1, 0];

            //    // Row  -- DOF_ROT at end
            //    LoadVector[elem.EndNode.DOF_IndexVector[2], 0] = elem.EndNode.ForceVector[2, 0];

            //    //Console.WriteLine(this.ToString());
            //    //Console.WriteLine("=================================\n");
            //}
        }

        /// <summary>
        /// Assembles all matrixes and vectors needed for our model
        /// </summary>
        public void AssembleAllMatrix()
        {
            AssembleStiffnessMatrix();
            AssembleDisplacementVector();
            AssembleLoadVector();
        }


        /// <summary>
        /// Function that rearranges rows and columns of an array matrix.
        /// Used for grouping degrees of freedom 
        /// </summary>
        protected void GroupFixedFreeDOF()
        {
            List<double> DOF_tobeMoved = new List<double>(); // stores the DOF to be moved.
            
            // Copy the unrestrained rows
            for (int i = 0; i < m_Rows; i++)
            {
                if (DisplacementVector[i, 0] == 0)
                {
                    DOF_tobeMoved.Add(DOF_Indices[i, 0]);
                }
            }

            List<double> copyDOF_tobeMoved = new List<double>(DOF_tobeMoved); // stores the DOF to be moved.

            // Swap the rows
            for (int i = 0; i < copyDOF_tobeMoved.Count; i++)
            {
                for (int j = (int)copyDOF_tobeMoved[i]; j < m_Rows - 1; j++)
                {
                    MatrixOperations.SwapRows(ref m_GlobalStiffnessMatrix, j, j + 1);
                    MatrixOperations.SwapRowsNullableVector(ref m_DisplacementVector, j, j + 1);
                    MatrixOperations.SwapRowsNullableVector(ref m_LoadVector, j, j + 1);
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

        public string DisplayMatrixInfo(double[,] arr)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            string str = "Indices: ";
            for (int i = 0; i < rows; i++)
            {
                // TODO: Get a better way of tracking indices.  This method doesn't work for submatrices since we don't know the indices 
                // based on the 'i' variable above anymore.
                str += DOF_Indices[i, 0].ToString() + "  ";
            }
            str += "\n";
            str += "-----------------------------------------\n";
            for (int i = 0; i < rows; i++)
            {
                double val = DOF_Indices[i, 0];
                // Get the node number
                str += ((int)val / 3).ToString();

                // Get the X,Y,Rot DOF value
                if (val % 3 == 0)
                    str += "-X  :";
                else if (val % 3 == 1)
                    str += "-Y  :";
                else
                    str += "-ROT :";

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

        public string DisplayMatrixInfoNullable(double?[,] arr)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            string str = "Indices: ";
            for (int i = 0; i < rows; i++)
            {
                // TODO: Get a better way of tracking indices.  This method doesn't work for submatrices since we don't know the indices 
                // based on the 'i' variable above anymore.

                str += DOF_Indices[i, 0].ToString() + "  ";
            }
            str += "\n";
            str += "-----------------------------------------\n";
            for (int i = 0; i < rows; i++)
            {
                double val = DOF_Indices[i, 0];
                // Get the node number
                str += ((int)val / 3).ToString();

                // Get the X,Y,Rot DOF value
                if (val % 3 == 0)
                    str += "-X  :";
                else if (val % 3 == 1)
                    str += "-Y  :";
                else
                    str += "-ROT :";

                for (int j = 0; j < cols; j++)
                {
                    if (arr[i, j] == null)
                        str += String.Format("null |  \t");
                    else
                        str += String.Format("{0}    \t", arr[i, j].ToString().PadLeft(5, ' ') + "|");

                    if (j < cols - 1)
                    {
                        str += "   ,   ";
                    }
                }

                str += "\n";
            }

            return str;
        }

        /// <summary>
        /// Counts the restrained DOF for the model
        /// </summary>
        /// <returns></returns>
        private void CountRestrainedDOF()
        {
            this.numRestrainedDOF = 0;  // reset our restrained degrees of freedom counter
            List<double> DOF_tobeMoved = new List<double>(); // stores the DOF to be moved.

            // Copy the unrestrained rows
            for (int i = 0; i < m_Rows; i++)
            {
                if (DisplacementVector[i, 0] == 0)
                {
                    this.numRestrainedDOF++;  // increment our count
                }
            }

            this.numUnrestrainedDOF = this.Rows - this.numRestrainedDOF;
        }

        /// <summary>
        /// Populates our submatrix partitions
        /// </summary>

        public void PopulateStiffnessPartitions()
        {
            int numUnrestrainedDOF = this.Rows - numRestrainedDOF;

            // populate K_FREE_FREE
            K_Free_Free = MatrixOperations.CreateSubmatrix(m_GlobalStiffnessMatrix, 0, 0, numUnrestrainedDOF-1, numUnrestrainedDOF-1);
                
            // populate K_FIXED_FIXED
            K_Fixed_Fixed = MatrixOperations.CreateSubmatrix(m_GlobalStiffnessMatrix, numUnrestrainedDOF, numUnrestrainedDOF, this.Rows-1, this.Cols-1);

            // populate K_FREE_FIXED
            K_Free_Fixed = MatrixOperations.CreateSubmatrix(m_GlobalStiffnessMatrix, 0, numUnrestrainedDOF, numUnrestrainedDOF-1, this.Cols - 1); ;

            // populate K_FIXED_FREE
            K_Fixed_Free = MatrixOperations.CreateSubmatrix(m_GlobalStiffnessMatrix, numUnrestrainedDOF, 0, this.Rows-1, numUnrestrainedDOF - 1); ;

            // Display the matrices
            Console.WriteLine("K_FREE_FREE\n" + MatrixOperations.Display(K_Free_Free));
            Console.WriteLine("K_FIXED_FIXED\n" + MatrixOperations.Display(K_Fixed_Fixed));
            Console.WriteLine("K_FIXED_FREE\n" + MatrixOperations.Display(K_Fixed_Free));
            Console.WriteLine("K_FREE_FIXED\n" + MatrixOperations.Display(K_Free_Fixed));
        }

        /// <summary>
        /// Solves the system of matrices
        /// </summary>
        public void Solve()
        {


            // 1. Assemble the matrices
            this.AssembleAllMatrix(); // assemble the stiffness matrix



            if (!MatrixOperations.IsSymmetric(this.GlobalStiffnessMatrix))
                throw new InvalidOperationException("In Solve():  Global Stiffness Matrix is not symmetric!");

            Console.WriteLine("Ungrouped Global Stiffness Matrix\n" + MatrixOperations.Display(m_GlobalStiffnessMatrix));

            // 2. Count restrained DOF
            CountRestrainedDOF();

            // 3. Collect / Sort the free and fixed DOF
            this.GroupFixedFreeDOF();

            Console.WriteLine("Grouped DOF Stiffness Matrix\n" + MatrixOperations.Display(m_GlobalStiffnessMatrix));

            // 4. First populate the partitions for stiffness
            this.PopulateStiffnessPartitions();

            // Check symmetric status of stiffness partitions
            if (!MatrixOperations.IsSymmetric(this.K_Fixed_Fixed))
                throw new InvalidOperationException("K_Fixed_Fixed is not symmetric!");
            if (!MatrixOperations.IsSymmetric(this.K_Free_Free))
                throw new InvalidOperationException("K_Free_Free is not symmetric!");



            //// populate DISP_FREE
            //Disp_Free = MatrixOperations.ConvertFromNullable(MatrixOperations.CreateSubmatrixNullable(m_DisplacementVector, 0, 0, numUnrestrainedDOF - 1, 0));
            Disp_Fixed = MatrixOperations.ConvertFromNullable(MatrixOperations.CreateSubmatrixNullable(m_DisplacementVector, numUnrestrainedDOF, 0, this.Rows - 1, 0));

            // populate Load_FREE
            Load_Free = MatrixOperations.ConvertFromNullable(MatrixOperations.CreateSubmatrixNullable(m_LoadVector, 0, 0, numUnrestrainedDOF - 1, 0));


            // 2. Invert FREE FREE stiffness matrix
            invK_Free_Free = MatrixOperations.MatrixInverse(K_Free_Free);
            Console.WriteLine("Inverse K_FREE_FREE\n" + MatrixOperations.Display(invK_Free_Free));

            // 3. Solve for free displacements, Disp_Free
            double[,] prod = MatrixOperations.MatrixProduct(K_Free_Fixed, Disp_Fixed);
            double[,] diff = MatrixOperations.MatrixSubtract(Load_Free, prod);

            Disp_Free = MatrixOperations.MatrixProduct(invK_Free_Free, diff);



            // 4. Solve for support reactions, Load_Fixed
            double[,] prod1 = MatrixOperations.MatrixProduct(K_Fixed_Free, Disp_Free);
            double[,] prod2 = MatrixOperations.MatrixProduct(K_Fixed_Fixed, Disp_Fixed);

            Load_Fixed = MatrixOperations.MatrixAdd(prod1, prod2);

            // Populate our full system vectors
            CreateDisplacementVectorFull();
            CreateLoadVectorFull();
        }

        protected void CreateDisplacementVectorFull()
        {
            int free_DOF_count = Disp_Free.GetLength(0);
            int fixed_DOF_count = Disp_Fixed.GetLength(0);
            for (int i = 0; i < DisplacementVector.GetLength(0); i++)
            {
                if (i < free_DOF_count)
                {
                    DisplacementVector[i, 0] = Disp_Free[i, 0]; // Free displacements first
                }
                else
                {
                    DisplacementVector[i, 0] = Disp_Fixed[i - free_DOF_count, 0];
                }
            }
        }


        protected void CreateLoadVectorFull()
        {
            int free_DOF_count = Load_Free.GetLength(0);
            int fixed_DOF_count = Load_Fixed.GetLength(0);
            for (int i = 0; i < LoadVector.GetLength(0); i++)
            {
                if (i < free_DOF_count)
                {
                    LoadVector[i,0] = Load_Free[i, 0]; // Free displacements first
                }
                else
                {
                    LoadVector[i, 0] = Load_Fixed[i - free_DOF_count, 0];
                }
            }
        }

        /// <summary>
        /// Displays data of the structural stiffness model.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "\n";

            // The current displacement vector
            int dof_disp_rows = DisplacementVector.GetLength(0);
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
            int dof_index_rows = DOF_Indices.GetLength(0);
            str += "\n";
            for (int i = 0; i < dof_index_rows; i++)
            {
                    str += String.Format("{0}    \t", DOF_Indices[i, 0].ToString());
                str += "      ";
            }
            str += "\n";

            str += "[ \n";
            str += "--------------------------------------------------------------------------\n";

            int gs_rows = GlobalStiffnessMatrix.GetLength(0);
            int gs_cols = GlobalStiffnessMatrix.GetLength(1);

            for (int i = 0; i < gs_rows; i++)
            {
                for (int j = 0; j < gs_cols; j++)
                {
                    str += String.Format("{0}    \t", GlobalStiffnessMatrix[i, j].ToString());
                    if (j < m_Cols-1)
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
