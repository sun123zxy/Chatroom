using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Net.Sockets;
using System.Net;

namespace Chatroom {
    /// <summary>
    /// UniversalWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UniversalWindow : Window {
        public UniversalWindow() {
            InitializeComponent();
        }

        public void ShowMsg(string msg) {
            this.Dispatcher.Invoke(delegate () { //MultiThread need this
                txbShow.Text += msg + "\n";
                txbShow.ScrollToEnd();
            });
        }
        public void DisableInput(string text = "") {
            this.Dispatcher.Invoke(delegate () {
                txbInput.IsEnabled = false;
                txbInput.Text = text;
                btnSend.IsEnabled = false;
            });
        }
        public void EnableInput(string text = "") {
            this.Dispatcher.Invoke(delegate () {
                txbInput.IsEnabled = true;
                txbInput.Text = text;
                btnSend.IsEnabled = true;
            });
        }
        public delegate void CallBack();

        public void BtnSend_Click(object sender, RoutedEventArgs e) {
            string text = txbInput.Text;
            if (text == "") return;
            txbInput.Text = "";

            if (text[0] == '/') {
                OptCmdFromSelf(text);
            } else {
                Send(text);
            }
        }
        public virtual void Send(string text) {

        }
        public virtual void OptCmdFromSelf(string text) {

        }
        public virtual void UniversalWindow_Loaded(object sender, RoutedEventArgs e) {

        }
        public void UniversalWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {// Hotkey for btnSend
                btnSend.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSend));
            }
        }
        public virtual void UniversalWindow_Unloaded(object sender, RoutedEventArgs e) {

        }
    }
}
