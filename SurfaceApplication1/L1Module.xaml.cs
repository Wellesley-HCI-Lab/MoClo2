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

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for L1Module.xaml
    /// </summary>
    public partial class L1Module : ScatterViewItem
    {
        public static SurfaceWindow1 sw1; //Reference to SurfaceWindow1
        public static PrimerDesigner2 pd2;
        private Point Original; //Stores position at ContainerManipulatedStarted
        private L1Module myClone; //Reference to its clone for ContainerManipulationCompleted
        private L2Module targetL2Module;

        #region Properties

        public L1Module MyClone
        {
            get { return myClone; }
            set { myClone = value; }
        }

        #endregion

        /// <summary>
        /// Empty placeholder part
        /// </summary>
        public L1Module()
        {
            InitializeComponent();
            L1Prom.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_prom);
            L1RBS.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_rbs);
            L1CDS.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_cds);
            L1Term.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_term);
            foreach (UIElement p in L1Grid.Children)
            {
                if (p.GetType() == typeof(Part))
                {
                    ((Part)p).Opacity = 0.5;
                    //p.IsEnabled = false;
                    ((Part)p).ShowsActivationEffects = false;
                    ((Part)p).IsTopmostOnActivation = false;
                    ((Part)p).ElementMenu.IsEnabled = false;
                    ((Part)p).ElementMenu.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Populated part: used in L1 & L2
        /// NOT ACTUALLY USED. Copying over of Part information displays properly 
        /// only when copyPartInfoFram is called during the adding of permuted L1Modules to the permTabs.
        /// </summary>
        public L1Module(Part p, Part r, Part c, Part t)
        {
            InitializeComponent();
            L1Prom.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_prom);
            L1RBS.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_rbs);
            L1CDS.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_cds);
            L1Term.imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_term);

            L1Prom = p;
            L1Prom.partName.Text = p.myRegDS.Name;
            L1Prom.partCategory.Text = p.partCategory.Text;
            //L1Prom.copyPartInfoFrom(p);
            L1RBS = r;
            L1RBS.partName.Text = r.myRegDS.Name;
            //L1RBS.copyPartInfoFrom(r);
            L1CDS = c;
            //L1CDS.partName.Text = c.myRegDS.Name;
            //L1RBS.copyPartInfoFrom(c);
            L1Term = t;
            //L1Term.partName.Text = t.myRegDS.Name;
            //L1Term.copyPartInfoFrom(t);
        }

        #region Element menu method stubs
        private void select_Sequence(object sender, RoutedEventArgs e)
        {
            String s = "";
            foreach (UIElement p in L1Grid.Children)
            {
                if (p.GetType() == typeof(Part)) s = s + ((Part)p).myRegDS.BasicInfo.Sequence + "\n";
            }
            TextBlock sequence = new TextBlock();
            sequence.Text = s;
            ScatterViewItem svi = new ScatterViewItem();
            svi.Content = sequence;
            svi.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(seq_ContainerManipulationCompleted);
            sw1.L1.L1_SV.Items.Add(svi);
        }

        void seq_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            sw1.swipeToDelete((ScatterViewItem)sender);
        }

        private void select_PrimerDesigner(object sender, RoutedEventArgs e)
        {
            //Iterate through parts and get data for each before entering Primer Designer
            foreach (UIElement p in L1Grid.Children)
            {
                if (p.GetType() == typeof(Part)) ((Part)p).getDataForPrimerDesigner();
            }
            L1Module copy = clone();
            copy.PD.IsEnabled = false;
            copy.PD.Visibility = Visibility.Collapsed;
            sw1.SW_SV.Items.Add(new PrimerDesigner2(copy));
        }

        #endregion

        #region DRAG AND DROP L1MODULES INTO AND WITHIN L2

        //Saves starting point of the manipulation
        private void L1Module_ContainerManipulationStarted(object sender, ContainerManipulationStartedEventArgs e)
        {
            Original = this.Center;
        }

        //Differentiates ElementMenu interaction from drag/drop interaction using minimum distance
        //Once minimum distance met, create copy to leave in original spot
        private void L1Module_ContainerManipulationDelta(object sender, ContainerManipulationDeltaEventArgs e)
        {
            try
            {
                Point current = this.Center;
                if (Math.Abs(Original.Y - current.Y) > 10 || Math.Abs(Original.X - current.X) > 10)
                {
                    L1Module l = sender as L1Module;
                    myClone = l.clone();
                    myClone.Center = Original;
                    myClone.BorderBrush = l.BorderBrush;

                    ScatterView parent = l.Parent as ScatterView;
                    parent.Items.Add(myClone);

                    if ((parent.Name == "L1_permTab") || (parent.Name == "L1_manTab"))
                    { //Check if in Level1 and give clone L1-appropriate behavior
                        myClone.Opacity = 0.5;
                        myClone.IsManipulationEnabled = false;
                    }
                    else
                    { //Moving L1Module within L2
                        l.changeParents_SV(parent, sw1.L2.L2_SV);
                    }

                    //Prevent it from continuously creating copies
                    l.ContainerManipulationDelta -= L1Module_ContainerManipulationDelta;
                }
            }
            catch (Exception exc) { Console.WriteLine("L1M Delta \n" + exc); }
        }

        //Determines user intent by checking location of drop against threshold and whether a clone exists (i.e. drag intent)
        private void L1Module_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            L1Module l = sender as L1Module;
            ScatterView parent = l.Parent as ScatterView;

            try
            {
                if (!(myClone == null))
                {
                    if ((parent.Name == "L1_permTab") || (parent.Name == "L1_manTab"))
                    {
                        //Drop L1Module into L2 or delete
                        l.L1ModuleInL1();
                    }
                    else
                    {
                        //Drop L1Module into L2Module or delete
                        l.L1ModuleInL2();
                    }

                    parent.Items.Remove(l);
                }
                else
                {
                    if (parent.Name == "L2_L1ModulesSV")
                    { //If in L2
                        if (l.BorderBrush != Brushes.White)
                        {
                            //Restore white border
                            l.BorderBrush = Brushes.White;
                        }
                        else
                        {
                            l.BorderBrush = Brushes.Navy;
                        } //highlights border 
                    }
                }
            }
            catch (Exception exc) { Console.WriteLine("L1M Completd \n" + exc); }
        }
        
        //L1 behavior handler: drops L1Modules into L2
        //Checks center against threshold value, below which drop occurs
        //Currently handles Permutations functions of Level 2 (consider moving to L1ModuleInL2())
        private void L1ModuleInL1()
        {
            ScatterView parent = Parent as ScatterView;
            //Places item in L2 if user wants to drop it in
            double yL1 = sw1.L1.Center.Y;
            double yL2 = sw1.L2.Center.Y;
            double yThreshold = yL2 - yL1 - 100; //For a 50 margin and 50 more because the center is relative to L1_SV

            //Check if user is dropping or dumping
            Point transformedCenter = SurfaceWindow1.transformCoords(this, sw1.L1.L1_SV);
            if (transformedCenter.Y > yThreshold) //0 is the top relative to L1_xTabs; adjust accordingly. Need to fix this to a relative height. 
            {
                L1Module cloneToL2 = clone();
                sw1.L2.L2_L1ModulesSV.Items.Add(cloneToL2);
                cloneToL2.Center = SurfaceWindow1.SetPosition(cloneToL2);

            }
            else
            { //Dumped; restore function to clone
                myClone.IsManipulationEnabled = true;
                myClone.Opacity = 1;
            }
        }

        //L2 behavior handler: drops L1Modules into L2Modules
        //Detects L2Modules via hittesting, using the center of the L1module as the initial value
        private void L1ModuleInL2()
        {
            Point pt = SurfaceWindow1.transformCoords(this, sw1.L2.L2_manTab);
            VisualTreeHelper.HitTest(sw1.L2.L2_manTab, null, new HitTestResultCallback(TargetL2MCallback), new PointHitTestParameters(pt));

            if (targetL2Module != null)
            { //Dropped
                L1Module manTabClone = clone();
                manTabClone.IsManipulationEnabled = false;
                manTabClone.targetL2Module = targetL2Module;
                manTabClone.Background = sw1.L2.L1MColors.ElementAt(targetL2Module.Children.Count - 1);
                manTabClone.BorderBrush = manTabClone.Background;
                //If targetL2M only contains an element menu, add margin to left
                //This leaves space so L2M can be interacted with and element menu can be accessed; this isn't the way to do it. When switching order, margin moves too.
                //if (targetL2Module.Children.Count == 1) manTabClone.Margin = new Thickness(30,0,0,0);
                targetL2Module.Children.Add(manTabClone);
            }
            else
            { //Dumped; check if delete from palette
                //Console.WriteLine("Missed!");
                sw1.swipeToDelete(this);
            }
        }

        //public Point SetPosition()
        //{
        //    Point newCenter;
        //    ScatterView parentSV = (ScatterView)this.Parent;
        //    double ParentWidth = parentSV.Width;
        //    int NumberParts = (int)Math.Floor((ParentWidth / (this.Width + 10)));
        //    double startX = Width / 2 + 10;
        //    double startY = (this.Height + 10) / 2;
        //    int count = parentSV.Items.Count;
        //    int multiplierX = (count - 1) % NumberParts;
        //    int multiplierY = (count - 1) / NumberParts;
        //    newCenter = new Point(startX + multiplierX * (Width + 10), startY + multiplierY * (Height + 10));


        //    return newCenter;
        //}
        
        #endregion

        #region DRAG AND DROP HELPERS

        //Clones the part and copies content values and type
        //Used in both Part_ContainerManipulationDelta and _...Completed
        public L1Module clone()
        {
            L1Module copy = new L1Module();
            copy.L1Prom.copyPartInfoFrom(this.L1Prom);
            copy.L1RBS.copyPartInfoFrom(this.L1RBS);
            copy.L1CDS.copyPartInfoFrom(this.L1CDS);
            copy.L1Term.copyPartInfoFrom(this.L1Term);
            return copy;
        }

        //Removes from current parent and adds to destination parent at same location
        private void changeParents_SV(ScatterView parentSV, ScatterView destination)
        {
            //Point newPoint = this.transformCoords(destination);
            Point newPoint = SurfaceWindow1.transformCoords(this, destination);
            parentSV.Items.Remove(this);
            destination.Items.Add(this);
            this.Center = newPoint;

        }

        //Assigns first L2Module found to global variable targetL2Module and stops HitTest
        public HitTestResultBehavior TargetL2MCallback(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(L2Module))
            {
                targetL2Module = result.VisualHit as L2Module;
                return HitTestResultBehavior.Stop;
            }
            else
            {
               // Console.WriteLine("Didn't detect a grid.");
                return HitTestResultBehavior.Continue;
            }
        }

        //Returns given point relative to a destination element
        private Point transformCoords(Visual Destination)
        {
            Point spawnPoint;
            try
            {
                GeneralTransform spawnTransform = this.TransformToVisual(Destination);
                spawnPoint = spawnTransform.Transform(new Point());
                //Adjust point - maybe TransformToVisual gets top-left corner? It's shifted up and left relative to the contact point.
                spawnPoint.X = spawnPoint.X + this.Width / 2;
                spawnPoint.Y = spawnPoint.Y + this.Height / 2;
            }
            catch
            {
                Console.WriteLine("You need to handle dropping into the Permutations tab.");
                spawnPoint = new Point(200, 200);
            }
            return spawnPoint;
        }
#endregion

        private void select_cusTUButton(object sender, RoutedEventArgs e)
        {
            sw1.cF.cusForm.Visibility = Visibility.Visible;
            sw1.cF.cus_Type.Visibility = Visibility.Collapsed;
            customForm.modlevel = 1;
            customForm.module = this;

            //Get sequence
            String s = "";
            foreach (UIElement p in L1Grid.Children)
            {
                if (p.GetType() == typeof(Part)) s = s + ((Part)p).myRegDS.BasicInfo.Sequence;
            }
            sw1.cF.cus_Seq.Text = s;
            sw1.cF.cus_Seq.IsReadOnly = true;
        }

  



    }
}
