using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;
using VMDiagrammer.Models;
using VMDiagrammer.Models.Elements;

namespace VMDiagrammer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Point m_CurrentMouseLocation = new Point();
        private string m_MouseLocationString = "N/A";
        private List<IDrawingObjects> l_Nodes = new List<IDrawingObjects>();
        private List<IDrawingObjects> l_Beams = new List<IDrawingObjects>();
        private List<IDrawingObjects> l_Loads = new List<IDrawingObjects>();
        private List<IDrawingObjects> l_CriticalPoints = new List<IDrawingObjects>();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // List containing all of our objects to be drawn
        public List<IDrawingObjects> CriticalPoints
        {
            get
            {
                return l_CriticalPoints;
            }
            set
            {
                l_CriticalPoints = value;
            }
        }

        public List<IDrawingObjects> Nodes
        {
            get
            {
                return l_Nodes;
            }
            set
            {
                l_Nodes = value;
            }
        }

        public List<IDrawingObjects> Beams
        {
            get
            {
                return l_Beams;
            }
            set
            {
                l_Beams = value;
            }
        }

        public List<IDrawingObjects> Loads
        {
            get
            {
                return l_Loads;
            }
            set
            {
                l_Loads = value;
            }
        }

        public VM_Node LeftMostNode { get; set; } = null; // leftmost node on the structure
        public VM_Node RightMostNode { get; set; } = null; // leftmost node on the structure

        public string strDisplayInfo
        {
            get => DisplayInfo();
        }

        protected void UpdateBeamExtents()
        {
            // if there arent current nodes in the list, exit
            if (Nodes.Count == 0)
                return;

            VM_Node temp = (VM_Node)Nodes[0];

            // first check for leftmost
            foreach (VM_Node item in Nodes)
            {
                if (item.X < temp.X)
                    temp = item;
                else if (item.X == temp.X)
                {
                    if (item.Y < temp.Y)
                        temp = item;
                }
            }
            LeftMostNode = temp;

            // now check for rightmost node
            if(Nodes.Count > 1)
            {
                temp = (VM_Node)Nodes[Nodes.Count - 1];

                foreach (VM_Node item in Nodes)
                {
                    if (item.X > temp.X)
                        temp = item;
                    else if (item.X == temp.X)
                    {
                        if (item.Y > temp.Y)
                            temp = item;
                    }
                }
                RightMostNode = temp;
            } else
            {
                RightMostNode = LeftMostNode;
            }
        }

        /// <summary>
        /// Adds a node to the Nodes list and updates the reference
        /// </summary>
        /// <param name="node"></param>
        protected void AddNode(VM_Node node)
        {
            Nodes.Add(node);

            // Now update our leftmost and right most nodes -- and our critical points
            UpdateBeamExtents();
        }

        /// <summary>
        /// Returns a VM_Node object for specified coordinates if it exists in a list.
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        /// <param name="l">the list of <see cref="IDrawingObjects"/> to be searched for matching nodes</param>
        /// <returns>true if a matching node is not found, otherwise it returns the false</returns>
        private VM_Node FindNodeInList(double x, double y, List<IDrawingObjects> l)
        {
            double temp_x = x;
            double temp_y = y;

            // search our Nodes list for node at this location
            foreach (VM_Node n in l)
            {
                if (n.X == temp_x)
                {
                    // A node at this point has same x value
                    if (n.Y == temp_y)
                    {
                        return n;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Function that creates a sorted list of critical points from lowest to highest
        /// </summary>
        /// <returns></returns>
        protected List<IDrawingObjects> ListCriticalPoints()
        {
            List<IDrawingObjects> list = new List<IDrawingObjects>();

            // Support Reactions
            foreach (VM_Node item in Nodes)
            {
                if(item.SupportType != SupportTypes.SUPPORT_UNDEFINED)
                {
                    list.Add(item);
                }
            }

            // Begin / End of the beam (leftmost / right most)
            if (!list.Contains(LeftMostNode))
                list.Add(LeftMostNode);
            if (!list.Contains(RightMostNode))
                list.Add(RightMostNode);

            
            // Concentrated forces and moments
            foreach (VM_BaseLoad item in Loads)
            {
                // Concentrated forces or moments
                if (item.LoadType == LoadTypes.LOADTYPE_CONC_FORCE || item.LoadType == LoadTypes.LOADTYPE_CONC_MOMENT)
                {
                    // Are the start and end points the same?  -- they should be be ifs a concentrated effect
                    // Make a nodes based on location the load
                    if (item.D1 == item.D2)
                    {
                        // Is the load at the start node or end node?
                        // 1. It's the start node?
                        if (item.D1 == 0)
                        {
                            if (!list.Contains(item.Beam.Start))
                                list.Add(item.Beam.Start);
                        }

                        // 2. It's the end node?
                        else if (item.D2 == Math.Abs(item.Beam.End.X - item.Beam.Start.X))
                        {
                            if (!list.Contains(item.Beam.End))
                                list.Add(item.Beam.End);
                        }

                        // 3. It's in the middle somewhere, so make a temp node and add it to the list
                        // Check that the new node isn't already in the Nodes list.
                        else
                        {
                            double temp_x = item.Beam.Start.X + item.D1;
                            double temp_y = item.Beam.Start.Y;

                            // First check our Nodes list to see if it exists
                            VM_Node tempNode = FindNodeInList(temp_x, temp_y, Nodes);

                            // Is the node already in Nodes list
                            if (tempNode != null)
                            {
                                // Is the node in the Critical Point list?
                                if (FindNodeInList(tempNode.X, tempNode.Y, list) == null)
                                {
                                    // No matching node already in the critical point list..
                                    list.Add(tempNode);
                                } else
                                {
                                    // Matching node found so do nothing
                                }
                            } else
                            {
                                // Is a node with these coordinates already in the Critical Point list?
                                if(FindNodeInList(temp_x,temp_y,list) == null)
                                {
                                    // No matching node already in critical point list..
                                    list.Add(new VM_Node(temp_x, temp_y, false, false, false));
                                } else
                                {
                                    // Do nothing
                                }
                            }
                        }
                    }
                }
                // Begin / End of distributed forces
                else if (item.LoadType == LoadTypes.LOADTYPE_DIST_FORCE || item.LoadType == LoadTypes.LOADTYPE_DIST_MOMENT)
                {
                    // if the end of dist. load is to the right of start of dist. load
                    if (item.D2 > item.D1)
                    {
                        // 1. Is the start point at the beam's start node
                        if (item.D1 == 0)
                        {
                            if (!list.Contains(item.Beam.Start))
                                list.Add(item.Beam.Start);
                        }
                        else
                        {
                            double temp_x = item.Beam.Start.X + item.D1;
                            double temp_y = item.Beam.Start.Y;

                            // First check our Nodes list to see if it exists
                            VM_Node tempNode = FindNodeInList(temp_x, temp_y, Nodes);

                            // Is the node already in Nodes list
                            if (tempNode != null)
                            {
                                // Is the node in the Critical Point list?
                                if (FindNodeInList(tempNode.X, tempNode.Y, list) == null)
                                {
                                    // No matching node already in the critical point list..
                                    list.Add(tempNode);
                                }
                                else
                                {
                                    // Matching node found so do nothing
                                }
                            }
                            else
                            {
                                // Is a node with these coordinates already in the Critical Point list?
                                if (FindNodeInList(temp_x, temp_y, list) == null)
                                {
                                    // No matching node already in critical point list..
                                    list.Add(new VM_Node(temp_x, temp_y, false, false, false));
                                }
                                else
                                {
                                    // Do nothing
                                }
                            }                            
                        }

                        // 2. Is the end point of the distributed load at the beam's end node?
                        if (item.D2 == (Math.Abs(item.Beam.End.X - item.Beam.Start.X)))
                        {
                            if (!list.Contains(item.Beam.End))
                                list.Add(item.Beam.End);
                        }
                    }
                    else
                    {
                        double temp_x = item.Beam.Start.X + item.D2;
                        double temp_y = item.Beam.Start.Y;

                        // First check our Nodes list to see if it exists
                        VM_Node tempNode = FindNodeInList(temp_x, temp_y, Nodes);

                        // Is the node already in Nodes list
                        if (tempNode != null)
                        {
                            // Is the node in the Critical Point list?
                            if (FindNodeInList(tempNode.X, tempNode.Y, list) == null)
                            {
                                // No matching node already in the critical point list..
                                list.Add(tempNode);
                            }
                            else
                            {
                                // Matching node found so do nothing
                            }
                        }
                        else
                        {
                            // Is a node with these coordinates already in the Critical Point list?
                            if (FindNodeInList(temp_x, temp_y, list) == null)
                            {
                                // No matching node already in critical point list..
                                list.Add(new VM_Node(temp_x, temp_y, false, false, false));
                            }
                            else
                            {
                                // Do nothing
                            }
                        }
                    }
                }
            }

            // TODO:: Shear = 0 critical point location
            // Shear = 0;

            return list;
        }

        /// <summary>
        /// The current mouse location stored as a Windows Point.
        /// </summary>
        public Point CurrentMouseLocationPoint
        {
            get => m_CurrentMouseLocation;
            set
            {
                m_CurrentMouseLocation = value;
                MouseLocationString = MouseLocationString = CurrentMouseLocationPoint.X + " , " + CurrentMouseLocationPoint.Y;
                OnPropertyChanged("CurrentMouseLocationPoint");
            }
        }

        /// <summary>
        /// Returns the current mouse location as a string for displaying in the UI
        /// </summary>
        public string MouseLocationString
        {
            get
            {
                return m_MouseLocationString;
            }
            set
            {
                m_MouseLocationString = value;
                OnPropertyChanged("MouseLocationString");
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this; // Needed for the data binding!  Don't forget this line!!

            OnUserCreate();  // called once when application starts

            OnUserUpdate();  // called when a redraw is needed (once per frame?)
        }

        /// <summary>
        /// Routine that is called once per cycle (frame) to update the visuals for the model
        /// </summary>
        private void OnUserUpdate()
        {
            // Draw the beams -- drawn first so they are behind everything on the screen
            foreach (VM_Beam item in Beams)
                item.Draw(MainCanvas);

            // Draw the nodes
            foreach (VM_Node item in Nodes)
                item.Draw(MainCanvas);

            // Draw the loads
            foreach (VM_BaseLoad item in Loads)
            {
                switch(item.LoadType)
                {
                    case LoadTypes.LOADTYPE_CONC_FORCE:
                        ((VM_PointForce)item).Draw(MainCanvas);
                        break;
                    case LoadTypes.LOADTYPE_DIST_FORCE:
                        ((VM_DistributedForce)item).Draw(MainCanvas);
                        break;
                    case LoadTypes.LOADTYPE_CONC_MOMENT:
                        ((VM_PointMoment)item).Draw(MainCanvas);
                        break;
                    case LoadTypes.LOADTYPE_UNDEFINED:
                    case LoadTypes.LOADTYPE_DIST_MOMENT:
                    default:
                        throw new NotImplementedException("In drawing loads of MainWindow.xaml.cs -- load type detected for which there is no DRAW implemented!");
                }
            }

            // Draw the lines for the critical points
            foreach(VM_Node item in CriticalPoints)
            {
                DrawingHelpers.DrawLine(MainCanvas, item.X, item.Y + 50, item.X, item.Y + 600, Brushes.Green, item.Thickness);
            }

            // Draw a reference line just below the nodes (temp)
            //           DrawingHelpers.DrawLine(MainCanvas, LeftMostNode.X, LeftMostNode.Y + 40, RightMostNode.X, RightMostNode.Y + 40, Brushes.Blue);

            // Testing node location function
            //for(double i = LeftMostNode.X; i<= 520; i = i+10)
            //Console.WriteLine(i.ToString() + " is on Beam #" + GetBeamByXCoord(i).Index) ;
        
        }

        /// <summary>
        /// Routine that is called once on application started
        /// </summary>
        private void OnUserCreate()
        {
            Nodes = new List<IDrawingObjects>();
            Beams = new List<IDrawingObjects>();
            Loads = new List<IDrawingObjects>();

            // Create some nodes
            VM_Node NodeC = new VM_Node(320, 200, true, true, true);
            AddNode(NodeC);
            VM_Node NodeA = new VM_Node(120, 200, false, false, false);
            AddNode(NodeA);

            //VM_Node NodeB = new VM_Node(420, 200, false, true, false);
            //AddNode(NodeB);
            //VM_Node NodeD = new VM_Node(520, 200, false, false, false);
            //AddNode(NodeD);
            //VM_Node NodeE = new VM_Node(50, 200, false, false, false);
            //AddNode(NodeE);

            // Create some beams
            VM_Beam Beam1 = new VM_Beam(NodeC, NodeA);
            Beams.Add(Beam1);
            //VM_Beam Beam2 = new VM_Beam(NodeC, NodeB);
            //Beams.Add(Beam2);
            //VM_Beam Beam3 = new VM_Beam(NodeB, NodeD);
            //Beams.Add(Beam3);
            //VM_Beam Beam4 = new VM_Beam(NodeE, NodeA);
            //Beams.Add(Beam4);

            //// Add point load
            //VM_BaseLoad loada = new VM_PointForce((VM_Beam)Beams[0], 100, 100, -50, -50);
            //Loads.Add(loada);

            //// Add point load
            //VM_BaseLoad loadb = new VM_PointForce((VM_Beam)Beams[2], 60, 60, +100, +100);
            //Loads.Add(loadb);

            //// Add distributed load
            //VM_BaseLoad load2 = new VM_DistributedForce((VM_Beam)Beams[0], 50, 150, -50, -80);
            //Loads.Add(load2);
            //// Add distributed load
            //VM_BaseLoad load4 = new VM_DistributedForce((VM_Beam)Beams[0], 150, 200, -80, 0);
            //Loads.Add(load4);
            //// Add distributed load
            //VM_BaseLoad load3 = new VM_DistributedForce((VM_Beam)Beams[1], 20, 60, +30, +100);
            //Loads.Add(load3);
            //// Add distributed load
            //VM_BaseLoad load5 = new VM_DistributedForce((VM_Beam)Beams[1], 60, 80, +100, +100);
            //Loads.Add(load5);

            //// Add concentrated moments
            //VM_BaseLoad loadc = new VM_PointMoment((VM_Beam)Beams[3], 70, 70, 50, 50, ArrowDirections.ARROW_COUNTERCLOCKWISE);
            //Loads.Add(loadc);
            //VM_BaseLoad loadd = new VM_PointMoment((VM_Beam)Beams[2], 0, 0, 50, 50, ArrowDirections.ARROW_CLOCKWISE);
            //Loads.Add(loadd);

            // Create a list of Critical Points.
            CriticalPoints = ListCriticalPoints();

            // Sort lists in order of lowest to highest X-coord
            MathHelpers.BubbleSortNodesByXCoord(ref l_Nodes);
            MathHelpers.BubbleSortBeamsByXCoord(ref l_Beams);
            MathHelpers.BubbleSortNodesByXCoord(ref l_CriticalPoints);

            // TODO:: Sort the loads from left to right based on D1 position and Start node of beam position....



            // Create our Structure Stiffness Model from our beams
            StructureStiffnessModel model = new StructureStiffnessModel(Nodes.Count*3, Nodes.Count*3);
            foreach (var beam in Beams)
            {
                model.AddElement((VM_Beam)beam);
                // TODO::  Add matrix transformations.
            }

            model.Assemble();
            Console.WriteLine(model.ToString());
            model.GroupFixedFree();
            Console.WriteLine(model.ToString());

            // Testing for our matrix manipulation functions
            model.GlobalStiffnessMatrix = MatrixOperations.RemoveDOF(model.GlobalStiffnessMatrix, model.Rows, model.Cols, 0);
            model.Rows--;  // update the model number of rows
            model.Cols--;  // update the model number of columns
            Console.WriteLine(model.ToString());

            model.GlobalStiffnessMatrix = MatrixOperations.RemoveDOF(model.GlobalStiffnessMatrix, model.Rows, model.Cols, 4);
            model.Rows--;  // update the model number of rows
            model.Cols--;  // update the model number of columns
            Console.WriteLine(model.ToString());

            model.GlobalStiffnessMatrix = MatrixOperations.RemoveDOF(model.GlobalStiffnessMatrix, model.Rows, model.Cols, 3);
            model.Rows--;  // update the model number of rows
            model.Cols--;  // update the model number of columns
            Console.WriteLine(model.ToString());

            //BeamElement elem = new BeamElement((VM_Node)Nodes[0], (VM_Node)Nodes[1], 1, 1, 1, 1);
            //Console.WriteLine(elem.ToString());

        }

        /// <summary>
        /// Our event for when the mouse moves over the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CurrentMouseLocationPoint = e.GetPosition(MainCanvas);
        }

        /// <summary>
        /// Out event for when the mouse button is released.  Used to draw a node at that location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(MainCanvas);  // retrieve the current mouse poistion

            // Create a node at the position
            VM_Node newNode = new VM_Node(p.X, p.Y, false, false, false);  

            // Add it to the model
            Nodes.Add(newNode);

            OnUserUpdate();
        }


        private protected string DisplayInfo()
        {
            string str = "";
            str += NodesListToString() + "\n";
            str += BeamsListToString()+ "\n";
            str += CriticalPointsListToString() + "\n";


            return str;
        }

        private protected string NodesListToString()
        {
            string str = "";
            str += "NODES\n";
            str += "-----------------------------------\n";
            foreach (IDrawingObjects item in Nodes)
            {
                str += ((VM_Node)item).ToString();
            }

            return str;
        }

        private protected string CriticalPointsListToString()
        {
            string str = "";
            str += "CRITICAL POINTS\n";
            str += "-----------------------------------\n";
            foreach (IDrawingObjects item in CriticalPoints)
            {
                str += ((VM_Node)item).ToString();
            }

            return str;
        }

        private protected string BeamsListToString()
        {
            string str = "";
            str += "BEAMS\n";
            str += "-----------------------------------\n";
            foreach (IDrawingObjects item in Beams)
            {
                str += ((VM_Beam)item).ToString();
            }

            return str;
        } 

        /// <summary>
        /// Returns the distance of an offset from the START node of a beam
        /// to the LeftMostNode
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected double GetDistanceFromLeftMost(double offset, VM_Beam beam)
        {
            return beam.Start.X + offset - LeftMostNode.X;
        }

        /// <summary>
        /// Returns the beam that contains a specified x-coordinate.  If the x-coord is at the Rightmost node, return the
        /// rightmost beam
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected VM_Beam GetBeamByXCoord(double x)
        {
            foreach(VM_Beam b in Beams)
            {
                // Is x at the RightMostNode?  If so, consider it found, return the beam
                if(((VM_Beam)b).End.X == x)
                        return b;

                // Otherwise, check that x is between the start (inclusive) and end of the beam
                if(((VM_Beam)b).End.X > x && ((VM_Beam)b).Start.X <= x)
                {
                    // found our beam!
                    return b;
                }
            }

            // Otherwise throw an exception since our X coord isn't on a current beam.
            throw new NotImplementedException("No beam found contains specified position of X: " + x);
        }

        /// <summary>
        /// Algorithm for computing support reactions of statically determinate beam
        /// </summary>
        protected void ComputeVerticalSupportReactions()
        {
            // Compute total vertical force on beam
            double totalVert = 0.0;
            foreach(IDrawingObjects b in Loads)
            {
                VM_BaseLoad bl = (VM_BaseLoad)b;

                switch (bl.LoadType)
                {

                    case LoadTypes.LOADTYPE_CONC_FORCE:
                        break;
                    case LoadTypes.LOADTYPE_CONC_MOMENT:
                        break;
                    case LoadTypes.LOADTYPE_DIST_FORCE:
                        break;
                    case LoadTypes.LOADTYPE_DIST_MOMENT:
                        break;
                    case LoadTypes.LOADTYPE_UNDEFINED:                      
                    default:
                        throw new NotImplementedException("Unrecognized Load Type in load " + ": " + bl.LoadType);
                        break;
                }
            }
                    
            // Compute equivalent moment calculation at leftmost support reaction
                    //Compute moments of all loads relative to leftmost support reaction
                    // Add results to get total equivalent moment

            // If selected support is a fixed support (has a concentrated moment).
                   // Moment reaction is Total equivalent moment.
                   // Vertical force at support is total vertical force
            // Else
                   // Divide total by distance to other support to get second vertical reaction
                   // Compute second vertical reaction = Total vertical force minus 2nd reaction.
        }

        /// <summary>
        /// Algorithm for finding the shear value at a specified point x along the beam.
        /// </summary>
        /// <param name="x"></param>
        protected void ComputeShearValueAtX(double x)
        {
            // Create a list for shear values
            // Create a correspondind list for position x

            ////////////////////////////////////////
            /// Begin Algorithm
            /// ////////////////////////////////////
            
            // Start at leftmost (this is a CP).
                // Plot a zero point.

            // If on critical point, check for instantaneous critical point changes
                // 1. Support reactions
                // 2. Concentrated forces
                    // Compute value to left of CP
                    // Compute value just to right of CP

            // Search for distributed loads between previous critical point and current point.
                // Does dist. load straddle the CP?
                   // 1. No -- it ends at CP.  And isn't allowed to extend beyond -- we throw an exception when the load is created if this happens..

                // Iterate along distributed load for 10x points to establish the line.

            // At last point (after CP), plot a zero point
        }
    }

}
