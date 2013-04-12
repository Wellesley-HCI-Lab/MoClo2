using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.IO;

namespace SurfaceApplication1
{
    class PubAbstract
    {
        private string journal;
        private string date;
        private string title;
        private List<string> authors;
        private string affiliation;
        private string abstractText;


        /// <summary>
        /// Links to Entrez Gene and parses source code in order to obtain general information and the abstract
        /// for a particular publication
        /// @ Mikey Lintz & Consuelo Valdes
        /// </summary>
        /// <param name="link"></param>
        public PubAbstract(string link, List<string> authors)
        {
            journal = "";
            date = "";
            title = "";
            this.authors = authors;
            affiliation = "";
            abstractText = "";
            bool linkValid = true;

            #region WebRequest for abstract
            //the first link was set to -1 if no pub titles were found in PubList, hence the url below
            if (link.Equals("http://www.ncbi.nlm.nih.gov-1")) 
            {
                linkValid = false;
            }

            if (linkValid)
            {
                try
                {
                    //indicator strings
                    string abstractHeader = "<h3>Abstract";
                    string JournalHeader = "href=\"#\" title=";
                    string DateHeader = "</a></div><div>"; 
                    string noiseAfterAbstractTag = "<//h3><p>";
                    string noiseAfterJournalHeader = "abstractLink=\"yes\" alsec=\"jour\"";
                    string noiseAfterDateHeader = "</div><h1>";
                    string middle = "</a>";
                    //string authorsHeader = "auth_list\"><a";
                    

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

                    String htmlText = sb.ToString();
                    int index = 0;

                   //Find where the abstract starts
                    index = htmlText.IndexOf(abstractHeader);
                    if (index == -1)
                        abstractText = "{Abstract not found}";
                    else {//Find where abstract ends and save to description
                            htmlText = htmlText.Substring(index + abstractHeader.Length);
                            index = htmlText.IndexOf("</p>");
                            abstractText = htmlText.Substring(noiseAfterAbstractTag.Length-1, index);
                         }
                    if (abstractText.IndexOf("</p>") > 0)//get the last of the html tags
                    {
                        index = abstractText.IndexOf("</p>");
                        abstractText = abstractText.Substring(0, index);
                    }

                    String htmlText2 = sb.ToString(); 
                    int index2 = 0; 

                    index2 = htmlText2.IndexOf(JournalHeader); 
                    if (index2 == -1)
                        journal = "Wrong";
                    else 
                    {
                        htmlText2 = htmlText2.Substring(index2 + JournalHeader.Length);
                        journal = htmlText2.Substring(0, htmlText2.IndexOf(noiseAfterJournalHeader)); 
                    }

                    String htmlText3 = sb.ToString();
                    int index3 = 0; 


                    index3 = htmlText3.IndexOf(JournalHeader);
                    if (index3 == -1)
                        date = "Wrong";
                    else
                    {
                        htmlText3 = htmlText3.Substring(index3 + JournalHeader.Length);
                        date = htmlText3.Substring(htmlText3.IndexOf(middle) + middle.Length, htmlText3.IndexOf(noiseAfterDateHeader) - (htmlText3.IndexOf(middle) + middle.Length)); 
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

        public string getJournal()
        {
            return journal;
        }

        public string getDate()
        {
            return date;
        }

        public string getTitle()
        {
            return title;
        }

        public string getAffiliation()
        {
            return affiliation;
        }

        public string getAbstract()
        {
            return abstractText;
        }

        public override string ToString()
        {
            string tempString = "";

            if (abstractText.Equals(""))
                tempString = "Abstract not available";
            else
            {
                tempString = journal + date + "\n\n" + title + "\n\n";
               
                tempString = tempString.Remove(tempString.Length - 2) + ".";
                tempString += "\n" + affiliation + "\n\n" + abstractText;
            }
            return tempString;
        }

    }
}

//