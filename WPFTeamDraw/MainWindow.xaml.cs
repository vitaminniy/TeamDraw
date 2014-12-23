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

    public struct DLine{
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

        #region members
        private Polyline _pl;
        private bool _isDrawing = false;
        private Color color = Colors.Black;
        private Color lastColor;
        private int strokeThickness = 5;
        private int r = 0;
        private long luid = 0;
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

                //Send line to server, send first point to server
                byte rc =0;
                if (color == Colors.Black) rc = 0;
                if (color == Colors.Red) rc = 1;
                if (color == Colors.Green) rc = 2;
                if (color == Colors.Blue) rc = 3;
                if (color == Colors.Yellow) rc = 4;
                if (color == Colors.Purple) rc = 5;
                RLine rline = new RLine(rc, (byte)strokeThickness);
                luid = rline.uid;
                RPoint rpoint = new RPoint(p.X, p.Y, luid);

                byte[] ldata = new byte[19];
                ldata[0] = Client.lrequest;
                Array.Copy(rline.getBytes(), 0, ldata, 1, 18);
                byte[] pdata = new byte[25];
                pdata[0] = Client.prequest;
                Array.Copy(rpoint.getBytes(), 0, pdata, 1, 24);

                client.sendq.Enqueue(ldata);
                client.sendq.Enqueue(pdata);
            }
        }

        private void DrawArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (r==0 && e.LeftButton == MouseButtonState.Pressed && _isDrawing)
            {
                Point p = e.GetPosition(this);
                p.Y -= 100;
                _pl.Points.Add(p);

                //Send point to server
                RPoint rpoint = new RPoint(p.X, p.Y, luid);
                byte[] pdata = new byte[25];
                pdata[0] = Client.prequest;
                Array.Copy(rpoint.getBytes(), 0, pdata, 1, 24);
                client.sendq.Enqueue(pdata);
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
                //Send last point to server
                byte[] pdata;
                pdata = BitConverter.GetBytes(luid);
                byte[] data = new byte[9];
                data[0] = Client.lprequest;
                Array.Copy(pdata, 0, data, 1, 8);
                client.sendq.Enqueue(data);
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
            Color color = Colors.Black;
            if(rline.color == 0) color = Colors.Black;
            if(rline.color == 1) color = Colors.Red;
            if(rline.color == 2) color = Colors.Green;
            if(rline.color == 3) color = Colors.Blue;
            if(rline.color == 4) color = Colors.Yellow;
            if(rline.color == 5) color = Colors.Purple;
            pline.Stroke = new SolidColorBrush(color);
            pline.StrokeThickness = rline.thickness;

            
            plines.TryAdd(rline.uid, pline);
            DLine dline = new DLine();
            dline.pline = pline;
            dline.time = rline.servertime;
            nplines.Enqueue(dline);
        }

        public static void handleRPoint(RPoint rpoint)
        {
            Polyline pline;
            if (!plines.TryGetValue(rpoint.uid, out pline)) return;
            pline.Points.Add(new Point(rpoint.x, rpoint.y));
        }

        public static void handleLPoint(long uid)
        {
            Polyline pline;
            plines.TryRemove(uid, out pline);
        }

        #endregion

        #region colorpicker, stroke thickness and eraser
        private void BlackColorTB_Checked(object sender, RoutedEventArgs e)
        {
            color = Colors.Black;
            lastColor = Colors.Black;
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
            lastColor = Colors.Red;
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
            lastColor = Colors.Green;
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
            lastColor = Colors.Blue;
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
            lastColor = Colors.Yellow;
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
            lastColor = Colors.Purple;
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
            color = lastColor;
            EraseSmallTB.IsChecked = false;
            MediumEraseTB.IsChecked = false;
            LargeEraseTB.IsChecked = false;
        }
        private void MediumThickTB_Checked(object sender, RoutedEventArgs e)
        {
            strokeThickness = 10;
            SmallThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;
            color = lastColor;
            EraseSmallTB.IsChecked = false;
            MediumEraseTB.IsChecked = false;
            LargeEraseTB.IsChecked = false;
        }
        private void LargeThickTB_Checked(object sender, RoutedEventArgs e)
        {
            strokeThickness = 20;
            SmallThickTB.IsChecked = false;
            MediumThickTB.IsChecked = false;
            color = lastColor;
            EraseSmallTB.IsChecked = false;
            MediumEraseTB.IsChecked = false;
            LargeEraseTB.IsChecked = false;
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

            SmallThickTB.IsChecked = false;
            MediumThickTB.IsChecked = false;
            LargeThickTB.IsChecked = false;

            color = Colors.White;
            strokeThickness = 30;

            EraseSmallTB.IsChecked = false;
            MediumEraseTB.IsChecked = false;
        }
        #endregion

        private void MainWindows_Closed(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

       
    }
}
