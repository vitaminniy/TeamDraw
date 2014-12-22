using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFTeamDraw
{

    struct DLine{
        public Polyline pline;
        public long time;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Client client;

        public static ConcurrentDictionary<long, Polyline> plines
            = new ConcurrentDictionary<long,Polyline>();
        public static ConcurrentQueue<DLine> nplines
            = new ConcurrentQueue<DLine>();

        private static LinkedList<DLine> dlines = new LinkedList<DLine>();

        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            SmallThickTB.IsChecked = true;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 100);
            timer.Start();
        }
        //HEAD
        //private HashMap<Polyline, long> plines = new HashMap<Polyline,long>();

        private Dictionary<Polyline, long> plines = new Dictionary<Polyline,long>();
        //bd9e86b9e575a8272bd4a2f3fc6fc0332e101d3c

        #region members
        private Polyline _pl;
        private bool _isDrawing = false;
        private Color color = Colors.Black;
        private int strokeThickness = 5;
        private int r = 0;
        #endregion

        #region drawAction
        private void DrawArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _isDrawing = true;
                _pl = new Polyline();
                Point p = e.GetPosition(this);
                p.Y -= 100;
                _pl.Points.Add(p);
                _pl.Stroke = new SolidColorBrush(color);
                _pl.StrokeThickness = strokeThickness;
                DrawArea.Children.Add(_pl);
            }
        }

        private void DrawArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (r==0 && e.LeftButton == MouseButtonState.Pressed && _isDrawing)
            {
                Point p = e.GetPosition(this);
                p.Y -= 100;
                _pl.Points.Add(p);
            }
            r++;
            if (r >= 8) r = 0;
        }
        private void DrawArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                r = 0;
                //Send Line to server
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DLine dline;
            while (nplines.TryDequeue(out dline))
            {
                LinkedListNode<DLine> oline = dlines.Last;
                while (oline != null && oline.Value.time > dline.time)
                {
                    oline = oline.Previous;
                    if (oline != null) DrawArea.Children.Remove(oline.Value.pline);
                }
                if (oline == null) //Means we need to draw new line before every other line
                {
                    dlines.AddFirst(dline);
                    foreach (DLine ddline in dlines) DrawArea.Children.Add(ddline.pline);
                }
                else
                {
                    dlines.AddAfter(oline, dline);
                    while (oline != null)
                    {
                        DrawArea.Children.Add(oline.Value.pline);
                        oline = oline.Next;
                    }
                }
            }
        }

        public static void handleRLine(RLine rline)
        {
            Polyline pline = new Polyline();
        }

        public static void handleRPoint(RPoint rpoint, bool isFinal)
        {

        }

        #endregion

        #region colorpicker, stroke thickness and eraser
        private void BlackColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Black;
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
        }
        private void RedColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Red;
            BlackColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            RedColorTB.IsChecked = false;
        }
        private void GreenColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Green;
            RedColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
        }
        private void BlueColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Blue;
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
        }
        private void YellowColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Yellow;
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
        }
        private void PurpleColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Purple;
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
        }

        private void SmallThickTB_Checked(object sender, RoutedEventArgs e)
        {
            strokeThickness = 5;
            MediumThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;
        }
        private void MediumThickTB_Checked(object sender, RoutedEventArgs e)
        {
            strokeThickness = 10;
            SmallThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;
        }
        private void LargeThickTB_Checked(object sender, RoutedEventArgs e)
        {
            strokeThickness = 20;
            SmallThickTB.IsChecked = false;
            MediumThickTB.IsChecked = false;
        }

        private void EraseSmallTB_Checked(object sender, RoutedEventArgs e)
        {
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            SmallThickTB.IsChecked = false;
            MediumThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;

            color = Colors.White;
            strokeThickness = 5;

            LargeEraseTB.IsChecked = false;
            MediumEraseTB.IsChecked = false;

        }
        private void MediumEraseTB_Checked(object sender, RoutedEventArgs e)
        {
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            SmallThickTB.IsChecked = false;
            MediumThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;

            color = Colors.White;
            strokeThickness = 10;

            LargeEraseTB.IsChecked = false;
            EraseSmallTB.IsChecked = false;
        }
        private void LargeEraseTB_Checked(object sender, RoutedEventArgs e)
        {
            RedColorTB.IsChecked = false;
            GreenColorTB.IsChecked = false;
            BlueColorTB.IsChecked = false;
            YellowColorTB.IsChecked = false;
            PurpleColorTB.IsChecked = false;
            BlackColorTB.IsChecked = false;
            SmallThickTB.IsChecked = false;
            MediumThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;

            color = Colors.White;
            strokeThickness = 30;

            EraseSmallTB.IsChecked = false;
            MediumEraseTB.IsChecked = false;
        }
        #endregion

       
    }
}
