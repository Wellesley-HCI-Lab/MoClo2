using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;

namespace SurfaceApplication1
{
    /// <summary>
    /// All information for a part's data sheet extracted from the Registry
    /// </summary>
    public class RegDataSheet
    {
        private string _name;
        private string _type;
        private Terminators _terminator;
        private Promoter _promoter;
        private RBS _rbs;
        private Gene _gene;
        private BasicInfo _basicInfo;
        private Description _description;
        //private Protocol _protocol;
        private Reference _reference;
        private bool _none; //is not the type of part we want

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public Terminators Terminators
        {
            get { return _terminator; }
            set { _terminator = value; }
        }

        public Promoter Promoter
        {
            get { return _promoter; }
            set { _promoter = value; }
        }

        public RBS Rbs
        {
            get { return _rbs; }
            set { _rbs = value; }
        }

        public Gene Gene
        {
            get { return _gene; }
            set { _gene = value; }
        }

        public BasicInfo BasicInfo
        {
            get { return _basicInfo; }
            set { _basicInfo = value; }
        }

        public Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        //public Protocol Protocol
        //{
        //    get { return _protocol; }
        //    set { _protocol = value; }
        //}

        public Reference Reference
        {
            get { return _reference; }
            set { _reference = value; }
        }

        public bool isPart
        {
            get { return _none; }
            set { _none = value; }
        }


        #endregion

        public RegDataSheet()
        {
            _name = "test";
            _type = "test";
            _none = true;
            _terminator = new Terminators();
            _promoter = new Promoter();
            _rbs = new RBS();
            _gene = new Gene();
            _basicInfo = new BasicInfo();
            _description = new Description();
            //_protocol = new Protocol();
            _reference = new Reference();
        }

        //Constructor for custom Part datasheet: covers Description, Sequence, Associated wtih
        public RegDataSheet(String[] data)
        {
            _name = data[3].Trim(); //Set name
            _type = "";
            _none = true;
            _terminator = new Terminators();
            _promoter = new Promoter();
            _rbs = new RBS();
            _gene = new Gene();
            _basicInfo = new BasicInfo(data[5].Trim()); //Set sequence
            _description = new Description();
            //_protocol = new Protocol();
            _reference = new Reference();

            //Determine appropriate type
            _type = data[2];
            if (_type.Equals("prom"))
            {
                _promoter = new Promoter("custom");
            }
            else if (_type.Equals("rbs"))
            {
                _rbs = new RBS("custom");
            }
            else if (_type.Equals("cds"))
            {
                _gene = new Gene();
            }
            else
            {
                _terminator = new Terminators("custom");
            }

            //Save description to regDS
            _description.Desc = data[4].Trim();

        }

        /// <summary>
        /// 6/21/2012
        /// Links to Parts Registry pages and parses source code in order to obtain general information for a 
        /// particular part
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="link">Particular extension of URL specific to this page</param>

        public RegDataSheet(string link)
        {
            _name = "";
            _type = "";
            _none = true;
            _terminator = new Terminators();
            _promoter = new Promoter();
            _rbs = new RBS();
            _gene = new Gene();
            _basicInfo = new BasicInfo();
            _description = new Description();
            //_protocol = new Protocol();
            _reference = new Reference();

            bool linkValid = true;

            #region HTML Web Request
            //the first link was set to -1 if no results showed up
            if (link.Equals("http://partsregistry.org/-1"))
            {
                linkValid = false;
            }

            if (linkValid)
            {
                try
                {

                    //Open a connection to PubMed publication page to get the information
                    Uri uri = new Uri(link);
                    // used to build entire input
                    StringBuilder sb = new StringBuilder();

                    // used on each read operation
                    byte[] buf = new byte[8192];

                    // prepare the web page we will be asking for
                    HttpWebRequest request =
                        (HttpWebRequest)WebRequest.Create(uri);

                    // execute the request
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // we will read data via the response stream
                    Stream resStream = response.GetResponseStream();
                    string tempString = null;
                    int count = 0;

                    do
                    {
                        // fill the buffer with data
                        count = resStream.Read(buf, 0, buf.Length);

                        // make sure we read some data
                        if (count != 0)
                        {
                            // translate from bytes to ASCII text
                            tempString = Encoding.ASCII.GetString(buf, 0, count);

                            // continue building the string
                            sb.Append(tempString);
                        }
                    }
                    while (count > 0); // any more data to read?

            #endregion

                    String htmlText = sb.ToString(); //keep original to have objects parse through and find
                    //specific information
                    String tempText = sb.ToString(); //will be parsed through in this top-level method to find name and type of part
                    String sourceText = sb.ToString(); //will be parsed for sections of source code each class needs
                    int index = 0;
    

                    //find name of page
                    string pageTitle = "<h1 class='firstHeading'>"; //length: 25
                    string typeImg = "<img style='padding-top:0px' src='http://partsregistry.org/images/partbypart/icon_";
                    string partLead = "Part:";

                    //strings needed for each big class
                    string descriptionText = "";
                    string referenceText = "";

                    string categoriesBox = "";
                    string parametersBox = "";

                    if (sourceText.Contains("Designed by"))
                    {
                        index = sourceText.IndexOf("Designed by ");
                        sourceText = sourceText.Substring(index);
                        referenceText = sourceText.Substring(0, sourceText.IndexOf("</div>"));
                    }

                    if (sourceText.Contains("Assembly Compatibility:<UL>"))
                    {
                        index = sourceText.IndexOf("Assembly Compatibility:<UL>");
                        sourceText = sourceText.Substring(index);
                        descriptionText = sourceText.Substring(0, sourceText.IndexOf("<DIV id='reviews'>"));
                    }

                    if (sourceText.Contains("<div style='border:1px solid #aaa;'>"))
                    {
                        index = sourceText.IndexOf("<div style='border:1px solid #aaa;'>");
                        sourceText = sourceText.Substring(index);
                        categoriesBox = sourceText.Substring(0, sourceText.IndexOf("</div>"));
                    }

                    if (sourceText.Contains("<div id='parameters'>"))
                    {
                        index = sourceText.IndexOf("<div id='parameters'>");
                        sourceText = sourceText.Substring(index);
                        parametersBox = sourceText.Substring(0, sourceText.IndexOf("<!--"));
                    }

                    if (tempText.Contains(typeImg)) //checks if this page is a parts page
                    {
                        //Find type of the part
                        index = tempText.IndexOf(typeImg) + typeImg.Length;
                        tempText = tempText.Substring(index);
                        string temp = tempText.Substring(0, tempText.IndexOf("'>"));


                        //first case, where the type of the part is RBS
                        if (htmlText.Contains("rbs"))
                        {
                            //if (temp == "rbs.png" || htmlText.Contains("//rbs/prokaryote/regulated/issacs"))
                            //{
                                _type = "rbs";
                                _rbs = new RBS(categoriesBox);
                            //}
                            //Console.WriteLine("RBS!");
                        }
                        //second case, where the type of the part is promoter (different images for
                        //different types of promoters)
                        else if (htmlText.Contains("//promoter"))
                        {
                                _type = "promoter";
                                _promoter = new Promoter(categoriesBox + " " + parametersBox);
                                //Console.WriteLine("PROMOTER!");
                        
                        //if (temp == "regulatory.png" || temp == "reporter.png" ||
                        //    temp == "generator.png" || temp == "signalling.png" ||
                        //    temp == "intermediate.png" || temp == "composite.png")
                        //{
                            

                        }
                        //third case, where the type of the part is terminator
                        else if (temp == "terminator.png")
                        {
                            _type = "terminator";
                            _terminator = new Terminators(parametersBox);
                            //Console.WriteLine("TERMINATOR!");
                        }
                        //fourth case, where the type of the part is gene or protein coding sequence
                        else if (temp == "coding.png")
                        {
                            _type = "gene";
                            _gene = new Gene(categoriesBox);
                            //Console.WriteLine("GENE!");
                        }
                    }
                    //last case, where type of part is none of the 4 classified parts above
                    else
                    {
                        _none = false;
                    }

                    //Finding page title

                    //case where page is not a parts page
                    if (_none == false)
                    {
                        index = tempText.IndexOf("<h1 class=\"firstHeading\">") + "<h1 class=\"firstHeading\">".Length;
                        tempText = tempText.Substring(index);
                        _name = tempText.Substring(0, tempText.IndexOf("</h1>"));
                    }
                    //case where page is a parts page
                    else
                    {
                        index = tempText.IndexOf(pageTitle) + pageTitle.Length + partLead.Length;
                        tempText = tempText.Substring(index);
                        _name = tempText.Substring(0, tempText.IndexOf("</h1>"));
                    }

                    //string protocolLink = "http://partsregistry.org/cgi/partsdb/related.cgi?part=" + _name;

                    //if the page is a parts page
                    if (_none)
                    {
                        _basicInfo = new BasicInfo(htmlText, _type, _name);
                        //Console.WriteLine(_basicInfo); 
                        _description = new Description(descriptionText);
                        //Console.WriteLine(_description); 
                        //_protocol = new Protocol(protocolLink);
                        //Console.WriteLine(_protocol); 
                        _reference = new Reference(referenceText);
                        //Console.WriteLine(_reference); 
                    }

                    resStream.Close();
                    response.Close();
                }
                catch (Exception ex)
                {
                    Console.Write("Exception : " + ex.Message);
                }
            }
        }

        public override string ToString()
        {
            string temp = "Name:" + _name + "{0}Type:" + _type +
                //Basic Info:
                           "{0}SBOL Image:" + _basicInfo.SbolImg +
                           "{0}Description Name:" + _basicInfo.DescriptionName + 
                //Description
                           "{0}------Description" + "{0}----Properties" +
                           "{0}Sequence:" + _basicInfo.Sequence +
                           "{0}Length:" + _basicInfo.Length +
                //Separate Parts Type Classes:               
                           "{0}--Category: {0}Chassis:" + _description.chassisString();
                 
                 
             //"\n**Description:** " + _description
             //     + "\n**Protocol:** " + _protocol + "\n**Reference:** " + _reference;

            if (_type == "promoter") temp = temp + "{0}Regulation:" + _promoter.getReg();
            if (_type == "rbs") temp = temp + "{0}Family Name:" + _rbs.FamilyName;

            //Parameters:

            if (_type == "promoter") temp = temp + "{0}Positive Regulation:" + _promoter.PosReg +
                                            "{0}Negative Regulation:" + _promoter.NegReg;
            //if (_type == "rbs") temp = temp + "\n-->Positive Regulation: " + _rbs.PosReg +
            //                                "\n-->Negative Regulation: " + _rbs.NegReg; 
            if (_type == "terminator") temp = temp + _terminator;
            if (_type == "gene") temp = temp + "{0}Gene type:" + _gene.GeneType;

            temp = temp + "{0}Twins:" + _basicInfo.getTwins() + "{0}--Assembly Compatibility:" + _description.assembCompString() +
                    //"{0}------Related" +
                    //"{0}Other Related Parts:" + _protocol.getRelParts() +
                    "{0}------Availibility, Usefulness" + 
                    "{0}Availibility:" + _basicInfo.Availability +  
                    "{0}Usefulness:" + _basicInfo.Usefulness + 
                    "{0}------References" + _reference;

             return temp;
        }

    }

}

//