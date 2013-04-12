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
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Interop;

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for fusionSites.xaml
    /// </summary>
    public partial class fusionSites : ScatterViewItem
    {
        private string _fusionSiteName;
        private string _sequence;

        #region Properties
        public string FusionSiteName
        {
            get { return _fusionSiteName; }
            set { _fusionSiteName = value; }
        }
        public string Sequence
        {
            get { return _sequence; }
            set { _sequence = value; }
        }
        #endregion
       
        public fusionSites(string name, string seq, Brush bg)
        {
            InitializeComponent();
            _fusionSiteName = name;
            _sequence = seq;
            Background = bg;
            fsName.Text = name;
            seqText.Text = seq;

            //Remove shadow from control
            this.ApplyTemplate();
            this.ShowsActivationEffects = false;
            this.BorderBrush = System.Windows.Media.Brushes.Transparent;
            Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome ssc;
            ssc = this.Template.FindName("shadow", this) as Microsoft.Surface.Presentation.Generic.SurfaceShadowChrome;
            ssc.Visibility = Visibility.Hidden;
        }


    }
}
