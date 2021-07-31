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

        private int m_NextNodeNumber = 0;  // the numbering for the next node

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

        public StructureStiffnessModel Model = null;

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

        public string strDisplayDisplacementMatrixResults
        {
            get 
            {
                string str = "";
                //str += Model.DisplayMatrixInfo(Model.K_Free_Free);

                str += "\nDisplacement Vector \n";
                str += Model.DisplayMatrixInfoNullable(Model.DisplacementVector) + "\n";
                str += "********************************\n";
                return str;
            }
        }

        public string strDisplayLoadMatrixResults
        {
            get
            {
                string str = "";
                //str += Model.DisplayMatrixInfo(Model.K_Free_Free);
                str += "\nLoad Vector \n";
                str += Model.DisplayMatrixInfoNullable(Model.LoadVector) + "\n";
                str += "********************************\n";

                return str;
            }
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
        private VM_Node FindVMNodeInList(double x, double y, List<IDrawingObjects> l)
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
        protected List<IDrawingObjects> CreateVMNodeCriticalPointsList()
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
                            VM_Node tempNode = FindVMNodeInList(temp_x, temp_y, Nodes);

                            // Is the node already in Nodes list
                            if (tempNode != null)
                            {
                                // Is the node in the Critical Point list?
                                if (FindVMNodeInList(tempNode.X, tempNode.Y, list) == null)
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
                                if(FindVMNodeInList(temp_x,temp_y,list) == null)
                                {
                                    // No matching node already in critical point list..
                                    list.Add(new VM_Node(temp_x, temp_y, false, false, false, GetNextNodeNumber()));
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
                            VM_Node tempNode = FindVMNodeInList(temp_x, temp_y, Nodes);

                            // Is the node already in Nodes list
                            if (tempNode != null)
                            {
                                // Is the node in the Critical Point list?
                                if (FindVMNodeInList(tempNode.X, tempNode.Y, list) == null)
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
                                if (FindVMNodeInList(temp_x, temp_y, list) == null)
                                {
                                    // No matching node already in critical point list..
                                    list.Add(new VM_Node(temp_x, temp_y, false, false, false,GetNextNodeNumber()));
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
                        VM_Node tempNode = FindVMNodeInList(temp_x, temp_y, Nodes);

                        // Is the node already in Nodes list
                        if (tempNode != null)
                        {
                            // Is the node in the Critical Point list?
                            if (FindVMNodeInList(tempNode.X, tempNode.Y, list) == null)
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
                            if (FindVMNodeInList(temp_x, temp_y, list) == null)
                            {
                                // No matching node already in critical point list..
                                list.Add(new VM_Node(temp_x, temp_y, false, false, false, GetNextNodeNumber()));
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
        /// Helper function to quickly construct test cases
        /// </summary>
        private void TestMultispanContinuousComplexCase()
        {

            // Create some nodes
            VM_Node NodeA = new VM_Node(50, 100, true, true, true,0);
            AddNode(NodeA);
            VM_Node NodeB = new VM_Node(75, 100, false, false, false,1);
            AddNode(NodeB);
            VM_Node NodeC = new VM_Node(100, 100, false, false, false,2);
            AddNode(NodeC);
            VM_Node NodeD = new VM_Node(150, 100, false, false, false,3);
            AddNode(NodeD);
            VM_Node NodeE = new VM_Node(200, 100, false, false, false,4);
            AddNode(NodeE);
            VM_Node NodeF = new VM_Node(250, 100, false, true, false,5);
            AddNode(NodeF);
            VM_Node NodeG = new VM_Node(300, 100, false, false, false,6);
            AddNode(NodeG);
            VM_Node NodeH = new VM_Node(350, 100, false, true, false,7);
            AddNode(NodeH);
            VM_Node NodeI = new VM_Node(400, 100, false, false, false,8);
            AddNode(NodeI);
            VM_Node NodeJ = new VM_Node(450, 100, true, true, false,9);
            AddNode(NodeJ);


            //// Create some beams
            VM_Beam Beam1 = new VM_Beam(NodeB, NodeA, 10);
            Beams.Add(Beam1);
            VM_Beam Beam2 = new VM_Beam(NodeC, NodeB, 11);
            Beams.Add(Beam2);
            VM_Beam Beam3 = new VM_Beam(NodeC, NodeD, 12);
            Beams.Add(Beam3);
            VM_Beam Beam4 = new VM_Beam(NodeE, NodeD,13);
            Beams.Add(Beam4);
            VM_Beam Beam5 = new VM_Beam(NodeE, NodeF, 14);
            Beams.Add(Beam5);
            VM_Beam Beam6 = new VM_Beam(NodeF, NodeG,15);
            Beams.Add(Beam6);
            VM_Beam Beam7 = new VM_Beam(NodeG, NodeH,16);
            Beams.Add(Beam7);
            VM_Beam Beam8 = new VM_Beam(NodeH, NodeI, 17);
            Beams.Add(Beam8);
            VM_Beam Beam9 = new VM_Beam(NodeI, NodeJ, 18);
            Beams.Add(Beam9);
        }

        private void TestCantileverRightCase()
        {
            // Create some nodes
            VM_Node NodeA = new VM_Node(50, 100, true, true, true,0);
            AddNode(NodeA);
            VM_Node NodeB = new VM_Node(75, 100, false, false, false,1);
            AddNode(NodeB);
            VM_Node NodeC = new VM_Node(100, 100, false, false, false,2);
            AddNode(NodeC);
            VM_Node NodeD = new VM_Node(125, 100, false, false, false,3);
            AddNode(NodeD);
            VM_Node NodeE = new VM_Node(150, 100, false, false, false,4);
            AddNode(NodeE);
            VM_Node NodeF = new VM_Node(175, 100, false, false, false,5);
            AddNode(NodeF);
            VM_Node NodeG = new VM_Node(200, 100, false, false, false,6);
            AddNode(NodeG);
            VM_Node NodeH = new VM_Node(225, 100, false, false, false,7);
            AddNode(NodeH);
            VM_Node NodeI = new VM_Node(250, 100, false, false, false,8);
            AddNode(NodeI);


            //// Create some beams
            VM_Beam Beam1 = new VM_Beam(NodeA, NodeB, 9);
            Beams.Add(Beam1);
            VM_Beam Beam2 = new VM_Beam(NodeB, NodeC, 10);
            Beams.Add(Beam2);
            VM_Beam Beam3 = new VM_Beam(NodeC, NodeD, 11);
            Beams.Add(Beam3);
            VM_Beam Beam4 = new VM_Beam(NodeD, NodeE, 12);
            Beams.Add(Beam4);
            VM_Beam Beam5 = new VM_Beam(NodeE, NodeF, 13);
            Beams.Add(Beam5);
            VM_Beam Beam6 = new VM_Beam(NodeF, NodeG, 14);
            Beams.Add(Beam6);
            VM_Beam Beam7 = new VM_Beam(NodeG, NodeH, 15);
            Beams.Add(Beam7);
            VM_Beam Beam8 = new VM_Beam(NodeH, NodeI, 16);
            Beams.Add(Beam8);
        }


        private void TestCantileverLeftCase()
        {
            // Create some nodes
            VM_Node NodeA = new VM_Node(50, 100, false, false, false,0);
            AddNode(NodeA);
            VM_Node NodeB = new VM_Node(75, 100, false, false, false,1);
            AddNode(NodeB);
            VM_Node NodeC = new VM_Node(100, 100, false, false, false,2);
            AddNode(NodeC);
            VM_Node NodeD = new VM_Node(125, 100, false, false, false,3);
            AddNode(NodeD);
            VM_Node NodeE = new VM_Node(150, 100, false, false, false,4);
            AddNode(NodeE);
            VM_Node NodeF = new VM_Node(175, 100, false, false, false,5);
            AddNode(NodeF);
            VM_Node NodeG = new VM_Node(200, 100, false, false, false,6);
            AddNode(NodeG);
            VM_Node NodeH = new VM_Node(225, 100, false, false, false,7);
            AddNode(NodeH);
            VM_Node NodeI = new VM_Node(250, 100, true, true, true,8);
            AddNode(NodeI);


            //// Create some beams
            VM_Beam Beam1 = new VM_Beam(NodeA, NodeB, 9);
            Beams.Add(Beam1);
            VM_Beam Beam2 = new VM_Beam(NodeB, NodeC,10);
            Beams.Add(Beam2);
            VM_Beam Beam3 = new VM_Beam(NodeC, NodeD, 11);
            Beams.Add(Beam3);
            VM_Beam Beam4 = new VM_Beam(NodeD, NodeE, 12);
            Beams.Add(Beam4);
            VM_Beam Beam5 = new VM_Beam(NodeE, NodeF,13);
            Beams.Add(Beam5);
            VM_Beam Beam6 = new VM_Beam(NodeF, NodeG, 14);
            Beams.Add(Beam6);
            VM_Beam Beam7 = new VM_Beam(NodeG, NodeH, 15);
            Beams.Add(Beam7);
            VM_Beam Beam8 = new VM_Beam(NodeH, NodeI, 16);
            Beams.Add(Beam8);
        }


        /// <summary>
        /// Routine that is called once on application started
        /// </summary>
        private void OnUserCreate()
        {
            Nodes = new List<IDrawingObjects>();
            Beams = new List<IDrawingObjects>();
            Loads = new List<IDrawingObjects>();

            //// Several Test Scenario shortcuts
            //TestMultispanContinuousComplexCase();
            TestCantileverRightCase();
            //TestCantileverLeftCase();

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
            CriticalPoints = CreateVMNodeCriticalPointsList();

            // Sort lists in order of lowest to highest X-coord
            MathHelpers.BubbleSortNodesByXCoord(ref l_Nodes);
            MathHelpers.BubbleSortBeamsByXCoord(ref l_Beams);
            MathHelpers.BubbleSortNodesByXCoord(ref l_CriticalPoints);

            // TODO:: Sort the loads from left to right based on D1 position and Start node of beam position....


            // Create our Structure Stiffness Model from our beams
            Model = new StructureStiffnessModel(Nodes.Count*3, Nodes.Count*3);

            foreach (var beam in Beams)
            {
                Model.AddElement((VM_Beam)beam);
                // TODO::  Add matrix transformations.
            }

            // Add a nodal load vector to test
            //Model.LoadVector[13, 0] = 10; // Positive load is downwards?

            int testw1 = 10; // start intensity of the point load
            int testw2 = 10; // end intensity of the point load
            int val = 200; // Position of the load
            VM_Beam testbeam = GetBeamByXCoord(val);
            double testd1 = GetDistanceFromStartNode(val - testbeam.Start.X, testbeam);
            double testd2 = testd1;

            //// Start node nodal moment equivalent
            //double Ma_w1 = (testw1 * testd1 * testd2 * testd2 / (testbeam.Length * testbeam.Length));
            //double Ma_w2 = (Ma_w1);
            //VM_BaseLoad Ma = new VM_PointMoment(testbeam, 0, 0, Ma_w1, Ma_w2, ArrowDirections.ARROW_CLOCKWISE);
            //Loads.Add(Ma);
            //Model.LoadVector[testbeam.Start.DOF_ROT, 0] = Ma_w1;

            //Start node nodal moment equivalent
            double Mb_w1 = (testw1 * testd1 * testd1 * testd2 / (testbeam.Length * testbeam.Length));
            double Mb_w2 = (Mb_w1);
            VM_BaseLoad Mb = new VM_PointMoment(testbeam, testbeam.Length, testbeam.Length, Mb_w1, Mb_w2, ArrowDirections.ARROW_COUNTERCLOCKWISE);
            Loads.Add(Mb);
            Model.LoadVector[testbeam.End.DOF_ROT, 0] = -Mb_w1;

            //// Start node nodal force equivalent
            //double Pa_w1 = (testw1 * testd2 / (testbeam.Length));
            //double Pa_w2 = (Pa_w1);
            //VM_BaseLoad Pa = new VM_PointForce(testbeam, 0, 0, -Pa_w1, -Pa_w2);
            //Loads.Add(Pa);
            //Model.LoadVector[testbeam.Start.DOF_Y, 0] = Pa_w1;


            //// End node nodal moment equivalent
            //double Pb_w1 = (testw1 * testd1 / (testbeam.Length));
            //double Pb_w2 = (Pb_w1);
            //VM_BaseLoad Pb = new VM_PointForce(testbeam, testbeam.Length, testbeam.Length, -Pb_w1, -Pb_w2);
            //Loads.Add(Pb);
            //Model.LoadVector[testbeam.End.DOF_Y, 0] = Pb_w1;

            double testindex = testbeam.Index;


            Console.WriteLine("Load vector: " + MatrixOperations.DisplayNullable(Model.LoadVector));


            // Solve for the results of the model.
            Model.Solve();

            Console.WriteLine("Results: \n");
            Console.WriteLine("Free Displacements: \n" + MatrixOperations.Display(Model.Disp_Free) + "\n");
            Console.WriteLine("Support Reactions: \n" + MatrixOperations.Display(Model.Load_Fixed) + "\n");


            // *******************************
            // Drawing the deflected shape
            // *******************************
            // Draw the undeformed line...
            double vertOffset = 150;

            // Choose a suitable scaling factor by examining the x and y displacement vectors
            double maxDisp = 0;
            for (int i = 0; i < Model.Rows; i++)
            {
                // skip the rotational dof
                if (i % 3 == 2)
                    continue;

                // scale based on the X and Y disp
                if (Math.Abs((double)Model.DisplacementVector[i, 0]) > maxDisp)
                {
                    maxDisp = Math.Abs((double)Model.DisplacementVector[i, 0]);
                } 
            }

            // Scale displacments so that max displacement is 75 pixels
            double dispScale = 75 / maxDisp;

            DrawingHelpers.DrawLine(MainCanvas, LeftMostNode.X, LeftMostNode.Y + vertOffset, RightMostNode.X, RightMostNode.Y + vertOffset, Brushes.Purple, 1);

            foreach(IDrawingObjects node in Nodes)
            {
                
                // Find the displacements at the node we are searching for
                for (int i = 0; i < Model.DOF_Indices.GetLength(0); i++)
                {
                    // TODO:: Find the x displacement and update the deflected shape accordingly

                    // Find the y displacement
                    int index = (int)((VM_Node)node).DOF_IndexVector[1];
                    double test = Model.DOF_Indices[i, 0];

                    if (index == test)
                    {
                        // retrieve the nodal displacements
                        double dispY = (double)Model.DisplacementVector[i, 0] * dispScale;

                        // Draw deflected shape
                        DrawingHelpers.DrawCircle(MainCanvas, ((VM_Node)node).X, ((VM_Node)node).Y + vertOffset + dispY, Brushes.Purple, Brushes.Purple, 5, 1);
                        continue;
                    }
                }
            }



            // Draw deflected shape
            // 1. Start at leftmost...
            // 2. Draw displacement offset



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
            VM_Node newNode = new VM_Node(p.X, p.Y, false, false, false,GetNextNodeNumber());  

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
        /// Returns the distance of an offset from the END node of a beam
        /// 
        /// </summary>
        /// <param name="offset">Offset distance from the start node</param>
        /// <param name="beam">The beam object under consideration</param>
        /// <returns></returns>
        protected double GetDistanceFromStartNode(double offset, VM_Beam beam)
        {
            return offset;
        }

        /// <summary>
        /// Returns the distance of an offset from the END node of a beam
        /// 
        /// </summary>
        /// <param name="offset">Offset distance from the start node</param>
        /// <param name="beam">The beam object under consideration</param>
        /// <returns></returns>
        protected double GetDistanceFromBeamEndNode(double offset, VM_Beam beam)
        {
            return beam.End.X - beam.Start.X - offset;
        }

        /// <summary>
        /// Returns the beam that contains a specified x-coordinate.
        /// If the x-coord is at the Rightmost node, return the
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

        // Scans the node list for the next available node number
        public int GetNextNodeNumber()
        {
            int count = 0;
            bool found = true;

            while (true)
            {
                found = false;
                foreach (VM_Node node in l_Nodes)
                {
                    if (count == node.Index)
                        found = true;
                }

                if (found)
                    count++;
            }

            return count;

        }
    }

}
