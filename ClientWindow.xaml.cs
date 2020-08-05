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

        TcpClient client;
        NetworkStream ns;
        string nickName;

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
                Close();return;
            }
            if (!int.TryParse(ccw.txbPort.Text, out int port)) {
                MessageBox.Show("Port error");
                Close(); return;
            }
            nickName = ccw.txbNickname.Text;

            ShowMsg("Connecting " + ip + ":" + port + " ...");

            Thread connect = new Thread(delegate () {
                client = new TcpClient(ip, port);
                ns = client.GetStream();
                MyNetwork.Write(ns, nickName); //Send nickname
                EnableInput();

                Thread listenAndAct = new Thread(delegate () {
                    try {
                        while (true) {
                            string text = MyNetwork.Read(ns);
                            ShowMsg(text);
                        }
                    } catch {
                        ShowMsg("Connection lost.");
                        ns.Close();
                        client.Close();
                        DisableInput("Disconnected.");
                    }
                });
                listenAndAct.IsBackground = true;// For stopping running threads when the window closed.
                listenAndAct.Start();
            });
            connect.IsBackground = true;
            connect.Start();
        }
        
        private void BtnSend_Click(object sender, RoutedEventArgs e) {
            string text = txbInput.Text;
            if (text == "") return;
            DisableInput("Sending...");
            
            Thread send = new Thread(delegate () {
                MyNetwork.Write(ns, text);
                EnableInput();
            });
            send.IsBackground = true;
            send.Start();
        }

        private void ClientWindow_Unloaded(object sender, RoutedEventArgs e) {//porblem? TODO
            //MyNetwork.Write(ns, "\0");
            ns.Close();
            client.Close();
        }

        private void ClientWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {// Hotkey for btnSend
                btnSend.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSend));
            }
        }
    }
}
