using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    public class Promoter 
    {
        private List<string> _regulation; //categories box
        private string _posReg; //parameters box (comes after categories box)
        private string _negReg; //parameters box
        private bool _isPromoter;

        #region Properties
        public List<string> Regulation
        {
            get { return _regulation; }
            set { _regulation = value; }
        }

        public string PosReg
        {
            get { return _posReg; }
            set { _posReg = value; }
        }

        public string NegReg
        {
            get { return _negReg; }
            set { _negReg = value; }
        }

        public bool IsPromoter
        {
            get { return _isPromoter; }
            set { _isPromoter = value; }
        }
        
        #endregion


        public Promoter()
        {
            _isPromoter = false;
        }

        /// <summary>
        /// 6/22/2012
        /// Parses source code of promoters in order to obtain information special to promoters
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>
        
        public Promoter(String sourceCode)
        {
            _regulation = new List<string>();
            _posReg = "No Information";
            _negReg = "No Information";
            _isPromoter = true;

            //For custom Promoters: will not have source code needed for scraping, so end here
            if (sourceCode.Equals("custom")) return;

            int index = 0;

            string sc = sourceCode;
            string categoriesBox = "<div style='border:1px solid #aaa;'>";
            string regInd = "//regulation/";

            //goes into the code for the 'Categories' box in the bottom right hand corner
            index = sc.IndexOf(categoriesBox);
            sc = sc.Substring(index);

            //enters the code for the 'Parameters' box in the bottom left hand corner
            while (sc.Contains(regInd))
            {
                index = sc.IndexOf(regInd) + regInd.Length;
                sc = sc.Substring(index);
                _regulation.Add(transReg(sc.Substring(0, sc.IndexOf("<"))));
            }

            //obtains the string/category of 'negative regulators'
            if (sc.Contains("negative_regulators"))
            {
                index = sc.IndexOf("negative_regulators");
                sc = sc.Substring(index);
                index = sc.IndexOf(">");
                sc = sc.Substring(index + 1);
                _negReg = sc.Substring(0, sc.IndexOf("<"));
            }

            //obtains the string/category of 'positive regulators'
            if (sc.Contains("positive_regulators"))
            {
                index = sc.IndexOf("positive_regulators");
                sc = sc.Substring(index);
                index = sc.IndexOf(">");
                sc = sc.Substring(index + 1);
                _posReg = sc.Substring(0, sc.IndexOf("<"));
            }
            
        }

        //Converts strings to more preferred terms. The registry has the regulation terms as positive/negative,
        //when it should be inducible/repressible.
        public string transReg(string regulation)
        {

            if (regulation == "constitutive\n")
                return "Constitutive";
            if (regulation == "positive\n")
                return "Inducible";
            if (regulation == "negative\n")
                return "Repressible";
            if (regulation == "multiple\n")
                return "Multiple";
            return regulation;
        }

        //Returns a string representation of the part's regulation
        //EDITED TO DISPLAY MULTIPLE CORRECTLY; stores string in regString rather than direct return
        public string getReg()
        {
            String regString;
            if (_regulation != null)
            {
                regString = string.Join(",", _regulation.ToArray());
                if (regString.Contains("Multiple")) return "Multiple";
                return regString;
            }
            return "No Information";
        }

        //returns a string representation of the Promoter object
        public override string ToString()
        {
            if (_regulation != null)
            {
                string temp = string.Join(",", _regulation.ToArray());
                return "{0}Positive Regulation:" + _posReg + "{0}Negative Regulation:" + _negReg + "{0}Regulation:" + temp;
            }

            return "{0}Positive Regulation:" + _posReg + "{0}Negative Regulation:" + _negReg;
        }
    }
}
