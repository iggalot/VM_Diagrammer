using System.Collections.Generic;
using System.Windows;
using VMDiagrammer.Interfaces;
using VMDiagrammer.Models;

namespace VMDiagrammer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List containing all of our objects to be drawn
        public List<IDrawingObjects> Nodes{ get; set; }
        public List<IDrawingObjects> Beams { get; set; }

        public MainWindow()
        {
            InitializeComponent();

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

            foreach (VM_Node item in Nodes)
                item.Draw(MainCanvas);

            foreach (VM_Beam item in Beams)
                item.Draw(MainCanvas);
        }
    }
}
