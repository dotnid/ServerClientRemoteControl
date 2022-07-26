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


namespace RemoteControlV1
{


    class ServerRemote
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
        private const string WinRes = "WINRES";
        private const string CommandMouseMove = "MOUSEMOVE";
        private const string CLIENT = "CLIENT";
        private const string REMOTE = "REMOTE";

        public static int xmove { get; set; }
        public static int ymove { get; set; }
        public static int pbwidth { get; set; }
        public static int pbheight { get; set; }
        public static int clientwidth { get; set; }
        public static int clientheight { get; set; }

        public static event EventHandler<ServerEventArgs> EventImageRecieved;

        private static IPAddress serverIP;
        private static int serverPort;
        private static TcpListener server1;
        private static Boolean connectionTerminated = true;
        private static TcpClient client;

        public static TcpClient server;
        public static NetworkStream netStream;
        public static BinaryReader binaryReader;
        public static BinaryWriter binaryWriter;
        public static Boolean isOnline = false;
        public static Boolean sendMouseInput = true;
        private static BinaryFormatter binaryFormatter;

        public static Form parentForm;

        public static Task listeningTask;
        public static Task transmissionTask;

        public static void Connect(string ipAdress, int port)
        {
            SetupFields(new ServerErrorHandler("Error connecting to server"), ipAdress, port);
   
        }

        private static void SetupFields(IErrorLogger log, string ipAdress, int port)
        {
            if (server != null) server.Close();
            server = new TcpClient();
            try
            {
                server.Connect(ipAdress, port);
                netStream = server.GetStream();

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
                    isOnline = false;
                }
            }

        }
        
         public static void SendTransmission()
        {
            if (sendMouseInput == true)
            {
                Point startingPoint = new Point(0, 0);
                Point endingPoint = new Point(0, 0);
                Point deltaPoint = new Point(0, 0);
                int tmpx = 1;
                int tmpy = 1;
                while (isOnline)
                {
                    if (sendMouseInput == true)
                    {

                        try
                        {
                            int X = xmove;
                            int Y = ymove;
                            
                            deltaPoint.X = xmove + xmove * clientwidth / pbwidth;
                            deltaPoint.Y = ymove + ymove * clientheight / pbheight;

                            if (tmpx!=X || tmpy != Y)
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

                            tmpx=X;
                            tmpy=Y;
                            Thread.Sleep(30);
                            
                        }
                        catch (Exception ex)
                        {
                            isOnline = false;
                        }
                        
                    }
                    else
                    {
                        // Do nothing.
                    }


                }
            }
            

        }
        
        public static void Disconnect(IErrorLogger log)
        {
            isOnline = false;
            try
            {
                binaryReader.Close();
                binaryWriter.Close();
                netStream.Close();
                server.Close();
                if (parentForm != null)
                {
                    parentForm.Invoke((MethodInvoker)delegate () { parentForm.Close(); });
                }

            }
            catch(Exception ex)
            {
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
            if(EventImageRecieved != null)
            {
                EventImageRecieved(null, args);
            }
        }

        public static void SendCAD()
        {
            binaryWriter.Write(CommandCtrlAltDelete);
            binaryWriter.Flush();
            
        }
    }
}
