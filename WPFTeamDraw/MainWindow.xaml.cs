using System;
using System.Collections.Generic;
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

namespace WPFTeamDraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //private HashMap<Polyline, long> plines = new HashMap<Polyline,long>();

        private Polyline _pl;
        private bool _isDrawing = false;
        private Color color = Colors.Black;
        private int strokeThickness = 5;

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
            if (e.LeftButton == MouseButtonState.Pressed && _isDrawing)
            {
                Point p = e.GetPosition(this);
                p.Y -= 100;
                _pl.Points.Add(p);
            }
        }

        private void DrawArea_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _isDrawing = false;
                //Send Line to server
            }
        }

        public void DrawLineSync(RPoint rline)
        {
            
        }
        
    }
}
