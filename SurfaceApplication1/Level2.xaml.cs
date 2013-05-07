using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO; 


namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for Level2.xaml
    /// </summary>
    public partial class Level2 : ScatterViewItem
    {
        private Point low; //Center coordinates when initialized (i.e. snapped to bottom)
        private Point high; //Center coordinates when snapped to top
        private double snapThreshold; //Threshold distance from a snap-to point for snapping behavior
        private double snapThreshold_Level; //Snap threshold WRT L1

        private static Brush[] _l1mColors = new Brush[] { Brushes.PaleGreen, Brushes.PaleVioletRed, Brushes.Wheat, Brushes.Tomato, Brushes.Violet, Brushes.YellowGreen };

        public static SurfaceWindow1 sw1;

        #region Properties
        public Brush[] L1MColors
        {
            get { return _l1mColors; }
        }
        #endregion
        public Level2()
        {
            InitializeComponent();
            double parentHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double parentWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Width = parentWidth;
            this.Height = parentHeight - 20; //Height of all tabs together
            low.Y = parentHeight - 40 + this.Height / 2;
            low.X = parentWidth / 2;

            this.Center = low;
            high = low;
            high.Y = high.Y - this.Height + 100; //50 for height of tabs?
            snapThreshold = 200;
            snapThreshold_Level = 50;

            L2Module l2 = new L2Module();
            L2_manTab.Children.Add(l2);
        }

        private void Level2_ContainerManipulationDelta(object sender, ContainerManipulationDeltaEventArgs e)
        {
            ScatterViewItem L2 = (ScatterViewItem)sender;
            //If its center is ever higher than its highest point, lower than its lowest point, or less than 50 higher than L1's center
            if ((L2.Center.Y < high.Y) || (L2.Center.Y > low.Y) || (L2.Center.Y < (sw1.L1.Center.Y + snapThreshold_Level)))
                L2.CancelManipulation();
            L2.Center = new Point(low.X, L2.Center.Y);
        }

        private void Level2_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            ScatterViewItem L2 = (ScatterViewItem)sender;
            //If its center is within the threshold of low.Y or high.Y or L1
            if (L2.Center.Y < high.Y + snapThreshold)
                L2.Center = new Point(low.X, high.Y);
            if (L2.Center.Y > low.Y - snapThreshold)
                L2.Center = new Point(low.X, low.Y);
            if (L2.Center.Y < (sw1.L1.Center.Y + snapThreshold_Level))
                L2.Center = new Point(low.X, sw1.L1.Center.Y + snapThreshold_Level);
        }

        //Makes tabcontrol accept touch input
        private void TabControl_TouchDown(object sender, TouchEventArgs e)
        {
            TabItem tab = (TabItem)sender;
            L2_buildTabs.SelectedItem = tab;
            e.Handled = true;
        }

        //Testing out global variable checking with addition of new Parts
        private void partAdder_Click(object sender, RoutedEventArgs e)
        {
            L2Module l2 = new L2Module();
            L2_manTab.Children.Add(l2);
        }
        
        private void L2_buildTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabc = (TabControl)sender;
            //if manual is selected then
            if (tabc.SelectedIndex == 0)
            {
                partAdder.IsEnabled = true;
                partAdder.Visibility = System.Windows.Visibility.Visible;
                permMaker.IsEnabled = false;
                permMaker.Visibility = Visibility.Collapsed;
            }
            else
                //if automatic is selected
            {
                partAdder.IsEnabled = false;
                partAdder.Visibility = System.Windows.Visibility.Hidden;
                permMaker.IsEnabled = true;
                permMaker.Visibility = Visibility.Visible;
            }
        }

        private void permMaker_Click(object sender, RoutedEventArgs e)
        {
            permMaker.IsEnabled = false;

            List<L1Module> selectedL1Modules = new List<L1Module>();
            foreach (L1Module L in L2_L1ModulesSV.Items)
            {
                if (L.BorderBrush != Brushes.White) 
                {
                    selectedL1Modules.Add(L);
                }

            }

            if (selectedL1Modules.Count > 1 && selectedL1Modules.Count < 7)
            {
                EugeneModules em = new EugeneModules();
                sw1.L2.L2_permTab.Children.Clear();

                List<L2Module> listOfL2ModesToAddToPermTab = em.Permute(selectedL1Modules);

                foreach (L2Module L2ModesToAddToPermTab in listOfL2ModesToAddToPermTab)
                {

                    sw1.L2.L2_permTab.Children.Add(L2ModesToAddToPermTab);

                }
                //sw1.L2.L2_permTab
            }
            else
            {
                MessageBox.Show("Please select between 2 and 6 Level 1 modules to permute.");
            }

            permMaker.IsEnabled = true;

        }

        

    }
}
