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
using System.Windows.Interop;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for Part.xaml
    /// </summary>
    /// 

    public partial class Part : ScatterViewItem
    {

        public static SurfaceWindow1 sw1;
        public static Level0 l0; //To reference L0 properties before sw1 gets set
        public static PrimerDesigner1 pd1;
        public static PrimerDesigner2 pd2;
        private string _type; //Gets L0's global partTypeSelected variable //Might not need this, since you can only move type A parts when pTS = A
        private Point Original; //Stores position at ContainerManipulationStarted
        private Part myClone; //Reference to its clone for Part_ContainerManipulationCompleted
        private L1Module targetL1M; //Its drop-target during manual L1Module construction
        private RegDataSheet _myRegDS; //RegDataSheet containing information about this Part for searches
        private RegDataSheet _thisRegDS; //RegDataSheet containing information about this Part for filters


        private Part mySource; //Reference to original for updating Sites information
        private List<Sites> _sitesList;
        private Boolean _isCustom;

        private Thread _dataSheet;
        private Thread _Publist;
        private ProgressBarWrapper _progressBarWrapper;

        #region Properties
        public String Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public RegDataSheet myRegDS
        {
            get { return _myRegDS; }
            set { _myRegDS = value; }
        }

        public RegDataSheet thisRegDS
        {
            get { return _thisRegDS; }
            set { _thisRegDS = value; }
        }

        public Boolean isCustom
        {
            get { return _isCustom; }
        }

        public SurfaceWindow1 MySurfaceWindow1
        {
            set
            { sw1 = value; }
        }

        public PrimerDesigner2 MyPrimerDesigner2
        {
            set { pd2 = value; }
        }

        public Part MyClone
        {
            get { return myClone; }
            set { myClone = value; }
        }

        public List<Sites> SitesList
        {
            get { return _sitesList; }
            set { _sitesList = value; }
        }
        #endregion

        //Basic constructor; determines type from L0's top nav bar
        public Part() : this(l0.PartTypeSelected) {}

        //Constructor for searches unbounded by part types
        public Part(String partType) : this(partType, false)
        {

        }

        public Part(String partType, Boolean IsCustom)
        {
            InitializeComponent();
            Orientation = 0;
            CanScale = false;
            CanRotate = false;
            _isCustom = IsCustom;
            _sitesList = new List<Sites>{new Sites(), new Sites()};

            //if (sw1 != null)
            //{
                _type = partType;
                if (_type == "prom")
                {
                    imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_prom2);
                    //Background = Brushes.Orange;
                }
                else if (_type == "rbs")
                {
                    imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_rbs2);
                    //Background = Brushes.DodgerBlue;
                }
                else if (_type == "cds")
                {
                    imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_cds2);
                    _sitesList.ElementAt(0).copySitesInfoFrom(new Sites("aatg"));
                    //Background = Brushes.Green;
                }
                else //type == "term"
                {
                    imgType.Source = SurfaceWindow1.BitmapToImageSource(Resource1.sbol_term2);
                    //Background = Brushes.Red;
                }

                _myRegDS = new RegDataSheet();

                _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));

                //Stop here if Custom Part - do not need dummy sequence
                if (IsCustom) return;

                _myRegDS.BasicInfo.Sequence = "atgcatgctagcatccattacgatccgtcag";
        }

        private void showProgressBar()
        {
            SurfaceWindow1.getProgressIndicator(this).Visibility = Visibility.Visible;
        }

        private void hideProgressBar()
        {
            SurfaceWindow1.getProgressIndicator(this).Visibility = Visibility.Collapsed;
        } 

        #region Element Menu

        //Checks if ElementMenuItems will be cut off; shifts right to prevent cut
        private void ElementMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                ScatterView sv = (ScatterView)this.Parent;

                if (Center.X < Width * 1.5)
                {
                    foreach (ElementMenuItem item in ElementMenu.Items)
                    {
                        item.Margin = new Thickness(225, -60, 0, 0);
                    }
                }
                else if (Center.X > sv.Width - Width * 1.5)
                {
                    foreach (ElementMenuItem item in ElementMenu.Items)
                    {
                        item.Margin = new Thickness(0, -60, 50, 0);
                    }
                }
            }
            catch (Exception exc) { Console.WriteLine("Exception: " + exc); }
        }

        ScatterViewItem seq; //The Scatterviewitem generated when you click the "Sequence" element in the element menu
        MenuDataSheet ds; //The Scatterviewitem generated when you click the "MenuDataSheet" element in the element menu

        //Allows trashing of the sequence SVI when you swipe it far enough to the right or left
        private void onSeqManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            sw1.swipeToDelete((ScatterViewItem)sender);
        }

        //Allows trashing of the sequence SVI when you swipe it far enough to the right or left
        private void onDSManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            sw1.swipeToDelete((ScatterViewItem)sender);

        }

        private void absBox_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            sw1.swipeToDelete((ScatterViewItem)sender);
        }

        //Method for generating the Sequence scatterview item from the element menu
        private void select_Sequence(object sender, RoutedEventArgs e)
        {

            _thisRegDS = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + this.partName.Text);    
            
            seq = new ScatterViewItem();
                seq.CanRotate = false;
                seq.CanScale = false; 
                TextBlock seqtext = new TextBlock(); //Creates a textblock to be inserted into the scatterview
                
                seqtext.Margin = new Thickness(10); 
                seq.Background = Brushes.SteelBlue;
                seqtext.Background = Brushes.White;

                seq.Width = 800;
                seq.MinHeight = 600;
                seqtext.Width = 790;
                seqtext.MinHeight = 590;
                

                seqtext.Text = "Parts Registry ID: " + _thisRegDS.Name + "\n" + "\n" + "Sequence:" + "\n" + "\n" + _thisRegDS.BasicInfo.Sequence;
                seq.Content = seqtext;

                SurfaceWindow1.addData(sender, seq);
                seq.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(onSeqManipulationCompleted); //Event handler that allows trashing when swiped to the right
                seq.Orientation = 0;

                //Overwrites center calculated in addData(); consider checking out later
                Point PartCenter = this.Center;
                Point OriginalCenter = seq.Center;
                OriginalCenter.X = (PartCenter.X);
                OriginalCenter.Y = (PartCenter.Y + 370);

                seq.Center = OriginalCenter;
            }

        private void select_PrimerDesigner(object sender, RoutedEventArgs e)
        {
            getDataForPrimerDesigner();
            Part p = clone();
            p.PD.IsEnabled = false;
            p.PD.Visibility = Visibility.Collapsed;
            sw1.SW_SV.Items.Add(new PrimerDesigner2(p));

            //if (_myRegDS.Name != "test") //Already got data from registry, so don't make new one
            //{
            //    Part p = clone();
            //    p.PD.IsEnabled = false;
            //    p.PD.Visibility = Visibility.Collapsed;
            //    sw1.SW_SV.Items.Add(new PrimerDesigner2(p));
            //}
            //else
            //{
            //    Func<String, RegDataSheet> _getRegDataSheet =
            //    delegate(String pName) { return new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + pName); };
            //    Action<RegDataSheet> _getRegDataSheetCallback = delegate(RegDataSheet _regDS)
            //    {
            //        _myRegDS = _regDS;
            //        Part p = clone();
            //        p.PD.IsEnabled = false;
            //        p.PD.Visibility = Visibility.Collapsed;
            //        sw1.SW_SV.Items.Add(new PrimerDesigner2(p));
            //    };
            //    _dataSheet = _progressBarWrapper.execute<String, RegDataSheet>(_getRegDataSheet, partName.Text, _getRegDataSheetCallback);
            //}
        }

        //Primer Designer helper method: gets and sets data from Registry for Parts if not yet requested
        //Must be accessible to L1 modules/trans. units and L2 modules/multigene constructs
        //DOESN'T WORK: only seems to work when in above event handler; doesn't have a step where _regDS is returned, if _regDS was created at all
        public void getDataForPrimerDesigner()
        {
            if (_myRegDS.Name != "test") //Already got data from registry, so don't make new one
            {
            }
            else
            {
                _myRegDS = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + partName.Text);
                //Func<String, RegDataSheet> _getRegDataSheet =
                //delegate(String pName) { return new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + pName); };
                //Action<RegDataSheet> _getRegDataSheetCallback = delegate(RegDataSheet _regDS)
                //{
                //    _myRegDS = _regDS;
                //};
                //_dataSheet = _progressBarWrapper.execute<String, RegDataSheet>(_getRegDataSheet, partName.Text, _getRegDataSheetCallback);
            }
        }

        private PubAbstract abs;
        private TextBlock PubTitles;
        private SurfaceListBoxItem item;
        private PubList result;

        private void select_DataSheet(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //_thisRegDS = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + this.partName.Text);

            //If Part is custom, use customDataSheet instead of MenuDataSheet
            //customDS has only fields that are relevant to custom Parts (e.g. no Registry ID)
            if (_isCustom)
            {
                customDataSheet customDS = new customDataSheet(this);
                customDS.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(onDSManipulationCompleted);
                SurfaceWindow1.addData(sender, customDS);
                return;
            }

            ds = new MenuDataSheet();
            ds.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(onDSManipulationCompleted);

            if (_myRegDS.Name != "test") //Already got data from registry, so just populate from it
            {
                #region Populating the data sheet's datasheet
                //Specifies the Length of the Sequence on the Sequence tab of the data sheet
                int Length = _myRegDS.BasicInfo.Length;
                TextBlock LengthBlock = new TextBlock();
                //ds.SeqTab.Children.Add(LengthBlock);
                LengthBlock.VerticalAlignment = VerticalAlignment.Top;
                LengthBlock.Text = "Length: " + Length.ToString();

                //This is all pulling information from the Reg Data Sheet associated with the part and inserting it in the data sheet
                ds.PopulateDataSheet(_myRegDS.Name, 0, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.BasicInfo.DescriptionName, 1, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.Type, 2, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.Promoter.getReg(), 3, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.BasicInfo.Availability, 4, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.BasicInfo.Usefulness, 5, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.Description.assembCompString(), 6, 1, ds.DataSheet, "datasheet");
                ds.PopulateDataSheet(_myRegDS.Description.chassisString(), 7, 1, ds.DataSheet, "datasheet");

                

                //Specific to sequence tab
                ds.PopulateDataSheet(_myRegDS.BasicInfo.Sequence, 1, 1, ds.SeqTab, "seq");
                ds.PopulateDataSheet(Length.ToString(), 0, 1, ds.SeqTab, "length");

                //Specific to AuthorInfo Tab
                ds.PopulateDataSheet(_myRegDS.Reference.Author, 0, 1, ds.AuthorInfo, "author");
                ds.PopulateDataSheet(_myRegDS.Reference.Group, 1, 1, ds.AuthorInfo, "author");
                ds.PopulateDataSheet(_myRegDS.Reference.Date, 2, 1, ds.AuthorInfo, "author");

                if (_myRegDS.Type == "rbs")
                {
                    RowDefinition rowDef1 = new RowDefinition();
                    ds.DataSheet.RowDefinitions.Add(rowDef1);
                    ds.PopulateDataSheet(_myRegDS.Rbs.FamilyName, 8, 1, ds.DataSheet);
                    TextBlock RBSFam = new TextBlock();
                    RBSFam.Text = "Family";
                    ds.DataSheet.Children.Add(RBSFam);
                    Grid.SetRow(RBSFam, 8);
                }
                else if (_myRegDS.Type == "terminator")
                {
                    RowDefinition rowDef1 = new RowDefinition();
                    RowDefinition rowDef2 = new RowDefinition();
                    RowDefinition rowDef3 = new RowDefinition();
                    RowDefinition rowDef4 = new RowDefinition();

                    ds.DataSheet.RowDefinitions.Add(rowDef1);
                    ds.DataSheet.RowDefinitions.Add(rowDef2);
                    ds.DataSheet.RowDefinitions.Add(rowDef3);
                    ds.DataSheet.RowDefinitions.Add(rowDef4);

                    TextBlock Direction = new TextBlock();
                    Direction.Text = "Direction";
                    Direction.FontSize = 18;
                    ds.DataSheet.Children.Add(Direction);
                    Grid.SetRow(Direction, 8);

                    ds.PopulateDataSheet(_myRegDS.Terminators.Direction, 8, 1, ds.DataSheet);
                    ds.PopulateDataSheet(_myRegDS.Terminators.ForwardEff, 9, 1, ds.DataSheet);
                    ds.PopulateDataSheet(_myRegDS.Terminators.ReversedEff, 10, 1, ds.DataSheet);
                    ds.PopulateDataSheet(_myRegDS.Terminators.ReversedVers, 11, 1, ds.DataSheet);
                }

                #endregion

                #region Populating Publications
                Func<String, PubList> _GetPublications =
                        delegate(String id) { return new PubList(id); };
                Action<PubList> callback = delegate(PubList result)
                {
                    //_thisRegDS = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + this.partName.Text);

                    int m = 0;

                    if (result.Titles.Count == 0)
                    {
                        int Count = result.Titles.Count;
                        SurfaceListBoxItem NoResults = new SurfaceListBoxItem();
                        TextBlock Content = new TextBlock();
                        Content.Text = "No Results Found";
                        NoResults.Content = Content;
                        ds.Publictions.Items.Add(NoResults);
                    }

                    if (result.Titles.Count > 0)
                    {
                        int TotalArticles = result.Titles.Count;
                        SurfaceListBoxItem ArticleCount = new SurfaceListBoxItem();
                        TextBlock CountContent = new TextBlock();
                        CountContent.Text = "Results Found: " + TotalArticles as string;
                        ArticleCount.Content = CountContent;
                        ds.Publictions.Items.Add(ArticleCount);
                    }

                    foreach (String Titles in result.Titles)
                    {

                        //To Test if titles are actually adding
                        SurfaceListBoxItem item = new SurfaceListBoxItem();
                        TextBlock PubTitles = new TextBlock();
                        item.Selected += new RoutedEventHandler(item_Selected);
                        item.Content = Titles;
                        PubTitles.Tag = result.Links.ElementAt(m);
                        //item.Tag = result.Authors.ElementAt(m); 
                        ds.Publictions.Items.Add(item);
                        RowDefinition rowDef1 = new RowDefinition();
                        RowDefinition rowDef2 = new RowDefinition();
                        RowDefinition rowDef3 = new RowDefinition();
                        RowDefinition rowDef4 = new RowDefinition();

                        abs = new PubAbstract(PubTitles.Tag as string, result.getAuthors());

                        item.Tag = "Title:  " + item.Content as string + "\r\n" + "\r\n" + "\r\n"

                            + "Authors:   " + result.Authors.ElementAt(m) as string + "\r\n" + "\r\n"

                            + "Abstract:" + "\r\n" + "\r\n"

                            + abs.getAbstract() as string;

                        m += 1;
                        Console.WriteLine(m);

                        if (m > 20)
                            break;
                    }

                    /*foreach (String Titles in result.Titles)
                    {
                        TextBlock Middle = new TextBlock();
                        Middle.Text = Titles;
                        //ds.Publications.Children.Add(Middle);
                        Grid.SetRow(Middle, m);
                        m += 1; 
            #endregion
            //publications location maybe
            //Creates a list of PubMed source related to the query

                    }*/
                };
                _Publist = _progressBarWrapper.execute<String, PubList>(_GetPublications, _myRegDS.BasicInfo.DescriptionName, callback);
        #endregion
                SurfaceWindow1.addData(sender, ds);
            }
            else
            {
                Func<String, RegDataSheet> _getRegDataSheet =
                delegate(String pName) { return new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + pName); };
                Action<RegDataSheet> _getRegDataSheetCallback = delegate(RegDataSheet _regDS)
                {
                    _myRegDS = _regDS;
                    #region Populating the data sheet's datasheet
                    //Specifies the Length of the Sequence on the Sequence tab of the data sheet
                    int Length = _myRegDS.BasicInfo.Length;
                    TextBlock LengthBlock = new TextBlock();
                    //ds.SeqTab.Children.Add(LengthBlock);
                    LengthBlock.VerticalAlignment = VerticalAlignment.Top;
                    LengthBlock.Text = "Length: " + Length.ToString();

                    //This is all pulling information from the Reg Data Sheet associated with the part and inserting it in the data sheet
                    ds.PopulateDataSheet(_myRegDS.Name, 0, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.BasicInfo.DescriptionName, 1, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.Type, 2, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.Promoter.getReg(), 3, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.BasicInfo.Availability, 4, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.BasicInfo.Usefulness, 5, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.Description.assembCompString(), 6, 1, ds.DataSheet, "datasheet");
                    ds.PopulateDataSheet(_myRegDS.Description.chassisString(), 7, 1, ds.DataSheet, "datasheet");

                    //Specific to sequence tab
                    ds.PopulateDataSheet(_myRegDS.BasicInfo.Sequence, 1, 1, ds.SeqTab, "seq");
                    ds.PopulateDataSheet(Length.ToString(), 0, 1, ds.SeqTab, "length");

                    //Specific to AuthorInfo Tab
                    ds.PopulateDataSheet(_myRegDS.Reference.Author, 0, 1, ds.AuthorInfo, "author");
                    ds.PopulateDataSheet(_myRegDS.Reference.Group, 1, 1, ds.AuthorInfo, "author");
                    ds.PopulateDataSheet(_myRegDS.Reference.Date, 2, 1, ds.AuthorInfo, "author");

                    if (_myRegDS.Type == "rbs")
                    {
                        RowDefinition rowDef1 = new RowDefinition();
                        ds.DataSheet.RowDefinitions.Add(rowDef1);
                        ds.PopulateDataSheet(_myRegDS.Rbs.FamilyName, 8, 1, ds.DataSheet);
                        TextBlock RBSFam = new TextBlock();
                        RBSFam.Text = "Family";
                        RBSFam.FontSize= 14;
                        RBSFam.FontWeight = FontWeights.Bold;
                        RBSFam.VerticalAlignment = VerticalAlignment.Top;
                        // COME BACK TO THIS LATER

                        ds.DataSheet.Children.Add(RBSFam);
                        Grid.SetRow(RBSFam, 8);
                    }
                    else if (_myRegDS.Type == "terminator")
                    {
                        RowDefinition rowDef1 = new RowDefinition();
                        RowDefinition rowDef2 = new RowDefinition();
                        RowDefinition rowDef3 = new RowDefinition();
                        RowDefinition rowDef4 = new RowDefinition();

                        ds.DataSheet.RowDefinitions.Add(rowDef1);
                        ds.DataSheet.RowDefinitions.Add(rowDef2);
                        ds.DataSheet.RowDefinitions.Add(rowDef3);
                        ds.DataSheet.RowDefinitions.Add(rowDef4);

                        TextBlock Direction = new TextBlock();
                        Direction.Text = "Direction";
                        Direction.FontSize = 18;
                        ds.DataSheet.Children.Add(Direction);
                        Grid.SetRow(Direction, 8);

                        ds.PopulateDataSheet(_myRegDS.Terminators.Direction, 8, 1, ds.DataSheet);
                        ds.PopulateDataSheet(_myRegDS.Terminators.ForwardEff, 9, 1, ds.DataSheet);
                        ds.PopulateDataSheet(_myRegDS.Terminators.ReversedEff, 10, 1, ds.DataSheet);
                        ds.PopulateDataSheet(_myRegDS.Terminators.ReversedVers, 11, 1, ds.DataSheet);
                    }

                    #endregion

                    #region Populating Publications
                    Func<String, PubList> _GetPublications =
                            delegate(String id) { return new PubList(id); };
                    Action<PubList> callback = delegate(PubList result)
                    {
                        //_thisRegDS = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:" + this.partName.Text);

                        int m = 0;

                        if (result.Titles.Count == 0)
                        {
                            int Count = result.Titles.Count;
                            SurfaceListBoxItem NoResults = new SurfaceListBoxItem();
                            TextBlock Content = new TextBlock();
                            Content.Text = "No Results Found";
                            NoResults.Content = Content;
                            ds.Publictions.Items.Add(NoResults);
                        }

                        if (result.Titles.Count > 0)
                        {
                            int TotalArticles = result.Titles.Count;
                            SurfaceListBoxItem ArticleCount = new SurfaceListBoxItem();
                            TextBlock CountContent = new TextBlock();
                            CountContent.Text = "Results Found: " + TotalArticles as string;
                            ArticleCount.Content = CountContent;
                            ds.Publictions.Items.Add(ArticleCount);
                        }

                        foreach (String Titles in result.Titles)
                        {

                            //To Test if titles are actually adding
                            SurfaceListBoxItem item = new SurfaceListBoxItem();
                            TextBlock PubTitles = new TextBlock();
                            item.Selected += new RoutedEventHandler(item_Selected);
                            item.Content = Titles;
                            PubTitles.Tag = result.Links.ElementAt(m);
                            //item.Tag = result.Authors.ElementAt(m); 
                            ds.Publictions.Items.Add(item);
                            RowDefinition rowDef1 = new RowDefinition();
                            RowDefinition rowDef2 = new RowDefinition();
                            RowDefinition rowDef3 = new RowDefinition();
                            RowDefinition rowDef4 = new RowDefinition();

                            abs = new PubAbstract(PubTitles.Tag as string, result.getAuthors());

                            item.Tag = "Title:  " + item.Content as string + "\r\n" + "\r\n" + "\r\n"

                                + "Authors:   " + result.Authors.ElementAt(m) as string + "\r\n" + "\r\n"

                                + "Abstract:" + "\r\n" + "\r\n"

                                + abs.getAbstract() as string;

                            m += 1;
                            Console.WriteLine(m);

                            if (m > 20)
                                break;
                        }

                        /*foreach (String Titles in result.Titles)
                        {
                            TextBlock Middle = new TextBlock();
                            Middle.Text = Titles;
                            //ds.Publications.Children.Add(Middle);
                            Grid.SetRow(Middle, m);
                            m += 1; 
                #endregion
                //publications location maybe
                //Creates a list of PubMed source related to the query

                        }*/
                    };
                    _Publist = _progressBarWrapper.execute<String, PubList>(_GetPublications, _myRegDS.BasicInfo.DescriptionName, callback);
        #endregion
                    SurfaceWindow1.addData(sender, ds);
                };
                _dataSheet = _progressBarWrapper.execute<String, RegDataSheet>(_getRegDataSheet, partName.Text, _getRegDataSheetCallback);
            }
        }

        private void item_Selected(Object sender, RoutedEventArgs e)
        {
            try
            {
                SurfaceListBoxItem i = sender as SurfaceListBoxItem;

                //abs = new PubAbstract(result.Links.ElementAt(ds.Publictions.Items.IndexOf(i)), result.getAuthors());

                ScatterViewItem absBox = new ScatterViewItem();
                absBox.CanRotate = false;
                absBox.CanScale = false; 
                TextBlock abstext = new TextBlock();
                abstext.Text = i.Tag as string;

                SurfaceWindow1.addData(sender, absBox);
                
                absBox.Content = abstext; 
                
                //absBox.absgrid.Children.Add(abstext);

                
                absBox.Background = Brushes.SteelBlue;
                abstext.Background = Brushes.White;
                abstext.Margin = new Thickness(10);

                absBox.Width = 800;
                absBox.MinHeight = 600; 
                abstext.Width = 780;
                abstext.MinHeight = 580; 

                absBox.ContainerManipulationCompleted += new ContainerManipulationCompletedEventHandler(absBox_ContainerManipulationCompleted);
                
                /*TextBlock Title = new TextBlock();
                Title.Text = i.Content as string;
                absBox.absgrid.Children.Add(Title);
                Grid.SetRow(Title, 0);
                Grid.SetColumn(Title, 1);

                TextBlock Authors = new TextBlock();
                Authors.Text = result.Authors.ElementAt(ds.Publictions.Items.IndexOf(i));
                absBox.absgrid.Children.Add(Title);
                Grid.SetRow(Authors, 1);
                Grid.SetColumn(Authors, 1);

                TextBlock Journal = new TextBlock();
                Journal.Text = abs.getJournal();
                absBox.absgrid.Children.Add(Journal);
                Grid.SetRow(Journal, 2);
                Grid.SetColumn(Journal, 1); 
            

                ds.Publictions.Items.IndexOf(i); 

                /*ScatterViewItem abstractbox = new ScatterViewItem();
                TextBlock abstext = new TextBlock();
                SurfaceListBoxItem i = sender as SurfaceListBoxItem;
                abstext.Text = i.Tag as string; 
                abstractbox.Content = abstext;*/

                sw1.L0.L0_SV.Items.Add(absBox);
            }
            catch (Exception exc) { Console.WriteLine(exc); }
        }

        #endregion

        #region DRAG AND DROP PARTS INTO AND WITHIN L1

        //Saves starting point of the manipulation to determine if user wants drag/clone or contact/ElementMenu
        //Also to determine where to place the clone if drag/clone occurs
        private void Part_ContainerManipulationStarted(object sender, ContainerManipulationStartedEventArgs e)
        {
            Original = this.Center;
        }

        //Differentiates ElementMenu interaction from drag/drop interaction
        //Once minimum distance met, create copy to leave in original spot
        private void Part_ContainerManipulationDelta(object sender, ContainerManipulationDeltaEventArgs e)
        {
            try
            {
                Point x = this.Center;
                if (Math.Abs(Original.Y - x.Y) > 20 || Math.Abs(Original.X - x.X) > 20)
                {
                    Part p = sender as Part;
                    myClone = p.clone();
                    myClone.Center = Original; //Put clone in original's place
                    myClone.BorderBrush = p.BorderBrush;

                    ScatterView parent = p.Parent as ScatterView;
                    parent.Items.Add(myClone);

                    if (parent.Name == "L0_resultsSV")
                    { //Check if Level0 and give clone L0-appropriate behavior (e.g. dimming/disabling)
                        myClone.Opacity = 0.5; //Visually indicate not selectable
                        myClone.IsManipulationEnabled = false; //Disable manipulation until original's fate determined
                    }
                    else
                    { //Moving Part from L1 parts holder to L1Module build tabs; needs to be in L1_SV during transfer
                        p.changeParents_SV(parent, sw1.L1.L1_SV);
                    }

                    //Prevent it from continuously creating copies
                    p.ContainerManipulationDelta -= Part_ContainerManipulationDelta;
                }
            }
            catch (Exception exc) { Console.WriteLine("Part Delta \n" + exc); }
        }

        //Determines user intent by checking location of drop against threshold and whether a clone exists (i.e. drag intent)
        private void Part_ContainerManipulationCompleted(object sender, ContainerManipulationCompletedEventArgs e)
        {
            Part p = sender as Part;
            ScatterView parent = p.Parent as ScatterView;

            try
            {
                if (!(myClone == null))
                { //Clone has been created, indicating drag interaction
                    if (parent == null)
                    {
                        sw1.L0.L0_resultsSV.Items.Add(p);
                        p.PartInL0();
                        sw1.L0.L0_resultsSV.Items.Remove(p);
                    }
                    else
                    {
                        if (parent.Name == "L0_resultsSV")
                        { //Drops Part into L1
                            p.PartInL0();
                        }
                        else
                        { //Drops Part in L1Module
                            p.PartInL1();
                        }
                        parent.Items.Remove(p);
                    }
                }
                else
                {
                    if (parent == null) return; //In Primer Designer's StackPanel

                    Center = Original; //Reset center so non-drag interactions don't mess up positioning
                    if (parent.Name == "L1_prom" || parent.Name == "L1_rbs" || parent.Name == "L1_cds" || parent.Name == "L1_term")
                    { //If in an L1 palette
                        if (p.BorderBrush != Brushes.Gray)
                        {
                            //Restore original border
                            p.BorderBrush = Brushes.Gray;
                        }
                        else
                        {
                            p.BorderBrush = Brushes.Navy;
                        } //highlights border
                    }
                }
            }
            catch (Exception exc) { Console.WriteLine("Part Completed \n" + exc); }
        }

        //Places clone of dragged part into L1 if below threshold
        private void PartInL0()
        {
            try
            {
                double yL1 = sw1.L1.Center.Y;
                double yThreshold = yL1 - sw1.L1.Height / 2 - 180;

                //Check if user is dropping or dumping
                if (Center.Y > yThreshold)
                { //Create clone, check Part type, and place in appropriate L1 partsbox
                    Part L1clone = clone();

                    if (_type == "prom")
                    { sw1.L1.L1_prom.Items.Add(L1clone); }
                    else if (_type == "rbs")
                    { sw1.L1.L1_rbs.Items.Add(L1clone); }
                    else if (_type == "cds")
                    { sw1.L1.L1_cds.Items.Add(L1clone); }
                    else if (_type == "term")
                    { sw1.L1.L1_term.Items.Add(L1clone); }

                    L1clone.Center = SurfaceWindow1.SetPosition(L1clone);
                    Console.WriteLine("Part was added to L1");
                }
                else
                {
                    myClone.IsManipulationEnabled = true;
                    myClone.Opacity = 1;
                }
            }
            catch (Exception exc) { Console.WriteLine("PartInL0 \n" + exc); }
        }

        //Detects L1module drop target and copies its data into appropriate placeholder
        //Checks if module is full; if so, auto-generate new template and enable dragging on full module
        private void PartInL1()
        {
            //try
            //{
                Point pt = SurfaceWindow1.transformCoords(this, sw1.L1.L1_manTab);
                VisualTreeHelper.HitTest(sw1.L1.L1_manTab, null, new HitTestResultCallback(TargetL1MCallback), new PointHitTestParameters(pt));
                
                

                if (targetL1M != null)
                {
                    Part L1manclone = clone();
                    L1manclone.IsManipulationEnabled = false;
                    L1manclone.targetL1M = targetL1M;

                    if (_type == "prom")
                    {
                        targetL1M.L1Prom.copyPartInfoFrom(this);
                    }
                    else if (_type == "rbs")
                    {
                        targetL1M.L1RBS.copyPartInfoFrom(this);
                    }
                    else if (_type == "cds")
                    {
                        targetL1M.L1CDS.copyPartInfoFrom(this);
                    }
                    else if (_type == "term")
                    {
                        targetL1M.L1Term.copyPartInfoFrom(this);
                    }
                    //Console.WriteLine("Target detected!");

                    //Check to see if L1M is full and new template needed
                    Boolean isFull = true;
                    foreach (UIElement elem in targetL1M.L1Grid.Children)
                    {
                        if (elem.GetType() == typeof(Part))
                        {
                            Part p = (Part)elem;
                            if (p.Opacity < 1)
                            {
                                isFull = false;
                                break;
                            }
                        }
                    }

                    if (isFull)
                    {
                        sw1.L1.addL1Module();
                        targetL1M.IsManipulationEnabled = true;
                        targetL1M.Template.Visibility = System.Windows.Visibility.Hidden;
                        targetL1M.L1ElementMenu.ActivationMode = ElementMenuActivationMode.AlwaysActive;
                    }

                }
                else
                { //Dumped; check if delete from palette, too
                    //Console.WriteLine("Missed!");
                    sw1.swipeToDelete(this);
                }
            //}
            //catch (Exception exc) { Console.WriteLine("PartInL1 \n" + exc); }
        }

        #endregion

        #region DRAG AND DROP HELPERS

        //Clones the part and copies content values and type
        //Used in both Part_ContainerManipulationDelta and _...Completed
        public Part clone()
        {
            Part copy = new Part();
            copy.copyPartInfoFrom(this);

            return copy;
        }

        //Copies properties from the source Part to the receiver
        public void copyPartInfoFrom(Part source)
        {
            try
            {
                partName.Text = source.partName.Text;
                partCategory.Text = source.partCategory.Text;
                imgType.Source = source.imgType.Source;
                //Background = source.Background;
                _type = source._type;
                _myRegDS = source._myRegDS;
                _sitesList.Clear();
                foreach (Sites s in source._sitesList)
                {
                    _sitesList.Add(new Sites(s.Sequence));
                }
                //_sitesList = source._sitesList;
                Opacity = source.Opacity;
                mySource = source;
            }
            catch (Exception exc) { Console.WriteLine(exc); }
        }

        //Moves objects from parent to parent while maintaining a consistent center
        private void changeParents_SV(ScatterView parentSV, ScatterView destinationSV)
        {
            try
            {
                Point newPoint = SurfaceWindow1.transformCoords(this, destinationSV);

                parentSV.Items.Remove(this);
                destinationSV.Items.Add(this);
                this.Center = newPoint;
            }
            catch (Exception exc) { Console.WriteLine("changeParents_SV \n" + exc); }
        }

        //Result callback behavior for detection of target L1Modules in the Generate Manually tab
        //Detects Grids belonging to Parts and L1Modules and checks the name to differentiate between the two
        //Sets the dropped Part's targetL1Module variable to the L1Module parent detected
        public HitTestResultBehavior TargetL1MCallback(HitTestResult result)
        {
            if (result.VisualHit.GetType() == typeof(Grid))
            {
                Console.WriteLine("Detected a grid. Is it an L1Grid?");
                Grid targetGrid = (Grid)result.VisualHit;
                if (targetGrid.Name == "L1Grid")
                {
                    Console.WriteLine("It was an L1Grid");
                    targetL1M = (L1Module)targetGrid.Parent;
                    Console.WriteLine(targetGrid.Name);
                    return HitTestResultBehavior.Stop;
                }
                else
                {
                    Console.WriteLine("Detected the grid " + targetGrid.Name);
                    return HitTestResultBehavior.Continue;
                }
            }
            else
            {
                Console.WriteLine("Didn't detect a grid.");
                return HitTestResultBehavior.Continue;
            }
        }

        #endregion

        #region Primer Designer helpers
        //Updates SitesList of the Part that launched Primer Designer, but no further
        public void updateSites(StackPanel pan)
        {
            //Store Sites info in own list
            int index = pan.Children.IndexOf(this);
            Sites s1 = (Sites)VisualTreeHelper.GetChild(pan, index - 1);
            _sitesList.ElementAt(0).copySitesInfoFrom(s1);
            //Store Sites info in original's list
            mySource._sitesList.ElementAt(0).copySitesInfoFrom(s1);

            //modified so only execute code relating to second site if applicable 
            //@T.Feng (dealing with removing duplicate fusion site issue)
            try
            {
                Sites s2 = (Sites)VisualTreeHelper.GetChild(pan, index + 1);
                _sitesList.ElementAt(1).copySitesInfoFrom(s2);
                mySource._sitesList.ElementAt(1).copySitesInfoFrom(s2);

            }
            catch (Exception ex)
            {
                //Do nothing for exception: means second site doesn't exist (Primer Designer launched from Level 1 or 2)
            }
        }

        #endregion




    }
}
