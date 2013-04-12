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
using System.Collections.ObjectModel;
using Microsoft.Surface.Presentation.Generic;

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for Sites.xaml
    /// </summary>
    public partial class Sites : ScatterViewItem
    {
        public static PrimerDesigner2 pd2;
        private string _label;
        private string _siteName;
        private String _sequence;
        private Sites myClone;
        //private Color _siteColor;

        #region Properties
        public string SiteName
        {
            get { return _siteName; }
            set { _siteName = value; }
        }

        public String Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }

        public String Label
        {
            get { return _label; }
            set { _label = value; }
        }
        //public Color SiteColor
        //{
        //    get { return _siteColor; }
        //    set { _siteColor = value; }
        //}
        #endregion

        //Creates placeholders/blanks
        public Sites()
        {
            InitializeComponent();
            Opacity = 0.5;
            IsManipulationEnabled = false;
            _sequence = "site";
            CircleText.Text = _sequence;
            CircleText.IsReadOnly = true;

            //Remove shadow from control
            this.ApplyTemplate();
            //this.Background = new SolidColorBrush(Colors.Transparent);
            this.ShowsActivationEffects = false;
            this.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = this.Template.FindName("shadow", this) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        public Sites(String seq)
        {
            InitializeComponent();

            Width = 50;
            Height = 50;
            _label = seq;
            _sequence = seq;
            Circle.Background = convertColorFromSeq(_sequence);
            CircleText.Text = _sequence;
            CircleText.IsReadOnly = true;

            if (seq == "site") Opacity = 0.5;

            //Remove shadow from control
            this.ApplyTemplate();
            this.ShowsActivationEffects = false;
            this.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = this.Template.FindName("shadow", this) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        public Sites(String txt, String seq, Brush bg)
        {
            InitializeComponent();
            Width = 50;
            Height = 50;
            _label = txt;
            _sequence = seq;
            Circle.Background = bg;
            CircleText.Text = _sequence;
            CircleText.IsReadOnly = true;

            //Remove shadow from control
            this.ApplyTemplate();
            this.ShowsActivationEffects = false;
            this.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = this.Template.FindName("shadow", this) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }

        private Point originalCenter;
        private void Sites_ContainerManipulationStarted(object sender, ContainerManipulationStartedEventArgs e)
        {
            originalCenter = Center;
        }

        private void Sites_ContainerManipulationDelta(object sender, ContainerManipulationDeltaEventArgs e)
        {
            try
            {
                myClone = clone();
                myClone.Center = originalCenter;

                int i = pd2.FusionSiteLibrary.IndexOf(this);
                Point newPoint = SurfaceWindow1.transformCoords(this, pd2.PD2_SV);
                //pd2.SourceItems.Insert(i, copy);
                //pd2.SourceItems.Remove(this);

                pd2.PD2_siteLibrary.Items.Remove(this);
                pd2.PD2_siteLibrary.Items.Add(myClone);
                pd2.PD2_SV.Items.Add(this);
                this.Center = newPoint;

                this.ContainerManipulationDelta -= Sites_ContainerManipulationDelta;
            }
            catch (Exception exc) { Console.WriteLine(exc); }
        }

        //When manipulation completed, check location for drop and transfer data to placeholder; then delete
        private void Sites_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            try
            {
                Sites s = sender as Sites;
                if (myClone != null)
                {
                    Point pt = SurfaceWindow1.transformCoords(this, pd2.PD2_manual);
                    if (pd2.PD2_buildTabs.SelectedIndex == 0) //If Manual is selected
                    {
                        VisualTreeHelper.HitTest(pd2.PD2_manual, null, new HitTestResultCallback(sitesCallback), new PointHitTestParameters(pt));
                    }
                    ScatterView parent = (ScatterView)s.Parent;
                    parent.Items.Remove(s);
                }
            }
            catch (Exception exc) { Console.WriteLine(exc); }
        }

        //Given a Site and its parent StackPanel, returns index of adjacent Site
        public static int neighborSiteIndex(int i, StackPanel pan)
        {
            int index = 0;

            //If element is first or last in StackPanel, go straight to 2nd or 2nd to last element
            if (i == 0 || i == VisualTreeHelper.GetChildrenCount(pan) - 1)
            {
                index = i + 1;
                if (i != 0) index = i - 1;
            }
            else
            {
                index = i + 1;
                if (VisualTreeHelper.GetChild(pan, i - 1).GetType() == typeof(Sites))
                {
                    index = i - 1;
                }
            }
            return index;
        }

        //Detects placeholders and doubles sites appropriately
        public HitTestResultBehavior sitesCallback(HitTestResult result) 
        {
            if (result.VisualHit.GetType() == typeof(Grid))
            {
                Grid gridResult = (Grid)result.VisualHit;
                if (gridResult.Name == "sitesGrid" && ((Sites)gridResult.Parent).Sequence != "aatg")
                {
                    //add fusion site check

                    //Copy data into target Site
                    Sites target = (Sites)gridResult.Parent;
                    target.copySitesInfoFrom(this);

                    //Find and define neighbor Site
                    StackPanel parentPanel = (StackPanel)target.Parent;
                    int targetIndex = pd2.PD2_manual.Children.IndexOf(target);

                    //catch bug when element returned is not a Sites object
                    object childElement = VisualTreeHelper.GetChild(parentPanel, neighborSiteIndex(targetIndex, parentPanel));

                    if (childElement is Sites)
                    {
                        Sites neighbor = (Sites)childElement;
                        neighbor.copySitesInfoFrom(this);
                    }
                    return HitTestResultBehavior.Stop;

                    //if (targetIndex == 0)
                    //{ //If dropped into the first
                    //    Sites afterTarget = (Sites)VisualTreeHelper.GetChild(parentPanel, targetIndex + 1);
                    //    afterTarget.copySitesInfoFrom(this);
                    //}
                    //else if (targetIndex == pd2.PD2_manual.Children.Count - 1)
                    //{ //If dropped into the last
                    //    Sites beforeTarget = (Sites)VisualTreeHelper.GetChild(parentPanel, targetIndex - 1);
                    //    beforeTarget.copySitesInfoFrom(this);
                    //} 
                    //else
                    //{ //if dropped into the in-between
                    //    if (VisualTreeHelper.GetChild(parentPanel, targetIndex - 1).GetType() == typeof(Sites))
                    //    { //If element in front is a Sites
                    //        Sites beforeTarget = (Sites)VisualTreeHelper.GetChild(parentPanel, targetIndex - 1);
                    //        beforeTarget.copySitesInfoFrom(this);
                    //    }
                    //    else
                    //    { //If element behind is a Sites
                    //        Sites afterTarget = (Sites)VisualTreeHelper.GetChild(parentPanel, targetIndex + 1);
                    //        afterTarget.copySitesInfoFrom(this);
                    //    }
                    //}
                    
                }
            }
            return HitTestResultBehavior.Continue;
        }

        public Sites clone()
        {
            Sites s = new Sites(_label, _sequence, Circle.Background);
            return s;
        }

        //Copies label, sequence, and appearance from Sites parameter (used to transfer data to placeholder)
        public void copySitesInfoFrom(Sites s)
        {
            _label = s.Label;
            _sequence = s.Sequence;
            Circle.Background = s.Circle.Background;
            CircleText.Text = _sequence;
            Opacity = s.Opacity;
        }

        private bool isSiteMeetingRequirement(Grid visualHitResultGrid)
        {

            //Won't work because of way hit test is set up, won't be able to know which exact fusion site location is being acted upon unless re-iterate through visual tree to try and locate it
            //Check can't determine if user is adding a fusion site to blank or to overwrite an existing one
            //Need more time and thoughts on this...which we don't have...@T.Feng


            //String currentSite = ((Sites)visualHitResultGrid.Parent).Sequence;

            ////check for no duplicate fusion sites for Part and L1Module
            //if (pd2.SitesAdded.Length < 6)
            //{
            //    //Go through attray containing all added fusion sites
            //    for (int i = 0; i < pd2.SitesAdded.Length; i++) 
            //    {
            //        Sites currentInArray = pd2.SitesAdded[i];

            //        //if fusion site already exists in array, throw error
            //        if ((currentInArray != null) && (currentSite == currentInArray.Sequence))
            //        {
            //            MessageBox.Show("Cannot have duplicate fusion sites, please alter existing site before adding current one.");
            //            return false;
            //        }
            //        //if no fusion site exists (already made sure no duplicates before this), add site into array
            //        else if (currentInArray == null)
            //        {
            //            pd2.SitesAdded[i] = (Sites)visualHitResultGrid.Parent;
            //            return true;
            //        }
            //        //else if current fusion site doesn't exist in array, continue loop
            //    }

                


            //}

            return false;

        }

        //Create a user-determined site from a template and adds it to siteLibrary and SourceItems list
        private void CircleText_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    String siteSeq = CircleText.Text;
                    Boolean isNotDupe = true;

                    foreach (Sites site in pd2.FusionSiteLibrary)
                    {
                        if (siteSeq == site.Sequence) isNotDupe = false;
                    }

                    if (siteSeq.All(c => "actg".Contains(c)) && siteSeq.Length < 5 && isNotDupe && siteSeq != "aatg") //aatg can only be used between RBS and CDS so they shouldn't need it in the library as a drag and droppable Site
                    {
                        Point originalCenter = Center;
                        Sites s = new Sites(siteSeq);
                        s.Center = originalCenter;
                        pd2.PD2_siteLibrary.Items.Add(s);
                        pd2.FusionSiteLibrary.Add(s);
                        pd2.PD2_siteLibrary.Items.Remove(this);
                    }
                }
            }
            catch (Exception exc) { Console.WriteLine(exc); }
        }

        //Enables touch on the CircleText textbox
        private void CircleText_TouchDown(object sender, TouchEventArgs e)
        {
            if (!CircleText.IsReadOnly) CircleText.Text = "";
            CircleText.Focus();
        }

        //Creates hexadecimal color from sequence
        private Brush convertColorFromSeq(String s)
        {
            try
            {
                if (s.All(c => "actg".Contains(c)))
                {
                    String color = "#";
                    char[] array = s.ToArray();
                    for (int i = 0; i < 4; i++)
                    {
                        char c = char.ToLower(array[i]);
                        if (c == 'a') color += "99";
                        if (c == 'c') color += "bb";
                        if (c == 't') color += "dd";
                        if (c == 'g') color += "ff";
                    }
                    return (Brush)new BrushConverter().ConvertFromString(color); //To always ensure Opacity = 1, use i < 3 and initialize color = "#ff"
                }
                else { return Brushes.LightGray; }
            }
            catch (Exception exc) { 
                Console.WriteLine(exc);
                return Brushes.LightGray;
            }
        }

        
    }
}
