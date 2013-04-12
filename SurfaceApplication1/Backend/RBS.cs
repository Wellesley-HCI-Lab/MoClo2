using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    public class RBS 
    {
        
        private string _familyName;
        /*
        private int members; //DK
        private int posReg; //DK
        private int negReg; //DK
        */
        private bool _isRBS;

        #region Properties
        public string FamilyName
        {
            get { return _familyName; }
            set { _familyName = value; }
        }

        #endregion
        
        public RBS()
        {
            _isRBS = false;
        }

        /// <summary>
        /// 6/22/2012
        /// Parses source code of RBS in order to obtain information special to RBS
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>
        
        public RBS(String sourceCode)
        {
            _familyName = "";
            
            /*
            members = 0;
            posReg = 0;
            negReg = 0;
            */
            _isRBS = true;

            //For custom RBS: will not have source code needed for scraping, so end here
            if (sourceCode.Equals("custom")) return;

            int index = 0;
            string sc = sourceCode;
            string categoriesBox = "<div style='border:1px solid #aaa;'>";
            string anderInd = "//rbs/prokaryote/constitutive/anderson";
            string commInd = "//rbs/prokaryote/constitutive/community";
            string isaaInd = "//rbs/prokaryote/regulated/issacs";
            string rackInd = "//rbs/prokaryote/constitutive/rackham";
            string miscInd = "//rbs/prokaryote/constitutive/miscellaneous";

            //goes into the code for the 'Categories' box in the bottom right hand corner
            index = sc.IndexOf(categoriesBox);
            sc = sc.Substring(index);

            //finds each family name string on parts page
            if (sc.Contains(anderInd))
            {
                _familyName = "Anderson";
            }
            else if (sc.Contains(commInd))
            {
                _familyName = "Community";
            }
            else if (sc.Contains(isaaInd))
            {
                _familyName = "Isaacs";
            }
            else if (sc.Contains(rackInd))
            {
                _familyName = "Rackham";
            }
            else if (sc.Contains(miscInd))
            {    
                _familyName = "Miscellaneous";
            }
            else
            {
                _familyName = "";
            }
        }
    }
}
