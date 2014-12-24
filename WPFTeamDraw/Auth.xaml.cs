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
        private MainWindow Drawer;
        public Auth()
        {
            InitializeComponent();
            Drawer = new MainWindow();
        }

        private string ip;
        private int port;

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            
            while (true)
            {
                ip = ServerIP.Text;
                port = int.Parse(ServerPort.Text);
                
                //if IP and Port are ok - then await connect and open a new window

                try
                {
                    MainWindow.client = new Client(ip, port);
                    MainWindow.client.Start();
                    break;
                }
                catch (Exception)
                {
                    var error = "Couldn't connect to IP: " + ip;
                    var mbox = MessageBox.Show(error, "Error");
                    return;
                }

            }
            
            Drawer.Show();
            Drawer.Focus();
            this.Close();
        }
    }
}
