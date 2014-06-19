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
    /// Interaction logic for Level0.xaml
    /// </summary>
    public partial class Level0 : ScatterViewItem
    {
        public static SurfaceWindow1 sw1;
        private String[][] partsFilters; //Holds filter names (and other info not yet added)
        private String _partTypeSelected; //What type is displayed

        //private List<List<String>> promotersList;
        //private List<List<String>> rbsList;
        //private List<List<String>> cdsList;
        //private List<List<String>> terminatorsList;

        private ProgressBarWrapper _progressBarWrapper;
        private Thread _currentSearch;
        private String filterText;
        

        //Properties
        public String PartTypeSelected
        {
            get { return _partTypeSelected; }
            set { _partTypeSelected = value; }
        }
        
        public Level0()
        { 
            InitializeComponent();
            Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            Center = new Point(Width / 2.0, Height / 2.0);

            IsTopmostOnActivation = false;
            Part.l0 = this;

            //Create String[] containing filter names for each part; add to partsFilters for later reference
            String[] promFilters = { "inducible", "constitutive", "repressible", "multiple" };
            String[] rbsFilters = { "anderson", "community", "isaacs", "rackham", "miscellaneous" };
            String[] cdsFilters = { "reporter", "transcriptional", "selection marker", "protease", "post-translational modification", "DNA modification", "membrane proteins", "receptors/ligands", "lysis protein", "biosynthesis" }; //Add separate categories later
            String[] termFilters = { "forward", "bidirectional", "reverse"};
            String[] emptyFilters = { };
            partsFilters = new String[5][];
            partsFilters[0] = promFilters;
            partsFilters[1] = rbsFilters;
            partsFilters[2] = cdsFilters;
            partsFilters[3] = termFilters;
            partsFilters[4] = emptyFilters; 

            //Initialize progress bar
            _progressBarWrapper = new ProgressBarWrapper(new Action(showProgressBar), new Action(hideProgressBar));

            //Create arrays of Parts
            //String loc = Directory.GetCurrentDirectory();
            //loc = loc.Substring(0, loc.IndexOf("bin"));

            //promotersList = createPartsList(loc + "promoters.txt");
            //rbsList = createPartsList(loc + "rbs.txt");
            //cdsList = createPartsList(loc + "cds.txt");
            //terminatorsList = createPartsList(loc + "terminators.txt");

            //Start with promoters
            //_partTypeSelected = "prom";
            //filterText = "inducible"; 
            try
            {
                prom.IsChecked = true;
            }
            catch (Exception e)
            {
                //heck knows why
            }
            //populate_Filters(0);
            //SurfaceRadioButton firstfilter = VisualTreeHelper.GetChild(filtersPanel, 0) as SurfaceRadioButton;
            //firstfilter.IsChecked = true; 

        }

        private void showProgressBar()
        {
            ProgressIndicator.Visibility = Visibility.Visible;
        }

        private void hideProgressBar()
        {
            ProgressIndicator.Visibility = Visibility.Collapsed;
        }

        //Clears and repopulates old filters; sets first filter to checked
        private void nav_Checked(object sender, RoutedEventArgs e)
        {
            int a = 0;
            filtersPanel.Children.Clear();
            if (sender == all)
            {
                _partTypeSelected = "all";
                L0_resultsSV.Items.Clear();
                CategoryBrowse.Visibility = System.Windows.Visibility.Hidden; 
            }

            else 
            {
                CategoryBrowse.Visibility = System.Windows.Visibility.Visible; 

                if (sender == prom)
                {
                    a = 0;
                    _partTypeSelected = "prom";
                }
                else if (sender == rbs)
                {
                    a = 1;
                    _partTypeSelected = "rbs";
                }
                else if (sender == cds)
                {
                    a = 2;
                    _partTypeSelected = "cds";
                }
                else if (sender == term)
                {
                    a = 3;
                    _partTypeSelected = "term";
                }
                else if (sender == cus)
                {
                    //If custom parts, skip steps involving filters; proceed directly to populating with parts
                    //But can add filters for part type (e.g. custom promoters) if necessary
                    a = 4;
                    _partTypeSelected = "cus";
                    searchByFilter();
                    return;
                }

                populate_Filters(a);
                SurfaceRadioButton firstfilter = VisualTreeHelper.GetChild(filtersPanel, 0) as SurfaceRadioButton;
                firstfilter.IsChecked = true;
            }
        }

        //Reads in Part Registry ID and type from text file
        private void populate_ResultsPage(string FilePath)
        {
            StreamReader reader = new StreamReader(FilePath);
            while (reader.EndOfStream != true)
            {
                //Read in data
                string ThisLine = reader.ReadLine();

                //Determine if reading custom Part data
                if (!ThisLine.Substring(0, 3).Equals("P |"))
                {
                    string[] SplitLine = ThisLine.Split(',');
                    string CategorySplit = SplitLine[1].Trim();
                    string CommonNameSplit = (SplitLine[2].Trim()).Replace("&amp;", "");

                    //Filter data, add Parts
                    if (CategorySplit == filterText)
                    {
                        Part p = new Part();
                        p.Type = _partTypeSelected;
                        p.partName.Text = SplitLine[0];
                        p.partCategory.Text = CommonNameSplit;
                        this.L0_resultsSV.Items.Add(p);
                        p.Center = SurfaceWindow1.SetPosition(p);
                    }
                }
                else
                {
                    string[] SplitLine = ThisLine.Split('|');
                    Part p = new Part(SplitLine[1].Trim(), true);
                    p.Type = SplitLine[1].Trim();
                    p.partCategory.Text = SplitLine[2].Trim();
                    
                    //Enter information in RegDataSheet: description, sequence, associated with
                    p.myRegDS = new RegDataSheet(SplitLine);
                    this.L0_resultsSV.Items.Add(p);
                    p.Center = SurfaceWindow1.SetPosition(p);
                }
            }
        }

        //Creates filters and adds to filtersPanel
        private void populate_Filters(int a)
        {
            for (int i = 0; i < partsFilters[a].Length; i++)
            {
                SurfaceRadioButton filter;
                filter = new SurfaceRadioButton();
                filter.Foreground = Brushes.Black;
                filter.Background = Brushes.LightGray;
                filter.Name = "filter" + a + "_" + i;
                filter.Content = partsFilters[a][i];
                filter.Checked += new RoutedEventHandler(filterIsChecked);
                filtersPanel.Children.Add(filter);
            }
        }

        public void filterIsChecked(object sender, RoutedEventArgs e)
        {
            SurfaceRadioButton filter = (SurfaceRadioButton)sender;
            filter.Focus();
            filterText = (String)filter.Content;
            searchByFilter();
        }

        //Resets results field to empty and original size; aborts old search
        //Calls populate_ResultsPage and sets searchType
        public void searchByFilter()
        {
            //If filter is checked and it's not for restore (i.e. index is -1)
            if (storedFilterIndex == -1)
            {
                String loc = Directory.GetCurrentDirectory();
                L0_resultsSV.Items.Clear();
                L0_resultsSV.Height = 730;
                if (_currentSearch != null) _currentSearch.Abort();
                ProgressIndicator.Visibility = Visibility.Collapsed;

                if (_partTypeSelected == "prom")
                {
                    loc = loc.Substring(0, loc.IndexOf("bin")) + @"promoters2.txt";
                }
                if (PartTypeSelected == "rbs")
                {
                    loc = loc.Substring(0, loc.IndexOf("bin")) + @"rbs2.txt";
                }
                if (PartTypeSelected == "cds")
                {
                    loc = loc.Substring(0, loc.IndexOf("bin")) + @"cds2.txt";
                }
                if (PartTypeSelected == "term")
                {
                    loc = loc.Substring(0, loc.IndexOf("bin")) + @"terminators2.txt";
                }
                if (PartTypeSelected == "cus")
                {
                    loc = loc.Substring(0, loc.IndexOf("bin")) + @"CustomParts.txt";
                }
                populate_ResultsPage(loc);
            }

            #region if(sw1) else statement (commented out)
            //int index = filtersPanel.Children.IndexOf(filter);

                //_currentSearch = _progressBarWrapper.execute<int, String>(searchByFilter_longOp, index, new Action<String> (searchByFilter_callback));
                //searchByFilter_longOp(index);
            //}
            //catch (Exception exc) { Console.WriteLine(exc); }

                     //populate_ResultsPage(loc2);

                     //int m = L0_resultsSV.Items.Count; 
            /*for (int i = L0_resultsSV.Items.Count - 1; i >= 0; i--)
            {
                Part p = L0_resultsSV.Items.GetItemAt(i) as Part;
                if (p.partCategory.Text != filterText)
                {
                    L0_resultsSV.Items.Remove(p);
                }
            }*/

            //else
            //{
            //    String loc2 = Directory.GetCurrentDirectory();
            //    L0_resultsSV.Items.Clear();
            //    if (_currentSearch != null) _currentSearch.Abort();
            //    ProgressIndicator.Visibility = Visibility.Collapsed;

            //    SurfaceRadioButton filter = (SurfaceRadioButton)sender;
            //    String filterText = (String)filter.Content;


            //    loc2 = loc2.Substring(0, loc2.IndexOf("bin")) + @"promoters.txt";
            //    populate_ResultsPage(loc2);


            //    Console.WriteLine("The Current Count is: " + L0_resultsSV.Items.Count);
            //    for (int i = L0_resultsSV.Items.Count - 1; i >= 0; i--)
            //    {
            //        Console.WriteLine(L0_resultsSV.Items.Count);
            //        Part p = L0_resultsSV.Items.GetItemAt(i) as Part;
            //        if (p.partCategory.Text != filterText)
            //        {
            //            L0_resultsSV.Items.Remove(p);
            //        }
            //    }

            //}
            #endregion

            #region Attempted threading for not so great search (commented out)

            //Search search = null;
            #region Extra search code
            //BackgroundWorker searcher = new BackgroundWorker();
            //searcher.WorkerSupportsCancellation = true;

            //searcher.DoWork += delegate(object s, DoWorkEventArgs args)
            //{};

            //searcher.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            //{};

            //searcher.RunWorkerAsync();

            /*Thread searchThread = new Thread(delegate()
                {
                    search = new Search(convertType_MCPToPR(_partTypeSelected));
                });
            searchThread.Start();
            searchThread.Join();*/
            #endregion
            /*Func<String, Search> _searchByFilter =
                delegate(String query) { return new Search(query); };
            Action<Search> callback = delegate(Search result)
            {
                foreach (RegDataSheet regDS in result.RegDS)
                {
                    if (convertType_RDSToMCP(regDS.Type) == _partTypeSelected)
                    {
                        if (_partTypeSelected == "prom")
                        {
                            if (regDS.Promoter.getReg().Contains(filterText))
                            { partFromRegDS(regDS, regDS.Promoter.getReg()); }
                        }
                        else if (_partTypeSelected == "rbs")
                        {
                            if (regDS.Rbs.FamilyName.Contains(filterText))
                            {  partFromRegDS(regDS, regDS.Rbs.FamilyName); }
                        }
                        else if (_partTypeSelected == "cds")
                        { partFromRegDS(regDS, "CDS"); }
                        else //term
                        {
                            if (regDS.Terminators.Direction == filterText)
                            { partFromRegDS(regDS, regDS.Terminators.Direction); }
                        }
                    }
                }
            }*/

            //_currentSearch = _progressBarWrapper.execute<String, Search>(_searchByFilter, convertType_MCPToPR(_partTypeSelected), callback);
#endregion
        }

        #region okay but not great search
        //Just here to keep _progressBarWrapper happy
        void searchByFilter_callback(String s) { }

        //Stored separate from searchByFilter so it can be called by _progressBarWrapper as the long operation
        //Returns a string to keep _progressBarWrapper happy
        //private String searchByFilter_longOp(int filterIndex)
        //{
        //    //Get list to populate from
        //    List<List<String>> sourceList;
        //    if (_partTypeSelected == "prom")
        //    {
        //        sourceList = promotersList;
        //    }
        //    else if (_partTypeSelected == "rbs")
        //    {
        //        sourceList = rbsList;
        //    }
        //    else if (_partTypeSelected == "cds")
        //    {
        //        sourceList = cdsList;
        //    }
        //    else //"term"
        //    {
        //        sourceList = terminatorsList;
        //    }

        //    //Find indexes to start and stop populating from in partsList[0]
        //    List<String> sourceIndexes = sourceList.ElementAt(0);
            
        //    int firstIndex = Convert.ToInt32(sourceIndexes.ElementAt(filterIndex));
        //    int lastIndex;
        //    //If filterIndex refers to the last index stored in sourceIndexes, i.e. the start of the last category
        //    if (filterIndex == sourceIndexes.Count - 1)
        //    {   //lastIndex is the index of the last part stored in sourceList
        //        lastIndex = sourceList.Count - 1;
        //    }
        //    else
        //    {   //Otherwise, lastIndex is the index of the last part before the start index of the next category
        //        lastIndex = Convert.ToInt32(sourceIndexes.ElementAt(filterIndex + 1)) - 1;
        //    }

        //    populate_Results(sourceList, firstIndex, lastIndex);

        //    return "I'm only here because threading wants a return type";
        //}

        //Populate L0_resultsSV with parts from partsList between given indexes
        private void populate_Results(List<List<String>> partsList, int first, int last)
        {
            for (int i = first; i <= last; i++)
            {
                Part p = new Part();
                p.Type = _partTypeSelected;
                p.partName.Text = (partsList.ElementAt(i)).ElementAt(0);
                p.partCategory.Text = (partsList.ElementAt(i)).ElementAt(1);
                this.L0_resultsSV.Items.Add(p);
                p.Center = SurfaceWindow1.SetPosition(p);
            }
        }
        #endregion

        #region Search-associated behaviors, e.g. checking/unchecking buttons, focusing/unfocusing searchBar
        private int storedTypeIndex = -1;
        private int storedFilterIndex = -1;
        private Boolean isRestore = false;

        //Enables touch activation of searchBar
        private void searchBar_TouchDown(object sender, TouchEventArgs e)
        {

            searchBar.Focus();
            Boolean focused = searchBar.IsFocused;
        }

        //Clears old text; unchecks filters - searchbar does not search within filter results
        private void searchBar_GotFocus(object sender, RoutedEventArgs e)
        {
            //Store indexes of checked srbs in case search is not run and restoring previous state is needed
            //storedTypeIndex = indexOfCheckedSRB(navigation);
            //storedFilterIndex = indexOfCheckedSRB(filtersPanel);

            searchBar.Text = "";
            //If searchInAll is not checked (i.e. not fired by user selecting searchInAll), check Type
            /*if (!(Boolean)searchInAll.IsChecked) searchInType.IsChecked = true;
            foreach (SurfaceRadioButton srb in filtersPanel.Children)
            {
                srb.IsChecked = false;
            }*/
        }

        //If user removes focus from search, uncheck radio buttons
        private void searchBar_LostFocus(object sender, RoutedEventArgs e)
        {
            searchBar.Text = "Search";
            //searchInType.IsChecked = false;
            //searchInAll.IsChecked = false;
            //If doing search hasn't yet reset them and focus is lost to Level 0, recheck the type and filter
            /*if (storedTypeIndex != -1 && storedFilterIndex != -1 && isRestore)
            {
                ((SurfaceRadioButton)VisualTreeHelper.GetChild(navigation, storedTypeIndex)).IsChecked = true;
                ((SurfaceRadioButton)VisualTreeHelper.GetChild(filtersPanel, storedFilterIndex)).IsChecked = true;
            }*/
            //else if (isRestore) //Focus is lost to Level 0 but search has reset storedIndexes
            //{
            //    ((SurfaceRadioButton)VisualTreeHelper.GetChild(navigation, storedTypeIndex)).IsChecked = true;
            //    ((SurfaceRadioButton)VisualTreeHelper.GetChild(filtersPanel, storedFilterIndex)).IsChecked = true;
            //}
            ////else if (isRestore) //Focus is lost to Level 0 but search has reset storedIndexes
            ////{
            ////    uncheckSurfaceRadioButton(navigation);
            ////    uncheckSurfaceRadioButton(filtersPanel);
            ////}
            //storedFilterIndex = -1;
            //storedTypeIndex = -1;
            //isRestore = false;
            //filtersPanel.IsEnabled = true;
            //filtersPanel.Visibility = Visibility.Visible;
        }

        //If searchInType is checked, focus searchBar
        /*private void searchInType_Checked(object sender, RoutedEventArgs e)
        {
            if (!searchBar.IsFocused) searchBar.Focus();
            if (!hasCheckedSurfaceRadioButton(navigation)) prom.IsChecked = true;
        }*/

        //If searchInAll is checked, focus searchBar and uncheck part type button at the top
        /*private void searchInAll_Checked(object sender, RoutedEventArgs e)
        {
            if (!searchBar.IsFocused) searchBar.Focus();
            uncheckSurfaceRadioButton(navigation);
            filtersPanel.IsEnabled = false;
            filtersPanel.Visibility = Visibility.Hidden;
        }*/

        //Removes focus from searchBar and determines if restore is appropriate
        private void Main_TouchDown(object sender, TouchEventArgs e)
        {
            //if (storedTypeIndex != -1 && storedFilterIndex != -1) isRestore = true;
            //Main.Focus();
        }
        #endregion

        //Search using string entered into searchBar
        private void searchByString(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //if ((Boolean)searchInAll.IsChecked) filtersPanel.Children.Clear();
                L0_resultsSV.Items.Clear();
                L0_resultsSV.Height = 730;
                if (_currentSearch != null) _currentSearch.Abort();
                ProgressIndicator.Visibility = Visibility.Collapsed;
                ////Search has been run, so reset indexes to prevent restoring
                //storedFilterIndex = -1;
                //storedTypeIndex = -1;

                //Creates search and filters through returned data sheets according to whether searchInAll or searchInType is checked
                Func<String, Search> _searchByString =
                    delegate(String query) { return new Search(query); };
                Action<Search> callback = delegate(Search result)
                {
                    foreach (RegDataSheet regDS in result.RegDS)
                    {
                        String typeConverted = convertType_RDSToMCP(regDS.Type);
                        String category = "";
                        if (typeConverted == "prom") { category = regDS.Promoter.getReg(); }
                        if (typeConverted == "rbs") { category = regDS.Rbs.FamilyName; }
                        if (typeConverted == "cds") { category = regDS.Gene.GeneType; }
                        if (typeConverted == "term") { category = regDS.Terminators.Direction; }

                        //If searchInAll is checked, don't filter. If searchInType is selected, filter.
                        if (_partTypeSelected == "all")
                        {
                            partFromRegDS(regDS, category);
                        }
                        else
                        {
                            if (typeConverted == _partTypeSelected)
                            {
                                partFromRegDS(regDS, category);
                                //if (typeConverted == "prom") { partFromRegDS(regDS, regDS.Promoter.getReg()); }
                                //if (typeConverted == "rbs") { partFromRegDS(regDS, regDS.Rbs.FamilyName); }
                                //if (typeConverted == "cds") { partFromRegDS(regDS, "CDS"); }
                                //if (typeConverted == "term") { partFromRegDS(regDS, regDS.Terminators.Direction); }
                            }
                        }
                    }
                };
                _currentSearch = _progressBarWrapper.execute<String, Search>(_searchByString, searchBar.Text, callback);

                #region Non-delegate search
                //Search s = new Search(searchBar.Text);
                //foreach (RegDataSheet regDS in s.RegDS)
                //{
                //    String typeConverted = convertType_RDSToMCP(regDS.Type);
                //    if (typeConverted == _partTypeSelected)
                //    {
                //        if (typeConverted == "prom") { Part p = partFromRegDS(regDS, regDS.Promoter.getReg()); }
                //        if (typeConverted == "rbs") { Part p = partFromRegDS(regDS, regDS.Rbs.FamilyName); }
                //        if (typeConverted == "cds") { Part p = partFromRegDS(regDS, "CDS"); }
                //        if (typeConverted == "term") { Part p = partFromRegDS(regDS, regDS.Terminators.Direction); }

                //    }
                //}
                #endregion
            }
        }

        //Converts words used for type in RegDataSheet to MoCloPlanner equivalents
        private string convertType_RDSToMCP(string p)
        {
            if (p == "promoter") { return "prom"; }
            else if (p == "gene") { return "cds"; }
            else if (p == "terminator") { return "term"; }
            else /*rbs*/ { return p; }
        }

        //Converts words used for type in MoCloPlanner to Parts Registry keywords
        private string convertType_MCPToPR(string p)
        {
            if (p == "prom") { return "promoter"; }
            else if (p == "rbs") { return "ribosome"; }
            else if (p == "cds") { return "gene enzyme protein reporter"; }
            else /*term*/ { return "terminator"; }
        }

        //Creates a new Part based on RegDataSheet and category; do not add duplicates
        private void partFromRegDS(RegDataSheet regDS, String cat)
        {
            String currentPartsList = listCurrentPartsInResults();
            if (!currentPartsList.Contains(regDS.Name))
            {
                Part p = new Part(convertType_RDSToMCP(regDS.Type));
                p.myRegDS = regDS;
                p.partName.Text = p.myRegDS.Name;
                p.partCategory.Text = p.myRegDS.BasicInfo.DescriptionName;
                L0_resultsSV.Items.Add(p);
                p.Center = SurfaceWindow1.SetPosition(p);
            }
        }

        //Lists all parts in L0_resultsSV by registry ID
        private String listCurrentPartsInResults()
        {
            List<String> partList = new List<String>();
            foreach (Part p in L0_resultsSV.Items)
            {
                partList.Add(p.myRegDS.Name);
            }
            String resultingList = string.Join(",", partList.ToArray());
            Console.WriteLine(resultingList);
            return resultingList;
        }

        //Checks if there is a radiobutton in a particular collection that's checked
        private static Boolean hasCheckedSurfaceRadioButton(Panel pan)
        {
            foreach (SurfaceRadioButton srb in pan.Children)
            {
                if ((Boolean)srb.IsChecked) return true;
            }
            return false;
        }

        //Unchecks radiobuttons in a given panel
        private static void uncheckSurfaceRadioButton(Panel pan)
        {
            foreach (SurfaceRadioButton srb in pan.Children)
            {
                srb.IsChecked = false;
            }
        }

        //Returns index of checked radiobutton in a particular collection
        private static int indexOfCheckedSRB(Panel pan)
        {
            foreach (SurfaceRadioButton srb in pan.Children)
            {
                if ((Boolean)srb.IsChecked) return pan.Children.IndexOf(srb);
            }
            return -1;
        }

        //Testing out global variable checking with addition of new Parts
        private void partAdder_Click(object sender, RoutedEventArgs e)
        {
            Part part = new Part();
            ImageSourceConverter icon = new ImageSourceConverter();
            part.partName.Text = "BBa##)@)#";
            part.partCategory.Text = "no filter";
            L0_resultsSV.Items.Add(part);
            part.Center = SurfaceWindow1.SetPosition(part);
        }

        //Makes visible the data form for user to input data on their custom part
        private void cusPartButton_Click(object sender, RoutedEventArgs e)
        {
            sw1.cF.cusForm.Visibility = Visibility.Visible;
            customForm.modlevel = 0;
            sw1.cF.cus_Seq.IsReadOnly = false;
        }



    }
}
