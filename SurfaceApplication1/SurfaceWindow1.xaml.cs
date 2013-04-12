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
using System.IO;
using System.Diagnostics;

namespace SurfaceApplication1
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        public static PrimerDesigner1 pd1;
        public static PrimerDesigner2 pd2;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            //Make friends with everybody
            Part.sw1 = this;
            L1Module.sw1 = this;
            L2Module.sw1 = this;
            Level0.sw1 = this;
            Level1.sw1 = this;
            Level2.sw1 = this;
            PrimerDesigner2.sw1 = this;
            PrimerDesigner1.sw1 = this;
            EugeneModules.sw1 = this;
            customForm.sw1 = this;

            #region Testing data sheets
            //Testing data sheets
            //RegDataSheet r = new RegDataSheet("http://partsregistry.org/Part:BBa_K266006");
            //Console.WriteLine(r.Promoter.getReg());

            //RegDataSheet r = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_K137030");
            //StreamWriter s = new StreamWriter("BBa_K137030.txt");
            //s.WriteLine(r.ToString(), Environment.NewLine);
            //s.Close();

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //RegDataSheet r = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_K156010");
            //StreamWriter s = new StreamWriter("BBa_K156010-5.txt");
            //s.WriteLine(r.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r1 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_J61100");
            //s.WriteLine(r1.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r2 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_B0015");
            //s.WriteLine(r2.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r3 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_E0040");
            //s.WriteLine(r3.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r4 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_K206000");
            //s.WriteLine(r4.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r5 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_I15008");
            //s.WriteLine(r5.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r6 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_T9150");
            //s.WriteLine(r6.ToString(), Environment.NewLine);
            //s.WriteLine("-------------------------------");

            //RegDataSheet r7 = new RegDataSheet("http://partsregistry.org/wiki/index.php?title=Part:BBa_I716153");
            //s.WriteLine(r7.ToString(), Environment.NewLine);

            //sw.Stop();
            //s.WriteLine("Elapsed time: " + sw.Elapsed);
            //s.Close();
            #endregion
        }

        #region SurfaceWindow stuff

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        #endregion

        #region Static helpers (and non-static helpers)

        //Bitmap-to-ImageSource converter
        public static ImageSource BitmapToImageSource(System.Drawing.Bitmap bmp)
        {
            IntPtr hBitmap = bmp.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bmp.Dispose();
            return wpfBitmap;
        }

        //Transform coordinates of an element relative to a destination
        public static Point transformCoords(ScatterViewItem elem, Visual Destination)
        {
            Point spawnPoint;
            try
            {
                GeneralTransform spawnTransform = elem.TransformToVisual(Destination);
                spawnPoint = spawnTransform.Transform(new Point());
                //Adjust point - maybe TransformToVisual gets top-left corner? It's shifted up and left relative to the contact point.
                spawnPoint.X = spawnPoint.X + elem.Width / 2;
                spawnPoint.Y = spawnPoint.Y + elem.Height / 2;
                
            }
            catch
            {
                spawnPoint = new Point(0, 0);
            }
            return spawnPoint;
        }

        //Set position of ScatterViewItems in a ScatterView as organized rows
        public static Point SetPosition(ScatterViewItem svi)
        {
            Point newCenter;
            try
            {
                ScatterView parentSV = (ScatterView)svi.Parent;
                double ParentWidth = parentSV.Width;
                double ParentHeight = parentSV.Height;
                int NumberParts = (int)Math.Floor((ParentWidth / (svi.Width + 10)));
                int count = parentSV.Items.Count;
                double startX = svi.Width / 2 + 10;
                double startY = (svi.Height + 10) / 2;
                int multiplierX = (count - 1) % NumberParts;
                int multiplierY = (count - 1) / NumberParts;
                newCenter = new Point(startX + multiplierX * (svi.Width + 10), startY + multiplierY * (svi.Height + 10));

                //If newCenter is too low to fit entire svi into the bottom row, increase parentSV height
                if (newCenter.Y > ParentHeight - (svi.Width / 2))
                {
                    parentSV.Height = parentSV.Height + (svi.Height + 10);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                newCenter = new Point(0, 0);
            }
            return newCenter;
        }



        //Add data to appropriate window
        public static void addData(object sender, ScatterViewItem data)
        {
            try
            {
                //Find appropriate Level to add to
                DependencyObject elem = VisualTreeHelper.GetParent((UIElement)sender);
                while (elem.GetType() != typeof(Level0) &&
                    elem.GetType() != typeof(Level1) &&
                    elem.GetType() != typeof(Level2) &&
                    elem.GetType() != typeof(PrimerDesigner1) &&
                    elem.GetType() != typeof(PrimerDesigner2))
                {
                    elem = VisualTreeHelper.GetParent(elem);
                }
                if (elem.GetType() == typeof(Level0)) ((Level0)elem).L0_SV.Items.Add(data);
                if (elem.GetType() == typeof(Level1)) ((Level1)elem).L1_SV.Items.Add(data);
                if (elem.GetType() == typeof(Level2)) ((Level2)elem).L2_SV.Items.Add(data);
                if (elem.GetType() == typeof(PrimerDesigner1)) ((PrimerDesigner1)elem).MainSV.Items.Add(data);
                if (elem.GetType() == typeof(PrimerDesigner2)) ((PrimerDesigner2)elem).MainSV.Items.Add(data);

                data.Center = new Point (((ScatterView)data.Parent).Width/2, ((ScatterView)data.Parent).Height/2);
            }
            catch (Exception exc) { Console.WriteLine(exc); }
        }

        //Returns progress indicator of the appropriate window when needed by Parts, L1Modules, L2Modules
        public static Grid getProgressIndicator(object sender)
        {
            try
            {
                DependencyObject elem = VisualTreeHelper.GetParent((UIElement)sender);
                Grid c;
                while (elem.GetType() != typeof(Level0) &&
                    elem.GetType() != typeof(Level1) &&
                    elem.GetType() != typeof(Level2) &&
                    elem.GetType() != typeof(PrimerDesigner1) &&
                    elem.GetType() != typeof(PrimerDesigner2))
                {
                    elem = VisualTreeHelper.GetParent(elem);
                }
                if (elem.GetType() == typeof(Level0)) { c = ((Level0)elem).ProgressIndicator; }
                else if (elem.GetType() == typeof(Level1)) { c = ((Level1)elem).ProgressIndicator; }
                else if (elem.GetType() == typeof(Level2)) { c = ((Level2)elem).ProgressIndicator; }
                else if (elem.GetType() == typeof(PrimerDesigner1)) { c = ((PrimerDesigner1)elem).ProgressIndicator; }
                else /*(elem.GetType() == typeof(PrimerDesigner2))*/ { c = ((PrimerDesigner2)elem).ProgressIndicator; }

                return c;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return new Grid();
            }
        }

        //If scatterviewitem (Part, DS, Seq) swipe to delete
        public void swipeToDelete(ScatterViewItem svi)
        {
            if (svi.Center.X > (L0.Width - 100) || svi.Center.X < 100)
            {
                if (svi.GetType() == typeof(Part) || svi.GetType() == typeof(L1Module))
                {
                    ScatterView palette = new ScatterView();
                    
                    if (svi.GetType() == typeof(Part))
                    {
                        Part p = (Part)svi;
                        palette = (ScatterView)p.MyClone.Parent;
                        palette.Items.Remove(p.MyClone);
                    }
                    if (svi.GetType() == typeof(L1Module))
                    {
                        L1Module l = (L1Module)svi;
                        palette = (ScatterView)l.MyClone.Parent;
                        palette.Items.Remove(l.MyClone);
                    }

                    List<ScatterViewItem> storeSVIList = new List<ScatterViewItem>();
                    foreach (ScatterViewItem m in palette.Items)
                    {
                        storeSVIList.Add(m);
                    }
                    palette.Items.Clear();
                    foreach (ScatterViewItem o in storeSVIList)
                    {
                        palette.Items.Add(o);
                        o.Center = SetPosition(o);
                    }
                    return;
                }
                L0.L0_SV.Items.Remove(svi);
                L1.L1_SV.Items.Remove(svi);
                L2.L2_SV.Items.Remove(svi);
                if (pd1 != null) pd1.MainSV.Items.Remove(svi);
                if (pd2 != null) pd2.MainSV.Items.Remove(svi);
            }
        }

        #endregion
    }
}