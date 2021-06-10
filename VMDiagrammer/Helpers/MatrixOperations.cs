using System;
using System.Windows.Media.Media3D;

namespace VMDiagrammer.Helpers
{
    public static class MatrixOperations
    {
        /// <summary>
        /// Returns the vector sum of two <see cref="Vector3D"/>
        /// </summary>
        /// <param name="a">vector A</param>
        /// <param name="b">vector B</param>
        /// <returns></returns>
        public static Vector3D Add(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y+b.Y, a.Z+b.Z);
        }

        /// <summary>
        /// Returns the dot product of two <see cref="Vector3D"/>
        /// </summary>
        /// <param name="a">vector A</param>
        /// <param name="b">vector B</param>
        /// <returns></returns>
        public static double DotProduct(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// Return the cross product of two <see cref="Vector3D"/>
        /// </summary>
        /// <param name="a">vector A</param>
        /// <param name="b">vector B</param>
        /// <returns></returns>
        public static Vector3D CrossProduct(Vector3D a, Vector3D b)
        {
            return new Vector3D(
                a.Y*b.Z-a.Z*b.Y,
                a.Y*b.X-a.X*b.Y,
                a.X*b.Y-a.Y*b.X
                );
        }

        /// <summary>
        /// Swaps the rows of a two-dimensional array in-place
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public static void SwapRows(ref double[,] arr, int rows, int cols, int first, int second)
        {
            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (first > rows || first < 0)
                throw new ArgumentOutOfRangeException("First row (" + rows + ")");
            if (second > rows || second < 0)
                throw new ArgumentOutOfRangeException("Second row (" + rows + ")");

            for (int i = 0; i < cols; i++)
            {
                double temp = arr[first,i];
                arr[first, i] = arr[second, i];
                arr[second, i] = temp;
            }
        }

        /// <summary>
        /// Swaps the columns of a two-dimensional array in-place
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="first">first column</param>
        /// <param name="second">second column</param>
        public static void SwapCols(ref double[,] arr, int rows, int cols, int first, int second)
        {
            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (first >= cols || first < 0)
                throw new ArgumentOutOfRangeException("First column (" + cols + ")");
            if (second >= cols || second < 0)
                throw new ArgumentOutOfRangeException("Second column (" + cols + ")");


            for (int i = 0; i < rows; i++)
            {
                double temp = arr[i, first];
                arr[i, first] = arr[i, second];
                arr[i, second] = temp;
            }
        }
    }
}
