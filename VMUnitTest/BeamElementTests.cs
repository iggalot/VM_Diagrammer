using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VMDiagrammer.Helpers;
using VMDiagrammer.Models;

namespace VMUnitTest
{
    public static class TestMatrices
    {
        /// <summary>
        /// Beam unit stiffness element
        /// A=1, L=1, E=1
        /// </summary>
        public static double[,] UnitStiffnessElementOneElement = new double[,]
        {
            {1, 0, 0, -1, 0, 0},
            {0, 12, 6, 0, -12, 6},
            {0, 6, 4, 0, -6, 2},
            {-1, 0, 0, 1, 0, 0},
            {0, -12, -6, 0, 12, -6},
            {0, 6, 2, 0, -6, 4}
        };

        /// <summary>
        /// Global stiffness matrix for 1 --- 2 ---- 3  model of unit elements
        /// A=1, L=1, E=1
        /// </summary>
        public static double[,] UnitGlobalStiffnessMatrixTwoElements = new double[,]
        {
            {1, 0, 0, -1, 0, 0, 0, 0, 0},
            {0, 12, 6, 0, -12, 6, 0, 0, 0},
            {0, 6, 4, 0, -6, 2, 0, 0, 0},
            {-1, 0, 0, 2, 0, 0, -1, 0, 0},
            {0, -12, -6, 0, 24, 0, 0, -12, 6},
            {0, 6, 2, 0, 0, 8, 0, -6, 2},
            {0, 0, 0, -1, 0, 0, 1, 0, 0},
            {0, 0, 0, 0, -12, -6, 0, 12, -6},
            {0, 0, 0, 0, 6, 2, 0, -6, 4}
        };

        /// <summary>
        /// Free DOF stiffness matrix for 1 --- 2 ---- 3  model of UnitGlobalStiffnessMatrixTwoElements
        /// DOF 3, 4, 5, 6, 7, 8
        /// A=1, L=1, E=1
        /// </summary>
        public static double[,] UnitK_free_free = new double[,]
        {
            { 2,     0,    0,     -1,     0,     0},
            { 0,    24,    0,      0,   -12,     6},
            { 0,     0,    8,      0,    -6,     2},
            {-1,     0,    0,      1,     0,     0},
            { 0,   -12,   -6,      0,    12,    -6},
            { 0,     6,    2,      0,    -6,     4}
        };

        /// <summary>
        /// Free DOF stiffness matrix for 1 --- 2 ---- 3  model of UnitGlobalStiffnessMatrixTwoElements
        /// DOF 3, 4, 5, 6, 7, 8
        /// A=1, L=1, E=1
        /// </summary>
        public static double[,] UnitK_fixed_fixed = new double[,]
        {
            { 1,  0,  0},
            { 0, 12,  6},
            { 0,  6,  4}
        };

        /// <summary>
        /// Inverse stiffness matrix for 1 --- 2 ---- 3  model of UnitGlobalStiffnessMatrixTwoElements
        /// with DOF 0, 1, 2 removed
        /// A=1, L=1, E=1
        /// </summary>
        public static double[,] InvK_free_free = new double[,]
        {
            {1,         0,         0,        1,        0,        0},
            {0,   0.33333,   0.50000,        0,  0.83333,  0.50000},
            {0,   0.50000,         1,        0,  1.50000,        1},
            {1,         0,         0,        2,        0,        0},
            {0,   0.83333,   1.50000,        0,  2.66666,        2},
            {0,   0.50000,         1,        0,        2,        2}
        };




    }

    [TestClass]
    public class BeamElementTests
    {
        private VM_Node NodeA = null;
        private VM_Node NodeB = null;
        private VM_Node NodeC = null;
        private VM_Beam Beam1 = null;
        private VM_Beam Beam2 = null;

        public BeamElementTests()
        {
            //ARRANGE
            //Create the nodes
            if(NodeA == null)
                NodeA = new VM_Node(0, 0, true, true, true,0);
            if (NodeB == null) 
                NodeB = new VM_Node(1, 0, false, false, false,1);
            if (NodeC == null)
                NodeC = new VM_Node(2, 0, false, false, false,2);

            if (Beam1 == null)
                Beam1 = new VM_Beam(NodeA, NodeB, 3);
            if (Beam2 == null)
                Beam2 = new VM_Beam(NodeB, NodeC, 4);
        }

        [TestMethod]
        public void ElementIsSquareAndSymmetric()
        {
            //Assert
            Assert.IsTrue(MatrixOperations.IsSquare(TestMatrices.UnitStiffnessElementOneElement));
            Assert.IsTrue(MatrixOperations.IsSymmetric(TestMatrices.UnitStiffnessElementOneElement));
        }

        [TestMethod]
        public void SingleBeamGlobalStiffnessMatrixConstruction()
        {
            //Create a model
            StructureStiffnessModel Model1 = new StructureStiffnessModel(9, 9);

            //ACT
            //Add the elements
            Model1.AddElement(Beam1);
            Model1.AssembleStiffnessMatrix();

            // ASSERT
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Assert.AreEqual(TestMatrices.UnitStiffnessElementOneElement[i, j],
                        Model1.GlobalStiffnessMatrix[i, j]);
                }
            }
        }

        [TestMethod]
        public void GlobalStiffnessMatrixConstruction()
        {

            // Create the model
            StructureStiffnessModel Model2 = new StructureStiffnessModel(9, 9);

            // ACT
            // Add the elements
            Model2.AddElement(Beam1);
            Model2.AddElement(Beam2);
            Model2.AssembleStiffnessMatrix();

            // ASSERT
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[i, j],
                        Model2.GlobalStiffnessMatrix[i, j]);
                }
            }
        }

        [TestMethod]
        public void PopulateStiffnessPartitionsConstruction()
        {
            // Create the model
            StructureStiffnessModel Model6 = new StructureStiffnessModel(9, 9);

            // ACT
            // Add the elements
            Model6.AddElement(Beam1);
            Model6.AddElement(Beam2);
            Model6.AssembleAllMatrix();

            Model6.PopulateStiffnessPartitions();

            // ASSERT
            // Fixed Fixed stiffness matrix
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.AreEqual(TestMatrices.UnitK_fixed_fixed[i, j],
                        Model6.K_Fixed_Fixed[i, j], " Fixed-Fixed partition does not match for element"+i.ToString()+ ","+j.ToString());
                }
            }

            // Free Free stiffness matrix
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Assert.AreEqual(TestMatrices.UnitK_free_free[i, j],
                        Model6.K_Free_Free[i, j], " Free-Free partition does not match for element" + i.ToString() + "," + j.ToString());
                }
            }
        }

        [TestMethod]
        public void GlobalStiffnessIsSquareAndSymmetric()
        {
            // Assert
            Assert.IsTrue(MatrixOperations.IsSquare(TestMatrices.UnitGlobalStiffnessMatrixTwoElements));
            Assert.IsTrue(MatrixOperations.IsSymmetric(TestMatrices.UnitGlobalStiffnessMatrixTwoElements));
        }

        [TestMethod]
        public void MatrixCreateSubmatrix()
        {
            // ACT
            // Row 0-4 (incl.), Col 0-4 (incl.)
            double[,] submatrix = MatrixOperations.CreateSubmatrix(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 0, 0, 4, 4);

            // ASSERT
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    string str = i.ToString() + "  , " + j.ToString() + "failed";
                    Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[i,j],submatrix[i,j],str);
                }
            }

            // ACT
            // Row 0-4 (incl.), Col 5-8 (incl.)
            double[,] submatrix2 = MatrixOperations.CreateSubmatrix(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 0, 5, 4, 8);

            // ASSERT
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string str = i.ToString() + "  , " + j.ToString() + "failed";
                    Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[i, j+5], submatrix2[i, j], str);
                }
            }

            // ACT
            // Row 5-8 (incl.), Col 0-4 (incl.)
            double[,] submatrix3 = MatrixOperations.CreateSubmatrix(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 5, 0, 8, 4);

            // ASSERT
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    string str = i.ToString() + "  , " + j.ToString() + "failed";
                    Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[i+5, j], submatrix3[i, j], str);
                }
            }

            // ACT
            // Row 5-8 (incl.), Col 5-8 (incl.)
            double[,] submatrix4 = MatrixOperations.CreateSubmatrix(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 5, 5, 8, 8);

            // ASSERT
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string str = i.ToString() + "  , " + j.ToString() + "failed";
                    Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[i + 5, j+5], submatrix4[i, j], str);
                }
            }

            // ACT
            // Row 2-6 (incl.), Col 2-6 (incl.)
            double[,] submatrix5 = MatrixOperations.CreateSubmatrix(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 2, 2, 6, 6);

            // ASSERT
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    string str = i.ToString() + "  , " + j.ToString() + "failed";
                    Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[i + 2, j + 2], submatrix5[i, j], str);
                }
            }
        }

        [TestMethod]
        public void MatrixRemoveColumn()
        {
            // Create the model
            StructureStiffnessModel Model4 = new StructureStiffnessModel(9, 9);

            // ACT
            // Add the elements
            Model4.AddElement(Beam1);
            Model4.AddElement(Beam2);
            Model4.AssembleStiffnessMatrix();

            // Assert
            Assert.AreEqual(Model4.GlobalStiffnessMatrix.GetLength(0), 9, " Before Row dimension does not match");
            Assert.AreEqual(Model4.GlobalStiffnessMatrix.GetLength(1), 9, " Before Column dimension does not match");

            // Remove the Column 0
            double[,] temp = MatrixOperations.RemoveColumn(Model4.GlobalStiffnessMatrix, 0);

            // Assert
            Assert.AreEqual(temp.GetLength(0), 9, " After Row dimension does not match");
            Assert.AreEqual(temp.GetLength(1), 8, " After Column dimension does not match");

            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[0, 1], temp[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp[8, 7]);

            // Remove Column 4
            double[,] temp2 = MatrixOperations.RemoveColumn(Model4.GlobalStiffnessMatrix, 4);

            // Assert
            Assert.AreEqual(temp2.GetLength(0), 9, " After Row dimension does not match");
            Assert.AreEqual(temp2.GetLength(1), 8, " After Column dimension does not match");

            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[0, 0], temp2[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[0, 4], temp2[0, 5]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[0, 5], temp2[0, 6]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp2[8, 7]);
        }

        [TestMethod]
        public void MatrixRemoveRow()
        {
            // Create the model
            StructureStiffnessModel Model5 = new StructureStiffnessModel(9, 9);

            // ACT
            // Add the elements
            Model5.AddElement(Beam1);
            Model5.AddElement(Beam2);
            Model5.AssembleStiffnessMatrix();

            // Assert
            Assert.AreEqual(Model5.GlobalStiffnessMatrix.GetLength(0), 9, " Before Row dimension does not match");
            Assert.AreEqual(Model5.GlobalStiffnessMatrix.GetLength(1), 9, " Before Column dimension does not match");

            // Remove the Row 0
            double[,] temp = MatrixOperations.RemoveRow(Model5.GlobalStiffnessMatrix, 0);

            // Assert
            Assert.AreEqual(temp.GetLength(0), 8, " After Row dimension does not match");
            Assert.AreEqual(temp.GetLength(1), 9, " After Column dimension does not match");

            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[1, 0], temp[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp[7, 8]);

            // Remove Row 4
            double[,] temp2 = MatrixOperations.RemoveRow(Model5.GlobalStiffnessMatrix, 4);

            // Assert
            Assert.AreEqual(temp2.GetLength(0), 8, " After Row dimension does not match");
            Assert.AreEqual(temp2.GetLength(1), 9, " After Column dimension does not match");

            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[0, 0], temp2[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[3, 0], temp2[3, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[5, 0], temp2[4, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp2[7, 8]);
        }

        [TestMethod]
        public void MatrixRemoveDOF()
        {
            double[,] temp = MatrixOperations.RemoveDOF(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 0);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[1, 1], temp[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 1], temp[7, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp[7, 7]);

            double[,] temp2 = MatrixOperations.RemoveDOF(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 2);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[0, 0], temp2[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[1, 1], temp2[1, 1]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[3, 3], temp2[2, 2]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp2[7, 7]);
        }

        [TestMethod]
        public void MatrixInversionTest()
        {
            // Remove DOF 0, 1, 2
            double[,] temp = MatrixOperations.RemoveDOF(TestMatrices.UnitGlobalStiffnessMatrixTwoElements, 2);
            temp = MatrixOperations.RemoveDOF(temp, 1);
            temp = MatrixOperations.RemoveDOF(temp, 0);

            // Still Square and symmetric?
            Assert.IsTrue(MatrixOperations.IsSymmetric(temp));
            Assert.IsTrue(MatrixOperations.IsSquare(temp));

            // Check that DOF removals match expected results
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[3, 3], temp[0, 0]);
            Assert.AreEqual(TestMatrices.UnitGlobalStiffnessMatrixTwoElements[8, 8], temp[5, 5]);

            // ACT
            // Invert the matrix.
            double[,] temp_inverse = MatrixOperations.MatrixInverse(temp);

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    string str = "--  " + i.ToString() + " , " + j.ToString() + " does not match";
                    Assert.AreEqual(TestMatrices.InvK_free_free[i, j], temp_inverse[i, j], 0.00001, str);
                }
            }

        }
    }
}
