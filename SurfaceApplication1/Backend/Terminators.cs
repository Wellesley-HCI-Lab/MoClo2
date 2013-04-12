using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    public class Terminators
    {
        //all of these are in the parameters box
        private string _biology;
        private string _direction;
        private string _forwardEff;
        private string _reversedVers;
        private string _reversedEff;
        private bool _isTerminator;

        #region Properties
        
        public string Biology
        {
            get { return _biology; }
            set { _biology = value; }
        }

        public string Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public string ForwardEff
        {
            get { return _forwardEff; }
            set { _forwardEff = value; }
        }

        public string ReversedVers
        {
            get { return _reversedVers; }
            set { _reversedVers = value; }
        }

        public string ReversedEff
        {
            get { return _reversedEff; }
            set { _reversedEff = value; }
        }

        public bool IsTerminator
        {
            get { return _isTerminator; }
            set { _isTerminator = value; }
        }

        #endregion

        public Terminators()
        {
            _isTerminator = false;
        }

        /// <summary>
        /// 6/22/2012
        /// Parses source code of terminators in order to obtain information special to terminators
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>
        
        public Terminators(String sourceCode)
        {
            _biology = "No Information";
            _direction = "No Information";
            _forwardEff = "No Information";
            _reversedVers = "No Information";
            _reversedEff = "No Information";
            _isTerminator = true;

            //For custom terminators: will not have source code needed for scraping, so end here
            if (sourceCode.Equals("custom")) return;

            string sc = sourceCode;

            //searches for and obtains the biology of the terminator part
            int index = sc.IndexOf("biology");
            sc = sc.Substring(index);
            index = sc.IndexOf(">") + 1;
            sc = sc.Substring(index);
            _biology = sc.Substring(0, sc.IndexOf("<"));

            //searches for and obtains the direction of the terminator part
            index = sc.IndexOf("direction");
            sc = sc.Substring(index);
            index = sc.IndexOf(">") + 1;
            sc = sc.Substring(index);
            _direction = sc.Substring(0, sc.IndexOf("<"));

            //searches for and obtains the forward efficiency of the terminator part
            index = sc.IndexOf("forward_efficiency");
            sc = sc.Substring(index);
            index = sc.IndexOf(">") + 1;
            sc = sc.Substring(index);
            _forwardEff = sc.Substring(0, sc.IndexOf("<"));

            //searches for and obtains the reversed version of the terminator part
            index = sc.IndexOf("reversed_version");
            sc = sc.Substring(index);
            index = sc.IndexOf(">") + 1;
            sc = sc.Substring(index);
            _reversedVers = sc.Substring(0, sc.IndexOf("<"));

            //searches for and obtains the reverse efficiency of the terminator part
            index = sc.IndexOf("reverse_efficiency");
            sc = sc.Substring(index);
            index = sc.IndexOf(">") + 1;
            sc = sc.Substring(index);
            _reversedEff = sc.Substring(0, sc.IndexOf("<"));
        }

        //returns a string representation of the Terminator objects
        public override string ToString()
        {
            return "{0}Biology:" + _biology + "{0}Direction:" + _direction + "{0}Forward Efficiency:" + _forwardEff
                + "{0}Reversed Version:" + _reversedVers + "{0}Reversed Efficiency:" + _reversedEff;
        }

    }
}
