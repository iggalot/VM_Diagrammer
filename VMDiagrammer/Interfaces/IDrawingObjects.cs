using System.Windows.Controls;

namespace VMDiagrammer.Interfaces
{
    /// <summary>
    /// A interface for drawable objects
    /// </summary>
    public class IDrawingObjects
    {
        public virtual void Draw(Canvas c) { }
    }
}
