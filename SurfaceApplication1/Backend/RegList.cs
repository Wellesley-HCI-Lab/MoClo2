using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Windows.Threading;
using System.IO;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace SurfaceApplication1
{
    class RegList
    {
        private List<string> _titleHead;
        private List<string> _titleLinks; //links of pages whose titles match the query
        private List<string> _textHead;
        private List<string> _textLinks; //links of pages whose content match the query
        private string _totalNumberOfArticles;
        private int _count;
        private string _id;
        private bool _titleMatches;

        #region Properties

        public List<string> TitleHead
        {
            get { return _titleHead; }
            set { _titleHead = value; }
        }

        public List<string> TitleLinks
        {
            get { return _titleLinks; }
            set { _titleLinks = value; }
        }

        public List<string> TextHead
        {
            get { return _textHead; }
            set { _textHead = value; }
        }

        public List<string> TextLinks
        {
            get { return _textLinks; }
            set { _textLinks = value; }
        }

        public string TotalNumberOfArticles
        {
            get { return _totalNumberOfArticles; }
            set { _totalNumberOfArticles = value; }
        }

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public bool AreTitleMatches
        {
            get { return _titleMatches; }
            set { _titleMatches = value; }
        }


        #endregion

        /// <summary>
        /// 6/20/2012
        /// Creates lists of names of parts and links to their pages by  publication titles by parsing  
        /// Parts Registry search results page
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="query">Search query inputted to Parts Registry search engine.</param>
        
        public RegList(string query)
        {
            _titleHead = new List<string>();
            _titleLinks = new List<string>();
            _textHead= new List<string>();
            _textLinks = new List<string>();
            _totalNumberOfArticles = "";
            _id = query;
            _titleMatches = true;
            
            if (string.IsNullOrEmpty(_id) == false)
            {
                _id.Trim();
                _id.Replace(" ", "+");
            }//ensures that query can create a valid link

            try
            {
                # region HTML WebRequest

                //Go to main gene page in Entrez and find PubMed ID for gene
                // used to build entire input
                StringBuilder sb = new StringBuilder();

                // used on each read operation
                byte[] buf = new byte[8192];

                // prepare the web page we will be asking for
                HttpWebRequest request =
                    (HttpWebRequest)WebRequest.Create("http://partsregistry.org/Special:Search?search=" + query + "&fulltext=Search");
                // Using the leading string instead of "&go=Go" will always ensure that the results page will be processed. With
                // the go string, if a query entered matches the title of a page, the link will lead the user straight to that page.

                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // we will read data via the response stream
                Stream resStream = response.GetResponseStream();
                string tempString = null;
                _count = 0;

                do
                {
                    // fill the buffer with data
                    _count = resStream.Read(buf, 0, buf.Length);

                    // make sure we read some data
                    if (_count != 0)
                    {
                        // translate from bytes to ASCII text
                        tempString = Encoding.ASCII.GetString(buf, 0, _count);

                        // continue building the string
                        sb.Append(tempString);
                    }
                }
                while (_count > 0); // any more data to read?
                String htmlText = sb.ToString();

                #endregion

                #region Search Parts Registry for results and parse code for publication links and authors (difference between page text and page title matches)

                // Link template for Parts Registry biological parts
                String link = "http://partsregistry.org/";
                // Vars necessary to parse the source code
                Char[] delimiters = { ',' };
                String htmlRefToArticles = "<li><a href=\"/";
                String htmlRefToArticleTitles = "\" title=\"";
                String totalNumberOfArticlesHeading = "Showing below";
                int index = 0;

                /*******/
                // Parsing to get the number of total results.
                // Has 3 cases depending on the number of results that show up from the query inputted
                // 1) Too many results because of a very generic query like "lacz" or "yeast", in which case
                // the message is: "Showing below up to 20 results starting with #1", or
                // 2) 'Countable' number of results, in which case the message is: "Showing below # results starting
                // with #1", or
                // 3) No articles show up at all.

                if (htmlText.Contains(totalNumberOfArticlesHeading))
                {
                    index = htmlText.IndexOf(totalNumberOfArticlesHeading);
                    htmlText = htmlText.Substring(index + totalNumberOfArticlesHeading.Length);

                    if (htmlText.Contains("up to"))
                    {
                        index = htmlText.IndexOf("</p>");
                        _totalNumberOfArticles = "{>500 Results}";
                    }
                    else
                    {
                        String startStr = "<b>";
                        int start = htmlText.IndexOf(startStr);
                        _totalNumberOfArticles = htmlText.Substring(start + startStr.Length, htmlText.IndexOf("<"));
                    }
                }
                else
                {
                    _totalNumberOfArticles = "{There are no results for this query}";
                }
                /*******/
                // Find the beginning of the search results in the HTML
                index = htmlText.IndexOf("<a name=\"");
                htmlText = htmlText.Substring(index);


                // Results are separated into two sections: "Page title matches" and "Page text matches"
                // Case: Results under "Page title matches"

                string endResult = "</li>\n</ol>";

                do
                {
                    if (htmlText.Contains("No page title matches"))
                    {
                        _titleMatches = false;
                        _titleLinks.Add("-1");
                        break;
                    }

                    // Grab the link and add it onto appropriate list
                    index = htmlText.IndexOf(htmlRefToArticles);
                    htmlText = htmlText.Substring(index + htmlRefToArticles.Length);
                    tempString = htmlText.Substring(0, htmlText.IndexOf(htmlRefToArticleTitles));
                    //Console.WriteLine(tempString);
                    if (!tempString.Contains("Part:")) continue; //does not include non-parts pages in list
                    tempString = tempString.Substring(tempString.IndexOf("Part:") + "Part:".Length);
                    if (tempString.Contains(":")) tempString = tempString.Remove(tempString.LastIndexOf(":"));
                    _titleLinks.Add(link + "Part:" + tempString);

                    // Do the same for title of page linked to
                    index = htmlText.IndexOf(htmlRefToArticleTitles);
                    htmlText = htmlText.Substring(index + htmlRefToArticleTitles.Length);
                    tempString = htmlText.Substring(0, htmlText.IndexOf("\""));
                    tempString = tempString.Substring(tempString.IndexOf("Part:") + "Part:".Length);
                    if (tempString.Contains(":")) tempString = tempString.Remove(tempString.LastIndexOf(":"));
                    _titleHead.Add(tempString);

                } while (htmlText.Substring(htmlText.IndexOf(tempString), htmlText.IndexOf(endResult)).Contains("<li>"));

                // Case: Results under "Page text matches"
                if (!_titleMatches)
                {
                    index = htmlText.IndexOf("</h2>");
                }
                else
                {
                    index = htmlText.IndexOf(endResult);
                }

                htmlText = htmlText.Substring(index);

                do
                {
                    if (htmlText.Contains("No page text matches"))
                    {
                        _textLinks.Add("-1");
                        break;
                    }

                    // Grab the link and add it onto appropriate list
                    index = htmlText.IndexOf(htmlRefToArticles);
                    htmlText = htmlText.Substring(index + htmlRefToArticles.Length);
                    tempString = htmlText.Substring(0, htmlText.IndexOf("\""));
                    if (!tempString.Contains("Part:")) continue; //does not include non-parts pages in list
                    tempString = tempString.Substring(tempString.IndexOf("Part:") + "Part:".Length);
                    if (tempString.Contains(":")) tempString = tempString.Remove(tempString.LastIndexOf(":"));
                    //Console.WriteLine(tempString);
                    _textLinks.Add(link + "Part:" + tempString);

                    // Do the same for title of page linked to
                    index = htmlText.IndexOf(htmlRefToArticleTitles);
                    htmlText = htmlText.Substring(index + htmlRefToArticleTitles.Length);
                    tempString = htmlText.Substring(0, htmlText.IndexOf("\""));
                    tempString = tempString.Substring(tempString.IndexOf("Part:") + "Part:".Length);
                    if (tempString.Contains(":")) tempString = tempString.Remove(tempString.LastIndexOf(":"));
                    _textHead.Add(tempString);

                } while (htmlText.Substring(htmlText.IndexOf(tempString), htmlText.IndexOf(endResult)).Contains("<li>"));

                #endregion

                resStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in reglist: " + e.Message);
            }

        }

            
    }
}