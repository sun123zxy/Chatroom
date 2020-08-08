using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
using System.Data;

namespace Chatroom {
    /// <summary>
    /// ClientWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientWindow : Window {

        public ClientWindow() {
            InitializeComponent();
        }
        void ShowMsg(string msg) {
            this.Dispatcher.Invoke(delegate () { //MultiThread need this
                txbShow.Text += msg + "\n";
                txbShow.ScrollToEnd();
            });
        }
        void DisableInput(string text = "") {
            this.Dispatcher.Invoke(delegate () {
                txbInput.IsEnabled = false;
                txbInput.Text = text;
                btnSend.IsEnabled = false;
            });
        }
        void EnableInput(string text = "") {
            this.Dispatcher.Invoke(delegate () {
                txbInput.IsEnabled = true;
                txbInput.Text = text;
                btnSend.IsEnabled = true;
            });
        }

        string nickName;

        Socket client;

        void ConnectionLost() {
            ShowMsg("Connection lost.");
            if(client != null) client.Close();
            DisableInput("Disconnected.");
        }
        private void ClientWindow_Loaded(object sender, RoutedEventArgs e) {

            DisableInput("Connecting...");

            ClientConfigWindow ccw = new ClientConfigWindow();
            bool ret = (bool)ccw.ShowDialog();
            if (ret == false) { //Cancel
                Close();
                return;
            }

            string ip = ccw.txbIP.Text;
            if (!IPAddress.TryParse(ip, out _)) {
                MessageBox.Show("IP error");
                ConnectionLost();
                return;
            }
            if (!int.TryParse(ccw.txbPort.Text, out int port)) {
                MessageBox.Show("Port error");
                ConnectionLost();
                return;
            }
            nickName = ccw.txbNickname.Text;

            ShowMsg("Connecting " + ip + ":" + port + " ...");

            Thread tConnect = new Thread(delegate() { Connect(ip, port); });
            tConnect.IsBackground = true; // For stopping running threads when the window closed.
            tConnect.Start();
        }

        void Connect(string ip, int port) {
            try {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(ip, port);
                MyNetwork.Write(client, nickName); //Send nickname
            } catch {
                ConnectionLost();
            }
            EnableInput();

            Thread tRAS = new Thread(delegate () { ReadAndShow(); });
            tRAS.IsBackground = true;
            tRAS.Start();
        }

        void ReadAndShow() {
            try {
                while (true) {
                    string text = MyNetwork.Read(client);
                    ShowMsg(text);
                }
            } catch {
                ConnectionLost();
            }
        }
        
        private void BtnSend_Click(object sender, RoutedEventArgs e) {
            string text = txbInput.Text;
            if (text == "") return;
            DisableInput("Sending...");
            
            Thread send = new Thread(delegate () {
                MyNetwork.Write(client, text);
                EnableInput();
            });
            send.IsBackground = true;
            send.Start();
        }

        private void ClientWindow_Unloaded(object sender, RoutedEventArgs e) {
            if(client != null) client.Close();
        }

        private void ClientWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {// Hotkey for btnSend
                btnSend.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSend));
            }
        }
    }
}
