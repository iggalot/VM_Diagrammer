namespace VMDiagrammer.Models
{
    /// <summary>
    /// A base class for all model objects in the VM Diagrammer program.
    /// Handles unique ID creation
    /// </summary>
    public class BaseVMObject
    {
        private double m_Index = 0;  // our object's object id

        private double m_Thickness = 1;  // the line thickness of this object

        public BaseVMObject(int index)
        {
            m_Index = index;
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
