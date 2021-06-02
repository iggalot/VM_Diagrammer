using System.Windows.Controls;

namespace VMDiagrammer.Interfaces
{
    /// <summary>
    /// A interface for drawable objects
    /// </summary>
    public interface IDrawingObjects
    {
        /// <summary>
        /// Routine to draw a drawable object
        /// </summary>
        /// <param name="c"></param>
        void Draw(Canvas c);
    }
}
