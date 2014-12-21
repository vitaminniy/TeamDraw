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

        private int ServerIDValue;
        private int ServerPasswordValue;

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ServerIDValue = int.Parse(ServerID.Text);
            ServerPasswordValue = int.Parse(ServerPassword.Password);
            //if ID and Pwd are ok - then await connect and open a new window
            //else throw new message that ID and/or Pwd is/are invalid.

            //delete this later pls
            var Drawer = new MainWindow();
            Drawer.Show();
            Drawer.Focus();
            this.Close();
        }
    }
}
