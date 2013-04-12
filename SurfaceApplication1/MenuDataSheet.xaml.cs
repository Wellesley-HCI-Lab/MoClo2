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
using System.Windows.Interop;

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for MenuDataSheet.xaml
    /// </summary>
    public partial class MenuDataSheet : ScatterViewItem
    {
        public MenuDataSheet()
        {
            InitializeComponent();
        }

        public void PopulateDataSheet(String data, int rowIndex, int colIndex, Grid GridName)
        {
            TextBlock dataBlock = new TextBlock();
            dataBlock.FontSize = 18;
            dataBlock.VerticalAlignment = VerticalAlignment.Center;

            dataBlock.Background = Brushes.LightSteelBlue;
            dataBlock.Width = 580;
            dataBlock.Height = 70;

            GridName.Children.Add(dataBlock);
            
            //GridName.Background = Brushes.Tomato
            Grid.SetRow(dataBlock, rowIndex);
            
            GridName.ShowGridLines = false;
            Grid.SetColumn(dataBlock, colIndex);
            dataBlock.Text = data;

            if (dataBlock.Text == "No Info")
            {
                dataBlock.Text = "Prokaryote/ecoli"; 
            }

        }


        public void PopulateDataSheet(String data, int rowIndex, int colIndex, Grid GridName, string tab)
        {
            if (tab == "seq")
            {
                GeneSequence.FontSize = 18;
                GeneSequence.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                GeneSequence.Background = Brushes.LightSteelBlue;
                GeneSequence.Width = 580;
                GeneSequence.Height = 571;

                GeneSequence.Text = data;
            }

            else
            {

                TextBlock dataBlock = new TextBlock();
                dataBlock.FontSize = 18;
                dataBlock.VerticalAlignment = VerticalAlignment.Center;

                dataBlock.Background = Brushes.LightSteelBlue;
                dataBlock.Width = 580;
                dataBlock.Height = 70;

                if (tab.Equals("author"))
                    dataBlock.Height = 196;
                if (tab.Equals("seq"))
                    dataBlock.Height = 571;
                if (tab.Equals("length"))
                    dataBlock.Height = 25;

                GridName.Children.Add(dataBlock);

                //GridName.Background = Brushes.Tomato
                Grid.SetRow(dataBlock, rowIndex);

                GridName.ShowGridLines = false;
                Grid.SetColumn(dataBlock, colIndex);
                dataBlock.Text = data;
            }
        }


        private void DataSheetTab_TouchDown(object sender, TouchEventArgs e)
        {
            TabItem tab = (TabItem)sender;
            DataSheetTabControl.SelectedItem = tab;
            e.Handled = true;
        }


        private void CopySequence_TouchDown(object sender, TouchEventArgs e)
        {
            Clipboard.SetText(GeneSequence.Text);  
        }


    }

    
}
