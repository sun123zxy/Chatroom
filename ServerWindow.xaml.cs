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
    class User {
        public Socket socket;
        public string nickname;
        public User(Socket socket, string nickname) {
            this.socket = socket;
            this.nickname = nickname;
        }
    }
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

        List<User> users;
        Socket server;
        void Broadcast(string text) {
            ShowMsg(text);
            Thread broadcast = new Thread(delegate () {
                foreach (User user in users) {
                    MyNetwork.Write(user.socket, text);
                }
            });
            broadcast.IsBackground = true;
            broadcast.Start();
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

            users = new List<User>();

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(10);
            ShowMsg("Listening for client connection at port " + port + " ...");
            EnableInput();

            Thread tListening = new Thread(delegate () { Listening(); });
            tListening.IsBackground = true;
            tListening.Start();
        }
        void Listening() {
            while (true) {
                Socket client = server.Accept();
                string nickname = MyNetwork.Read(client); // Get user's nickname

                User user = new User(client, nickname);
                users.Add(user);
                ShowMsg("New connection recieved from " + user.nickname + " (" + user.socket.RemoteEndPoint.ToString() + ")");

                MyNetwork.Write(user.socket, "Server: Connection confirmed.");
                Broadcast(user.nickname + " entered the chatroom.");

                Thread read = new Thread(delegate () { ReadAndBroadCast(user); });
                read.IsBackground = true;
                read.Start();
            }
        }
        void ReadAndBroadCast(User user) {
            try {
                while (true) {
                    string text = MyNetwork.Read(user.socket);
                    Broadcast(user.nickname + ": " + text);
                }
            } catch {
                user.socket.Close();
                users.Remove(user);
                Broadcast(user.nickname + " was no longer in the chatroom.");
            }
        }
        private void ServerWindow_Unloaded(object sender, RoutedEventArgs e) {
            if(server != null) server.Close();
            foreach (User user in users) {
                user.socket.Close();
            }
        }
    }
}
