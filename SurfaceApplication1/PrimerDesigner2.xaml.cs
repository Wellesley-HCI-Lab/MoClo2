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
using System.Collections;
using System.IO;
using System.Threading;


namespace SurfaceApplication1
{
    /// <summary>
    /// 7/13/2012
    /// @ Veronica Lin
    /// Interaction logic for PrimerDesigner2.xaml
    /// CURRENT ISSUES:
    /// No checks on fusion sites for empties, duplicates, or compatibility 
    /// - commented them out for new structure of data transfer and didn't have time to fix them
    /// </summary>
    public partial class PrimerDesigner2 : ScatterViewItem
    {
        public static SurfaceWindow1 sw1;        
        private static List<Sites> _fusionSiteLibrary;

        private List<Part> _partsList;
        private List<Sites> _fusionSitesList;
        
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
        private string CompleteSequence = "";

        //for duplicate fusion site checks 
        private int _moduleNum; //to indicate which module launched this PrimerDesigner
        private Sites[] _sitesAdded; //stores all fusion sites added to the object 
        private List<int> usedSites;
        private int _l2Modulel1Count;
        private List<String> results;

        #region Sequences for building primers
        private static String Level02RestrictionSitesBeforeFusionSite = "GTTCTTTACTAGTGGGTCTCA"; //followed by fusion site and first 24
        private static String Level02ReverseRestrictionSitesAfterFusionSite = "AGAGACCTACTAGTAGCGGCCGC"; //after last 24 and fusion site

        private static String Level02Bpi1AfterFusionSiteBeforeLacZ = "GTCTTC";
        private static String Level02Bpi1AfterBeforeLacZ = "GAAGAC";

        private static String lacZ = "ACCATGATTACGGATTCACTGGCCGTCGTTTTACAACGTCGTGACTGGGAAAACCCTGGCGTTACCCAACTTAATCGCCTTGCAGCACATCCCCCTTTCGCCAGCTGGCGTAATAGCGAAGAGGCCCGCACCGATCGCCCTTCCCAACAGTTGCGCAGCCTGAATGGCGAATGGCGCTTTGCCTGGTTTCCGGCACCAGAAGCGGTGCCGGAAAGCTGGCTGGAGTGCGATCTTCCTGAGGCCGATACTGTCGTCGTCCCCTCAAACTGGCAGATGCACGGTTACGATGCGCCCATCTACACCAACGTAACCTATCCCATTACGGTCAATCCGCCGTTTGTTCCCACGGAGAATCCGACGGGTTGTTACTCGCTCACATTTAATGTTGATGAAAGCTGGCTACAGGAAGGCCAGACGCGAATTATTTTTGATGGCGTTAACTCGGCGTTTCATCTGTGGTGCAACGGGCGCTGGGTCGGTTACGGCCAGGACAGTCGTTTGCCGTCTGAATTTGACCTGAGCGCATTTTTACGCGCCGGAGAAAACCGCCTCGCGGTGATGGTGCTGCGTTGGAGTGACGGCAGTTATCTGGAAGATCAGGATATGTGGCGGATGAGCGGCATTTTCCGTGACGTCTCGTTGCTGCATAAACCGACTACACAAATCAGCGATTTCCATGTTGCCACTCGCTTTAATGATGATTTCAGCCGCGCTGTACTGGAGGCTGAAGTTCAGATGTGCGGCGAGTTGCGTGACTACCTACGGGTAACAGTTTCTTTATGGCAGGGTGAAACGCAGGTCGCCAGCGGCACCGCGCCTTTCGGCGGTGAAATTATCGATGAGCGTGGTGGTTATGCCGATCGCGTCACACTACGTCTGAACGTCGAAAACCCGAAACTGTGGAGCGCCGAAATCCCGAATCTCTATCGTGCGGTGGTTGAACTGCACACCGCCGACGGCACGCTGATTGAAGCAGAAGCCTGCGATGTCGGTTTCCGCGAGGTGCGGATTGAAAATGGTCTGCTGCTGCTGAACGGCAAGCCGTTGCTGATTCGAGGCGTTAACCGTCACGAGCATCATCCTCTGCATGGTCAGGTCATGGATGAGCAGACGATGGTGCAGGATATCCTGCTGATGAAGCAGAACAACTTTAACGCCGTGCGCTGTTCGCATTATCCGAACCATCCGCTGTGGTACACGCTGTGCGACCGCTACGGCCTGTATGTGGTGGATGAAGCCAATATTGAAACCCACGGCATGGTGCCAATGAATCGTCTGACCGATGATCCGCGCTGGCTACCGGCGATGAGCGAACGCGTAACGCGAATGGTGCAGCGCGATCGTAATCACCCGAGTGTGATCATCTGGTCGCTGGGGAATGAATCAGGCCACGGCGCTAATCACGACGCGCTGTATCGCTGGATCAAATCTGTCGATCCTTCCCGCCCGGTGCAGTATGAAGGCGGCGGAGCCGACACCACGGCCACCGATATTATTTGCCCGATGTACGCGCGCGTGGATGAAGACCAGCCCTTCCCGGCTGTGCCGAAATGGTCCATCAAAAAATGGCTTTCGCTACCTGGAGAGACGCGCCCGCTGATCCTTTGCGAATACGCCCACGCGATGGGTAACAGTCTTGGCGGTTTCGCTAAATACTGGCAGGCGTTTCGTCAGTATCCCCGTTTACAGGGCGGCTTCGTCTGGGACTGGGTGGATCAGTCGCTGATTAAATATGATGAAAACGGCAACCCGTGGTCGGCTTACGGCGGTGATTTTGGCGATACGCCGAACGATCGCCAGTTCTGTATGAACGGTCTGGTCTTTGCCGACCGCACGCCGCATCCAGCGCTGACGGAAGCAAAACACCAGCAGCAGTTTTTCCAGTTCCGTTTATCCGGGCAAACCATCGAAGTGACCAGCGAATACCTGTTCCGTCATAGCGATAACGAGCTCCTGCACTGGATGGTGGCGCTGGATGGTAAGCCGCTGGCAAGCGGTGAAGTGCCTCTGGATGTCGCTCCACAAGGTAAACAGTTGATTGAACTGCCTGAACTACCGCAGCCGGAGAGCGCCGGGCAACTCTGGCTCACAGTACGCGTAGTGCAACCGAACGCGACCGCATGGTCAGAAGCCGGGCACATCAGCGCCTGGCAGCAGTGGCGTCTGGCGGAAAACCTCAGTGTGACGCTCCCCGCCGCGTCCCACGCCATCCCGCATCTGACCACCAGCGAAATGGATTTTTGCATCGAGCTGGGTAATAAGCGTTGGCAATTTAACCGCCAGTCAGGCTTTCTTTCACAGATGTGGATTGGCGATAAAAAACAACTGCTGACGCCGCTGCGCGATCAGTTCACCCGTGCACCGCTGGATAACGACATTGGCGTAAGTGAAGCGACCCGCATTGACCCTAACGCCTGGGTCGAACGCTGGAAGGCGGCGGGCCATTACCAGGCCGAAGCAGCGTTGTTGCAGTGCACGGCAGATACACTTGCTGATGCGGTGCTGATTACGACCGCTCACGCGTGGCAGCATCAGGGGAAAACCTTATTTATCAGCCGGAAAACCTACCGGATTGATGGTAGTGGTCAAATGGCGATTACCGTTGATGTTGAAGTGGCGAGCGATACACCGCATCCGGCGCGGATTGGCCTGAACTGCCAGCTGGCGCAGGTAGCAGAGCGGGTAAACTGGCTCGGATTAGGGCCGCAAGAAAACTATCCCGACCGCCTTACTGCCGCCTGTTTTGACCGCTGGGATCTGCCATTGTCAGACATGTATACCCCGTACGTCTTCCCGAGCGAAAACGGTCTGCGCTGCGGGACGCGCGAATTGAATTATGGCCCACACCAGTGGCGCGGCGACTTCCAGTTCAACATCAGCCGCTACAGTCAACAGCAACTGATGGAAACCAGCCATCGCCATCTGCTGCACGCGGAAGAAGGCACATGGCTGAATATCGACGGTTTCCATATGGGGATTGGTGGCGACGACTCCTGGAGCCCGTCAGTATCGGCGGAATTCCAGCTGAGCGCCGGTCGCTACCATTACCAGTTGGTCTGGTGTCAAAAATAATAATAAcggctgccgt".ToLower();

        private static String Level1RestrictionSitesAfterFusionSite = "GTTCTTTACTAGTGGAAGACAT"; //followed by fusion site and first 24
        private static String Level1ReverseRestrictionSitesAfterFusionSite = "ATGTCTTCTACTAGTAGCGGCCGC"; //after last 24 and fusion site
        

        #endregion

        #region Properties

        public List<Sites> FusionSiteLibrary
        {
            get { return _fusionSiteLibrary; }
            set { _fusionSiteLibrary = value; }
        }

              

    
        #endregion

        #region Constructors
        
        
        /// <summary>
        /// Constructor: L0 Primer Designer launched by a Part
        /// </summary>
        /// <param name="p"></param>
        public PrimerDesigner2(Part p) 
        {
            InitializeComponent();
            Sites.pd2 = this;
            Part.pd2 = this;
            L1Module.pd2 = this;
            L2Module.pd2 = this;
            //PrimerDesigner1.pd2 = this;
            SurfaceWindow1.pd2 = this;
            _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));

            _fusionSitesList = new List<Sites>();

            _partsList = new List<Part>();
            _partsList.Add(p); //save only part for primer design

            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Center = new Point(Width / 2.0, Height / 2.0);

            LevelIndicator.Text = "0";

            DestinationVectorText.Text = Level02RestrictionSitesBeforeFusionSite;

            _moduleNum = 0; //launced by Part

            addPartSiteSets(PD2_auto, p, true);
            addPartSiteSets(PD2_manual, p, true);
            
            matchSites(PD2_auto);
            matchSites(PD2_manual);

            initializeFusionSiteChecker(); //initialize array to hold all possible fusion sites to be added
        }

        /// <summary>
        /// Constructor: L1 Primer Designer launched by an L1Module
        /// </summary>
        /// <param name="l1"></param>
        public PrimerDesigner2(L1Module l1)
        {
            InitializeComponent();
            Sites.pd2 = this;
            Part.pd2 = this;
            L1Module.pd2 = this;
            L2Module.pd2 = this;
            _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));
            _fusionSitesList = new List<Sites>();
            LevelIndicator.Text = "1";
            DestinationVectorText.Text = Level1RestrictionSitesAfterFusionSite;

            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Center = new Point(Width / 2.0, Height / 2.0);

            _moduleNum = 1; //launched by L1Module

            int forLoopCounter = 0; //for keeping track of last part to add second fusion site
            _partsList = new List<Part>();
            
            foreach (UIElement elem in l1.L1Grid.Children)
            {
                if (elem.GetType() == typeof(Part))
                {
                    Part p = (Part)elem;
                    _partsList.Add(p); //save parts for primer design

                    if (forLoopCounter == 3) //4th Part being added, add second fusion site
                    {
                        addPartSiteSets(PD2_auto, p, true);
                        addPartSiteSets(PD2_manual, p, true);
                    }
                    else
                    {
                        addPartSiteSets(PD2_auto, p, false);
                        addPartSiteSets(PD2_manual, p, false);
                    }
                    forLoopCounter++;
                }
            }

            matchSites(PD2_auto); //remove duplicate fusion sites
            matchSites(PD2_manual); //remove duplicate fusion sites

            initializeFusionSiteChecker(); //initialize array to hold all possible fusion sites to be added
        }

        /// <summary>
        /// Constructor: L2 Primer Designer launched by an L2Module
        /// </summary>
        /// <param name="l2"></param>
        public PrimerDesigner2(L2Module l2)
        {
            InitializeComponent();
            Sites.pd2 = this;
            Part.pd2 = this;
            L1Module.pd2 = this;
            L2Module.pd2 = this;
            LevelIndicator.Text = "2";

            _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));
            _fusionSitesList = new List<Sites>();
            DestinationVectorText.Text = Level02RestrictionSitesBeforeFusionSite;

            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Center = new Point(Width / 2.0, Height / 2.0);

            _moduleNum = 2; //launched by L2Module
            _l2Modulel1Count = l2.Children.Count - 2; //Minus 2 for Element Menu and StackPanel, remaining are L1Modules
            
            int forLoopCounter = 0; //for keeping track of last part to add second fusion site in inner loop
            int lastL1ModuleCounter = 0; //to indicate when for loop is on last L1Module (so can add second fusion site), (l2.Children.Count - 1) to account for ElementMenu and Grid

            _partsList = new List<Part>();

            foreach (UIElement l1 in l2.Children)
            {
                if (l1.GetType() == typeof(L1Module))
                {
                    foreach (UIElement elem in ((L1Module)l1).L1Grid.Children)
                    {
                        if (elem.GetType() == typeof(Part))
                        {
                            Part p = (Part)elem;
                            _partsList.Add(p); //save parts for primer design

                            if ((forLoopCounter == 3) && (lastL1ModuleCounter == l2.Children.Count - 1)) //4th Part of last L1Module being added, add second fusion site
                            {
                                addPartSiteSets(PD2_auto, p, true);
                                addPartSiteSets(PD2_manual, p, true);
                            }
                            else
                            {
                                addPartSiteSets(PD2_auto, p, false);
                                addPartSiteSets(PD2_manual, p, false);
                            }
                            forLoopCounter++;
                        }
                    }
                    forLoopCounter = 0; //reset inner loop counter
                }
                lastL1ModuleCounter++;

            }

            matchSites(PD2_auto);
            matchSites(PD2_manual);

            //initializeFusionSiteChecker(); //initialize array to hold all possible fusion sites to be added
        }

        #endregion


        #region Constructor helpers

        /// <summary>
        /// Constructor helper: adds Parts and their fusion sites to panel
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="p"></param>
        /// <param name="isLevel0">Boolean flag implemented to indicate whether this method is called in class constructor via Level 0, 1, or 2 
        /// (to determine whether to add a second fusion site after Part -- deals with duplicate fusion site issue @T.Feng)</param>
        private void addPartSiteSets(StackPanel parent, Part p, bool isLevel0)
        {
            Part copy = p.clone();
            String seq1 = copy.SitesList.ElementAt(0).Sequence;
            String seq2 = copy.SitesList.ElementAt(1).Sequence;

            parent.Children.Add(new Sites(seq1));
            copy.ElementMenu.Items.Remove(copy.PD);
            copy.BorderBrush = p.BorderBrush;
            copy.IsManipulationEnabled = false;
            parent.Children.Add(copy);

            //only add fusion site for seq2 if in Level 0 (Part), else next Part's seq1 fusion site will do it @T.Feng
            if (isLevel0)
                parent.Children.Add(new Sites(seq2));
        }

        /// <summary>
        /// Constructor helper: checks for empty fusion sites and matches them to neighboring defined Sites, if any
        /// </summary>
        /// <param name="pan"></param>
        private void matchSites(StackPanel pan)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(pan); i++)
            {
                UIElement elem = (UIElement)VisualTreeHelper.GetChild(pan, i);
                
                //If element is Sites and empty, find its neighbor and copy data from it
                if (elem.GetType() == typeof(Sites) && ((Sites)elem).Sequence == "site")
                {
                    int sourceIndex = Sites.neighborSiteIndex(i, pan);

                    //to catch when for some reason the copySource is a Part instead of a Sites object
                    object indexedElement = VisualTreeHelper.GetChild(pan, sourceIndex);
                    if (indexedElement is Sites)
                    {
                        Sites copySource = (Sites)indexedElement;
                        ((Sites)elem).copySitesInfoFrom(copySource);
                    }
                    
                }
            }
        }

        /// <summary>
        /// Initialize SitesAdded array to proper size based on Part/L1Module/L2Module to accommodate all possible fusion sites. 
        /// Actual fusion site requirement checking done in Sites.xaml.cs during hit test
        /// </summary>
        /// @T.Feng
        private void initializeFusionSiteChecker()
        {
            switch (_moduleNum)
            {
                case 0: _sitesAdded = new Sites[2]; break;//Part has 2 fusion sites
                case 1: _sitesAdded = new Sites[5]; break;//L1Module has 5 fusion sites
                case 2: _sitesAdded = new Sites[_l2Modulel1Count * 4 + 1]; break; //L2Module has L1ModuleNum*4 + 1 fusion sites
                default: Console.WriteLine("No fusion site-added storage constructed, no info on object that launched Primer Designer. PrimerDesigner2.xaml.cs Ln 250"); break;
            }
        }

        /// <summary>
        /// Sets up the fusion site library
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContext = this;
            _fusionSiteLibrary = new List<Sites>();

            _fusionSiteLibrary.Add(new Sites("acaa"));
            _fusionSiteLibrary.Add(new Sites("atgc"));
            _fusionSiteLibrary.Add(new Sites("atgg"));
            _fusionSiteLibrary.Add(new Sites("gtca"));
            _fusionSiteLibrary.Add(new Sites("cctg"));
            _fusionSiteLibrary.Add(new Sites("ggta"));
            _fusionSiteLibrary.Add(new Sites("catt"));
            _fusionSiteLibrary.Add(new Sites("gact"));

            foreach (Sites s in _fusionSiteLibrary)
            {
                PD2_siteLibrary.Items.Add(s);
                s.Center = SurfaceWindow1.SetPosition(s);
            }
        }

        #endregion
                    

        #region Backend

        private string CreateL0L2DestinationVector(string fusionSite1, string fusionSite2)
        {
            string dvSequence = "";

            dvSequence = Level02RestrictionSitesBeforeFusionSite + fusionSite1 + Level02Bpi1AfterFusionSiteBeforeLacZ 
                            + lacZ + Level02Bpi1AfterBeforeLacZ + fusionSite2+ Level02ReverseRestrictionSitesAfterFusionSite;

            return dvSequence;
        }

        private string CreateL1DestinationVector(string fusionSite, string fusionSite2)
        {
            string dvSequence = "";

            dvSequence = Level1RestrictionSitesAfterFusionSite + fusionSite + Level02Bpi1AfterFusionSiteBeforeLacZ
                            + lacZ + Level02Bpi1AfterBeforeLacZ + fusionSite2 + Level1ReverseRestrictionSitesAfterFusionSite;

            return dvSequence;
        }
        
        //Returns a list of alignmentTest results organized by test type
        private List<double> _getTestResults()
        {
            List<double> testResults = new List<double>();
                     

            //alignmentTests at1 = new alignmentTests(_leftSeq);
            //alignmentTests at2 = new alignmentTests(_rightSeq);

            alignments = new LinkedList<String>[6];

            ////the following is for HAIRPIN RUN only!!
            ////  testResults.Add(at1.hairpinRun());
            //alignments[0] = at1.hairpinRunAlignments();
            //// testResults.Add(at2.hairpinRun());

            //alignments[1] = at2.hairpinRunAlignments();

            ////the following is for SELF DIMER only!!
            ////testResults.Add(at1.selfDimerRun());

            //alignments[2] = at2.selfDimerRunAlignments();

            //// testResults.Add(at2.selfDimerRun());
            //alignments[3] = at2.selfDimerRunAlignments();


            ////the following is for HETERODIMER only!!
            //// testResults.Add(at1.heteroDimerRun(_rightSeq));
            //alignments[4] = at2.heteroDimerRunAlignments(_rightSeq);

            //// testResults.Add(at2.heteroDimerRun(_leftSeq));
            //alignments[5] = at2.heteroDimerRunAlignments(_leftSeq);


            //Console.WriteLine(testResults);
            return testResults;
        }

        //Writes results into PD1, compares results to Gibbs free enrgy and checks forward/reverse success checkboxes accordingly
        private void _getTestResultsCallback(List<double> resultsList)
        {

            //Console.WriteLine(alignments);
            //Console.WriteLine("results SVI here!!");

            ScatterViewItem testResultsObject = new ScatterViewItem();
            ScrollViewer scroller = new ScrollViewer();
            TextBlock block = new TextBlock();
            String resultString = "";

            for (int i = 0; i < 5; i++)
            {
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

            this.ResultGrid.Visibility = Visibility.Visible;
            StackPanel activeTab = PD2_manual;
            if (PD2_buildTabs.SelectedIndex == 1) { activeTab = PD2_auto; }

            List<SurfaceCheckBox> checkList = new List<SurfaceCheckBox>();
            List<TextBlock> numList = new List<TextBlock>();


            _leftGibbsFree = calcDeltaG(_leftSeq);
            FwdGCBox.Text = calcDeltaG(_leftSeq).ToString();
            _rightGibbsFree = calcDeltaG(_rightSeq);
            RevGCBox.Text = calcDeltaG(_rightSeq).ToString();
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
        public Double calcDeltaG(String s)
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

                FwdGCBox.Text = toReturn.ToString();

                return toReturn;
            }

            FwdGCBox.Text = "0.00";
            return 0.00;
        }
        


        //Put Parts and Sites into a list. Generate string of sequences
        private void GeneratePrimers_Click(object sender, RoutedEventArgs e)
        {
            //toggle Grid to visible
            this.ResultGrid.Visibility = Visibility.Visible;
            StackPanel activeTab = PD2_manual;
            if (PD2_buildTabs.SelectedIndex == 1) { activeTab = PD2_auto; }

            //Check for validity and populate fields
            if (CheckFusionSites())
            {
                //Get current design as string -> Flanks + FS + Part + FS + Flanks
                int fscounter = 0;
                foreach (Part currentPart in _partsList)
                {
                    try
                    {
                        CompleteSequence = CompleteSequence + _fusionSitesList.ElementAt(fscounter).Sequence;

                        CompleteSequence = CompleteSequence + currentPart.myRegDS.BasicInfo.Sequence;

                        fscounter++;
                        CompleteSequence = CompleteSequence + _fusionSitesList.ElementAt(fscounter).Sequence;
                        fscounter++;
                    }
                    catch (Exception ex) { }
                }

                string leftPrimer = leftGetSeq(24, CompleteSequence);
                string rightPrimer = rightGetSeq(24, CompleteSequence);
                PopulateGCTextBoxes(leftPrimer, rightPrimer);
                PopulateLengthBoxes(leftPrimer, rightPrimer);

                ForwardPrimerSequenceBox.Text = leftPrimer;
                ReversePrimerSequenceBox.Text = rightPrimer;

                DestinationVectorText.Text = CreateDestinationVector();

            }
            else
            {
                MessageBox.Show("Couldn't generate primers. Please check for duplicate and empty fusion sites.");
            }
            
                   
            

            
        }

        private string CreateDestinationVector()
        {
            if (_moduleNum == 0)
            {
               return CreateL0L2DestinationVector(_fusionSitesList.ElementAt(0).Sequence, _fusionSitesList.Last().Sequence);
            }
            else if (_moduleNum == 1)
            {
                return CreateL1DestinationVector(_fusionSitesList.ElementAt(0).Sequence, _fusionSitesList.Last().Sequence);
            }
            else
            {
                return CreateL0L2DestinationVector(_fusionSitesList.ElementAt(0).Sequence, _fusionSitesList.Last().Sequence);
            }

        }

        private bool CheckFusionSites()
        {
            //Check sitesList for duplicates
            bool noDupes = true;
            bool noEmpty = true;
            int dupecount = 0;
            
            foreach( Sites s in _fusionSitesList)
            {                
                noEmpty = s.Sequence != "site";

                foreach (Sites fs in _fusionSitesList)
                {
                    if (fs.Sequence.Equals(s.Sequence))
                        dupecount++;
                }

                if (dupecount > 1)
                {
                    noDupes = false;
                    return false;
                }
                dupecount = 0;
            }

            return noDupes && noEmpty;
        }

        private void PopulateLengthBoxes(string leftPrimer, string rightPrimer)
        {
            //Show number of basepairs
            FwdLengthBox.Text = leftPrimer.Length.ToString();
            RevLengthBox.Text = rightPrimer.Length.ToString();

        }

        private void PopulateGCTextBoxes(string leftPrimer, string rightPrimer)
        {
            //Show percent of gc bases
            double countForward = (double)(leftPrimer.Split('c').Length + leftPrimer.Split('g').Length - 2);
            double countReverse = (double)(rightPrimer.Split('c').Length + rightPrimer.Split('g').Length - 2);
            double percentForward = Math.Round(100 * (countForward / ((double)leftPrimer.Length)), 3);
            double percentReverse = Math.Round(100 * (countReverse / ((double)rightPrimer.Length)), 3);
            //Just make it show for now
            FwdGCBox.Text = percentForward.ToString() + "%";
            RevGCBox.Text = percentReverse.ToString() + "%";
        }

        private void PrintPrimersToFileCallback(string file)
        {
            if(file != "FAIL")
                MessageBox.Show("Primers have been printed to a text file at location: " + file);
            else
                MessageBox.Show("Primers were not saved. Please check design for errors.");
        }


        private string PrintPrimersToFile()
        {
            String site1seq = "";
            String site2seq = "";
            String textTitle = "primer" + System.DateTime.Today.ToString();
            textTitle = textTitle.Replace("/", "-"); //originally uses / which doesn't work with file names...
            textTitle = textTitle.Replace(":", "-");
            try
            {
                //Get current working directory
                string file = Directory.GetCurrentDirectory();
                //change directory to EugeneFiles directory and read text file based on ListModulesToPermute count
                file = file.Substring(0, file.IndexOf("bin")) + @"PrimerResults/" + textTitle + ".csv";
                StreamWriter writer = new StreamWriter(file);

                //Forward
                writer.WriteLine("Forward Name," + FwdPrimerName.Text);
                writer.WriteLine("\t# of Base Pairs," + ForwardPrimerSequenceBox.Text.Length);
                writer.WriteLine(Level02RestrictionSitesBeforeFusionSite + site1seq + ForwardPrimerSequenceBox.Text);

                //Reverse
                writer.WriteLine("Reverse,");
                writer.WriteLine("\t# of Base Pairs," + ReversePrimerSequenceBox.Text.Length);
                //Convert RString and Site sequence in 3' to 5', to match _rightPrimer, which is the last ~24 bases in 3' to 5'
                String RString3to5 = new String(Level02ReverseRestrictionSitesAfterFusionSite.ToCharArray().ToArray());
                String site2seq3to5 = new String(site2seq.ToCharArray().Reverse().ToArray());
                String reverse3to5 = RString3to5 + site2seq3to5 + ReversePrimerSequenceBox.Text;
                writer.WriteLine(reverse3to5);

                //Reverse complement
                writer.WriteLine("Reverse Complement Name," + RevPrimerName.Text);
                writer.WriteLine("\t# of Base Pairs," + ReversePrimerSequenceBox.Text.Length);
                //Transform reverse3to5 into its complement
                writer.WriteLine(Transform(reverse3to5));

                writer.WriteLine("Forward Name," + FwdPrimerName.Text);
                writer.WriteLine("Reverse Name," + RevPrimerName.Text);
                writer.WriteLine("Complete Sequence," + ForwardPrimerSequenceBox.Text + ReversePrimerSequenceBox.Text);
                writer.WriteLine("Forward Sequence," + ForwardPrimerSequenceBox.Text);
                writer.WriteLine("\t# of Base Pairs," + ForwardPrimerSequenceBox.Text.Length);
                writer.WriteLine("Reverse Sequence," + ReversePrimerSequenceBox.Text);
                writer.WriteLine("\t# of Base Pairs," + ReversePrimerSequenceBox.Text.Length);

                writer.Close();

                return file;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "FAIL";
            }
        }


        //Transform string into its complement
        private string Transform(String s)
        {
            String complement1 = "";
            char[] array = s.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                if (c == 'a') complement1 += 't';
                if (c == 't') complement1 += 'a';
                if (c == 'g') complement1 += 'c';
                if (c == 'c') complement1 += 'g';
                if (c == '-') complement1 += '-';
            }
            return complement1;
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Enables touch on tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_TouchDown(object sender, TouchEventArgs e)
        {
            TabItem tab = (TabItem)sender;
            PD2_buildTabs.SelectedItem = tab;
            e.Handled = true;
        }

        /// <summary>
        /// Removes pd2 from sw1 and ### WILL #### saves Sites data to Part
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel sp = PD2_manual;
            if (PD2_buildTabs.SelectedIndex == 1) sp = PD2_auto;

            foreach (UIElement elem in sp.Children)
            {
                if (elem.GetType() == typeof(Part))
                {
                    ((Part)elem).updateSites(sp);
                }
            }
            sw1.SW_SV.Items.Remove(this);
            
        }

        private void showProgressBar()
        {
            ProgressIndicator.Visibility = Visibility.Visible;
        }

        private void hideProgressBar()
        {
            ProgressIndicator.Visibility = Visibility.Collapsed;
        }
        
        /// <summary>
        /// Adds an editable fusion site template to the library
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void siteAdder_Click(object sender, RoutedEventArgs e)
        {
            Sites template = new Sites();
            template.CircleText.IsReadOnly = false;
            template.Height = 50;
            template.Width = 50;
            PD2_siteLibrary.Items.Add(template);
            template.Center = SurfaceWindow1.SetPosition(template);
        }

        
        /// <summary>
        /// Enables touch on the build tabs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PD2_buildTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabc = (TabControl)sender;
            //if manual is selected then
            if (tabc.SelectedIndex == 0)
            {
                permMaker.IsEnabled = false;
                permMaker.Visibility = Visibility.Collapsed;
            }
            else
            //if automatic is selected
            {
                permMaker.IsEnabled = true;
                permMaker.Visibility = Visibility.Visible;
            }
        }

        
        /// <summary>
        /// Store used indices in a list to check
        /// Needs more complex checks: first/last sites of L1Ms must be unique, but others only need to be unique within the given L1M
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRandomFusionSites_Click(object sender, RoutedEventArgs e)
        {
            permMaker.IsEnabled = false;

            try
            {
                //Set usedSites to empty List and clear all Sites if necessary
                usedSites = new List<int>();
                foreach (UIElement elem in PD2_auto.Children)
                {
                    if (elem.GetType() == typeof(Sites))
                    {
                        ((Sites)elem).copySitesInfoFrom(new Sites());
                    }
                }

                foreach (UIElement elem in PD2_auto.Children)
                {
                    //If the element is Sites and an empty placeholder
                    if (elem.GetType() == typeof(Sites))
                    {
                        if (((Sites)elem).Sequence == "site")
                        {
                            //Get a random index and checks if it's already used
                            int randIndex = (new Random()).Next(0, PD2_siteLibrary.Items.Count);
                            while (usedSites.Contains(randIndex))
                            {
                                randIndex = (new Random()).Next(0, PD2_siteLibrary.Items.Count);
                            }

                            //Copy in data from a random Site in the library
                            Sites current = (Sites)elem;
                            current.copySitesInfoFrom((Sites)PD2_siteLibrary.Items.GetItemAt(randIndex));

                            //Double the Site
                            //int nextIndex = PD2_auto.Children.IndexOf(current) + 2;
                            //Sites next = (Sites)VisualTreeHelper.GetChild(PD2_auto, nextIndex);
                            //next.copySitesInfoFrom(current);

                            usedSites.Add(randIndex);
                           
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            foreach (int r in usedSites)
            {
                Sites current = new Sites();
                current.copySitesInfoFrom((Sites)PD2_siteLibrary.Items.GetItemAt(r));
                _fusionSitesList.Add(current); //maintains order as long as it's random...
            }
            permMaker.IsEnabled = true;

        }

        //Show Eugene Checklist
        private void ViewEugeneChecklist_Click(object sender, RoutedEventArgs e)
        {
            this.EugeneChecklist.Visibility = Visibility.Visible; 
        }

        private void EugeneOk_Click(object sender, RoutedEventArgs e)
        {
            this.EugeneChecklist.Visibility = Visibility.Collapsed;
        }

        private void SavePrimers_Click(object sender, RoutedEventArgs e)
        {
            //_progressBarWrapper.execute<string>(PrintPrimersToFile, PrintPrimersToFileCallback);
            _progressBarWrapper.Show();
            string temp = PrintPrimersToFile();
            _progressBarWrapper.Hide();
            PrintPrimersToFileCallback(temp);
        }



        private void ViewCompleteSequence_Click(object sender, RoutedEventArgs e)
        {
            ScatterViewItem testResultsObject = new ScatterViewItem();
            ScrollViewer scroller = new ScrollViewer();
            TextBlock block = new TextBlock();
            
            block.Text = CompleteSequence;

            scroller.Content = block;


            testResultsObject.Content = scroller;

            sw1.SW_SV.Items.Add(testResultsObject);
        }

        #endregion
    }
}
