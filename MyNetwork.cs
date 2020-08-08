using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Chatroom {
    public static class MyNetwork {
        public static string Read(Socket socket, int mxLen = 1024) {
            // There won't be two threads reading bytes, mostly...
            byte[] bytes = new byte[mxLen];
            int bytesLen = socket.Receive(bytes);
            return Encoding.UTF8.GetString(bytes, 0, bytesLen);
        }
        public static void Write(Socket socket, string text) {
            lock (socket) { // But it can be here!
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                socket.Send(bytes);
            }
        }
    }
}