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
using System.IO;
using System.Threading;

namespace SurfaceApplication1
{
    /// <summary>
    /// 7/13/2012
    /// @ Veronica Lin
    /// Interaction logic for PrimerDesigner1.xaml
    /// CURRENT ISSUES: 
    /// Without the orientation and sequences for BpiI, BsaI, or the Ns, the primers shown aren't accurate
    /// in terms of length, %GC, or sequence. The reverse primer/reverse complement can't be derived either.
    /// Fusion sites don't always carry over correctly (?)
    /// </summary>
    public partial class PrimerDesigner1 : ScatterViewItem
    {
        public static SurfaceWindow1 sw1;
        public static PrimerDesigner2 pd2;
        private UIElement[][] _partSiteSets;
        private List<List<Part>> _partList;
        private List<List<String>> _partPrimerList;
        private List<List<String>> _l0PrimerList;
        private List<List<String>> _l1PrimerList;
        private List<List<String>> _l2PrimerList;

        private double _leftGibbsFree;
        private double _rightGibbsFree;
        private String _leftSeq;
        private String _rightSeq;
        private Thread _primerTest;
        private ProgressBarWrapper _progressBarWrapper;
        private LinkedList<String>[] alignments;

        #region Sequences for building primers
        private static String lString = "AT-GAAGAC-GT-";
        private static String rString = "-AG-GTCTTC-GT";

        private static String lacZ = "ACCATGATTACGGATTCACTGGCCGTCGTTTTACAACGTCGTGACTGGGAAAACCCTGGCGTTACCCAACTTAATCGCCTTGCAGCACATCCCCCTTTCGCCAGCTGGCGTAATAGCGAAGAGGCCCGCACCGATCGCCCTTCCCAACAGTTGCGCAGCCTGAATGGCGAATGGCGCTTTGCCTGGTTTCCGGCACCAGAAGCGGTGCCGGAAAGCTGGCTGGAGTGCGATCTTCCTGAGGCCGATACTGTCGTCGTCCCCTCAAACTGGCAGATGCACGGTTACGATGCGCCCATCTACACCAACGTAACCTATCCCATTACGGTCAATCCGCCGTTTGTTCCCACGGAGAATCCGACGGGTTGTTACTCGCTCACATTTAATGTTGATGAAAGCTGGCTACAGGAAGGCCAGACGCGAATTATTTTTGATGGCGTTAACTCGGCGTTTCATCTGTGGTGCAACGGGCGCTGGGTCGGTTACGGCCAGGACAGTCGTTTGCCGTCTGAATTTGACCTGAGCGCATTTTTACGCGCCGGAGAAAACCGCCTCGCGGTGATGGTGCTGCGTTGGAGTGACGGCAGTTATCTGGAAGATCAGGATATGTGGCGGATGAGCGGCATTTTCCGTGACGTCTCGTTGCTGCATAAACCGACTACACAAATCAGCGATTTCCATGTTGCCACTCGCTTTAATGATGATTTCAGCCGCGCTGTACTGGAGGCTGAAGTTCAGATGTGCGGCGAGTTGCGTGACTACCTACGGGTAACAGTTTCTTTATGGCAGGGTGAAACGCAGGTCGCCAGCGGCACCGCGCCTTTCGGCGGTGAAATTATCGATGAGCGTGGTGGTTATGCCGATCGCGTCACACTACGTCTGAACGTCGAAAACCCGAAACTGTGGAGCGCCGAAATCCCGAATCTCTATCGTGCGGTGGTTGAACTGCACACCGCCGACGGCACGCTGATTGAAGCAGAAGCCTGCGATGTCGGTTTCCGCGAGGTGCGGATTGAAAATGGTCTGCTGCTGCTGAACGGCAAGCCGTTGCTGATTCGAGGCGTTAACCGTCACGAGCATCATCCTCTGCATGGTCAGGTCATGGATGAGCAGACGATGGTGCAGGATATCCTGCTGATGAAGCAGAACAACTTTAACGCCGTGCGCTGTTCGCATTATCCGAACCATCCGCTGTGGTACACGCTGTGCGACCGCTACGGCCTGTATGTGGTGGATGAAGCCAATATTGAAACCCACGGCATGGTGCCAATGAATCGTCTGACCGATGATCCGCGCTGGCTACCGGCGATGAGCGAACGCGTAACGCGAATGGTGCAGCGCGATCGTAATCACCCGAGTGTGATCATCTGGTCGCTGGGGAATGAATCAGGCCACGGCGCTAATCACGACGCGCTGTATCGCTGGATCAAATCTGTCGATCCTTCCCGCCCGGTGCAGTATGAAGGCGGCGGAGCCGACACCACGGCCACCGATATTATTTGCCCGATGTACGCGCGCGTGGATGAAGACCAGCCCTTCCCGGCTGTGCCGAAATGGTCCATCAAAAAATGGCTTTCGCTACCTGGAGAGACGCGCCCGCTGATCCTTTGCGAATACGCCCACGCGATGGGTAACAGTCTTGGCGGTTTCGCTAAATACTGGCAGGCGTTTCGTCAGTATCCCCGTTTACAGGGCGGCTTCGTCTGGGACTGGGTGGATCAGTCGCTGATTAAATATGATGAAAACGGCAACCCGTGGTCGGCTTACGGCGGTGATTTTGGCGATACGCCGAACGATCGCCAGTTCTGTATGAACGGTCTGGTCTTTGCCGACCGCACGCCGCATCCAGCGCTGACGGAAGCAAAACACCAGCAGCAGTTTTTCCAGTTCCGTTTATCCGGGCAAACCATCGAAGTGACCAGCGAATACCTGTTCCGTCATAGCGATAACGAGCTCCTGCACTGGATGGTGGCGCTGGATGGTAAGCCGCTGGCAAGCGGTGAAGTGCCTCTGGATGTCGCTCCACAAGGTAAACAGTTGATTGAACTGCCTGAACTACCGCAGCCGGAGAGCGCCGGGCAACTCTGGCTCACAGTACGCGTAGTGCAACCGAACGCGACCGCATGGTCAGAAGCCGGGCACATCAGCGCCTGGCAGCAGTGGCGTCTGGCGGAAAACCTCAGTGTGACGCTCCCCGCCGCGTCCCACGCCATCCCGCATCTGACCACCAGCGAAATGGATTTTTGCATCGAGCTGGGTAATAAGCGTTGGCAATTTAACCGCCAGTCAGGCTTTCTTTCACAGATGTGGATTGGCGATAAAAAACAACTGCTGACGCCGCTGCGCGATCAGTTCACCCGTGCACCGCTGGATAACGACATTGGCGTAAGTGAAGCGACCCGCATTGACCCTAACGCCTGGGTCGAACGCTGGAAGGCGGCGGGCCATTACCAGGCCGAAGCAGCGTTGTTGCAGTGCACGGCAGATACACTTGCTGATGCGGTGCTGATTACGACCGCTCACGCGTGGCAGCATCAGGGGAAAACCTTATTTATCAGCCGGAAAACCTACCGGATTGATGGTAGTGGTCAAATGGCGATTACCGTTGATGTTGAAGTGGCGAGCGATACACCGCATCCGGCGCGGATTGGCCTGAACTGCCAGCTGGCGCAGGTAGCAGAGCGGGTAAACTGGCTCGGATTAGGGCCGCAAGAAAACTATCCCGACCGCCTTACTGCCGCCTGTTTTGACCGCTGGGATCTGCCATTGTCAGACATGTATACCCCGTACGTCTTCCCGAGCGAAAACGGTCTGCGCTGCGGGACGCGCGAATTGAATTATGGCCCACACCAGTGGCGCGGCGACTTCCAGTTCAACATCAGCCGCTACAGTCAACAGCAACTGATGGAAACCAGCCATCGCCATCTGCTGCACGCGGAAGAAGGCACATGGCTGAATATCGACGGTTTCCATATGGGGATTGGTGGCGACGACTCCTGGAGCCCGTCAGTATCGGCGGAATTCCAGCTGAGCGCCGGTCGCTACCATTACCAGTTGGTCTGGTGTCAAAAATAATAATAAcggctgccgt".ToLower();
        
        private static String lStringOut0 = "BBPre-BsaI-N-"; //"atgaagacgt";
        private static String lStringIn0 = "-NN-BpiI-";
        private static String rStringIn0 = "-BpiI-NN-"; //aggtcttcgt";
        private static String rStringOut0 = "-N-BsaI-BBSuf";

        private static String lStringOut1 = "BBPre-BpiI-NN-";
        private static String lStringIn1 = "-N-BsaI-";
        private static String rStringIn1 = "-BsaI-N-";
        private static String rStringOut1 = "-NN-BpiI-BBSuf";
        #endregion

        //Populates showPanels with L1 StackPanels that hold 1-4 L0 StackPanels that hold 1 Part and 2 Sites
        //Grouping into set0s organizes data for L0 PCR products and destination vectors
        //Grouping into set1s organizes data for L1 PCR products (LacZ?) and destination vectors
        public PrimerDesigner1(List<List<Part>> partListFromPD2)
        {
            InitializeComponent();
            Part.pd1 = this;
            SurfaceWindow1.pd1 = this;
            _partList = partListFromPD2;
            _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));


            /* NOTE: Removing duplicate fusion site visuals is MAD HACK right now, and I know that it's totally wrong and lazy fix. Will have to put in 
             * counters and conditionals to figure out when the last part of the last subpart list is to add in that last fusion site. Also need to somehow
             * preserve the fusion site sequence string so it'll display the correct sequence with double fusion site SEQUENCE still there.
             * SHOOT ME IN THE HEAD @T.Feng (will come back to this after dinner)*/


            int partCounter = 0;
            int sublistCounter = 0;

            foreach (List<Part> sublist in _partList)
            {
                StackPanel set1 = new StackPanel();
                set1.Orientation = System.Windows.Controls.Orientation.Horizontal;
                foreach (Part p in sublist)
                {
                    StackPanel set0 = new StackPanel();
                    set0.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    set0.Background = new SolidColorBrush(Colors.Transparent);
                    Sites s1 = p.SitesList.ElementAt(0).clone();
                    s1.IsManipulationEnabled = false;
                    set0.Children.Add(s1);
                    set0.Children.Add(p);
                    set1.Children.Add(set0);

                    //if iteration is on last part of entire _partList, add second fusion site to end
                    if ((partCounter == sublist.Count - 1) && (sublistCounter == _partList.Count - 1))
                    {
                        Sites s2 = p.SitesList.ElementAt(1).clone();
                        s2.IsManipulationEnabled = false;
                        set0.Children.Add(s2);
                    }

                    //Generate primers for the Part PCR product and L0 DV and add to list
                    if (_partPrimerList == null) _partPrimerList = new List<List<String>>();
                    _partPrimerList.Add(generatePrimers(-1, set0));
                    if (_l0PrimerList == null) _l0PrimerList = new List<List<String>>();
                    _l0PrimerList.Add(generatePrimers(0, set0));

                    partCounter++;
                }
                showPanel.Children.Add(set1);

                //If more than one Part in sublist, generate primers for the L1 DV and add to list
                if (sublist.Count > 1)
                {
                    if (_l1PrimerList == null) _l1PrimerList = new List<List<String>>();
                    _l1PrimerList.Add(generatePrimers(1, set1));
                    l1DV.IsEnabled = true;
                    l1DV.Visibility = Visibility.Visible;
                }

                partCounter = 0;
                sublistCounter++;

            //If more than one sublist/L1 module in _partList, generate primers for the L2 DV and add to list
            }if (_partList.Count > 1)
            {
                if (_l2PrimerList == null) _l2PrimerList = new List<List<String>>();
                _l2PrimerList.Add(generatePrimers(2, showPanel));
                l2DV.IsEnabled = true;
                l2DV.Visibility = Visibility.Visible;
            }

            //On initialization, auto-select first Part
            StackPanel show = (StackPanel)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(showPanel, 0),0);
            show.Background = new SolidColorBrush(Colors.WhiteSmoke);
            pcr.IsChecked = true;
        }

        //Finds first and last fusion site contained in a given set and adds appropriate gene
        //level == -1 (Part PCR product), 0 (L0 destination vector), 1 (L1 DV), 2 (L2 DV)
        private List<String> generatePrimers(int level, StackPanel set)
        {
            DependencyObject firstKid = VisualTreeHelper.GetChild(set, 0);
            //DependencyObject lastKid = VisualTreeHelper.GetChild(set, VisualTreeHelper.GetChildrenCount(set) - 1);

            //If set is a L0 panel, it is both the first and last Panel to contain Sites and Parts
            //Requests for flanking sites should look inside set, therefore set first/lastKid to set
            if (firstKid.GetType() != typeof(StackPanel))
            {
                firstKid = set;
                //lastKid = set;
            }

            int i = 0;

            //Drill down to the StackPanels (L0 Panels) that contain Sites and Parts
            while (VisualTreeHelper.GetChild(firstKid,i).GetType() != typeof(Sites))
            {
                firstKid = VisualTreeHelper.GetChild(firstKid, i);
                i++;
                //lastKid = VisualTreeHelper.GetChild(lastKid, VisualTreeHelper.GetChildrenCount(lastKid) - 1);
            }

            
           
            Part pt = (Part)VisualTreeHelper.GetChild(firstKid, i+1);
            String s1 = (pt.SitesList.ElementAt(i-1)).Sequence;
            i++;
            String s2 = (pt.SitesList.ElementAt(i)).Sequence;
            //String s1 = (String)((Sites)VisualTreeHelper.GetChild(firstKid, 0)).Sequence;
            //String s2 = (String)((Sites)VisualTreeHelper.GetChild(lastKid, 2)).Sequence;
            String p = lacZ;

            String flank1 = ""; //Flanking sequence, including s1, restriction sites, etc.
            String flank2 = ""; //Flanking sequence, including s2, restriction sites, etc.

            if (level == -1)
            {
                p = (String)((Part)VisualTreeHelper.GetChild(firstKid, 1)).myRegDS.BasicInfo.Sequence;
                flank1 = lString + s1 + "-";
               flank2 = "-" + s2 + rString;
            }
            else if (level == 1)
            {
                flank1 = lStringOut1 + s1 + lStringIn1;
                flank2 = rStringIn1 + s2 + rStringOut1;
            }
            else
            {
                flank1 = lStringOut0 + s1 + lStringIn0;
                flank2 = rStringIn0 + s2 + rStringOut0;
            }

            String complete = flank1 + p +flank2;
            String forward = flank1 + leftGetSeq(24, p);
            String reverse = rightGetSeq(24, p) +flank2;
            String reverseComplement = "";

            return new List<String>() { complete, forward, reverse, reverseComplement };
        }

        public PrimerDesigner1(UIElement[][] partSiteSetsFromPD2)
        {
            InitializeComponent();
            Part.pd1 = this;
            SurfaceWindow1.pd1 = this;
            _partSiteSets = partSiteSetsFromPD2;
            _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));

            //Populate showPanel with elements grouped into StackPanels
            //Grouping helps determine which Parts and Sites data to display when one member is selected
            StackPanel firstPanel = new StackPanel();
            firstPanel.Children.Add(_partSiteSets[0][0]);
            showPanel.Children.Add(firstPanel);
            firstPanel.Visibility = Visibility.Collapsed;

            int partCounter = 0;
            int sublistCounter = 0;

            for (int i = 1; i < _partSiteSets.Count() - 1; i++)
            {
                StackPanel set = new StackPanel();
                foreach (UIElement element in _partSiteSets[i])
                {
                    set.Children.Add(element);
                }
                showPanel.Children.Add(set);

                //Generate primers for the Part PCR product and L0 DV and add to list
                if (_partPrimerList == null) _partPrimerList = new List<List<String>>();
                _partPrimerList.Add(generatePrimers(-1, firstPanel));
                if (_l0PrimerList == null) _l0PrimerList = new List<List<String>>();
                _l0PrimerList.Add(generatePrimers(0, firstPanel));

                partCounter++;

                //If more than one Part in sublist, generate primers for the L1 DV and add to list
                //if (sublist.Count > 1)
                //{
                //    if (_l1PrimerList == null) _l1PrimerList = new List<List<String>>();
                //    _l1PrimerList.Add(generatePrimers(1, lastPanel));
                //    l1DV.IsEnabled = true;
                //    l1DV.Visibility = Visibility.Visible;
                //}//If more than one sublist/L1 module in _partList, generate primers for the L2 DV and add to list

                partCounter = 0;
                sublistCounter++;
            }

            StackPanel lastPanel = new StackPanel();
            lastPanel.Children.Add(_partSiteSets[_partSiteSets.Count() - 1][0]);
            showPanel.Children.Add(lastPanel);
            lastPanel.Visibility = Visibility.Collapsed;

            foreach (StackPanel sp in showPanel.Children)
            {
                foreach (UIElement elem in sp.Children) elem.IsManipulationEnabled = false;
                sp.Orientation = System.Windows.Controls.Orientation.Horizontal;
                sp.Background = new SolidColorBrush(Colors.Transparent);
                sp.Margin = new Thickness(3);
            }

            if (_l2PrimerList == null) _l2PrimerList = new List<List<String>>();
            _l2PrimerList.Add(generatePrimers(2, showPanel));
            /* NOTE: Removing duplicate fusion site visuals is MAD HACK right now, and I know that it's totally wrong and lazy fix. Will have to put in 
             * counters and conditionals to figure out when the last part of the last subpart list is to add in that last fusion site. Also need to somehow
             * preserve the fusion site sequence string so it'll display the correct sequence with double fusion site SEQUENCE still there.
             * SHOOT ME IN THE HEAD @T.Feng (will come back to this after dinner)*/
                          

            //On initialization, auto-select first Part
            StackPanel show = (StackPanel)VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(showPanel, 0), 0);
            show.Background = new SolidColorBrush(Colors.WhiteSmoke);
            pcr.IsChecked = true;


            //Currently, show first part
            showSetData(0, _l0PrimerList);
        }

        private void showProgressBar()
        {
            ProgressIndicator.Visibility = Visibility.Visible;
        }

        private void hideProgressBar()
        {
            ProgressIndicator.Visibility = Visibility.Collapsed;
        }

        #region old showSetData
        //private void showSetData(int index)
        //{
        //    //int primerLength = 24;
        //    String s1;
        //    String p;
        //    String s2;
        //    String FString = "ATGAAGACGT-".ToLower();
        //    String RString = "-AGGTCTTCGT".ToLower();

        //    if (false) //(index == 0 || index == showPanel.Children.Count - 1) - used when vector backbones were to show
        //    {
        //        ((StackPanel)VisualTreeHelper.GetChild(showPanel, 0)).Background = new SolidColorBrush(Colors.WhiteSmoke);
        //        ((StackPanel)VisualTreeHelper.GetChild(showPanel, showPanel.Children.Count - 1)).Background = new SolidColorBrush(Colors.WhiteSmoke);
        //        s1 = ((Sites)_partSiteSets[0][0]).Sequence;
        //        p = "actcagcatgcatgcatcgtcagtacgtcatgactgactacgtcgttggtgggggaaaaa";
        //        s2 = ((Sites)_partSiteSets[_partSiteSets.Count() - 1][0]).Sequence;
        //    }
        //    else
        //    {

        //        ((StackPanel)VisualTreeHelper.GetChild(showPanel, index)).Background = new SolidColorBrush(Colors.WhiteSmoke);
        //        s1 = ((Sites)_partSiteSets[index][0]).Sequence;
        //        p = ((Part)_partSiteSets[index][1]).myRegDS.BasicInfo.Sequence;
        //        s2 = ((Sites)_partSiteSets[index][2]).Sequence;
        //    }

        //    String completeSequence = s1 + "-" + p + "-" + s2;
        //    seqComplete.Text = completeSequence;

        //    _leftSeq = leftGetSeq(24, seqComplete.Text);
        //    _rightSeq = rightGetSeq(24, seqComplete.Text);

        //    //Forward
        //    String leftPrimer = FString + _leftSeq;
        //    seqForward.Text = leftPrimer;

        //    //Convert RString and Site sequence in 3' to 5', to match _rightPrimer, which is the last ~24 bases in 3' to 5'
        //   String RString3to5 = new String(RString.ToCharArray().Reverse().ToArray());
        //    String site2seq3to5 = new String(s2.ToCharArray().Reverse().ToArray());
        //    String reverse = RString3to5 + _rightSeq;

        //    //Reverse complement
        //    //Transform reverse3to5 into its complement
        //    String rightPrimer = Transform(reverse);
        //    seqReverse.Text = rightPrimer;

        //    //Show number of basepairs
        //    lengthForward.Text = seqForward.Text.Length.ToString();
        //    lengthReverse.Text = seqReverse.Text.Length.ToString();

        //    //Show percent of gc bases
        //    double countForward = (double)(leftPrimer.Split('c').Length + leftPrimer.Split('g').Length - 2);
        //    double countReverse = (double)(rightPrimer.Split('c').Length + rightPrimer.Split('g').Length - 2);
        //    double percentForward = Math.Round(100*(countForward / ((double)seqForward.Text.Length)),3);
        //    double percentReverse = Math.Round(100*(countReverse / ((double)seqReverse.Text.Length)),3);
        //    //Just make it show for now
        //    gcForward.Text = percentForward.ToString() + "%";
        //    gcReverse.Text = percentReverse.ToString() + "%";

        //    //seqForward.Text = completeSequence.Substring(0, primerLength);
        //    //String txt = completeSequence.Substring(completeSequence.Length - primerLength, primerLength);
        //    //seqReverse.Text = new String(txt.ToCharArray().Reverse().ToArray());
        //}
        #endregion

        //Currently shows nothing for reverse primer - haven't figured out reverse complement
        //For reverse complement, need sequence and orientation of BpiI and BsaI, as well as NNs
        private void showSetData(int index, List<List<String>> primerList)
        {
            List<String> primers = primerList.ElementAt(index);
            seqComplete.Text = primers.ElementAt(0);
            seqForward.Text = primers.ElementAt(1);

            //Show number of basepairs
            lengthForward.Text = (seqForward.Text.Replace("-", "")).Length.ToString();

            //Show percent of gc bases
            double countForward = (double)(primers.ElementAt(1).Split('c').Length + primers.ElementAt(1).Split('g').Length - 2);
            double percentForward = Math.Round(100 * (countForward / ((double)seqForward.Text.Length)), 3);
            //Just make it show for now
            gcForward.Text = percentForward.ToString() + "%";
        }

        private void select_otherPrimers(object sender, RoutedEventArgs e)
        {
            if (sender == l2DV) { showSetData(0, _l2PrimerList); }
            else
            {
                int i = -1;
                int j = -1;

                //Find indexes of selected L0 Panel and its parent L1 Panel
                foreach (StackPanel set1 in showPanel.Children)
                {
                    foreach (StackPanel set0 in set1.Children)
                    {
                        if (set0.Background != Brushes.Transparent) 
                        { 
                            j = set1.Children.IndexOf(set0);
                            break;
                        }
                    }
                    if (j != -1)
                    {
                        i = showPanel.Children.IndexOf(set1);
                        break;
                    }
                }

                if (sender == l1DV) { showSetData(i, _l1PrimerList); }
                else if (sender == l0DV) { showSetData(4*i + j, _l0PrimerList); }
                else /*sender == pcr*/ { showSetData(4*i + j, _partPrimerList); }
            }
        }

        private void showPanel_PreviewTouchDown(object sender, TouchEventArgs e)
        {

            Point pt = e.GetTouchPoint(showPanel).Position;
            VisualTreeHelper.HitTest(showPanel, null, new HitTestResultCallback(getIndexCallback), new PointHitTestParameters(pt));
        }

        //Displays selected Part's PCR primers (no DV primers)
        private HitTestResultBehavior getIndexCallback(HitTestResult result)
        {
            //If StackPanel detected and is grandchild of a StackPanel (i.e. a set0 inside set1 inside showPanel)
            if (result.VisualHit.GetType() == typeof(StackPanel) &&
                ((StackPanel)result.VisualHit).Parent.GetType() == typeof(StackPanel) &&
                ((StackPanel)((StackPanel)result.VisualHit).Parent).Parent.GetType() == typeof(StackPanel))
            {
                //Remove previous selection
                foreach (StackPanel sp1 in showPanel.Children)
                {
                    foreach (StackPanel sp0 in sp1.Children)
                    {
                        sp0.Background = new SolidColorBrush(Colors.Transparent);
                    }
                }

                //Highlight selected set0, get primerlist index, and write in primer data
                StackPanel set0 = (StackPanel)result.VisualHit;
                StackPanel set1 = (StackPanel)set0.Parent;
                int i = showPanel.Children.IndexOf(set1);
                int j = set1.Children.IndexOf(set0);

                int listIndex = i * 4 + j;
                showSetData(listIndex, _partPrimerList);
                set0.Background = new SolidColorBrush(Colors.WhiteSmoke);
                pcr.IsChecked = true;

                Part set0Part = new Part();
                foreach (UIElement elem in set0.Children)
                {
                    if (elem.GetType() == typeof(Part)) set0Part = (Part)elem;
                }
                
                //showSetData(i);
                //foreach (UIElement elem in testGrid.Children)
                //{
                //    if (elem.GetType() == typeof(Grid))
                //    {
                //        Grid g = (Grid)elem;
                //        foreach (UIElement e in g.Children)
                //        {
                //            if (e.GetType() == typeof(SurfaceCheckBox))
                //            {
                //                ((SurfaceCheckBox)e).IsChecked = false;
                //            }
                //            if (e.GetType() == typeof(TextBlock))
                //            {
                //                ((TextBlock)e).Text = "--";
                //            }
                //        }
                //    }
                //}
                return HitTestResultBehavior.Stop;
            }
            return HitTestResultBehavior.Continue;
        }

        //Shuffle Zindex so this goes below, without removing any user changes
        //private void backButton_Click(object sender, RoutedEventArgs e)
        //{
        //    int tempZ = pd2.ZIndex;
        //    pd2.ZIndex = ZIndex;
        //    ZIndex = tempZ;
        //    pd2.forwardButton.IsEnabled = true;
        //    pd2.forwardButton.Visibility = Visibility.Visible;
        //}

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            sw1.SW_SV.Items.Remove(this);
            sw1.SW_SV.Items.Remove(pd2);
        }

        #region Primer tests
        //Returns a list of alignmentTest results organized by test type
        private List<double> _getTestResults()
        {
            List<double> testResults = new List<double>();
            alignmentTests at1 = new alignmentTests(_leftSeq);
            alignmentTests at2 = new alignmentTests(_rightSeq);

            alignments= new LinkedList<String>[6];
            
            //the following is for HAIRPIN RUN only!!
          //  testResults.Add(at1.hairpinRun());
            alignments[0]=at1.hairpinRunAlignments();
           // testResults.Add(at2.hairpinRun());

            alignments[1] = at2.hairpinRunAlignments();

            //the following is for SELF DIMER only!!
            //testResults.Add(at1.selfDimerRun());

            alignments[2] = at2.selfDimerRunAlignments();

           // testResults.Add(at2.selfDimerRun());
            alignments[3] = at2.selfDimerRunAlignments();

            
            //the following is for HETERODIMER only!!
           // testResults.Add(at1.heteroDimerRun(_rightSeq));
            alignments[4] = at2.heteroDimerRunAlignments(_rightSeq);

           // testResults.Add(at2.heteroDimerRun(_leftSeq));
            alignments[5] = at2.heteroDimerRunAlignments(_leftSeq);


            //Console.WriteLine(testResults);
            return testResults;
        }

        //Writes results into PD1, compares results to Gibbs free enrgy and checks forward/reverse success checkboxes accordingly
        private void _getTestResultsCallback(List<double> resultsList)
        {

            //Console.WriteLine(alignments);
            //Console.WriteLine("results SVI here!!");

            ScatterViewItem testResultsObject= new ScatterViewItem();
            ScrollViewer scroller = new ScrollViewer();
            TextBlock block = new TextBlock();
            String resultString = "";

            for(int i =0; i <5; i++){
                foreach (String s in alignments[i])
                {
                    resultString += s + "\n\n";
                }
            }

            block.Text = resultString;

            scroller.Content = block;


            testResultsObject.Content = scroller;

            sw1.SW_SV.Items.Add(testResultsObject);
            //testGrid.Children.Add(testResultsObject);
           


            List<SurfaceCheckBox> checkList = new List<SurfaceCheckBox>();
            List<TextBlock> numList = new List<TextBlock>();

            foreach (UIElement elem in testGrid.Children)
            {
                if (elem.GetType() == typeof(Grid))
                {
                    foreach (UIElement e in ((Grid)elem).Children)
                    {
                        if (e.GetType() == typeof(SurfaceCheckBox))
                        {
                            checkList.Add((SurfaceCheckBox)e);
                        }
                        else if (e.GetType() == typeof(TextBlock))
                        {
                            numList.Add((TextBlock)e);
                        }
                    }
                }

            }


            _leftGibbsFree = calcDeltaG(_leftSeq);
            _rightGibbsFree = calcDeltaG(_rightSeq);
            double gibbsTestValue;

            for (int i = 0; i < resultsList.Count; i++)
            {
            
                
                double d = resultsList.ElementAt(i);
                numList.ElementAt(i).Text = d.ToString();
                if (i < 3) gibbsTestValue = _leftGibbsFree;
                else gibbsTestValue = _rightGibbsFree;
                if (d < gibbsTestValue) checkList.ElementAt(i).IsChecked = true;
            
            }


        }

        /// <summary>
        /// When the Test Selected Primers button is pressed, the tests are called and
        /// the displays are updated accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testRunner_Click(object sender, RoutedEventArgs e)
        {
            _primerTest = _progressBarWrapper.execute<List<double>>(_getTestResults, _getTestResultsCallback);

            Console.WriteLine(alignments);
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            #region dumb
            //    try
        //    {
        //        // at1 is for all of the tests for the LEFT PRIMER
        //        alignmentTests at1 = new alignmentTests(_leftSeq);

        //        //the following is for HAIRPIN RUN only!!
        //        double worstCaseHP1 = at1.hairpinRun();
        //        _leftGibbsFree = calcDeltaG(_leftSeq);
        //        hNumF.Text = worstCaseHP1.ToString();
        //        if (worstCaseHP1 < _leftGibbsFree)
        //        {
        //            hCheckF.IsChecked = true;
        //        }
        //        else
        //        {
        //            hCheckF.IsChecked = false;
        //        }

        //        //the following is for SELF DIMER only!!
        //        double worstCaseSD1 = at1.selfDimerRun();
        //        sNumF.Text = worstCaseSD1.ToString();
        //        if (worstCaseSD1 < _leftGibbsFree)
        //        {
        //            sCheckF.IsChecked = true;
        //        }
        //        else
        //        {
        //            sCheckF.IsChecked = false;
        //        }

        //        //the following is for HETERODIMER only!!
        //        double worstCaseHD1 = at1.heteroDimerRun(_rightSeq);
        //        _rightGibbsFree = calcDeltaG(_rightSeq);
        //        tNumF.Text = worstCaseHD1.ToString();
        //        if (worstCaseHD1 < _rightGibbsFree)
        //        {
        //            tCheckF.IsChecked = true;
        //        }
        //        else
        //        {
        //            tCheckF.IsChecked = false;
        //        }

        //        // at2 is for all of the tests for the RIGHT PRIMER
        //        alignmentTests at2 = new alignmentTests(_rightSeq);

        //        //the following is for HAIRPIN RUN only!!
        //        double worstCaseHP2 = at2.hairpinRun();
        //        hNumR.Text = worstCaseHP2.ToString();
        //        if (worstCaseHP2 < _leftGibbsFree)
        //        {
        //            hCheckR.IsChecked = true;
        //        }
        //        else
        //        {
        //            hCheckR.IsChecked = false;
        //        }

        //        //the following is for SELF DIMER only!!
        //        double worstCaseSD2 = at2.selfDimerRun();
        //        sNumR.Text = worstCaseSD2.ToString();
        //        if (worstCaseSD2 < _rightGibbsFree)
        //        {
        //            sCheckR.IsChecked = true;
        //        }
        //        else
        //        {
        //            sCheckR.IsChecked = false;
        //        }

        //        //the following is for HETERODIMER only!!
        //        double worstCaseHD2 = at2.heteroDimerRun(_leftSeq);
        //        tNumR.Text = worstCaseHD2.ToString();
        //        if (worstCaseHD2 < _rightGibbsFree)
        //        {
        //            tCheckR.IsChecked = true;
        //        }
        //        else
        //        {
        //            tCheckR.IsChecked = false;
        //        }
        //    }
            //    catch (Exception exc) { Console.WriteLine(exc); }
            #endregion
        }

        /// <summary>
        /// Given a full sequence and length, this method returns the highlighted left primer sequence
        /// </summary>
        /// <param name="len">integer representing the length of the primer sequence</param>
        /// <param name="fullSeq">string representing the full sequence displayed on the primer designer</param>
        /// <returns>string representing the left highlighted primer sequence</returns>
        private String leftGetSeq(int len, String fullSeq)
        {
            if (len > 40)
                len = 40;
            if (fullSeq.Length < len) len = fullSeq.Length;
            String primer = "";
            //try
            //{
                for (int i = 0; i < len; i++)
                {
                    char c = fullSeq[i];
                    primer += c;
                }
            //}
            //catch (Exception exc) { Console.WriteLine(exc); }
            return primer;
        }

        /// <summary>
        /// Given a full sequence and length, this method returns the highlighted right primer sequence
        /// </summary>
        /// <param name="len">integer representing the length of the primer sequence</param>
        /// <param name="fullSeq">string representing the full sequence displayed on the primer designer</param>
        /// <returns>string representing the right highlighted primer sequence</returns>
        private String rightGetSeq(int len, String fullSeq)
        {
            if (len > 40)
                len = 40;
            if (fullSeq.Length < len) len = fullSeq.Length;
            String primer = "";
            //try
            //{
                for (int i = fullSeq.Length - 1; i >= fullSeq.Length - len; i--)
                {
                    char c = fullSeq[i];
                    primer += c;
                }
            //}
            //catch (Exception exc) { Console.WriteLine(exc); }
            return primer;
        }

        /// <summary>
        /// Calculates gibbs free energy for a processed string
        /// </summary>
        /// <param name="s">String whose delta G is being calculated</param>
        /// <returns> double representing delta G for inputted sequence</returns>
        public static Double calcDeltaG(String s)
        {
            Double toReturn = 0.0;
            if (s != null)
            {
                s = s.ToUpper();
                if (s.StartsWith("C") || s.StartsWith("G"))
                {
                    toReturn = toReturn + 0.98;
                }
                else
                {
                    toReturn = toReturn + 1.03;
                }
                if (s.EndsWith("C") || s.EndsWith("G"))
                {
                    toReturn = toReturn + 0.98;
                }
                else
                {
                    toReturn = toReturn + 1.03;
                }
                for (int i = 0; i < s.Length - 1; i++)
                {
                    String token = s.Substring(i, 2);
                    if (token.Equals("AA") || token.Equals("TT"))
                    {
                        toReturn = toReturn - 1.00;
                    }
                    else if (token.Equals("AT"))
                    {
                        toReturn = toReturn - 0.88;
                    }
                    else if (token.Equals("TA"))
                    {
                        toReturn = toReturn - -0.58;
                    }
                    else if (token.Equals("CA") || token.Equals("AC"))
                    {
                        toReturn = toReturn - 1.45;
                    }
                    else if (token.Equals("GT") || token.Equals("TG"))
                    {
                        toReturn = toReturn - 1.44;
                    }
                    else if (token.Equals("CT") || token.Equals("TC"))
                    {
                        toReturn = toReturn - 1.28;
                    }
                    else if (token.Equals("GA") || token.Equals("AG"))
                    {
                        toReturn = toReturn - 1.30;
                    }
                    else if (token.Equals("CG"))
                    {
                        toReturn = toReturn - 2.17;
                    }
                    else if (token.Equals("GC"))
                    {
                        toReturn = toReturn - 2.24;
                    }
                    else if (token.Equals("GG") || token.Equals("CC"))
                    {
                        toReturn = toReturn - 1.84;
                    }
                }

                //rounding
                toReturn = toReturn * 10;
                int toReturnInt = (int)toReturn;
                toReturn = (double)toReturnInt / (double)10;

                return toReturn;
            }

            return 0.00;
        }
        #endregion

        private void PrintPrimers_Click(object sender, RoutedEventArgs e)
        {
            String FString = "ATGAAGACGT";
            String RString = "AGGTAGGTCTTCGT";

            MessageBox.Show("Primers have been printed to a text file.");

            //foreach (StackPanel s in showPanel.Children)
            //{
            StackPanel sParent = (StackPanel)showPanel.Children[showPanel.Children.Count - 1];
            StackPanel s = (StackPanel)sParent.Children[sParent.Children.Count - 1];
                if(s.Children.Count == 3) 
                {
                    String site1seq = "";
                    String site2seq = "";
                    String textTitle = "";

                    foreach (UIElement elem in s.Children)
                    {
                        if (elem.GetType() == typeof(Sites))
                        {
                            if (site1seq == "")
                            {
                                site1seq = ((Sites)elem).Sequence;
                            }
                            else
                            {
                                site2seq = ((Sites)elem).Sequence;
                            }
                        }
                        else if (elem.GetType() == typeof(Part))
                        {
                            Part p = (Part)elem;
                            textTitle = DateTime.Now.ToShortDateString();//p.myRegDS.Name;
                            textTitle = textTitle.Replace('/', '-');
                            Console.WriteLine(textTitle);
                        }
                    }
                        //Get current working directory
                        string file = Directory.GetCurrentDirectory();
                        //change directory to EugeneFiles directory and read text file based on ListModulesToPermute count
                        file = file.Substring(0, file.IndexOf("bin")) + @"PrimerResults/" + textTitle + ".txt";
                        StreamWriter writer = new StreamWriter(file);

                    //Forward
                            writer.WriteLine("Forward Name: " + nameForward.Text);
                            writer.WriteLine("\t# of Base Pairs: " + lengthForward.Text);
                            writer.WriteLine(FString + site1seq + _leftSeq);
                    
                    //Reverse
                            writer.WriteLine("Reverse");
                            writer.WriteLine("\t# of Base Pairs: " + lengthReverse.Text);
                            //Convert RString and Site sequence in 3' to 5', to match _rightPrimer, which is the last ~24 bases in 3' to 5'
                            String RString3to5 = new String(RString.ToCharArray().Reverse().ToArray());
                            String site2seq3to5 = new String(site2seq.ToCharArray().Reverse().ToArray());
                            String reverse3to5 = RString3to5 + site2seq3to5 + _rightSeq;
                            writer.WriteLine(reverse3to5);

                    //Reverse complement
                            writer.WriteLine("Reverse Complement Name: " + nameReverse.Text);
                            writer.WriteLine("\t# of Base Pairs: " + lengthReverse.Text);
                            //Transform reverse3to5 into its complement
                            writer.WriteLine(Transform(reverse3to5));

                            //writer.WriteLine("Forward Name: " + nameForward.Text);
                            //writer.WriteLine("Reverse Name: " + nameReverse.Text);
                            //writer.WriteLine("Complete Sequence: " + seqComplete.Text);
                            //writer.WriteLine("Forward Sequence: " + seqForward.Text);
                            //writer.WriteLine("\t# of Base Pairs: " + lengthForward.Text);
                            //writer.WriteLine("Reverse Sequence: " + seqReverse.Text);
                            //writer.WriteLine("\t# of Base Pairs: " + lengthReverse.Text);

                            writer.Close();
                }
            //}
        }

        //Transform string into its complement
        private String Transform(String s)
        {
            String complement = "";
            char[] array = s.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                if (c == 'a') complement += 't';
                if (c == 't') complement += 'a';
                if (c == 'g') complement += 'c';
                if (c == 'c') complement += 'g';
                if (c == '-') complement += '-';
            }
            return complement;
        }

        private void sequence_TouchDown(object sender, TouchEventArgs e)
        {
            TextBox t = (TextBox)sender;
            t.Focus();
        }

        private void CopyCompleteSequence_TouchDown(object sender, TouchEventArgs e)
        {
            Clipboard.SetText(seqComplete.Text); 
        }

        
    }
}
