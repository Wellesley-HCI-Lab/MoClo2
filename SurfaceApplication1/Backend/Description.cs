using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SurfaceApplication1
{
    public class Description
    {
        /*
        private string functions; //DK
        private string uses; //DK
        private string pictures; //DK
        private int promotStrength; //DK
        */
        private List<bool> _assemCompat;
        private List<string> _assembIncomp10; //"index" 0
        private List<string> _assembIncomp12; //"index" 1
        private List<string> _assembIncomp21; //"index" 2
        private List<string> _assembIncomp23; //"index" 3
        private List<string> _assembIncomp25; //"index" 4
        private List<string> _chassis;

        //For custom parts only
        private String _desc;

        #region Properties
        public String Desc
        {
            get { return _desc; }
            set { _desc = value; }
        }

        public List<string> Chassis
        {
            get { return _chassis; }
            set { _chassis = value; }
        }

        public List<bool> AssemblyCompatibility
        {
            get { return _assemCompat; }
            set { _assemCompat = value; }
        }

        public List<string> AssemblyIncompatibiltiy10
        {
            get { return _assembIncomp10; }
            set { _assembIncomp10 = value; }
        }

        public List<string> AssemblyIncompatibiltiy12
        {
            get { return _assembIncomp12; }
            set { _assembIncomp12 = value; }
        }

        public List<string> AssemblyIncompatibiltiy21
        {
            get { return _assembIncomp21; }
            set { _assembIncomp21 = value; }
        }

        public List<string> AssemblyIncompatibiltiy23
        {
            get { return _assembIncomp23; }
            set { _assembIncomp23 = value; }
        }

        public List<string> AssemblyIncompatibiltiy25
        {
            get { return _assembIncomp25; }
            set { _assembIncomp25 = value; }
        }

        #endregion

        public Description()
        {
            /*
            functions = "";
            uses = "";
            pictures = "";
            promotStrength = 0;
            */
            _assemCompat = new List<bool>();
            _assembIncomp10 = new List<string>();
            _assembIncomp12 = new List<string>();
            _assembIncomp21 = new List<string>();
            _assembIncomp23 = new List<string>();
            _assembIncomp25 = new List<string>();
            _chassis = new List<string>();

        }

        /// <summary>
        /// 6/22/2012
        /// Links to Parts Registry pages and parses source code in order to obtain description information 
        /// for a particular part
        /// @ Nicole Francisco & Veronica Lin
        /// </summary>
        /// <param name="sourceCode">Source code of a particular page</param>

        public Description(String sourceCode)
        {
            /*
            functions = "";
            uses = "";
            pictures = "";
            promotStrength = 0;
            */

            _assemCompat = new List<bool>();
            _assembIncomp10 = new List<string>();
            _assembIncomp12 = new List<string>();
            _assembIncomp21 = new List<string>();
            _assembIncomp23 = new List<string>();
            _assembIncomp25 = new List<string>();
            _chassis = new List<string>();

            int index = 0;
            string sc = sourceCode;
            string assembCompInd = "Assembly Compatibility:";
            string assembRed = "<LI class='boxctrl box_red'>";
            string compatMsg = ">COMPATIBLE WITH RFC[";
            string categoriesBox = "<div style='border:1px solid #aaa;'>";
            string chassisInd = "//chassis/";
            string tempString = sourceCode;

            //Looks at block of code containing information on the assembly compatibility for the part
            if (sc.Contains(assembCompInd))
            {
                index = sc.IndexOf(assembCompInd);
                sc = sc.Substring(index);
                string ulBlock = sc.Substring(0, sc.IndexOf("</UL>"));
                
                for (int i = 0; i < 5; i++)
                {
                    index = ulBlock.IndexOf("<LI class");
                    string liBlock = ulBlock.Substring(index);
                    liBlock = liBlock.Substring(0, liBlock.IndexOf("</LI>"));
                    //if part is compatible
                    if (liBlock.Contains(compatMsg))
                    {
                        _assemCompat.Add(true);
                        index = ulBlock.IndexOf("</div></LI>") + "</div></LI>".Length;
                        ulBlock = ulBlock.Substring(index);
                        continue;
                    }
                    //if part is incompatible
                    int compatNumInd = liBlock.IndexOf(assembRed) + assembRed.Length;
                    string compatNum = liBlock.Substring(compatNumInd, liBlock.IndexOf("<DIV class='box'>") - compatNumInd);
                    _assemCompat.Add(false);
                    //Find illegal sites listed
                    int incompatNumInd = liBlock.IndexOf("]</div>") + "]</div>".Length;
                    string illegalFound = liBlock.Substring(incompatNumInd, liBlock.IndexOf("<BR></div>") - incompatNumInd);
                    string[] illegalSites = Regex.Split(illegalFound, "<BR>");
                    //adds information of what illegal sites are found to appropriate lists
                    if (compatNum == "10") _assembIncomp10 = illegalSites.ToList();
                    if (compatNum == "12") _assembIncomp12 = illegalSites.ToList();
                    if (compatNum == "21") _assembIncomp21 = illegalSites.ToList();
                    if (compatNum == "23") _assembIncomp23 = illegalSites.ToList();
                    if (compatNum == "25") _assembIncomp25 = illegalSites.ToList();
                    index = ulBlock.IndexOf("</div></LI>") + "</div></LI>".Length;
                    ulBlock = ulBlock.Substring(index);                           
                }

            }

            //goes into the code for the 'Categories' box in the bottom right hand corner
            index = sc.IndexOf(categoriesBox);
            sc = sc.Substring(index);

            //finds chassis string on parts page
            while (sc.Contains(chassisInd))
            {
                index = sc.IndexOf(chassisInd) + chassisInd.Length;
                sc = sc.Substring(index);
                _chassis.Add(sc.Substring(0, sc.IndexOf("\n")));
            }

        }

        //Returns a string representation of assembly compatibility with certain RFCs
        public string assembCompString()
        {
            if (_assemCompat.Count != 0)
            {
                return "{0}Compatible with RFC[10]? " + _assemCompat[0] +
                        "{0}Compatible with RFC[12]? " + _assemCompat[1] +
                        "{0}Compatible with RFC[21]? " + _assemCompat[2] +
                        "{0}Compatible with RFC[23]? " + _assemCompat[3] +
                        "{0}Compatible with RFC[25]? " + _assemCompat[4];
            }

            return "No Information";
         }

        //Returns a string representation of part's chassis
        public string chassisString()
        {
            if(_chassis.Count != 0){
                return string.Join(",", _chassis.ToArray());
            }

            return "No Info";
        }

        //returns description info parsed from each part page's source code
        public override string ToString()
        {
            return string.Join(",", _chassis.ToArray()) + "{0}" + assembCompString();
        }
    }
}
