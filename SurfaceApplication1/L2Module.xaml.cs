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
    /// Interaction logic for L2Module.xaml
    /// </summary>
    public partial class L2Module : StackPanel
    {
        public static SurfaceWindow1 sw1;
        public static PrimerDesigner2 pd2;

        private UIElement hitResult;
        private UIElement dragObject;
        private int dragObjectIndex;
        private UIElement dropObject;
        private int dropObjectIndex;

        public L2Module()
        {
            InitializeComponent();
            AllowDrop = true;
            foreach (UIElement l in this.Children)
            {
                if (l.GetType() == typeof(L1Module)) ((L1Module)l).IsManipulationEnabled = false;
            }
            Console.WriteLine("Width upon initialization is " + ActualWidth);
        }

        private void L2M_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            try
            {
                //Find the index of the touched object to drag

                StackPanel L2M = (StackPanel)sender;
                VisualTreeHelper.HitTest(L2M, null, new HitTestResultCallback(resultCallback), new PointHitTestParameters(e.GetTouchPoint(L2M).Position));
                dragObject = hitResult;
                dragObjectIndex = this.Children.IndexOf(dragObject);
                //Console.WriteLine("The drag object is" + dragObject);
            }
            catch (Exception exc) { Console.WriteLine("L2M ptouchdown \n" + exc); }
        }

        private void L2M_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            try
            {
                VisualTreeHelper.HitTest(L2M, null, new HitTestResultCallback(resultCallback), new PointHitTestParameters(e.GetTouchPoint(L2M).Position));
                dropObject = hitResult;
                dropObjectIndex = this.Children.IndexOf(dropObject);
                //Console.WriteLine("The drop object is " + dropObject);
                //Console.WriteLine("The starting index is " + this.Children.IndexOf(dragObject));
                //Console.WriteLine("The final index should be " + this.Children.IndexOf(dropObject));

                StackPanel parent = (StackPanel)this.Parent;
                if (parent.Name == "L2_manTab" && dragObject != null)
                {
                    this.Children.Remove(dragObject);
                    this.Children.Insert(dropObjectIndex, dragObject);
                    //Console.WriteLine("The final index is " + this.Children.IndexOf(dragObject));
                }
            }
            catch (Exception exc) { Console.WriteLine("L2M ptouchup \n" + exc); }
        }


        public HitTestResultBehavior resultCallback(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(Grid))
            {
                Grid target = (Grid)result.VisualHit;
                if (target.Name == "L1Grid")
                {
                    //Console.WriteLine("Found an L1Module.");
                    L1Module parent = (L1Module)target.Parent;
                    hitResult = parent;
                    return HitTestResultBehavior.Stop;
                }
                else
                {
                    //Console.WriteLine("Didn't get the L1Module in the StackPanel.");
                    return HitTestResultBehavior.Continue;
                }
            }
            else
            {
                //Console.WriteLine("Didn't detect a Grid.");
                return HitTestResultBehavior.Continue;
            }
        }

        private void select_Sequence(object sender, RoutedEventArgs e)
        {
            String s = "";
            foreach (UIElement l1 in this.Children)
            {
                if (l1.GetType() == typeof(L1Module))
                {
                    foreach (UIElement p in ((L1Module)l1).L1Grid.Children)
                    {
                        if (p.GetType() == typeof(Part)) s = s + ((Part)p).myRegDS.BasicInfo.Sequence + "\n";
                    }
                }
            }
            TextBlock sequence = new TextBlock();
            sequence.Text = s;
            ScatterViewItem svi = new ScatterViewItem();
            svi.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(seq_ContainerManipulationCompleted);
            svi.Content = sequence;
            SurfaceWindow1.addData(sender, svi);
        }

        void seq_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            sw1.swipeToDelete((ScatterViewItem)sender);
        }

        private void select_PrimerDesigner(object sender, RoutedEventArgs e)
        {
            //Iterate through parts and get data for each before entering Primer Designer
            foreach (UIElement elem in L2M.Children)
            {
                if (elem.GetType() == typeof(L1Module))
                {
                    L1Module l1 = (L1Module)elem;
                    foreach (UIElement p in l1.L1Grid.Children)
                    {
                        if (p.GetType() == typeof(Part)) ((Part)p).getDataForPrimerDesigner();
                    }
                }
            }
            L2Module copy = clone();
            copy.PD.IsEnabled = false;
            copy.PD.Visibility = Visibility.Collapsed;
            sw1.SW_SV.Items.Add(new PrimerDesigner2(copy));
        }

        private L2Module clone()
        {
            L2Module copy = new L2Module();
            foreach (UIElement l1 in this.Children)
            {
                if (l1.GetType() == typeof(L1Module)) copy.Children.Add(((L1Module)l1).clone());
            }
            return copy;
        }

    }
}
