using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace SurfaceApplication1
{
    public class BasicInfo
    {
        private string _sbolImg;
        private string _availability;
        private string _usefulness;
        private string _sequence;
        private int _length;
        private List<string> _twins;
        private string _descriptionName;

        #region Properties
        public string SbolImg
        {
            get{ return _sbolImg;}
            set{ _sbolImg = value;}
        }

        public string Availability
        {
            get { return _availability; }
            set { _availability = value; }
        }

        public string Usefulness
        {
            get { return _usefulness; }
            set { _usefulness = value; }
        }

        public string Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }

        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }

        public List<string> Twins
        {
            get { return _twins; }
            set { _twins = value; }
        }

        public string DescriptionName
        {
            get { return _descriptionName; }
            set { _descriptionName = value; }
        }
        #endregion

        public BasicInfo()
        {
            _sbolImg = "";
            _availability = "";
            _usefulness = "";
            _sequence = "";
            _length = 0;
            _twins = new List<string>();
            _descriptionName = "";
        }

        //Constructor for custom Parts: contains Sequence data
        public BasicInfo(String seq)
        {
            _sbolImg = "";
            _availability = "";
            _usefulness = "";
            _sequence = seq;
            _length = seq.Length;
            _twins = new List<string>();
            _descriptionName = "";
        }

        /// <summary>
        /// 6/21/2012
        /// Links to Parts Registry pages and parses source code in order to obtain basic information for a 
        /// particular part
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>
        /// <param name="type">Type of part - promoter, terminator, rbs, or gene</param>
        /// <param name="name">Name of part</param>
        
        public BasicInfo(String sourceCode, String type, String name)
        {
            _sbolImg = "";
            _availability = "";
            _usefulness = "";
            _sequence = "";
            _length = 0;
            _twins = new List<string>();
            _descriptionName = "";

            int index = 0;
            string br = "\n";
            string sc = sourceCode;

            //finds availability of the part (on the top right corner of each part's page)
            //index = sc.IndexOf("<div style='float:right;padding-top:1px; font-size:95%;line-height:99%;text-align:center;margin-left:20px;'>");
            string availInd = "DNA ";
            if(sc.Contains("Part Deleted")) _availability = "Part Deleted";
            else {
                index = sc.IndexOf(availInd);
                sc = sc.Substring(index);
                _availability = sc.Substring(0, sc.IndexOf(br));
            }

            //finds usefulness of the part (on the top right corner of each part's page)
            
            string useInd = "<div style=";
            index = sc.IndexOf(useInd);
            sc = sc.Substring(index + useInd.Length);
            index = sc.IndexOf(br);
            sc = sc.Substring(index + br.Length);
            _usefulness = sc.Substring(0, sc.IndexOf("</")).Trim();
            //Case where Usefulness: 1 Registry Star
            if (_usefulness.Contains("http://partsregistry.org/images/stars/1star.png"))
                _usefulness = _usefulness.Substring(_usefulness.IndexOf(">") + 1);

            //finds the SBOL image
            index = sc.IndexOf("http://partsregistry.org/images/partbypart/icon_");
            sc = sc.Substring(index);
            _sbolImg = sc.Substring(0, sc.IndexOf("'>"));

            //finds description name
            if (sc.Contains("<SPAN style='font-size: 150%; font-weight: bold;'>"))
            {
                string descriptInd = "<SPAN style='font-size: 150%; font-weight: bold;'>";
                index = sc.IndexOf(descriptInd) + descriptInd.Length;
                sc = sc.Substring(index);
                _descriptionName = sc.Substring(0, sc.IndexOf("</SPAN>"));
            }

            //finds list of twins of the part (on the bottom of each part's page)
            index = sc.IndexOf("<A class='noul_link'");
            if (index == -1) _twins.Add("This part has no twins."); //case where no twins
            else
            {
                sc = sc.Substring(index);
                sc = sc.Substring(0, sc.IndexOf("</div>"));

                do
                {
                    string temp = sc.Substring(sc.IndexOf("'>") + 2, sc.IndexOf("</A>") - sc.IndexOf("'>") - 2);
                    sc = sc.Substring(sc.IndexOf("</A>") + "</A>".Length);
                    temp = temp + " :" + sc.Substring(0, sc.IndexOf("\n"));
                    temp = temp.Replace("\n", "").Trim();
                    _twins.Add(temp); //produces list of twins
                }
                while (!(sc.IndexOf("<BR>") == -1));

            }

            /* Checking if works with parts with multiple twins
             * string twinsList = string.Join(",", twins.ToArray());
             * Console.WriteLine(twinsList);
             */

            # region HTML WebRequest (Crawler goes into Designs Page for each part)

            //Go to part design page in Parts Registry
            // used to build entire input
            StringBuilder sb = new StringBuilder();

            // used on each read operation
            byte[] buf = new byte[8192];

            // prepare the web page we will be asking for
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create("http://partsregistry.org/Part:" + name + ":Design");
            // enters the part name in the link to produce the part design page

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

            //finds sequence and length of the part (on the design page for each part)
            index = 0;
            String htmlText = sb.ToString();
            if (htmlText.Contains("var sequence"))
            {
                index = htmlText.IndexOf("String('");
                htmlText = htmlText.Substring(index + "String('".Length);
                _sequence = htmlText.Substring(0, htmlText.IndexOf("')"));
                _length = _sequence.Length;   
            }

            if (_sequence == "")
            {
                _sequence = "No Information";
                _length = 0;
            }
            
            
            
            resStream.Close();
            response.Close();

        }

        //Returns a string representation of the part's twins
        public string getTwins()
        {
            if (_twins.Count != 0)
            {
                return string.Join(",", _twins.ToArray());
            }

            return "No Information";
        }

        //returns all of the basic info parsed from each part page's source code
        public override string ToString()
        {
            if (_twins.Count != 0)
                return "{0}Image:" + _sbolImg + "{0}Sequence:" + _sequence + "{0}Length:" + _length + "{0}Availability:" + _availability + "{0}Usefulness:"
                + _usefulness + "{0}Twins:" + _twins.Count;
            return "{0}Image:" + _sbolImg + "{0}Sequence:" + _sequence + "{0}Length:" + _length + "{0}Availability:" + _availability + "{0}Usefulness:"
                + _usefulness;
        }

    }
}
