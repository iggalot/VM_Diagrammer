using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using VMDiagrammer.Helpers;
using VMDiagrammer.Interfaces;
using VMDiagrammer.Models;

namespace VMDiagrammer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Point m_CurrentMouseLocation = new Point();
        private string m_MouseLocationString = "N/A";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // List containing all of our objects to be drawn
        public List<IDrawingObjects> Nodes{ get; set; }
        public List<IDrawingObjects> Beams { get; set; }

        public List<IDrawingObjects> Loads { get; set; }


        // A list of the critical points for drawing vertical lines.
        public List<VM_Node> CriticalPoints { get; set; } = null;


        public VM_Node LeftMostNode { get; set; } = null; // leftmost node on the structure
        public VM_Node RightMostNode { get; set; } = null; // leftmost node on the structure

        protected VM_Node CreateNode(double x, double y, SupportTypes s = SupportTypes.SUPPORT_UNDEFINED)
        {
            VM_Node newNode = new VM_Node(x, y, s);
            return newNode;
        }

        /// <summary>
        /// Adds a node to the Nodes list and updates the reference
        /// </summary>
        /// <param name="node"></param>
        protected void AddNode(VM_Node node)
        {
            Nodes.Add(node);
            UpdateBeamExtents();
        }

        /// <summary>
        /// Finds the refernce node (left-most and bottom most)
        /// </summary>
        protected void UpdateBeamExtents()
        {
            // TODO::: Sort the Nodes list based on X position using IComparator

            // In the meantime, brute force
            // if there aren't current nodes in the list, exit
            if (Nodes.Count == 0)
                return;

            VM_Node temp = (VM_Node)Nodes[0];

            // First check the leftmost
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

            if(Nodes.Count > 1)
            {
                temp = (VM_Node)Nodes[Nodes.Count - 1];
                // Then check for the rightmost.
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

            // Update our Critical Points List
            CriticalPoints = ListCriticalPoints();
        }

        /// <summary>
        /// Determine the critical points for our diagram
        /// 1. Support Reactions
        /// 2. Start and end of beam
        /// 3. Start and end of distributed loads
        /// 4. Concentrated loads
        /// </summary>
        /// <returns></returns>
        protected List<VM_Node> ListCriticalPoints()
        {
            List<VM_Node> list = new List<VM_Node>();

            // Support reactions
            foreach (VM_Node item in Nodes)
            {
                if(item.SupportType != SupportTypes.SUPPORT_UNDEFINED)
                {
                    list.Add(item);
                }
            }

            // Start and End of beam
            if (!list.Contains(LeftMostNode))
                list.Add(LeftMostNode);
            if (!list.Contains(RightMostNode))
                list.Add(RightMostNode);

            // For Loads
            foreach (VMBaseLoad item in Loads)
            {
                // Concentrated forces or moments
                if (item.LoadType == LoadTypes.LOADTYPE_CONC_FORCE || item.LoadType == LoadTypes.LOADTYPE_CONC_MOMENT)
                {
                    // Are the start and end points the same -- they should be if its a concentrated effect
                    // Make a node based on location of load
                    if (item.D1 == item.D2)
                    {
                        // Is the load at the start node or the end node?
                        // It's the start node
                        if (item.D1 == 0)
                        {
                            if (!list.Contains(item.Beam.Start))
                                list.Add(item.Beam.Start);


                        }
                        // It's the end node
                        else if (item.D2 == Math.Abs(item.Beam.End.X - item.Beam.Start.X))
                        {
                            if (!list.Contains(item.Beam.End))
                                list.Add(item.Beam.End);


                        }
                        // It's in the middle somewhere so make a temp node and add it to the list. 
                        // Check that the node isnt already in the Nodes list. (It shouldnt be).
                        else
                        {
                            double temp_x = item.Beam.Start.X + item.D1;
                            double temp_y = item.Beam.Start.Y;
                            bool node_found = false;
                            foreach (VM_Node n in Nodes)
                            {
                                if (n.X == temp_x)
                                {
                                    // A node at this point is already in the list
                                    if (n.Y == temp_y)
                                    {
                                        node_found = true;
                                        break;
                                    }
                                }
                            }
                            if (!node_found)
                            {
                                list.Add(new VM_Node(temp_x, temp_y));

                            }
                        }
                    }
                }
                // Find the nodes for distributed forces and distributed moments
                else if (item.LoadType == LoadTypes.LOADTYPE_DIST_FORCE || item.LoadType == LoadTypes.LOADTYPE_DIST_MOMENT)
                {
                    if (item.D2 > item.D1)
                    {
                        // Is the start point at the beam's start node 
                        if (item.D1 == 0)
                        {
                            if (!list.Contains(item.Beam.Start))
                                list.Add(item.Beam.Start);
                        }
                        else
                        {
                            double temp_x = item.Beam.Start.X + item.D1;
                            double temp_y = item.Beam.Start.Y;
                            bool node_found = false;
                            foreach (VM_Node n in Nodes)
                            {
                                if (n.X == temp_x)
                                {
                                    // A node at this point is already in the list
                                    if (n.Y == temp_y)
                                    {
                                        node_found = true;
                                        break;
                                    }
                                }
                            }
                            if (!node_found)
                            {
                                list.Add(new VM_Node(temp_x, temp_y));

                            }
                        }
                        // Is the end point at the beam's end node?
                        if (item.D2 == (Math.Abs(item.Beam.End.X - item.Beam.Start.X)))
                        {
                            if (!list.Contains(item.Beam.End))
                                list.Add(item.Beam.End);
                        }
                        else
                        {
                            double temp_x = item.Beam.Start.X + item.D2;
                            double temp_y = item.Beam.Start.Y;
                            bool node_found = false;
                            foreach (VM_Node n in Nodes)
                            {
                                if (n.X == temp_x)
                                {
                                    // A node at this point is already in the list
                                    if (n.Y == temp_y)
                                    {
                                        node_found = true;
                                        break;
                                    }
                                }
                            }
                            if (!node_found)
                            {
                                list.Add(new VM_Node(temp_x, temp_y));

                            }
                        }
                    }

                }
            }

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

            OnUserUpdate();  // called to create the initial drawing and then when a redraw is needed (once per frame?)
        }

        /// <summary>
        /// Routine that is called once per cycle (frame) to update the visuals for the model
        /// </summary>
        private void OnUserUpdate()
        {
            // Draw the nodes
            foreach (VM_Node item in Nodes)
                item.Draw(MainCanvas);

            // Draw the beams
            foreach (VM_Beam item in Beams)
                item.Draw(MainCanvas);

            // Draw the loads
            foreach (VMBaseLoad item in Loads)
            {
                switch (item.LoadType)
                {
                    case LoadTypes.LOADTYPE_CONC_FORCE:
                        ((VM_PointForce)item).Draw(MainCanvas);
                        break;
                    case LoadTypes.LOADTYPE_DIST_FORCE:
                        ((VM_DistributedForce)item).Draw(MainCanvas);
                        break;
                    case LoadTypes.LOADTYPE_UNDEFINED:
                    case LoadTypes.LOADTYPE_DIST_MOMENT:
                    case LoadTypes.LOADTYPE_CONC_MOMENT:
                    default:
                        throw new NotImplementedException("In drawing loads of MainWindow.xaml.cs -- load type detected for which there is now DRAW defined");
                }
            }


            // Draw the lines for the critical points
            foreach (VM_Node item in CriticalPoints)
            {
                DrawingHelpers.DrawLine(MainCanvas, item.X, item.Y+50, item.X, item.Y + 600, Brushes.Blue);
            }

            // Draw a reference line just below the nodes(temp)
            DrawingHelpers.DrawLine(MainCanvas, LeftMostNode.X, LeftMostNode.Y + 10, RightMostNode.X, RightMostNode.Y + 10, Brushes.Blue);


        }

        /// <summary>
        /// Routine that is called once on application started
        /// </summary>
        private void OnUserCreate()
        {
            Nodes = new List<IDrawingObjects>();
            Beams = new List<IDrawingObjects>();
            Loads = new List<IDrawingObjects>();



            VM_Node NodeB = CreateNode(420, 100, SupportTypes.SUPPORT_ROLLER_X);
            AddNode(NodeB);
            VM_Node NodeC = CreateNode(220, 100);
            AddNode(NodeC);
            VM_Node NodeA = CreateNode(20, 100, SupportTypes.SUPPORT_PIN);
            AddNode(NodeA);

            VM_Beam Beam1 = new VM_Beam(NodeC, NodeA);
            Beams.Add(Beam1);
            VM_Beam Beam2 = new VM_Beam(NodeC, NodeB);
            Beams.Add(Beam2);

            // Add point load to second beam
            VMBaseLoad load1 = new VM_PointForce((VM_Beam)Beams[1], LoadTypes.LOADTYPE_CONC_FORCE, 100, 100, +5, +5);
            Loads.Add(load1);

            // Add a distributed load to first beam
            VMBaseLoad load2 = new VM_DistributedForce((VM_Beam)Beams[0], LoadTypes.LOADTYPE_DIST_FORCE, 70, 150, +35, +15);
            Loads.Add(load2);


            // Update our list of critical points for the graph.
            CriticalPoints = ListCriticalPoints();

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
            VM_Node newNode = new VM_Node(p.X, p.Y);  

            // Add it to the model
            Nodes.Add(newNode);

            OnUserUpdate();
        }
    }
}
