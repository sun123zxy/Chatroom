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
    class ClientWindow : UniversalWindow {
        string username;

        Socket client;

        bool isConnectionLost;
        public void ConnectionLost() {
            lock ((object)isConnectionLost) {
                if (!isConnectionLost) {
                    isConnectionLost = true;
                    ShowMsg("Connection lost.");
                    if (client != null) client.Close();
                    DisableInput("Disconnected.");
                }
            }
        }
        public override void UniversalWindow_Loaded(object sender, RoutedEventArgs e) {

            DisableInput("Connecting...");
            isConnectionLost = false;

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
            username = ccw.txbUsername.Text;
            if (username.Contains(' ')) {
                MessageBox.Show("Username mustn't contains space");
                ConnectionLost();
                return;
            }

            Title += " - Client - " + username;

            ShowMsg("Connecting " + ip + ":" + port + " ...");
            Thread tConnect = new Thread(delegate () { Connect(ip, port); });
            tConnect.IsBackground = true; // For stopping running threads when the window closed.
            tConnect.Start();
        }
        void Connect(string ip, int port) {
            try {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(ip, port);
                MyNetwork.Write(client, username); //Send username
            } catch {
                ConnectionLost();
                return;
            }
            EnableInput();

            Thread tRAA = new Thread(delegate () { ReadAndAct(); });
            tRAA.IsBackground = true;
            tRAA.Start();
        }

        void ReadAndAct() {
            
            while (true) {
                string text;
                try {
                    text = MyNetwork.Read(client);
                } catch {
                    ConnectionLost();
                    return;
                }
                if (text[0] == '/') {
                    OptCmdFromServer(text);
                } else {
                    ShowMsg(text);
                }
            }
            
        }
        public void OptCmdFromServer(string cmd) {
            string[] args = cmd.Split(' ');
            switch (args[0]) {
                case "/kick":
                    ShowMsg("You were kicked out by server admin.");
                    ConnectionLost();
                    break;
                case "/ban":
                    ShowMsg("Your IP has been banned by server admin.");
                    ConnectionLost();
                    break;
                case "/refuse_duplicate":
                    ShowMsg("Cannot join the server because your username is already in use. Change your username and retry.");
                    ConnectionLost();
                    break;
                case "/refuse_banned":
                    ShowMsg("Cannot join the server because your IP has been banned by server admin.");
                    ConnectionLost();
                    break;
            }
        }
        public override void Send(string text) {

            Thread tSendToServer = new Thread(delegate () {
                try {
                    MyNetwork.Write(client, text);
                } catch {
                    ConnectionLost();
                }
            });
            tSendToServer.IsBackground = true;
            tSendToServer.Start();
            // Echo text will be sent back by the server
        }

        public override void UniversalWindow_Unloaded(object sender, RoutedEventArgs e) {
            if (client != null) client.Close();
        }
    }
}
