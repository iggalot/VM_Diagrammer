using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
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

            OnUserCreate();
            OnUserUpdate();
        }

        /// <summary>
        /// Routine that is called once per cycle (frame) to update the visuals for the model
        /// </summary>
        private void OnUserUpdate()
        {
            foreach (IDrawingObjects item in Nodes)
            {
                item.Draw(MainCanvas);
            }
        }

        /// <summary>
        /// Routine that is called once on application started
        /// </summary>
        private void OnUserCreate()
        {
            Nodes = new List<IDrawingObjects>();
            Beams = new List<IDrawingObjects>();
            Loads = new List<IDrawingObjects>();

            for (int i=0; i < 10; i++)
            {
                VM_Node node = new VM_Node(i * 100, i * 50);
                Nodes.Add(node);

                if (i > 0)
                {
                    VM_Beam beam = new VM_Beam((VM_Node) Nodes[i - 1], (VM_Node) Nodes[i]);
                    Beams.Add(beam);
                }
            }

            VMBaseLoad load1 = new VM_PointLoad((VM_Beam)Beams[0], LoadTypes.LOADTYPE_CONC_FORCE, 10, 20, 5, 5);
            Loads.Add(load1);


            // Creatre test support conditions
            VM_Node temp = (VM_Node)Nodes[0];
            temp.SupportType = SupportTypes.SUPPORT_PIN;

            VM_Node temp2 = (VM_Node)Nodes[1];
            VM_Node temp3 = (VM_Node)Nodes[2];
            temp2.SupportType = SupportTypes.SUPPORT_ROLLER_X;
            temp3.SupportType = SupportTypes.SUPPORT_FIXED_HOR;

            // Draw the nodes
            foreach (VM_Node item in Nodes)
                item.Draw(MainCanvas);

            // Draw the beams
            foreach (VM_Beam item in Beams)
                item.Draw(MainCanvas);

            // Draw the loads
            foreach (VMBaseLoad item in Loads)
            {
                if(item.LoadType == LoadTypes.LOADTYPE_CONC_FORCE)
                    ((VM_PointLoad)item).Draw(MainCanvas);
            }

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
