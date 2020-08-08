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
        public User() { }
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
                User user = new User();
                user.socket = server.Accept();
                try {
                    user.nickname = MyNetwork.Read(user.socket);
                } catch {
                    ShowMsg("A new connection recived from " + user.socket.RemoteEndPoint.ToString() + " lost when sending nickname.");
                    continue;
                }
                users.Add(user);
                ShowMsg("New connection recieved from " + user.nickname + " (" + user.socket.RemoteEndPoint.ToString() + ")");

                Broadcast(user.nickname + " entered the chatroom.");

                Thread read = new Thread(delegate () { ReadAndBroadcast(user); });
                read.IsBackground = true;
                read.Start();
            }
        }
        void ReadAndBroadcast(User user) {
            try {
                while (true) {
                    string text = MyNetwork.Read(user.socket);
                    Broadcast(user.nickname + ": " + text);
                }
            } catch {
                Offline(user);
            }
        }

        delegate void CallBack();
        void SendToUser(User user, string text, CallBack callBack = null) {
            Thread tSendToUser = new Thread(delegate () {
                try {
                    MyNetwork.Write(user.socket, text);
                } catch {
                    Offline(user);
                }
                if (callBack != null) callBack();
            });
            tSendToUser.IsBackground = true;
            tSendToUser.Start();
        }
        void Broadcast(string text) {
            foreach (User user in users) {
                SendToUser(user, text);
            }
            ShowMsg(text);
        }
        void Offline(User user) {
            if (users.Contains(user)) {
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

        private void BtnSend_Click(object sender, RoutedEventArgs e) {
            string text = txbInput.Text;
            if (text == "") return;
            txbInput.Text = "";

            if(text[0] == '/') {
                OperateCmdFromSelf(text);
            } else {
                Send(text);
            }
        }
        void Send(string text) {
            Broadcast("Server: " + text);
        }
        void OperateCmdFromSelf(string cmd) {
            ShowMsg(cmd);
            string[] args = cmd.Split(' ');
            if (args[0] == "/kick") {
                if(args.Length == 2) {
                    bool isKicked = false;
                    foreach (User user in users) {
                        if (user.nickname == args[1]) {
                            SendToUser(user, "/kick");
                            Offline(user);

                            Broadcast(user.nickname + " was kicked out by server admin.");
                            isKicked = true; break;
                        }
                    }
                    if (!isKicked) {
                        ShowMsg("Cannot find a user called " + args[1]);
                    }
                } else {
                    ShowMsg("Wrong usage.");
                }
            } else {
                ShowMsg("Cannot find this command.");
            }
        }

        private void ServerWindow_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {// Hotkey for btnSend
                btnSend.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSend));
            }
        }
    }
}
