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
using System.Runtime.CompilerServices;

namespace Chatroom {
    /// <summary>
    /// UniversalWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UniversalWindow : Window {
        public List<string> inputHistory;
        public int curHistoryId;

        public UniversalWindow() {
            InitializeComponent();

            inputHistory = new List<string>();
            curHistoryId = 0;
        }

        public void ShowMsg(string msg) {
            this.Dispatcher.BeginInvoke((Action)delegate () { //MultiThread need this
                txbShow.Text += msg + "\n";
                txbShow.ScrollToEnd();
            });
        }
        public void DisableInput(string text = "") {
            this.Dispatcher.BeginInvoke((Action)delegate () {
                txbInput.IsEnabled = false;
                txbInput.Text = text;
                btnSend.IsEnabled = false;
            });
        }
        public void EnableInput(string text = "") {
            this.Dispatcher.BeginInvoke((Action)delegate () {
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
            inputHistory.Add(text);
            curHistoryId = inputHistory.Count;
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
        public virtual void UniversalWindow_Unloaded(object sender, RoutedEventArgs e) {

        }
        public void TxbInput_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Enter: // Hotkey for btnSend
                    btnSend.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSend));
                    break;
                case Key.Up:
                    if (inputHistory.Count != 0) {
                        if (curHistoryId > 0) curHistoryId--;
                        if (curHistoryId < inputHistory.Count) {
                            txbInput.Text = inputHistory[curHistoryId];
                            txbInput.SelectionStart = txbInput.Text.Length; //Cursur to the end
                        }
                    }
                    break;
                case Key.Down:
                    if (inputHistory.Count != 0) {
                        if (curHistoryId < inputHistory.Count - 1) curHistoryId++;
                        if (curHistoryId < inputHistory.Count) {
                            txbInput.Text = inputHistory[curHistoryId];
                            txbInput.SelectionStart = txbInput.Text.Length;
                        }
                    }
                    break;
            }
        }
    }
}
