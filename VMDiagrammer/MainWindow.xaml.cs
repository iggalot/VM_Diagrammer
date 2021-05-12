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


        public VM_Node ReferenceNode { get; set; } = null; // leftmost node on the structure

        /// <summary>
        /// Adds a node to the Nodes list and updates the reference
        /// </summary>
        /// <param name="node"></param>
        protected void AddNode(VM_Node node)
        {
            Nodes.Add(node);

            VM_Node temp = (VM_Node)Nodes[0];
            foreach(VM_Node item in Nodes)
            {
                if (item.X < temp.X)
                    temp = item;
                else if (item.X == temp.X)
                {
                    if (item.Y < temp.Y)
                        temp = item;
                }
            }

            ReferenceNode = temp;
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
                if (item.LoadType == LoadTypes.LOADTYPE_CONC_FORCE)
                    ((VM_PointLoad)item).Draw(MainCanvas);
            }


            // Draw a reference line (temp)
            DrawingHelpers.DrawLine(MainCanvas, ReferenceNode.X, ReferenceNode.Y, 300, 300, Brushes.Blue);


        }

        /// <summary>
        /// Routine that is called once on application started
        /// </summary>
        private void OnUserCreate()
        {
            Nodes = new List<IDrawingObjects>();
            Beams = new List<IDrawingObjects>();
            Loads = new List<IDrawingObjects>();



            VM_Node NodeB = new VM_Node(420, 20, SupportTypes.SUPPORT_ROLLER_X);
            AddNode(NodeB);
            VM_Node NodeC = new VM_Node(220, 20);
            AddNode(NodeC);
            VM_Node NodeA = new VM_Node(20, 20, SupportTypes.SUPPORT_PIN);
            AddNode(NodeA);

            VM_Beam Beam1 = new VM_Beam(NodeA, NodeC);
            Beams.Add(Beam1);
            VM_Beam Beam2 = new VM_Beam(NodeC, NodeB);
            Beams.Add(Beam2);

            // Add point load
            VMBaseLoad load1 = new VM_PointLoad((VM_Beam)Beams[1], LoadTypes.LOADTYPE_CONC_FORCE, 10, 20, +5, +5);
            Loads.Add(load1);

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
