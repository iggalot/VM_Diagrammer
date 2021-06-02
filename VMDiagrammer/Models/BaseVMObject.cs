namespace VMDiagrammer.Models
{
    /// <summary>
    /// A base class for all model objects in the VM Diagrammer program.
    /// Handles unique ID creation
    /// </summary>
    public class BaseVMObject
    {
        private static double currentIndex = 0;
        private double m_Index = 0;  // our object's id

        private double m_Thickness = 1;  // the line thickness of this object

        public BaseVMObject()
        {
            m_Index = currentIndex;
            currentIndex++;
        }

        public double Index
        {
            get
            {
                return m_Index;
            }
            private set
            {
                m_Index = value;
            }
        }

        public double Thickness
        {
            get
            {
                return m_Thickness;
            }
            set
            {
                m_Thickness = value;
            }
        }
    }
}
