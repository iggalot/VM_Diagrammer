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
                Beam1 = new VM_Beam(NodeA, NodeB);
            if (Beam2 == null)
                Beam2 = new VM_Beam(NodeB, NodeC);
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
        public void GlobalStiffnessIsSquareAndSymmetric()
        {
            // Assert
            Assert.IsTrue(MatrixOperations.IsSquare(TestMatrices.UnitGlobalStiffnessMatrixTwoElements));
            Assert.IsTrue(MatrixOperations.IsSymmetric(TestMatrices.UnitGlobalStiffnessMatrixTwoElements));
        }

    }
}
