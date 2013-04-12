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
    class PubList
    {
        List<string> titles;
        List<string> links;
        List<string> authors;
        string totalNumberOfArticles = "";
        int count;
        string id;

        #region Properties
        public List<string> Titles
        {
            get { return titles; }
            set { titles = value; }
        }

        public List<string> Links
        {
            get { return links; }
            set { links = value; }
        }

        public List<string> Authors
        {
            get { return authors; }
            set { authors = value; }
        }
        #endregion

        /// <summary>
        /// 3/14/10
        /// Creates lists of publication titles and links to abstracts by parsing source code from Entrez Gene search page 
        /// and then parsing PubMed search results page.
        /// @ Mikey Lintz & Consuelo Valdes
        /// </summary>
        /// <param name="geneID">Name of gene to search for in Entrez.</param>
        public PubList(string query)
        {
            titles = new List<string>();
            links = new List<string>();
            authors = new List<string>();
            id = query;

            /*
            # region HTML WebRequest

            //Go to main gene page in Entrez and find PubMed ID for gene
            // used to build entire input
            StringBuilder sb = new StringBuilder();

            // used on each read operation
            byte[] buf = new byte[8192];

            // prepare the web page we will be asking for
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create("http://www.ncbi.nlm.nih.gov/sites/entrez?db=gene&cmd=search&term=" + geneID);

            // execute the request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            string tempString = null;
            count = 0;
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
            String htmlText = sb.ToString();
            #endregion

            
            #region Parsing Entrez gene results for gene pubmed reference # in page source code

            String pubMedNumberIDIndicatorTag = "IdsFromResult=";
            //Find PubMedID for gene in htmlText
            int index = htmlText.IndexOf(pubMedNumberIDIndicatorTag) + pubMedNumberIDIndicatorTag.Length;
            String entrezGeneReference = htmlText.Substring(index, 10);

            //parse it to only contain numbers for PubMedID
            index = entrezGeneReference.IndexOf("\"");//close tag of pubmed numerical reference
            if (index < 1)
            {
                returnNoPubs();
                return;
            }
            else
                entrezGeneReference = entrezGeneReference.Substring(0, index);

            #endregion
            */

            // used to build entire input
            StringBuilder sb = new StringBuilder();
            String htmlText = "";

            // used on each read operation
            byte[] buf = new byte[8192];

            #region Go to pubmed search results page with reference number from previous region and make WebRequest
            //Create uri for pubMed search results page from PubMedID
            Uri toPubs = new Uri("http://www.ncbi.nlm.nih.gov/pubmed?term=" + id);

            // prepare to ask for PubMed links and parse the page to gather links & titles to publications
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(toPubs);

            // execute the request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // we will read data via the response stream
            Stream resStream = response.GetResponseStream();
            string tempString = null;
            count = 0;
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
            htmlText = sb.ToString();
            #endregion

            #region Search Pubmed for results and parse code for publication links and authors

            //Link template for PubMed abstracts
            String link = "http://www.ncbi.nlm.nih.gov/pubmed/";
            //Vars necessary to parse the source code
            Char[] delimiters = { ',' };
            String htmlRefToArticles = "href=\"/pubmed/";
            String htmlRefToArticleTitles = "\" ref=\"ordinalpos=";
            String noisyArticleReference = "&amp;log$=free";
            String authorsListStart = "<p class=\"desc\">";
            String totalNumberOfArticlesHeading = "Results:";

            //Parsing to get the number of total articles in results. Comes in the form of Results: # to # with a close tag "<"
            //or Results: #
            int index = htmlText.IndexOf(totalNumberOfArticlesHeading);
            htmlText = htmlText.Substring(index + totalNumberOfArticlesHeading.Length);
            index = htmlText.IndexOf("of ");

            if (index > 9) { index = htmlText.IndexOf("<"); }
            else//Case: Results: # to # with a close tag "<"
            {
                htmlText = htmlText.Substring(index + 3);
                index = htmlText.IndexOf("<");
            }
            totalNumberOfArticles = htmlText.Substring(0, index);

            //Find the beginning of the search results in the HTML
            index = htmlText.IndexOf("rprt");

            if (index > 0)
            {
                htmlText = htmlText.Substring(index);
                //Try to parse page source. 30 because we want at most 15 articles and for each article there is an additional loop
                //to account for noise. -CV, 3/22/10

                    for (int i = 1; i < (Convert.ToInt32(totalNumberOfArticles) + 1) && i < 15; i++)
                       
                    {
                        Console.WriteLine(i);
                        
                        index = htmlText.IndexOf(htmlRefToArticles);
                        htmlText = htmlText.Substring(index + htmlRefToArticles.Length);
                        index = htmlText.IndexOf(htmlRefToArticleTitles);

                        tempString = htmlText.Substring(0, index + htmlRefToArticleTitles.Length + 15);
                        if (!tempString.Contains(noisyArticleReference))
                        {
                            //article reference found
                            index = htmlText.IndexOf("\"");
                            tempString = htmlText.Substring(0, index);//gets number reference to article for templating
                            links.Add(link + tempString);

                            index = htmlText.IndexOf(">");
                            htmlText = htmlText.Substring(index + 1);
                            index = htmlText.IndexOf("</a>");
                            tempString = htmlText.Substring(0, index);
                            if (tempString.StartsWith(">"))
                            {
                                tempString = tempString.Substring(1);
                                titles.Add(tempString);

                                if (index == -1)
                                    break;
                            }
                            titles.Add(removeTags(tempString));

                            if (index == -1)
                                break;

                            if (authors.Count() < 15)//After you do the 15th, you get nothing but noise. -CV, 3/22/10
                            {
                                index = htmlText.IndexOf(authorsListStart);
                                htmlText = htmlText.Substring(index + authorsListStart.Length);
                                index = htmlText.IndexOf("</p>");
                                authors.Add(htmlText.Substring(0, index) + "\n\n");

                                if (index == -1)
                                    break;
                            }
                        }
                    }
                

            }
            #endregion

            resStream.Close();
            response.Close();

        }

        /// <summary>
        /// If no publication titles can be obtained, sets the first title to "Publications not available".
        /// @ Sarah Elfenbein
        /// </summary>
        private void returnNoPubs()
        {
            if (count < 1)
            {
                titles.Add("Publications not available");
                links.Add("-1");             //this -1 is used in PubAbstract to determine if there's a valid link
                count++;
            }
        }

        public static string removeTags(string input)
        {
            return input.Replace("</b>", "").Replace("<b>", "");
        }

        public List<string> getTitles()
        {
            return titles;
        }

        public List<string> getLinks()
        {
            return links;
        }

        public List<string> getAuthors()
        {
            return authors;
        }

        public string getQuery()
        {
            return id;
        }

        public string getTotalNumberOfArticles()
        {
            return totalNumberOfArticles;
        }

    }
}
