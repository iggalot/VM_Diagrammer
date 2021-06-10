namespace VMDiagrammer.Models
{
    /// <summary>
    /// Enum for types of supports
    /// </summary>
    public enum SupportTypes
    {
        SUPPORT_UNDEFINED   = -1,
        SUPPORT_ROLLER_X    = 0,
        SUPPORT_ROLLER_Y    = 1,
        SUPPORT_PIN         = 2,
        SUPPORT_FIXED       = 3,
    }

    public class VM_Support : VM_Node
    {
       public void Draw() { }
    }
}
