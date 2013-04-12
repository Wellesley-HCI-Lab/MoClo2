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
    /// Interaction logic for customDataSheet.xaml
    /// </summary>
    public partial class customDataSheet : ScatterViewItem
    {
        public customDataSheet()
        {
            InitializeComponent();
        }

        //Constructor for creation from part
        public customDataSheet(Part p)
        {
            InitializeComponent();
            name.Text = p.myRegDS.Name;
            type.Text = p.myRegDS.Type;
            length.Text = p.myRegDS.BasicInfo.Length.ToString();
            seq.Text = p.myRegDS.BasicInfo.Sequence;
            desc.Text = p.myRegDS.Description.Desc;
        }
    }
}
