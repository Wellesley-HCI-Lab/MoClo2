using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO; 

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for customForm.xaml
    /// </summary>
    public partial class customForm : UserControl
    {
        public static SurfaceWindow1 sw1;
        public static int modlevel; //Refers to level of module
        public static object module; //Refers to module being saved

        public customForm()
        {
            InitializeComponent();
            module = new Part();
            modlevel = 0;
        }

        //Saves the custom part data to text file (will be read in results section)
        private void cusSave_Click(object sender, RoutedEventArgs e)
        {
            String path = Directory.GetCurrentDirectory();

            //Save module data to appropriate file
            //if (modlevel == 0)
            path = path.Substring(0, path.IndexOf("bin"));

            StringBuilder data = new StringBuilder();

            //Writing Part data
            if (module.GetType().Equals(typeof(Part)))
            {
                data.Append("P | ");

                //Store Part type: Find checked radio button, retrieve type as string, convert to standard lowercase 3-4 char format (e.g. "prom")
                var checkedButton = cus_Type.Children.OfType<SurfaceRadioButton>().FirstOrDefault(r => (bool)r.IsChecked);
                String type = ((String)((SurfaceRadioButton)checkedButton).Content).ToLower();
                if (type.Length > 4) type = type.Substring(0, 4);
                data.Append(type + " | ");

                //Store Name, Description, Sequence, Associated With
                data.Append(cus_Name.Text + " | ");
                data.Append(cus_Desc.Text + " | ");
                data.Append(cus_Seq.Text + " | ");
                data.Append(cus_Asso.Text);

                path += @"CustomParts.txt";
            }
            else /*if (module is transcriptional unit/L1module*/
            {
                data.Append("T | ");
                data.Append(cus_Name.Text + " | ");
                data.Append(cus_Desc.Text + " | ");
                data.Append(cus_Seq.Text + " | ");
                data.Append(cus_Asso.Text + "\n");

                L1Module tu = (L1Module)module;
                data.Append(partToData(tu.L1Prom) + "\n");
                data.Append(partToData(tu.L1RBS) + "\n");
                data.Append(partToData(tu.L1CDS) + "\n");
                data.Append(partToData(tu.L1Term) + "\n");
                data.Append("T | end");

                path += @"CustomTUs.txt";
            }

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(data);
            }


            cusCancel_Click(sender, e);

            //If currently viewing Custom Parts library, refresh listed results
            if (sw1.L0.PartTypeSelected == "cus") sw1.L0.searchByFilter();
            //Or Custom TU tab
            if (sw1.L1.L1_buildTabs.SelectedIndex == 2) sw1.L1.addCustomTUs();
        }

        //Converts data directly from a Part into the text file format
        private String partToData(Part p)
        {
            StringBuilder data = new StringBuilder("\tP | ");
            data.Append(p.Type + " | ");
            data.Append(p.partCategory.Text + " | ");
            data.Append("no description | ");
            data.Append(p.myRegDS.BasicInfo.Sequence + " | ");
            data.Append("no associations");

            return data.ToString();
        }

        public Part dataToPart(String ThisLine)
        {
            string[] SplitLine = ThisLine.Split('|');
            Part p = new Part(SplitLine[1].Trim(), true);
            p.Type = SplitLine[1].Trim();
            p.partCategory.Text = SplitLine[2].Trim();

            //Enter information in RegDataSheet: description, sequence, associated with
            p.myRegDS = new RegDataSheet(SplitLine);

            return p;
        }

        //Resets fields on the form and makes the form invisible
        private void cusCancel_Click(object sender, RoutedEventArgs e)
        {
            cusForm.Visibility = Visibility.Collapsed;
            cus_Type.Visibility = Visibility.Visible;
            cus_prom.IsChecked = true;
            cus_Name.Text = "";
            cus_Desc.Text = "";
            cus_Seq.Text = "";
            cus_Asso.Text = "";
        }
    }
}
