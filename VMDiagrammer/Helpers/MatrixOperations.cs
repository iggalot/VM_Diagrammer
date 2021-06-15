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
        public static void SwapRows(ref double?[,] arr, int first, int second)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (first > rows || first < 0)
                throw new ArgumentOutOfRangeException("First row (" + rows + ")");
            if (second > rows || second < 0)
                throw new ArgumentOutOfRangeException("Second row (" + rows + ")");

            for (int i = 0; i < cols; i++)
            {
                double? temp = arr[first,i];
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
        public static void SwapCols(ref double?[,] arr, int first, int second)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (first >= cols || first < 0)
                throw new ArgumentOutOfRangeException("First column (" + cols + ")");
            if (second >= cols || second < 0)
                throw new ArgumentOutOfRangeException("Second column (" + cols + ")");


            for (int i = 0; i < rows; i++)
            {
                double? temp = arr[i, first];
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
        public static double?[,] RemoveRow(double?[,] arr, int num)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (num < 0 || num >= rows)
                throw new ArgumentOutOfRangeException("Out of range index received:  (" + num + ") -- rows: " + rows + "  cols: " + cols);

            // create an array with one less row and column to hold our values
            double?[,] temp = new double?[rows - 1, cols];

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
        public static double?[,] RemoveColumn(double?[,] arr, int num)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (num >= cols || num < 0)
                throw new ArgumentOutOfRangeException("Out of range index received:  (" + num + ") -- rows: " + rows + "  cols: " + cols);

            // create an array with one less row and column to hold our values
            double?[,] temp = new double?[rows, cols - 1];

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
        public static double?[,] RemoveDOF(double?[,] arr, int num)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            if (rows != cols)
                throw new NotImplementedException("Method RemoveDOF only works on square matrices");

            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");

            if (num >= cols || num < 0 || num >= rows)
                throw new ArgumentOutOfRangeException("Out of range index received:  (" + num + ") -- rows: " + rows + "  cols: " + cols);

            arr = MatrixOperations.RemoveRow(arr, num);
            arr = MatrixOperations.RemoveColumn(arr, num);  // now there's one less row so remove the corresponding column

            return arr;
        }

        /// <summary>
        /// Creates a submatrix from a specified matrix array
        /// </summary>
        /// <param name="arr">the array to partition</param>
        /// <param name="first_col">first column index (inclusive) of partition</param>
        /// <param name="first_row">first row index (inclusive) of the partition</param>
        /// <param name="last_col">last column index (inclusive) of the partition</param>
        /// <param name="last_row">last row index (inclusive) of the partition</param>
        /// <returns></returns>
        public static double?[,] CreateSubmatrix(double?[,] arr, int first_row, int first_col, int last_row, int last_col )
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);

            // Error checking
            if (rows < 1 || cols < 1)
                throw new NotImplementedException("Invalid value in number of rows (" + rows + " or columns (" + cols + ") specified for the array[]");
            if(last_row < first_row)
                throw new ArgumentOutOfRangeException("Last row index (" + last_row + ") must be larger than first_row index (" + first_row + ")");
            if (last_col < first_col)
                throw new ArgumentOutOfRangeException("Last column index (" + last_col + ") must be larger than first_col index (" + first_col + ")");
            if (first_col >= cols || first_col < 0)
                throw new ArgumentOutOfRangeException("Out of range index for first_col received:  (" + first_col + ") -- rows: " + rows + "  cols: " + cols);
            if (last_col >= cols || last_col < 0)
                throw new ArgumentOutOfRangeException("Out of range index for last_col received:  (" + last_col + ") -- rows: " + rows + "  cols: " + cols);
            if (first_row >= rows || first_row < 0)
                throw new ArgumentOutOfRangeException("Out of range index for first_row received:  (" + first_row + ") -- rows: " + rows + "  cols: " + cols);
            if (last_row >= rows || last_row < 0)
                throw new ArgumentOutOfRangeException("Out of range index for first_col received:  (" + last_row + ") -- rows: " + rows + "  cols: " + cols);

            double?[,] temp = arr;

            // remove largest row index first to preserve index counter on subsequent cycles
            int rows_removed = 0;

            for (int i = rows-1; i >= 0; i--)
            {
                if (i >= first_row && i <= last_row)
                    continue;
                else
                {
                    temp = RemoveRow(temp, i);
                    rows_removed++;
                }                    
            }

            // now remove the columns starting with largest index first to preserve counters on subsequent cycles.
            int cols_removed = 0;

            for (int i = cols - 1; i >= 0; i--)
            {
                if (i >= first_col && i <= last_col)
                    continue;
                else
                {
                    temp = RemoveColumn(temp, i);
                    cols_removed++;
                }
            }

            return temp;
        }

        /// <summary>
        /// Creates an identity matrix of specified rows = columns
        /// </summary>
        /// <param name="n">number of rows or columns of the identity matrix</param>
        /// <returns></returns>
        public static double[,] MatrixIdentity(int n)
        {
            // return an n x n Identity matrix
            double[,] result = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                result[i,i] = 1.0;

            return result;
        }

        /// <summary>
        /// Computes the matrix product of two matrixes A and B
        /// </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static double[,] MatrixProduct(double[,] matrixA, double[,] matrixB)
        {
            int aRows = matrixA.Length; int aCols = matrixA.GetLength(0);
            int bRows = matrixB.Length; int bCols = matrixB.GetLength(1);
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices in MatrixProduct");

            double[,] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k less-than bRows
                        result[i,j] += matrixA[i,k] * matrixB[k,j];

            return result;
        }

        /// <summary>
        /// Creates a matrix of the specified rows and columns
        /// </summary>
        /// <param name="rows">number of ros</param>
        /// <param name="cols">number of columns</param>
        /// <returns></returns>
        public static double[,] MatrixCreate(int rows, int cols)
        {
            return new double[rows,cols];
        }

        /// <summary>
        /// Returns the inverse of a matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] MatrixInverse(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] result = MatrixDuplicate(matrix);

            int[] perm;
            int toggle;
            double[,] lum = MatrixDecompose(matrix, out perm,
              out toggle);
            if (lum == null)
                throw new Exception("Unable to compute inverse");

            double[] b = new double[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                {
                    if (i == perm[j])
                        b[j] = 1.0;
                    else
                        b[j] = 0.0;
                }

                double[] x = HelperSolve(lum, b);

                for (int j = 0; j < n; ++j)
                    result[j,i] = x[j];
            }
            return result;
        }

        /// <summary>
        /// Craete a dupicate copy of the matrix
        /// </summary>
        /// <param name="matrix">matrix to be duplicated</param>
        /// <returns></returns>
        public static double[,] MatrixDuplicate(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            // allocates/creates a duplicate of a matrix.
            double[,] result = MatrixCreate(rows, cols);
            for (int i = 0; i < rows; ++i) // copy the values
                for (int j = 0; j < cols; ++j)
                    result[i,j] = matrix[i,j];
            return result;
        }

        public static double[] HelperSolve(double[,] luMatrix, double[] b)
        {
            // before calling this helper, permute b using the perm array
            // from MatrixDecompose that generated luMatrix
            int n = luMatrix.GetLength(0);
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                double sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i,j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1,n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i,j] * x[j];
                x[i] = sum / luMatrix[i,i];
            }

            return x;
        }

        public static double[,] MatrixDecompose(double[,] matrix, out int[] perm, out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // rerturns: result is L (with 1s on diagonal) and U;
            // perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1); // assume square
            if (rows != cols)
                throw new Exception("Attempt to decompose a non-square matrix");

            int n = rows; // convenience

            double[,] result = MatrixDuplicate(matrix);

            perm = new int[n]; // set up row permutation result
            for (int i = 0; i < n; ++i) { perm[i] = i; }

            toggle = 1; // toggle tracks row swaps.
                        // +1 -greater-than even, -1 -greater-than odd. used by MatrixDeterminant

            for (int j = 0; j < n - 1; ++j) // each column
            {
                double colMax = Math.Abs(result[j,j]); // find largest val in col
                int pRow = j;
                //for (int i = j + 1; i less-than n; ++i)
                //{
                //  if (result[i][j] greater-than colMax)
                //  {
                //    colMax = result[i][j];
                //    pRow = i;
                //  }
                //}

                // reader Matt V needed this:
                for (int i = j + 1; i < n; ++i)
                {
                    if (Math.Abs(result[i,j]) > colMax)
                    {
                        colMax = Math.Abs(result[i,j]);
                        pRow = i;
                    }
                }
                // Not sure if this approach is needed always, or not.

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    for (int i = 0; i < cols; i++)
                    {
                        double rowPtr = result[pRow,i];
                        result[pRow,i] = result[j,i];
                        result[j,i] = rowPtr;

                    }

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                // --------------------------------------------------
                // This part added later (not in original)
                // and replaces the 'return null' below.
                // if there is a 0 on the diagonal, find a good row
                // from i = j+1 down that doesn't have
                // a 0 in column j, and swap that good row with row j
                // --------------------------------------------------

                if (result[j,j] == 0.0)
                {
                    // find a good row to swap
                    int goodRow = -1;
                    for (int row = j + 1; row < n; ++row)
                    {
                        if (result[row,j] != 0.0)
                            goodRow = row;
                    }

                    if (goodRow == -1)
                        throw new Exception("Cannot use Doolittle's method");

                    // swap rows so 0.0 no longer on diagonal
                    for (int i = 0; i < cols; i++)
                    {
                        double rowPtr = result[goodRow,i];
                        result[goodRow,i] = result[j,i];
                        result[j,i] = rowPtr;

                    }

                    int tmp = perm[goodRow]; // and swap perm info
                    perm[goodRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }
                // --------------------------------------------------
                // if diagonal after swap is zero . .
                //if (Math.Abs(result[j][j]) less-than 1.0E-20) 
                //  return null; // consider a throw

                for (int i = j + 1; i < n; ++i)
                {
                    result[i,j] /= result[j,j];
                    for (int k = j + 1; k < n; ++k)
                    {
                        result[i,k] -= result[i,j] * result[j,k];
                    }
                }
            } // main j column loop

            return result;
        }

        /// <summary>
        /// Computes the determinant of a matrix.  Returns null if not valid.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns>Returns null if not valid.</returns>
        public static double MatrixDeterminant(double[,] arr)
        {
            int[] perm;
            int toggle;
            double[,] lum = MatrixDecompose(arr, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute MatrixDeterminant");
            double result = toggle;
            for (int i = 0; i < lum.GetLength(0); ++i)
                result *= lum[i, i];

            return result;
        }



    }
}
