using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFTeamDraw
{
    /// <summary>
    /// Interaction logic for Auth.xaml
    /// </summary>
    public partial class Auth : Window
    {
        public Auth()
        {
            InitializeComponent();
        }

        private string ip;
        private int port;

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ip = ServerIP.Text;
            port = int.Parse(ServerPort.Text);
            var Drawer = new MainWindow();
            //if IP and Port are ok - then await connect and open a new window
            MainWindow.client = new Client(ip, port);
            MainWindow.client.Start();


            //delete this later pls
            
            Drawer.Show();
            Drawer.Focus();
            this.Close();
        }
    }
}
