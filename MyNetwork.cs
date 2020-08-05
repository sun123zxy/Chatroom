using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Chatroom {
    public static class MyNetwork {
        public static string Read(NetworkStream ns) {
            //lock (ns) {
                byte[] bytes = new byte[1024];
                int bytesLen = ns.Read(bytes, 0, bytes.Length);
                return Encoding.UTF8.GetString(bytes, 0, bytesLen);
            //}
        }
        public static void Write(NetworkStream ns, string text) {
            lock (ns) {
                byte[] byteMsg = Encoding.UTF8.GetBytes(text);
                ns.Write(byteMsg, 0, byteMsg.Length);
            }
        }
    }
}