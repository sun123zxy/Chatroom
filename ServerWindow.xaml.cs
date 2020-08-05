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
using System.Collections;

namespace Chatroom {
    /// <summary>
    /// ServerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ServerWindow : Window {
        public ServerWindow() {
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

        struct ClientInfo {
            public TcpClient client;
            public NetworkStream ns;
            public string nickname;
            public ClientInfo(TcpClient client, NetworkStream ns, string nickname) {
                this.client = client;
                this.ns = ns;
                this.nickname = nickname;
            }
        }
        List<ClientInfo> clientInfo;

        void Broadcast(string text) {
            ShowMsg(text);
            foreach (ClientInfo ci in clientInfo) {
                MyNetwork.Write(ci.ns, text);
            }
        }
        private void ServerWindow_Loaded(object sender, RoutedEventArgs e) {

            DisableInput("Initializing...");

            ServerConfigWindow scw = new ServerConfigWindow();
            bool ret = (bool)scw.ShowDialog();
            if (ret == false) { //Cancel
                Close();
                return;
            }

            if (!int.TryParse(scw.txbPort.Text, out int port)) {
                MessageBox.Show("Port error");
                Close(); return;
            }

            clientInfo = new List<ClientInfo>();

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            ShowMsg("Listening for client connection at port " + port + " ...");
            EnableInput();

            Thread listen = new Thread(delegate () {
                while (true) {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream ns = client.GetStream();
                    string nickname = MyNetwork.Read(ns); // Get nickname
                    ShowMsg("New connection recieved from " + nickname + " (" + client.Client.RemoteEndPoint.ToString() + ")");
                    clientInfo.Add(new ClientInfo(client, ns, nickname));
                    MyNetwork.Write(ns, "Server: Connection confirmed.");
                    Broadcast(nickname + " entered the chatroom.");

                    Thread read = new Thread(delegate () {
                        try {
                            while (true) {
                                string text = MyNetwork.Read(ns);
                                Broadcast(nickname + ": " + text);
                            }
                        } catch {
                            ns.Close();
                            client.Close();
                            clientInfo.Remove(new ClientInfo(client, ns, nickname));
                            Broadcast(nickname + " was no longer in the chatroom.");
                        }
                    });
                    read.IsBackground = true;
                    read.Start();
                }
            });
            listen.IsBackground = true;
            listen.Start();
        }
    }
}
