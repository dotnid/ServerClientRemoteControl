using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net.Sockets;

namespace RemoteControlV1{
    
    public class ServerEventArgs : EventArgs
    {
        public Image Image { get; set; }
        public Point CursorPosition { get; set; }
    }
    class SocketAcceptedEventHandler : EventArgs
    {
        public Socket Accepted
        {
            get;
            private set;
        }
        public SocketAcceptedEventHandler(Socket s)
        {
            Accepted = s;
        }
    }

}
