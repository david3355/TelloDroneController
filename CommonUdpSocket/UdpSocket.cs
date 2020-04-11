using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace CommonUdpSocket
{
    public class UDPSocket
    {
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint endpointSender = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public bool Connected
        {
            get { return socket.Connected; }
        }

        public void Server(string address, int port)
        {
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        }

        public void Client(string address, int port)
        {
            socket.Connect(IPAddress.Parse(address), port);
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, (asyncResult) =>
            {
                State so = (State)asyncResult.AsyncState;
                int bytes = socket.EndSend(asyncResult);
            }, state);
        }

        public void Receive(UdpResponseCallback ReadCallback)
        {
            socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref endpointSender, recv = (asyncResult) =>
            {
                State so = (State)asyncResult.AsyncState;
                int bytes = socket.EndReceiveFrom(asyncResult, ref endpointSender);
                socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref endpointSender, recv, so);
                string data = Encoding.ASCII.GetString(so.buffer, 0, bytes);
                string senderHost = (endpointSender as IPEndPoint).Address.ToString();
                int senderPort = (endpointSender as IPEndPoint).Port;
                if (ReadCallback != null) ReadCallback(senderHost, senderPort, data);
            }, state);
        }
    }
}
