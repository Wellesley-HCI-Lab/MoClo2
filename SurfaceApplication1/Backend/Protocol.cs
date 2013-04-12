using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace SurfaceApplication1
{
    public class Protocol
    {
        /*
        private string experience; //a lot of parts lacking info on this page
        private string pcrProt; //DK
        private string mocloReact; //DK
        private string measurements; //DK
        private List<string> relatedGenes; //DK
        private List<string> relatedOrgs;//DK
        */
        private List<string> _relatedPartsLinksUse;
        private List<string> _relatedPartsNamesUse;
        private List<string> _relatedPartsLinksCtg;
        private List<string> _relatedPartsNamesCtg;

        #region Properties

        public List<string> RelatedPartsLinksUse
        {
            get { return _relatedPartsLinksUse; }
            set { _relatedPartsLinksUse = value; }
        }

        public List<string> RelatedPartsNamesUse
        {
            get { return _relatedPartsNamesUse; }
            set { _relatedPartsNamesUse = value; }
        }

        public List<string> RelatedPartsLinksCtg
        {
            get { return _relatedPartsLinksCtg; }
            set { _relatedPartsLinksCtg = value; }
        }

        public List<string> RelatedPartsNamesCtg
        {
            get { return _relatedPartsNamesCtg; }
            set { _relatedPartsNamesCtg = value; }
        }
        #endregion
        
        public Protocol()
        {
            /*
            experience = "";
            pcrProt = "";
            mocloReact = "";
            measurements = "";
            relatedGenes = new List<string>();
            relatedOrgs = new List<string>();
            */
            _relatedPartsLinksUse = new List<string>();
            _relatedPartsNamesUse = new List<string>();
            _relatedPartsLinksCtg = new List<string>();
            _relatedPartsNamesCtg = new List<string>();

        }

        /// <summary>
        /// 6/22/2012
        /// Links to Parts Registry pages and parses source code in order to obtain protocol information 
        /// for a particular part
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>
        /// <param name="name">Name of the part</param>

        public Protocol(String link)
        {
            /*
            experience = "";
            pcrProt = "";
            mocloReact = "";
            measurements = "";
            relatedGenes = new List<string>();
            relatedOrgs = new List<string>();
            */
            _relatedPartsLinksUse = new List<string>();
            _relatedPartsNamesUse = new List<string>();
            _relatedPartsLinksCtg = new List<string>();
            _relatedPartsNamesCtg = new List<string>();

            # region HTML WebRequest

            //Go to part design page in Parts Registry
            // used to build entire input
            StringBuilder sb = new StringBuilder();

            // used on each read operation
            byte[] buf = new byte[8192];

            // prepare the web page we will be asking for
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(link);
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
            
            int index = 0;
            String htmlText = sb.ToString();
            string firstInd = "is in these";
            string secondInd = "<!-- categories -->";
            string endInd = "<!-- end content -->";
            string href = "href='";
            int value = 0;
            
            //extracts all links from the first table that contains parts related by use
            index = htmlText.IndexOf(firstInd);
            htmlText = htmlText.Substring(index + firstInd.Length);
            value = Convert.ToInt32(htmlText.Substring(0, htmlText.IndexOf("parts")).Trim());
            //Console.WriteLine(value);
            //int i = 0;
            if (value != 0)
            {
                do
                {
                    htmlText = htmlText.Substring(htmlText.IndexOf(href) + href.Length);
                    _relatedPartsLinksUse.Add(htmlText.Substring(0, htmlText.IndexOf("'>")));
                    index = htmlText.IndexOf("'>") + 2;
                    htmlText = htmlText.Substring(index);
                    _relatedPartsNamesUse.Add(htmlText.Substring(0, htmlText.IndexOf("</A>")));
                    //Console.WriteLine(i + ": " + htmlText.Substring(0, htmlText.IndexOf("</A>")));
                    //i++;
                }
                while (htmlText.Contains("class='noul_link'"));
            }
            //while (_relatedPartsLinksUse.Count != value);
            
            //extracts all links from the second table that contains parts related by category
            if (!htmlText.Contains("This part is not used in any other parts."))
            {
                index = htmlText.IndexOf(secondInd);
                htmlText = htmlText.Substring(index + secondInd.Length, htmlText.IndexOf(endInd));

                while (htmlText.Contains(href))
                {
                    htmlText = htmlText.Substring(htmlText.IndexOf(href) + href.Length);
                    _relatedPartsLinksCtg.Add(htmlText.Substring(0, htmlText.IndexOf("'>")));
                    index = htmlText.IndexOf("'>") + 2;
                    htmlText = htmlText.Substring(index);
                    _relatedPartsNamesCtg.Add(htmlText.Substring(0, htmlText.IndexOf("</A>")));
                }
            }

        }

        //Returns a string representation of part's related parts by Use and by Category
        public string getRelParts()
        {
            string byUse = string.Join(",", _relatedPartsNamesUse.ToArray());
            string byCat = string.Join(",", _relatedPartsNamesCtg.ToArray());
            return "{0}Related by Use:" + byUse + "{0}Related by Categories:" + byCat;
        }

        //Returns how many parts are related to this part by Use and by Category
        public override string ToString()
        {
            return "{0}Related by -- " + "Uses:" + _relatedPartsNamesUse.Count + "; Categories:" + _relatedPartsNamesCtg.Count;
        }
            
    }
}
