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
using System.Threading;
using Microsoft.VisualBasic;

namespace Chatroom {
    public class User {
        public Socket socket;
        public string username;
        public User() { }
        public User(Socket socket, string username) {
            this.socket = socket;
            this.username = username;
        }
        public IPAddress IP {
            get {
                return ((IPEndPoint)socket.RemoteEndPoint).Address;
            }
        }
    }
    public class ServerWindow : UniversalWindow {

        public Dictionary<string, User> users;
        public Socket server;

        public HashSet<IPAddress> bannedIp;

        public override void UniversalWindow_Loaded(object sender, RoutedEventArgs e) {
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

            Title += " - Server";

            users = new Dictionary<string, User>();
            bannedIp = new HashSet<IPAddress>();

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
                    user.username = MyNetwork.Read(user.socket);
                } catch {
                    ShowMsg("A new connection lost when sending username. " + "(" + user.socket.RemoteEndPoint.ToString() + ")");
                    continue;
                }
                if (bannedIp.Contains(user.IP)) { //Banned IP
                    SendToUser(user, "/refuse_banned");
                    ShowMsg("A new connection was refused because its IP has been banned. " + "(" + user.username + ", " + user.socket.RemoteEndPoint.ToString() + ")");
                    continue;
                }
                if (users.ContainsKey(user.username)) { //Duplicate Name
                    SendToUser(user, "/refuse_duplicate");
                    ShowMsg("A new connection was refused because of its duplicated username. " + "(" + user.username + ", " + user.socket.RemoteEndPoint.ToString() + ")");
                    continue;
                }
                users.Add(user.username ,user);
                ShowMsg("A new connection builded. " + "(" + user.username + ", " + user.socket.RemoteEndPoint.ToString() + ")");

                Broadcast(user.username + " entered the chatroom.");

                Thread read = new Thread(delegate () { ReadAndBroadcast(user); });
                read.IsBackground = true;
                read.Start();
            }
        }
        void ReadAndBroadcast(User user) {
            try {
                while (true) {
                    string text = MyNetwork.Read(user.socket);
                    Broadcast(user.username + ": " + text);
                }
            } catch {
                Offline(user);
            }
        }
        public void SendToUser(User user, string text, CallBack callBack = null) {
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
        public void Broadcast(string text) {
            foreach (KeyValuePair<string, User> kvp in users) { User user = kvp.Value;
                SendToUser(user, text);
            }
            ShowMsg(text);
        }
        public void Offline(User user) {
            lock (users) {
                if (users.ContainsKey(user.username)) {
                    users.Remove(user.username);
                    user.socket.Close();
                    Broadcast(user.username + " was no longer in the chatroom.");
                }
            }
        }
        public override void UniversalWindow_Unloaded(object sender, RoutedEventArgs e) {
            if (server != null) server.Close();
            foreach (KeyValuePair<string, User> kvp in users) { User user = kvp.Value;
                user.socket.Close();
            }
        }
        public override void Send(string text) {
            Broadcast("Server: " + text);
        }
        public override void OptCmdFromSelf(string cmd) {
            ShowMsg(cmd);
            string[] args = cmd.Split(' ');
            switch (args[0]) {
                case "/kick":
                    if (args.Length == 2) {
                        if (users.ContainsKey(args[1])) {
                            User user = users[args[1]];
                            SendToUser(user, "/kick", delegate() {
                                Offline(user);
                                Broadcast(user.username + " was kicked out by server admin.");
                            });
                        } else {
                            ShowMsg("Cannot find a user called " + args[1]);
                        }
                    } else {
                        ShowMsg("Wrong usage.");
                    }
                    break;
                case "/ban":
                    if(args.Length == 1) {
                        ShowMsg("Banned IPs:");
                        foreach(IPAddress ip in bannedIp) {
                            ShowMsg("    " + ip.ToString());
                        }
                    } else if (args.Length == 2) {
                        if (IPAddress.TryParse(args[1], out IPAddress ip)) {

                        } else if (users.ContainsKey(args[1])) {
                            User user = users[args[1]];
                            ip = user.IP;
                        } else {
                            ShowMsg(args[1] + " is not a IP Address or a username.");
                            break;
                        }
                        bannedIp.Add(ip);
                        Broadcast(ip.ToString() + " has been banned by server admin.");
                        foreach (KeyValuePair<string, User> kvp in users) { User user = kvp.Value;
                            if(Equals(user.IP, ip)) {
                                SendToUser(user, "/ban", delegate() {
                                    Broadcast(user.username + " has been banned according to the new IP ban list.");
                                    Offline(user);
                                });
                            }
                        }
                    } else {
                        ShowMsg("Wrong usage.");
                    }
                    break;
                case "/unban":
                    if (args.Length == 2) {
                        if (!IPAddress.TryParse(args[1], out IPAddress ip)) {
                            ShowMsg(args[1] + " is not an IP address");
                            break;
                        }
                        if (bannedIp.Contains(ip)) {
                            bannedIp.Remove(ip);
                            Broadcast(ip.ToString() + " is no longer in the ban list.");
                        } else {
                            ShowMsg(ip.ToString() + " is not in the ban list.");
                        }
                    } else {
                        ShowMsg("Wrong usage.");
                    }
                    break;
                default:
                    ShowMsg("Cannot find this command.");
                    break;
            }
        }
    }
}
