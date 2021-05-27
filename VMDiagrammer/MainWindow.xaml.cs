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


        public VM_Node LeftMostNode { get; set; } = null; // leftmost node on the structure
        public VM_Node RightMostNode { get; set; } = null; // leftmost node on the structure

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
            // Draw the nodes
            foreach (VM_Node item in Nodes)
                item.Draw(MainCanvas);

            // Draw the beams
            foreach (VM_Beam item in Beams)
                item.Draw(MainCanvas);

            // Draw the loads
            foreach (VMBaseLoad item in Loads)
            {
                switch(item.LoadType)
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
                        throw new NotImplementedException("In drawing loads of MainWindow.xaml.cs -- load type detected for which there is no DRAW implemented!");
                }
            }


            // Draw a reference line just below the nodes (temp)
            DrawingHelpers.DrawLine(MainCanvas, LeftMostNode.X, LeftMostNode.Y + 25, RightMostNode.X, RightMostNode.Y + 25, Brushes.Blue);


        }

        /// <summary>
        /// Routine that is called once on application started
        /// </summary>
        private void OnUserCreate()
        {
            Nodes = new List<IDrawingObjects>();
            Beams = new List<IDrawingObjects>();
            Loads = new List<IDrawingObjects>();



            VM_Node NodeB = new VM_Node(520, 200, SupportTypes.SUPPORT_ROLLER_X);
            AddNode(NodeB);
            VM_Node NodeC = new VM_Node(320, 200);
            AddNode(NodeC);
            VM_Node NodeA = new VM_Node(120, 200, SupportTypes.SUPPORT_PIN);
            AddNode(NodeA);

            VM_Beam Beam1 = new VM_Beam(NodeA, NodeC);
            Beams.Add(Beam1);
            VM_Beam Beam2 = new VM_Beam(NodeC, NodeB);
            Beams.Add(Beam2);

            // Add point load
            VMBaseLoad load1 = new VM_PointForce((VM_Beam)Beams[1], 100, 100, -5, -5);
            Loads.Add(load1);

         

            // Add distributed load
            VMBaseLoad load2 = new VM_DistributedForce((VM_Beam)Beams[0],  50, 150, -50, -80);
            Loads.Add(load2);
            // Add distributed load
            VMBaseLoad load4 = new VM_DistributedForce((VM_Beam)Beams[0],  150, 200, -80, 0);
            Loads.Add(load4);

            // Add distributed load
            VMBaseLoad load3 = new VM_DistributedForce((VM_Beam)Beams[1],  100, 120, +30, +100);
            Loads.Add(load3);
            // Add distributed load
            VMBaseLoad load5 = new VM_DistributedForce((VM_Beam)Beams[1],  120, 200, +100, +100);
            Loads.Add(load5);

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
