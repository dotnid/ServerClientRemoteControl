using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteControlV1
{
    internal class Server
    {
        
        TcpListener Listener = new TcpListener(IPAddress.Any, 2212);
        HashSet<Client> Clients = new HashSet<Client>();
        HashSet<Remote> Remotes = new HashSet<Remote>();
        object syncGate = new object();
        public void Start()
        {
            Listener.Start();
            Console.WriteLine("Server started");
            StartAccept();
        }
        private void StartAccept()
        {
            Listener.BeginAcceptTcpClient(HandleAsyncConnection, Listener);
        }
        private void HandleAsyncConnection(IAsyncResult res)
        {
            StartAccept(); //listen for new connections again
            var clientSocket = Listener.EndAcceptTcpClient(res);
            var netStream = clientSocket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);

            var ClientName = read.ReadString();
            var ClientAddress = read.ReadString();
            var ClientRole = read.ReadString();

            if (ClientRole == "client")
            {
                var client = new Client(this, clientSocket, ClientName, ClientAddress, ClientRole);
                new Task(delegate { client.ReceivedMessage(); }).Start();
                //new Task(() => client.ReceivedMessage()).Start();
                //client.StartClient();
                lock (syncGate)
                {
                    Clients.Add(client);
                    Console.WriteLine("New Client connected {0} => {1}", client.ClientSocket.GetHashCode(), client.ClientName);
                }

            }
            else if (ClientRole == "remote")
            {
                var remote = new Remote(this, clientSocket, ClientName, ClientAddress, ClientRole);
                new Task(delegate { remote.ReceivedMessage(); }).Start();
                //new Task(() => remote.ReceivedMessage()).Start();
                //remote.StartRemote();
                lock (syncGate)
                {
                    Remotes.Add(remote);
                    Console.WriteLine("New Client connected {0} => {1}", remote.ClientSocket.GetHashCode(), remote.ClientName);
                }
            }
            else 
            {
                Console.WriteLine("Role ERROR!");
            }
                /**/
            


        }
        internal void OnDisconnected(Client client)
        {
            lock (syncGate)
            {
                Clients.Remove(client);
                Console.WriteLine("Client disconnected {0} => {1}", client.ClientSocket.GetHashCode(), client.ClientName);
                
            }
        }
        internal void RemoteOnDisconnected(Remote remote)
        {
            lock (syncGate)
            {
                Remotes.Remove(remote);
                Console.WriteLine("Client disconnected {0} => {1}", remote.ClientSocket.GetHashCode(), remote.ClientName);
                
            }
        }
        internal void OnMessageReceived(Client sender, TcpClient socket)
        {
            lock (syncGate)
            {

                foreach (var remote in Remotes)
                {
                    //Console.WriteLine("Sender : {0}", sender.ClientName);

                    if (remote.ClientAddress == sender.ClientAddress)
                    {
                        remote.RemoteOnMessageReceived(sender, socket);
                        //new Task(delegate { remote.RemoteOnMessageReceived(sender, socket); }).Start();

                    }
                    else
                    {
                        remote.NoClient(remote, socket);
                    }
                }
            }
        }
        internal void RemoteOnMessageReceived(Remote sender, TcpClient socket)
        {
            lock (syncGate)
            {

                foreach (var client in Clients)
                {
                    //Console.WriteLine("Sender : {0}", sender.ClientName);
                    if (client.ClientAddress == sender.ClientAddress)
                    {
                        //new Task(delegate { client.OnMessageReceived(sender, socket); }).Start();
                        client.OnMessageReceived(sender, socket);
                    }
                    else
                    {
                        client.NoRemote(client, socket);
                    }
                }
            }
        }
    }

    internal class Client
    {
        public BinaryFormatter binaryFormatter;
        public const string CommandImage = "RECIEVEIMAGE";
        public const string CommandCursor = "RECIEVECURSORPOSITION";
        public const string CommandStop = "STOP";
        public const string CommandKeyPressed = "KEY";
        public const string CommandKeyDown = "KEYDOWN";
        public const string CommandKeyUp = "KEYUP";
        public const string CommandMousePressed = "MOUSE";
        public const string CommandLeftMouseUP = "LFU";
        public const string CommandLeftMouseDOWN = "LFD";
        public const string CommandRightMouseUP = "RFU";
        public const string CommandRightMouseDOWN = "RFD";
        public const string CommandCtrlAltDelete = "CAD";

        private const string CommandRemote = "CR";
        private const string CommandMiddleMouseUP = "MFU";
        private const string CommandMiddleMouseDOWN = "MFD";
        private const string CommandShutdown = "CSD";
        private const string WinRes = "WINRES";
        private const string CommandMouseMove = "MOUSEMOVE";
        private const string RIDLE = "RIDLE";
        private const string CIDLE = "CIDLE";

        private const string PING = "PING";
        private const string HANDLER = "HANDLER";
        private const string REMOTESET = "REMOTESET";
        private const string CLIENT = "CLIENT";
        private const string REMOTE = "REMOTE";
        private const string MSG = "MSG";
        private const string GETWINRES = "GETWINRES";

        public readonly Server Server;
        public TcpClient ClientSocket;
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientRole { get; set; }
        public static Task listeningTask;
        public static Task transmissionTask;
        public static Boolean isOnline { get; set; }
        object obj = new object();

        public Client(Server server, TcpClient clientSocket, string name, string address, string role)
        {
            Server = server;
            ClientSocket = clientSocket;
            var netStream = ClientSocket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);

            ClientName = name;
            ClientAddress = address;
            ClientRole = role;

        }
        public void StartClient()
        {

            isOnline = true;

            //new Thread(ReceivedMessage).Start();
            //var receivemessagetask = new Task(() => ReceivedMessage());
            //receivemessagetask.Start();
            //receivemessagetask.Wait();
            ReceivedMessage();
        }
        public void ReceivedMessage()
        {
            isOnline = true;
            try
            {
                var netStream = ClientSocket.GetStream();
                var read = new BinaryReader(netStream, Encoding.UTF8);
                var write = new BinaryWriter(netStream, Encoding.UTF8);

                while (true)
                {
                    lock (obj)
                    {
                        try
                        {
                            //new Task(() => Server.OnMessageReceived(this, ClientSocket)).Start();
                            Server.OnMessageReceived(this, ClientSocket);
                            //new Task(delegate { Server.OnMessageReceived(this, ClientSocket); }).Start();

                        }
                        catch (Exception ex)
                        {
                            read.Close();
                            netStream.Close();
                            Console.WriteLine(ex.Message);
                            return;
                        }
                        // Console.WriteLine("Loopsss..");
                        //Thread.Sleep(10);

                    }

                }
            }
            finally
            {
                Server.OnDisconnected(this);
            }
        }
        internal void OnMessageReceived(Remote sender, TcpClient socket)
        {

            var netStream = sender.ClientSocket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);
            var message = read.ReadString();

            //Console.WriteLine(sender.ClientName + ": " + message);
            switch (message)
            {
                case CommandCursor:
                    //Console.WriteLine("Server : Cursor {0} {1}", read.ReadInt32(), read.ReadInt32());
                    UpdateCursorPosition(read.ReadInt32(), read.ReadInt32());
                    break;
                case CommandMousePressed:
                    //PrintMsg("CommandMousePressed");
                    //new Task(()=>InputMouseClicked(read.ReadString())).Start();
                    InputMouseClicked(read.ReadString());
                    break;
                case CommandKeyPressed:
                    //PrintMsg("CommandKeyPressed");
                    //new Task(()=>InputKeyPressed(read.ReadInt32())).Start();
                    InputKeyPressed(read.ReadInt32());
                    break;
                case CommandKeyDown:
                    //PrintMsg("CommandKeyDown");
                    //new Task(()=>InputKeyDown(read.ReadInt32())).Start();
                    InputKeyDown(read.ReadInt32());
                    break;
                case CommandKeyUp:
                    //PrintMsg("CommandKeyUp");
                    //new Task(()=>InputKeyUp(read.ReadInt32())).Start();
                    InputKeyUp(read.ReadInt32());
                    break;
                case CommandMouseMove:
                    //Console.WriteLine("Server : mouse {0}", read.ReadBoolean());
                    mouseMove(read.ReadBoolean());
                    break;
                case CommandShutdown:
                    Process.Start("shutdown", "/s /t 0");
                    break;
                case CommandCtrlAltDelete:
                    InputCAD();
                    break;
                case PING:
                    PrintMsg("Ping Masuk!");
                    //new Task(()=>PINGING()).Start();
                    PINGING();
                    break;
                case RIDLE:
                    ridle();
                    PrintMsg("Receive and Send RIDLE..");
                    break;
                case CIDLE:
                    //new Thread(new ThreadStart(cidle)).Start();
                    //PrintMsg("Receive and Send CIDLE.");
                    break;
            
                default:

                    break;

            }

        }
        internal void NoRemote(Client sender, TcpClient socket)
        {
            var netStream = socket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);
            var message = read.ReadString();

        }
        private void mouseMove(bool mouseInput)
        {
            try
            {
                PrintMsg("Mouse Move : " + mouseInput);
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(CommandMouseMove);
                write.Write(mouseInput);
                write.Flush();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //new Task(delegate { sendmousemove(mouseInput); }).Start();
        }
        private void sendmousemove(bool input)
        {
            var val = input;
            lock (obj)
            {
                try
                {
                    PrintMsg("Mouse Move : " + val);
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);

                    write.Write(CommandMouseMove);
                    write.Write(val);
                    write.Flush();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        private void UpdateCursorPosition(int x, int y)
        {
            /*
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(CommandCursor);
                write.Write(x);
                write.Write(y);
                write.Flush();
                Console.WriteLine("Move {0} {1}", x, y);
            }
            catch (Exception ex)
            {

            }
            /**/
            int xx = x;
            int yy = y;
            
            new Task(delegate { sendcursor(xx, yy); }).Start();
        }
        private void sendcursor(int x, int y)
        {
            int xx = x;
            int yy = y;

            lock (obj)
            {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);

                    write.Write(CommandCursor);
                    write.Write(xx);
                    write.Write(yy);
                    write.Flush();
                    Console.WriteLine("Move {0} {1}", x, y);
                }
                catch (Exception ex)
                {

                }
            }
        }
        public void InputMouseClicked(string mouse)
        {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    string getmouse = mouse;
                    write.Write(CommandMousePressed);
                    write.Write(getmouse);
                    write.Flush();
                    Console.WriteLine("Clicked {0} ", mouse);
                }
                catch (Exception ex)
                {

                }
            
        }

        public void InputKeyPressed(Int32 input)
        {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    int getinput = input;
                    write.Write(CommandKeyPressed);
                    write.Write(getinput);
                    write.Flush();
                    Console.WriteLine("Input {0} ", getinput);
                }
                catch (Exception ex)
                {

                }
            
        }
        public void InputKeyDown(Int32 input)
        {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    int getinput = input;
                    write.Write(CommandKeyDown);
                    write.Write(getinput);
                    write.Flush();
                    Console.WriteLine("Input {0} ", getinput);
                }
                catch (Exception ex)
                {

                }
            
        }
        public void InputKeyUp(Int32 input)
        {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    int getinput = input;
                    write.Write(CommandKeyUp);
                    write.Write(getinput);
                    write.Flush();
                }
                catch (Exception ex)
                {

                }
            
        }
        public void InputCAD()
        {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    write.Write(CommandCtrlAltDelete);
                    write.Flush();
                }
                catch (Exception ex)
                {

                }
            

        }
        private void SendWinRes(int a, int b)
        {
                try
                {
                    var netStream = ClientSocket.GetStream();
                    var write = new BinaryWriter(netStream);

                    //Console.WriteLine("Send Winres.. {0}, {1}", a, b);
                    write.Write(WinRes);
                    write.Write(a);
                    write.Write(b);
                    write.Flush();
                }
                catch (Exception ex)
                {

                }
        }
        private void RecieveImage(int x, int y, NetworkStream ns)
        {
            //Console.WriteLine(sc);
            var netStream = ClientSocket.GetStream();
            var write = new BinaryWriter(netStream);
            try
            {
                write.Write(CommandImage);
                write.Write(x);
                write.Write(y);
                write.Flush();

                Bitmap screenshot = (Bitmap)DesktopScreen.DeserializeScreen(ns);
                DesktopScreen.SerializeScreen(netStream, screenshot);

                netStream.Flush();

                //Console.WriteLine("Received Winres and Send {0} {1}", aa, bb);
                Console.WriteLine("Server : Receive and Send image..");


            }
            catch (Exception e)
            {
                PrintMsg(e.ToString());
            }


        }
        public void PINGING()
        {
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(PING);
                write.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
            }

        }
        public void GetWinres()
        {
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(GETWINRES);
                write.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
            }

        }

        public void ridle()
        {
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(RIDLE);
                write.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
            }

        }

        public static void PrintMsg(String msg)
        {
            Console.WriteLine("Server : " + msg);
        }
        public static void PrintError(Exception msg)
        {
            Console.WriteLine("Error : " + msg);
        }

    }


    internal class Remote
    {
        public BinaryFormatter binaryFormatter;
        public const string CommandImage = "RECIEVEIMAGE";
        public const string CommandCursor = "RECIEVECURSORPOSITION";
        public const string CommandStop = "STOP";
        public const string CommandKeyPressed = "KEY";
        public const string CommandKeyDown = "KEYDOWN";
        public const string CommandKeyUp = "KEYUP";
        public const string CommandMousePressed = "MOUSE";
        public const string CommandLeftMouseUP = "LFU";
        public const string CommandLeftMouseDOWN = "LFD";
        public const string CommandRightMouseUP = "RFU";
        public const string CommandRightMouseDOWN = "RFD";
        public const string CommandCtrlAltDelete = "CAD";

        private const string CommandRemote = "CR";
        private const string CommandMiddleMouseUP = "MFU";
        private const string CommandMiddleMouseDOWN = "MFD";
        private const string CommandShutdown = "CSD";
        private const string WinRes = "WINRES";
        private const string CommandMouseMove = "MOUSEMOVE";
        private const string RIDLE = "RIDLE";
        private const string CIDLE = "CIDLE";

        private const string PING = "PING";
        private const string HANDLER = "HANDLER";
        private const string REMOTESET = "REMOTESET";
        private const string CLIENT = "CLIENT";
        private const string REMOTE = "REMOTE";
        private const string MSG = "MSG";
        private const string GETWINRES = "GETWINRES";

        public readonly Server Server;
        public TcpClient ClientSocket;
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientRole { get; set; }
        public static Task listeningTask;
        public static Task transmissionTask;
        public static Boolean isOnline = false;
        object obj = new object();

        public Remote(Server server, TcpClient clientSocket, string name, string address, string role)
        {
            Server = server;
            ClientSocket = clientSocket;
            var netStream = ClientSocket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);

            ClientName = name;
            ClientAddress = address;
            ClientRole = role;

        }
        public void StartRemote()
        {

            isOnline = true;

            //new Thread(ReceivedMessage).Start();
            //var receivemessagetask = new Task(() => ReceivedMessage());
            //receivemessagetask.RunSynchronously();
            ReceivedMessage();
        }
        public void ReceivedMessage()
        {
            isOnline = true;
            try
            {
                var netStream = ClientSocket.GetStream();
                var read = new BinaryReader(netStream, Encoding.UTF8);
                var write = new BinaryWriter(netStream, Encoding.UTF8);

                while (true)
                {
                    lock (obj)
                    {
                        try
                        {
                            //new Task(() => Server.RemoteOnMessageReceived(this, ClientSocket)).Start();
                            Server.RemoteOnMessageReceived(this, ClientSocket);
                            //new Task(delegate { Server.RemoteOnMessageReceived(this, ClientSocket); }).Start();
                            
                        }
                        catch (Exception ex)
                        {
                            read.Close();
                            netStream.Close();
                            Console.WriteLine(ex.Message);
                            return;
                        }
                    }
                    //Thread.Sleep(1);

                }
            }
            finally
            {
                Server.RemoteOnDisconnected(this);
            }
        }
        internal void NoClient(Remote sender, TcpClient socket)
        {
            var netStream = socket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);
            
            //Console.WriteLine(sender.ClientName + ": " + message);
            
        }

        internal void RemoteOnMessageReceived(Client sender, TcpClient socket)
        {
            var netStream = sender.ClientSocket.GetStream();
            var read = new BinaryReader(netStream, Encoding.UTF8);
            var write = new BinaryWriter(netStream, Encoding.UTF8);
            var message = read.ReadString();

            //Console.WriteLine(sender.ClientName + ": " + message);
            switch (message)
            {
                case CommandImage:
                    RecieveImage(read.ReadInt32(), read.ReadInt32(), netStream);
                    break;
                case CommandShutdown:
                    Process.Start("shutdown", "/s /t 0");
                    break;
                case PING:
                    PrintMsg("Ping Masuk!");
                    //new Task(()=>PINGING()).Start();
                    PINGING();
                    break;
                case RIDLE:
                    ridle();
                    PrintMsg("Receive and Send RIDLE..");
                    break;
                case CIDLE:
                    //new Thread(new ThreadStart(cidle)).Start();
                    //PrintMsg("Receive and Send CIDLE.");
                    break;
                default:

                    break;

            }

        }
        private void RecieveImage(int x, int y, NetworkStream ns)
        {
            //Console.WriteLine(sc);
            var netStream = ClientSocket.GetStream();
            var write = new BinaryWriter(netStream);
            try
            {
                write.Write(CommandImage);
                write.Write(x);
                write.Write(y);
                write.Flush();

                Bitmap screenshot = (Bitmap)DesktopScreen.DeserializeScreen(ns);
                DesktopScreen.SerializeScreen(netStream, screenshot);

                netStream.Flush();

                //Console.WriteLine("Received Winres and Send {0} {1}", aa, bb);
                Console.WriteLine("Server from Client : Receive and Send image..");


            }
            catch (Exception e)
            {
                PrintMsg(e.ToString());
            }


        }
        public void PINGING()
        {
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(PING);
                write.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
            }

        }
        public void GetWinres()
        {
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(GETWINRES);
                write.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
            }

        }

        public void ridle()
        {
            try
            {
                var netStream = ClientSocket.GetStream();
                var write = new BinaryWriter(netStream);

                write.Write(RIDLE);
                write.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
            }

        }

        public static void PrintMsg(String msg)
        {
            Console.WriteLine("Server : " + msg);
        }
        public static void PrintError(Exception msg)
        {
            Console.WriteLine("Error : " + msg);
        }

    }

}