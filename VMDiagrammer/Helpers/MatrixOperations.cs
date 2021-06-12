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

        /// <summary>
        /// Helper function that removes a specified row from the matrix
        /// </summary>
        /// <param name="arr">the array to manipulate</param>
        /// <param name="rows">number of rows in the matrix</param>
        /// <param name="cols">number of cols in the matrix</param>
        /// <param name="num">index to remove</param>
        /// <returns></returns>
        public static double[,] RemoveRow(double[,] arr, int rows, int cols, int num)
        {
            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (num < 0 || num >= rows)
                throw new ArgumentOutOfRangeException("Out of range index received:  (" + num + ") -- rows: " + rows + "  cols: " + cols);

            // create an array with one less row and column to hold our values
            double[,] temp = new double[rows - 1, cols];

            int current_row_count = 0;
            for (int i = 0; i < rows; i++)
            {
                // if we found our row to remove, skip it
                if (i != num)
                {
                    // loop through each column and copy the elements to the new array
                    for (int j = 0; j < cols; j++)
                    {
                        temp[current_row_count, j] = arr[i, j];
                    }

                    current_row_count++;
                }
            }

            return temp;
        }

        /// <summary>
        /// Helper function that removes a specified column from the matrix
        /// </summary>
        /// <param name="arr">the array to manipulate</param>
        /// <param name="rows">number of rows in the matrix</param>
        /// <param name="cols">number of cols in the matrix</param>
        /// <param name="num">index to remove</param>
        /// <returns></returns>
        public static double[,] RemoveColumn(double[,] arr, int rows, int cols, int num)
        {
            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (num >= cols || num < 0)
                throw new ArgumentOutOfRangeException("Out of range index received:  (" + num + ") -- rows: " + rows + "  cols: " + cols);

            // create an array with one less row and column to hold our values
            double[,] temp = new double[rows, cols - 1];

            for (int i = 0; i < rows; i++)
            {
                int current_col_count = 0;
                for (int j = 0; j < cols; j++)
                {
                    // found our row to remove so skip it
                    if (j == num)
                        continue;

                    temp[i, current_col_count] = arr[i, j];
                    current_col_count++;
                }
            }

            return temp;
        }

        /// <summary>
        /// Removes a DOF (corresponding row and column) from a matrix.
        /// </summary>
        /// <param name="arr">array to manipulate</param>
        /// <param name="rows">number of rows in the matrix</param>
        /// <param name="cols">number of cols in the matrix</param>
        /// <param name="num">index of the dof to be removed</param>
        public static double[,] RemoveDOF(double[,] arr, int rows, int cols, int num)
        {
            if (rows != cols)
                throw new NotImplementedException("Method RemoveDOF only works on square matrices");

            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (num >= cols || num < 0 || num >= rows)
                throw new ArgumentOutOfRangeException("Out of range index received:  (" + num + ") -- rows: " + rows + "  cols: " + cols);

            arr = MatrixOperations.RemoveRow(arr, rows, cols, num);
            arr = MatrixOperations.RemoveColumn(arr, rows-1, cols, num);  // now there's one less row so remove the corresponding column

            return arr;
        }
    }
}
