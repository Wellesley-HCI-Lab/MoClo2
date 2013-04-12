using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    class EugeneModules
    {
        public String _SelectedProm; //take in the selected promoters previously chosen
        public String _SelectedRBS; //take in seleted RBS
        public String _SelectedCDS; //take in selected CDS
        public String _SelectedTerm; //take in selected terminator

        public static SurfaceWindow1 sw1;

        private StreamReader sr;

        ProcessStartInfo properties;
        Process process;

        public EugeneModules()
        {
            //this.properties = new ProcessStartInfo("cmd.exe");  // run Command executable
            //this.properties.CreateNoWindow = true;              // suppress opening of a UI
            //this.properties.UseShellExecute = false;
            //this.properties.RedirectStandardError = true;       // redirect the input, output, and errors through the Surface UI
            //this.properties.RedirectStandardInput = true;
            //this.properties.RedirectStandardOutput = true;
            
            //this.process = new Process();
        }

        /// <summary>
        /// Generates and prints device permutations from all available parts.  
        /// Obeys set permutation rules.
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <returns> All available device permutations from the parts given. </returns>
       
        public List<L2Module> Permute(List<L1Module> ListModulesToPermute)
        {
            List<L2Module> L2ModulesToReturn = new List<L2Module>();

            //Get current working directory
            string file = Directory.GetCurrentDirectory();
            //change directory to EugeneFiles directory and read text file based on ListModulesToPermute count
            file = file.Substring(0,file.IndexOf("bin")) + @"Resources\EugeneFiles\permute" + ListModulesToPermute.Count + ".txt";     
            string text = "";
            StreamReader sr = new StreamReader(file);
            while (!sr.EndOfStream) //read to end of file
            {
                text = sr.ReadLine().ToString();
            }
            sr.Close();

            text = text.Replace("Device hybridDevice_", "");
            text = text.Replace("absDevice", "");
            //parsing text file and adding permutations to the L2 module
            string[] textminusparentheses = text.Split(new char[]{'(',')'});

            foreach (string numberasstringcomma in textminusparentheses)
            {                
                if (numberasstringcomma.Replace(" ", "").Length > 2 && !(numberasstringcomma.StartsWith(","))) //go through each individual permutation
                {
                    string[] permOrderArray = numberasstringcomma.Split(new char[] { ',' });
                    //clean--> to do
                    L2Module tempL2Module = new L2Module();
                    foreach (string permNumberString in permOrderArray)
                    {
                        int index = Convert.ToInt32(permNumberString) -1;

                        L1Module childToAdd = ListModulesToPermute.ElementAt(index).clone();
                        childToAdd.IsManipulationEnabled = false;
                        //Disable manipulation and assoc. events
                        childToAdd.Background = sw1.L2.L1MColors.ElementAt(tempL2Module.Children.Count - 1);
                        //childToAdd.BorderBrush = childToAdd.Background;
                        tempL2Module.L2M.Children.Add(childToAdd);
                    }
                    L2ModulesToReturn.Add(tempL2Module);
                }
                
            }

            return L2ModulesToReturn;
        }   
    
        


        }
    }    
//}
