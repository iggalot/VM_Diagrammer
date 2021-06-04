using System.Collections.Generic;
using VMDiagrammer.Interfaces;
using VMDiagrammer.Models;

namespace VMDiagrammer.Helpers
{
    /// <summary>
    /// Contains useful math routines
    /// </summary>
    public static class MathHelpers
    {
        /// <summary>
        /// Bubble sort that sorts a list (in-place) based on the X-coordinate (smallest first)
        /// </summary>
        /// <param name="arr"></param>
        public static void BubbleSortXCoord(ref List<IDrawingObjects> arr)
        {
            // get number of elements
            int n = arr.Count;

            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < n-i-1; j++)
                    if(((VM_Node)arr[j]).X > ((VM_Node)arr[j + 1]).X)
                    {
                        // swap temp and arr[i]
                        VM_Node temp = ((VM_Node)arr[j]);
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
        }
    }
}
