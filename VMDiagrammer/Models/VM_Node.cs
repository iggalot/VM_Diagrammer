using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;

namespace VMDiagrammer.Models
{
    /// <summary>
    /// Data for a typical Node in our model
    /// </summary>
    public class VM_Node : BaseVMObject, IDrawingObjects
    {
        public const double DEFAULT_NODE_RADIUS = 15;
        public const double SUPPORT_LINE_THICKNESS = 5.0;

        /// <summary>
        /// Private members
        /// </summary>
        private double m_X;   // x-coordinate
        private double m_Y;   // y-coordinate
        private SupportTypes m_SupportType;  // type of support as an ENUM

        private static int dof_index_current = 0;

        private bool m_X_restrained = false;
        private bool m_Y_restrained = false;
        private bool m_ROT_restrained = false;

        // Displacement vector for boundary conditions (null = unknown)
        private double?[,] m_DisplaceVector = { { null }, { null }, { null } };
        private double?[,] m_ForceVector = { { null }, { null }, { null } };

        private int[] m_DOFIndices = { 0, 0, 0 };

        // Boundary conditions for displacements
        private double? disp_x = null;
        private double? disp_y = null;
        private double? disp_rot = null;
        public double?[,] DisplacementVector
        {
            get => m_DisplaceVector;
            set { m_DisplaceVector = value; }
        }

        // Load vector for this node
        private double? load_x = null;   // x direction nodal load
        private double? load_y = null;   // y direction nodal load
        private double? load_rot = null; // moment nodal load

        public double?[,] ForceVector
        {
            get => m_ForceVector;
            set
            {
                m_ForceVector = value;
            }
        }

        // index numbers for degrees of freedom
        private int m_dof_x_index = 0;
        private int m_dof_y_index = 0;
        private int m_dof_rot_index = 0;
        public int[] DOF_IndexVector
        {
            get => m_DOFIndices;
            set { m_DOFIndices = value; }
        }



        public int DOF_X
        {
            get => m_dof_x_index;
            set { m_dof_x_index = value; }
        }

        public int DOF_Y
        {
            get => m_dof_y_index;
            set { m_dof_y_index = value; }
        }

        public int DOF_ROT
        {
            get => m_dof_rot_index;
            set { m_dof_rot_index = value; }
        }

        /// <summary>
        /// Accessor for the X coordinate of our node
        /// </summary>
        public double X
        {
            get => m_X;
            set { m_X = value; }
        }

        /// <summary>
        /// Accessor for the Y coordinate of our node
        /// </summary>
        public double Y
        {
            get => m_Y;
            set { m_Y = value; }
        }

        public SupportTypes SupportType
        {
            get => m_SupportType;
            set { m_SupportType = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x position on the canvas</param>
        /// <param name="y">y position on the canvas</param>
        public VM_Node(double x, double y, bool x_restrain, bool y_restrain, bool rot_restrain)
        {
            m_X_restrained = x_restrain;
            m_Y_restrained = y_restrain;
            m_ROT_restrained = rot_restrain;

            X = x;
            Y = y;

            // Update degree of freedom indices
            DOF_X = dof_index_current;
            dof_index_current++;
            DOF_Y = dof_index_current;
            dof_index_current++;
            DOF_ROT = dof_index_current;
            dof_index_current++;

            // Store index numbers in array
            m_DOFIndices[0] = DOF_X;
            m_DOFIndices[1] = DOF_Y;
            m_DOFIndices[2] = DOF_ROT;


            // Determine if boundary conditions can be classified as a SupportType directly
            SupportType = SupportTypes.SUPPORT_UNDEFINED;  // default

            if (m_X_restrained)
            {
                if (m_Y_restrained)
                {
                    if (m_ROT_restrained)
                    {
                        m_SupportType = SupportTypes.SUPPORT_FIXED;
                        disp_x = 0;
                        disp_y = 0;
                        disp_rot = 0;
                    }
                    else
                    {
                        m_SupportType = SupportTypes.SUPPORT_PIN;
                        disp_x = 0;
                        disp_y = 0;
                        load_rot = 0;
                    }
                } else
                {
                    m_SupportType = SupportTypes.SUPPORT_ROLLER_Y;
                    disp_x = 0;
                    load_y = 0;
                    load_rot = 0;
                }
            } else
            {
                if (m_Y_restrained)
                {
                    m_SupportType = SupportTypes.SUPPORT_ROLLER_X;
                    disp_y = 0;
                    load_x = 0;
                    load_rot = 0;

                }
            }

            // Assign boundary condition to the nodes displacement vector.
            m_DisplaceVector[0, 0] = disp_x;
            m_DisplaceVector[1, 0] = disp_y;
            m_DisplaceVector[2, 0] = disp_rot;

            // Assign boundary condition to the nodes displacement vector.
            m_ForceVector[0, 0] = load_x;
            m_ForceVector[1, 0] = load_y;
            m_ForceVector[2, 0] = load_rot;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public VM_Node()
        {

        }

        public override string ToString()
        {
            string str = "";
            str += Index.ToString() + " -- X: " + X + "    Y: " + Y;
            if (SupportType != SupportTypes.SUPPORT_UNDEFINED)
                str += "    (S)";
            str += "-- DOF " + DOF_X.ToString() + "  " + DOF_Y.ToString() + "  " + DOF_ROT.ToString();
            str += "\n";
            return str;

        }

        /// <summary>
        /// The method to draw this object
        /// </summary>
        /// <param name="c"></param>
        public void Draw(Canvas c)
        {
            double radius = 15.0;
            double offset = radius / 2.0;

            switch (m_SupportType)
            {
                case SupportTypes.SUPPORT_ROLLER_X:
                    {
                        // Draw the node icon
                        DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS, 1.0);

                        // Draw the roller "ball"
                        DrawingHelpers.DrawCircleHollow(c, this.X, this.Y+2.0 * offset, Brushes.Blue, radius, SUPPORT_LINE_THICKNESS);

                        // Draw the surface line
                        double startX = this.X - radius;
                        double startY = this.Y + offset;
                        double endX = this.X + radius;
                        double endY = this.Y + offset;

                        DrawingHelpers.DrawLine(c, startX, startY + 2.0 * offset, endX, endY + 2.0 * offset, Brushes.Blue, SUPPORT_LINE_THICKNESS);
                    }
                    break;
                case SupportTypes.SUPPORT_ROLLER_Y:
                    break;
                case SupportTypes.SUPPORT_PIN:
                    {
                        double insertX = this.X;
                        double insertY = this.Y + offset;
                        double startX = insertX - 0.75 * radius;
                        double startY = insertY+ 2.0 * offset;
                        double endX = insertX + 0.75 * radius;
                        double endY = insertY + 2.0 * offset;

                        double base_startX = startX - 0.5 * radius;
                        double base_startY = startY;

                        double base_endX = endX + 0.52 * radius;
                        double base_endY = endY;

                        // Draw the node icon
                        DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS, 1.0);

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, startX, startY, insertX, insertY, Brushes.Green, SUPPORT_LINE_THICKNESS);
                        DrawingHelpers.DrawLine(c, insertX, insertY, endX, endY, Brushes.Green, SUPPORT_LINE_THICKNESS);
                        DrawingHelpers.DrawLine(c, endX, endY, startX, startY, Brushes.Green, SUPPORT_LINE_THICKNESS);

                        // Draw the base line
                        DrawingHelpers.DrawLine(c, base_startX, base_startY, base_endX, base_endY, Brushes.Green, SUPPORT_LINE_THICKNESS);

                        break;
                    }

                case SupportTypes.SUPPORT_FIXED:
                    {
                        double insertX = this.X;
                        double insertY = this.Y;
                        double startX = insertX - radius;
                        double startY = insertY - 3.0 * offset;
                        double endX = insertX - radius;
                        double endY = insertY + 3.0 * offset;

                        // Draw the node icon
                        DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS, 1.0);

                        // Draw the pin "triangle"
                        DrawingHelpers.DrawLine(c, startX, startY, endX, endY, Brushes.Red, SUPPORT_LINE_THICKNESS);

                        break;
                    }

                case SupportTypes.SUPPORT_UNDEFINED:
                    // Default node
                    DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS, 1.0);
                    break;
                default:
                    {
                        throw new NotImplementedException("Support type drawing ability not defined for support type: " + m_SupportType);
                    }
            }

            // Draw the node label
            DrawNodeLabel(c, this.X, this.Y, 0);
        }

        private void DrawNodeLabel(Canvas c, double x, double y, double z, double size=DrawingHelpers.DEFAULT_TEXT_HEIGHT, TextPositions pos=TextPositions.TEXT_ABOVE)
        {
            double xpos = x;
            double ypos = y;
            double zpos = z;

            switch (pos)
            {
                case TextPositions.TEXT_ABOVE:
                    ypos -= 2.5*size;
                    break;
                case TextPositions.TEXT_BELOW:
                    ypos += 0.5 * size -4;
                    break;
                case TextPositions.TEXT_LEFT:
                case TextPositions.TEXT_RIGHT:
                default:
                    throw new NotImplementedException("Invalid text position, " + pos + " detected in DrawText function");
            }

            // Draw the node icon
            DrawingHelpers.DrawCircle(c, this.X, this.Y, Brushes.Black, Brushes.White, DEFAULT_NODE_RADIUS, 1.0);
            
            // Draw the node label text
            DrawingHelpers.DrawText(c, xpos, ypos, zpos, Index.ToString(), Brushes.Black, DrawingHelpers.DEFAULT_TEXT_HEIGHT);

            // Draw the node label icon
            DrawingHelpers.DrawCircleHollow(c, xpos+3, ypos+8, Brushes.Black, 1.5 * DrawingHelpers.DEFAULT_TEXT_HEIGHT);
        }
    }
}
