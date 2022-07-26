using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;

namespace RemoteControlV1
{


    class ServerHost
    {
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
        public const string CommandShutdown = "CSD";
        public const string CommandRightMouseDOWN = "RFD";
        public const string CommandCtrlAltDelete = "CAD";

        private const string CommandMiddleMouseUP = "MFU";
        private const string CommandMiddleMouseDOWN = "MFD";
        private const string CommandMouseMove = "MOUSEMOVE";
        private const string CommandRemote = "CR";
        private const string WinRes = "WINRES";
        private const string CLIENT = "CLIENT";
        private const string REMOTE = "REMOTE";
        private const string IDLE = "IDLE";
        private const string PING = "PING";
        private const string HANDLER = "HANDLER";
        private const string TEXT = "TEXT";
        public static int xmove { get; set; }
        public static int ymove { get; set; }
        public static int pbwidth { get; set; }
        public static int pbheight { get; set; }
        public static int clientwidth { get; set; }
        public static int clientheight { get; set; }
        public static bool listening { get; set; }
        public static int newPort { get; set; }
        public static string clientname { get; set; }

        public static event EventHandler<ServerEventArgs> EventImageRecieved;

        private static IPAddress serverIP;
        private static int serverPort;
        private static TcpListener server;
        private static Boolean connectionTerminated = true;
        private static TcpClient client;
        private static MyTcpClient myclient;
        private static Thread thread;

        // public static TcpClient server;
        public static NetworkStream netStream;
        public static BinaryReader binaryReader;
        public static BinaryWriter binaryWriter;
        public static Boolean isOnline = false;
        public static Boolean sendMouseInput = false;
        private static BinaryFormatter binaryFormatter;
        public static Boolean screenRemote = false;

        public static Form parentForm;
        public static Form childForm;

        public static Task listeningTask;
        public static Task transmissionTask;

        //public static Task acceptClientTask;
        
        public static void Start()
        {
            try
            {
                
                serverIP = IPAddress.Any;
                serverPort = 2212;
                server = new TcpListener(serverIP, serverPort);
                binaryFormatter = new BinaryFormatter();
                isOnline = true;
                connectionTerminated = false;
                server.Start();

            }
            catch (Exception ex)
            {
                PrintError(ex);
            }


        }
        
        public static void Stop(IErrorLogger log)
        {

            if (connectionTerminated == false)
            {
                try
                {
                    isOnline = false;
                    binaryWriter.Close();
                    binaryReader.Close();
                    netStream.Close();
                    client.Close();
                    server.Stop();
                    connectionTerminated = true;
                    PrintMsg("Reconnecting...");
                    Start();
                    new Task(() => Listen()).Start();
                    //if (parentForm != null)
                    //{
                    //    parentForm.Invoke((MethodInvoker)delegate () { parentForm.Close(); });
                    //}

                }
                catch (Exception ex)
                {
                    log.HandleException(ex);
                }
            }

        }


        public static void AcceptClient()
        {
            client = server.AcceptTcpClient();
            netStream = client.GetStream();
            binaryReader = new BinaryReader(netStream, Encoding.UTF8);
            binaryWriter = new BinaryWriter(netStream, Encoding.UTF8);

            ///ThreadPool.QueueUserWorkItem(ThreadProc, client);
            
        }
        private static void ThreadProc(object obj)
        {
            try
            {
                //thread = Thread.CurrentThread;
                //string message = $"Background: {thread.IsBackground}, Thread Pool: {thread.IsThreadPoolThread}, Thread ID: {thread.ManagedThreadId}";
                //Console.WriteLine(message);
                client = (TcpClient)obj;
                myclient = new MyTcpClient(client);
                //Console.WriteLine(myclient.Id.Equals(0));
                //binaryReader = new BinaryReader(netStream, Encoding.UTF8);
                //binaryWriter = new BinaryWriter(netStream, Encoding.UTF8);
                // Do your work here
                /*
                binaryWriter.Write(HANDLER);
                binaryWriter.Write(message);
                binaryWriter.Flush();
                clientname = binaryReader.ReadString();
                binaryWriter.Flush();
                */
                if (myclient.Id.Equals(1))
                {
                    netStream = myclient.TcpClient.GetStream();
                    binaryReader = new BinaryReader(netStream, Encoding.UTF8);
                    binaryWriter = new BinaryWriter(netStream, Encoding.UTF8);

                    listeningTask = new Task(() => RecieveTransmission());
                    transmissionTask = new Task(() => SendTransmission());

                    listeningTask.Start();
                    transmissionTask.Start();
                    listeningTask.Wait();
                    transmissionTask.Wait();

                    Stop(new ServerErrorHandler("Client disconnected."));
                }
                if (myclient.Id.Equals(2))
                {
                    netStream = myclient.TcpClient.GetStream();
                    binaryReader = new BinaryReader(netStream, Encoding.UTF8);
                    binaryWriter = new BinaryWriter(netStream, Encoding.UTF8);

                    listeningTask = new Task(() => RecieveTransmission());
                    transmissionTask = new Task(() => SendTransmission());

                    listeningTask.Start();
                    transmissionTask.Start();
                    listeningTask.Wait();
                    transmissionTask.Wait();

                    Stop(new ServerErrorHandler("Client disconnected."));
                }
                //Console.WriteLine("New Connection.");

            }
            catch (Exception ex)
            {
                PrintError(ex);
            }
            
        }

        public static void Connect(string ipAdress, int port)
        {
            SetupFields(new ServerErrorHandler("Error connecting to server"), ipAdress, port);

        }

        private static void SetupFields(IErrorLogger log, string ipAdress, int port)
        {
            if (client != null) client.Close();
            client = new TcpClient();
            try
            {
                client.Connect(ipAdress, port);
                netStream = client.GetStream();

                binaryReader = new BinaryReader(netStream, Encoding.UTF8);
                binaryWriter = new BinaryWriter(netStream, Encoding.UTF8);
                binaryFormatter = new BinaryFormatter();
                isOnline = true;
            }
            catch (Exception e)
            {
                log.HandleException(e);
            }


        }
        public static void Listen()
        {
            /*
            while (isOnline)
            {
                Console.WriteLine("Waiting Client...");
                AcceptClient();

            }
            */
            Task acceptClientTask = new Task(() => AcceptClient());

            acceptClientTask.Start();
            acceptClientTask.Wait();
            
            listeningTask = new Task(() => RecieveTransmission());
            transmissionTask = new Task(() => SendTransmission());

            listeningTask.Start();
            transmissionTask.Start();
            listeningTask.Wait();
            transmissionTask.Wait();

            Stop(new ServerErrorHandler("Client disconnected."));
            

        }
        public static void Connect()
        {

            listeningTask = new Task(() => RecieveTransmission());
            transmissionTask = new Task(() => SendTransmission());

            listeningTask.Start();
            transmissionTask.Start();
            listeningTask.Wait();
            transmissionTask.Wait();


            Disconnect(new ServerErrorHandler("Connection ending error."));

        }
        public static void ConnectIP(string ipAdress, int port)
        {
            SetupFields(new ServerErrorHandler("Error connecting to server"), ipAdress, port);

        }

        public static void RecieveTransmission()
        {
            while (isOnline)
            {
                try
                {
                    string message = binaryReader.ReadString();

                    switch (message)
                    {
                        case CLIENT:
                            ClientToRemoteTransmission();
                            break;
                        case REMOTE:
                            RemoteToClientTransmission();
                            break;
                        case CommandImage:
                            RecieveImage();
                            break;
                        case WinRes:
                            clientwidth = binaryReader.ReadInt32();
                            clientheight = binaryReader.ReadInt32();
                            break;
                        case PING:
                            PrintMsg("Client Ping Received !");
                            break;
                        case IDLE:
                            PrintMsg("IDLE OK!");
                            //binaryWriter.Write("IDLE");
                            //binaryWriter.Flush();
                            break;

                        default:

                            break;

                    }

                }
                catch (Exception ex)
                {
                    PrintError(ex);
                    isOnline = false;
                }
            }

        }

        public static void SendTransmission()
        {

            //if (sendMouseInput == true)
            //{

            Point startingPoint = new Point(0, 0);
            Point endingPoint = new Point(0, 0);
            Point deltaPoint = new Point(0, 0);
            int tmpx = 1;
            int tmpy = 1;
            while (isOnline)
            {
                if (sendMouseInput == true && screenRemote == true)
                {

                    try
                    {
                        int X = xmove;
                        int Y = ymove;

                        //Console.WriteLine("X :" + Cursor.Position.X + " " + Cursor.Position.Y);
                        //deltaPoint.X = endingPoint.X - startingPoint.X;                                         // how to handle delta properly. AKA how to structure
                        //deltaPoint.Y = endingPoint.Y - startingPoint.Y;
                        //deltaPoint.X = Cursor.Position.X + Cursor.Position.X * clientwidth / Screen.PrimaryScreen.Bounds.Width;
                        //deltaPoint.Y = Cursor.Position.Y + Cursor.Position.Y * clientheight / Screen.PrimaryScreen.Bounds.Height;
                        //deltaPoint.X = xmove + xmove * clientwidth / pbwidth;
                        //deltaPoint.Y = ymove + ymove * clientheight / pbheight;

                        //startingPoint.X = X;
                        //startingPoint.Y = Y;
                        //Console.WriteLine("Delta X :" + startingPoint.X + " " + startingPoint.Y);
                        //Console.WriteLine("Delta X :" + deltaPoint.X + " " + deltaPoint.Y);
                        //Console.WriteLine("Delta X :" + endingPoint.X + " " + endingPoint.Y);
                        if (tmpx != X || tmpy != Y)
                        {
                            binaryWriter.Write(CommandMouseMove);
                            binaryWriter.Write(sendMouseInput);
                            binaryWriter.Flush();

                            binaryWriter.Write(CommandCursor);
                            binaryWriter.Write(X);
                            binaryWriter.Write(Y);
                            binaryWriter.Flush();
                            Console.WriteLine("Move :" + X + " " + Y);
                        }

                        tmpx = X;
                        tmpy = Y;
                        //sendMouseInput = false;
                        Thread.Sleep(0);
                        //endingPoint.X = X;
                        //endingPoint.Y = Y;

                    }
                    catch (Exception ex)
                    {
                        PrintError(ex);
                        isOnline = false;
                    }

                }
                else
                {
                    //binaryWriter.Write(IDLE);
                    //binaryWriter.Flush();

                    // Do nothing.
                }
            }
            //}
        }

        public static void Disconnect(IErrorLogger log)
        {
            isOnline = false;
            try
            {
                binaryReader.Close();
                binaryWriter.Close();
                netStream.Close();
                client.Close();
                if (parentForm != null)
                {
                    parentForm.Invoke((MethodInvoker)delegate () { parentForm.Close(); });
                }

            }
            catch (Exception ex)
            {
                PrintError(ex);
                log.HandleException(ex);
            }

        }

        private static void RecieveImage()
        {
            Image screenshot = (Image)DesktopScreen.DeserializeScreen(netStream);
            OnEventImageRecieved(new ServerEventArgs() { Image = screenshot });
        }



        public static void OnEventImageRecieved(ServerEventArgs args)
        {
            if (EventImageRecieved != null)
            {
                EventImageRecieved(null, args);
            }
        }

        public static void SendCAD()
        {
            binaryWriter.Write(CommandCtrlAltDelete);
            binaryWriter.Flush();

        }
        public static void RemoteToClientTransmission()
        {
            while (isOnline)
            {
                try
                {
                    string message = binaryReader.ReadString();

                    switch (message)
                    {
                        case CommandImage:
                            RecieveImage();
                            break;
                        case WinRes:
                            clientwidth = binaryReader.ReadInt32();
                            clientheight = binaryReader.ReadInt32();
                            break;
                        default:

                            break;

                    }

                }
                catch (Exception ex)
                {
                    PrintError(ex);
                    isOnline = false;
                }
            }

        }
        //Client Transmission to Remote
        public static void ClientToRemoteTransmission()
        {
            while (isOnline)
            {
                try
                {
                    string message = binaryReader.ReadString();

                    switch (message)
                    {
                        case CommandCursor:
                            UpdateCursorPosition();
                            break;
                        case CommandMousePressed:
                            InputMouseClicked();
                            break;
                        case CommandKeyPressed:
                            InputKeyPressed();
                            break;
                        case CommandKeyDown:
                            InputKeyDown();
                            break;
                        case CommandKeyUp:
                            InputKeyUp();
                            break;
                        case CommandShutdown:
                            //Process.Start("shutdown", "/s /t 0");
                            break;
                        case CommandMouseMove:
                            mouseMove();
                            break;
                        case CommandCtrlAltDelete:
                            InputCAD();
                            break;
                        case "PING":
                            PrintMsg("Ping OK!");
                            break;
                        default:

                            break;

                    }

                }
                catch (Exception ex)
                {
                    PrintError(ex);
                    isOnline = false;
                }
            }
        }
        private static void mouseMove()
        {
            binaryWriter.Write(binaryReader.ReadBoolean());
            binaryWriter.Flush();

        }
        private static void UpdateCursorPosition()
        {
            binaryWriter.Write(binaryReader.ReadInt32());
            binaryWriter.Write(binaryReader.ReadInt32());
            binaryWriter.Flush();
        }
        public static void InputMouseClicked()
        {
            binaryWriter.Write(binaryReader.ReadString());
            binaryWriter.Flush();
        }

        public static void InputKeyPressed()
        {
            binaryWriter.Write(binaryReader.ReadInt32());
            binaryWriter.Flush();
        }
        public static void InputKeyDown()
        {
            binaryWriter.Write(binaryReader.ReadInt32());
            binaryWriter.Flush();
        }
        public static void InputKeyUp()
        {
            binaryWriter.Write(binaryReader.ReadInt32());
            binaryWriter.Flush();
        }
        public static void InputCAD()
        {
            binaryWriter.Write(CommandCtrlAltDelete);
            binaryWriter.Flush();

        }

        public static void PrintMsg(String msg)
        {
            Console.WriteLine("Msg : " + msg);
        }
        public static void PrintError(Exception msg)
        {
            Console.WriteLine("Error : " + msg);
        }
        public static void PINGING()
        {
            binaryWriter.Write(PING);
            binaryWriter.Flush();
        }
        
    }
    
    
    public class MyTcpClient
    {
        private static int Counter = 0;

        public int Id
        {
            get;
            private set;
        }

        public TcpClient TcpClient
        {
            get;
            private set;
        }

        public MyTcpClient(TcpClient tcpClient)
        {
            if (tcpClient == null)
            {
                throw new ArgumentNullException("tcpClient");
            }

            this.TcpClient = tcpClient;
            this.Id = ++MyTcpClient.Counter;
        }
    }
}
