using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Chatroom {
    /// <summary>
    /// ClientConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientConfigWindow : Window {
        public ClientConfigWindow() {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Close();
        }
    }
}
