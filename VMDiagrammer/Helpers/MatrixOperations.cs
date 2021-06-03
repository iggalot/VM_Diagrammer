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
    }
}
