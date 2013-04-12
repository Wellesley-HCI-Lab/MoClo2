using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    class Search
    {
        private List<RegDataSheet> regDS; //List of all the Parts Data Sheets related to the search query
        private List<string> pubMedTitles;
        private List<string> pubMedLinks;

        /// <summary>
        /// 6/26/2012
        /// Parses through the Parts Registry and gathers information on parts related to the search query. 
        /// Combines results with related PubMed sources.
        /// Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="query">Search query that user inputs</param>

        #region Properties
        public List<RegDataSheet> RegDS
        {
            get { return regDS; }
            set { regDS = value; }
        }

        public List<string> PubMedTitles
        {
            get { return pubMedTitles; }
            set { pubMedTitles = value; }
        }

        public List<string> PubMedLinks
        {
            get { return pubMedLinks; }
            set { pubMedLinks = value; }
        }

        #endregion

        public Search(string query)
        {

            regDS = new List<RegDataSheet>();

            RegList r = new RegList(query);
            
            //goes through the links on the results page and creates a RegDataSheet from the pages
            if (r.TitleLinks.Count != 0)
            {

                if (r.TitleLinks[0] != "-1")
                {
                    for (int i = 0; i < r.TitleLinks.Count; i++)
                    {

                        RegDataSheet reg = new RegDataSheet(r.TitleLinks[i]);

                        regDS.Add(reg);
                    }
                }
            }

            if (r.TextLinks.Count != 0)
            {

                if (r.TextLinks[0] != "-1")
                {
                    for (int i = 0; i < r.TextLinks.Count; i++)
                    {
                        
                        RegDataSheet reg = new RegDataSheet(r.TextLinks[i]);
                        regDS.Add(reg);
                        //Console.WriteLine(i + ": " + r.TextLinks[i]);
                    }
                }
            }

            ////Creates a list of PubMed source related to the query
            //PubList p = new PubList(query);
            //pubMedTitles = p.Titles;
            //pubMedLinks = p.Links;

        }

        public override string ToString()
        {
            string[] regDSArr = new string[regDS.Count];
            for (int i = 0; i < regDS.Count; i++)
            {
                regDSArr[i] = regDS[i].ToString();
            }
            string regFullString = string.Join("{0}{0}", regDSArr) + "{0}";
            
            if(pubMedTitles.Count != 0)
                if(pubMedTitles[0] == "Publications not available")
                    return regFullString + "{0}----Other Sources" + "{0}PubMed Sources: No Matches Found";
            return regFullString + "{0}----Other Sources" + "{0}PubMed Sources:{0}" +
                        string.Join("{0}", pubMedTitles.ToArray());
        }

    }
}
