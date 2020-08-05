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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chatroom {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        public MainWindow() {
            InitializeComponent();

            /*MessageBoxResult messageBoxResult = MessageBox.Show("Client or Server? Yes for Client, No for Server", "Choose Type", MessageBoxButton.YesNo);
            if(messageBoxResult == MessageBoxResult.Yes) {//Client
                clientWindow = new ClientWindow();
                clientWindow.Show();
            } else {//Server
                
            }
            Close();*/
        }

        private void btnClient_Click(object sender, RoutedEventArgs e) {
            ClientWindow clientWindow = new ClientWindow();
            Close();
            clientWindow.Show();
        }

        private void BtnServer_Click(object sender, RoutedEventArgs e) {
            ServerWindow serverWindow = new ServerWindow();
            Close();
            serverWindow.Show();
        }
    }
}
