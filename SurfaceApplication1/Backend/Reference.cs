using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    public class Reference
    {
        private string _author;
        private string _group;
        private string _date;
        //private List<string> wikiLinks; 

        #region Properties
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        public string Group
        {
            get { return _group; }
            set { _group = value; }
        }

        public string Date
        {
            get { return _date; }
            set { _date = value; }
        }
        #endregion

        public Reference()
        {
            _author = "";
            _group = "";
            _date = "";
            //wikiLinks = new List<string>();
        }

        //Test constructor for custom Parts: contains DESCRIPTION and ASSOCIATED WITH
        public Reference(String d, String a)
        {
            _author = d;
            _group = a;
            _date = "";
        }

        /// <summary>
        /// 6/21/2012
        /// Takes the source code and extracts the following line: "Designed by xxx, Group xxx, Date xxx"
        /// and puts information in appropriate instance variables
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>

        public Reference(String sourceCode)
        {
            _author = "No Information";
            _group = "No Information";
            _date = "No Information";
            //wikiLinks = new List<string>();

            //searches for the author by using the string "Designed By" in the source code
            int index = 0;
            string sc = sourceCode;
            string designedBy = "Designed by";
            string temp = "";
            string tab = "&nbsp;";
            string groupInd = "Group:";

            if (sc.Contains(designedBy))
            {
                index = sc.IndexOf(designedBy);
                sc = sc.Substring(index + designedBy.Length);
                temp = sc.Substring(0, sc.IndexOf(tab));
                _author = temp.Trim();
            }
                //searches for the group by using the string "Group:" in the source code

            if (sc.Contains(groupInd))
            {
                sc = sc.Substring(sc.IndexOf(groupInd));
                temp = sc.Substring(groupInd.Length, sc.IndexOf(tab) - groupInd.Length);
                _group = temp.Trim();

                //searches for and obtains the date
                sc = sc.Substring(sc.IndexOf("("));
                temp = sc.Substring(1, sc.IndexOf(")") - 1);
                _date = temp.Trim();
            }
   
        }

        public string getAuthors()
        {
            return _author;
            
        }

        public string getGroup()
        {
            return _group;
        }

        public string getDate()
        {
            return _date;
        }

        //returns all of the reference info parsed from each part page's source code
        //in the specified format
        public override string ToString()
        {
            return "{0}Author(s):" + _author + "{0}Group:" + _group + "{0}Date:" + _date;
        }
    }
}
