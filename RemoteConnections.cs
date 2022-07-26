using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;

namespace RemoteControlV1
{
    internal class RemoteConnections
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
        private const string RIDLE = "RIDLE";
        private const string CIDLE = "CIDLE";
        private const string PING = "PING";
        private const string HANDLER = "HANDLER";
        private const string TEXT = "TEXT";
        private const string GETWINRES = "GETWINRES";
        public Boolean sendMouseInput { get; set; }

        public static Boolean screenRemote { get; set; }
        public static Boolean isOnline { get; set; }

        public int xmove { get; set; }
        public int ymove { get; set; }
        public int pbwidth { get; set; }
        public int pbheight { get; set; }
        public int clientwidth { get; set; }
        public int clientheight { get; set; }
        public static bool listening { get; set; }
        public static int newPort { get; set; }
        public static string clientname { get; set; }

        public static event EventHandler<ServerEventArgs> EventImageRecieved;
        public static BinaryFormatter binaryFormatter;
        public static TcpClient ServerSocket;

        public static Task listeningTask;
        public static Task transmissionTask;
        public static Task sendridletask;

        public static Form parentForm;
        public RemoteConnections()
        {

        }
        public RemoteConnections(TcpClient server)
        {
            ServerSocket = server;
            var netStream = ServerSocket.GetStream();
            var write = new BinaryWriter(netStream);
            var read = new BinaryReader(netStream);
            

        }
        public void StartSendReceive()
        {

            isOnline = true;
            /*
            var sendThread = new Thread(SendTransmission);
            var receiveThread = new Thread(RecieveTransmission);
            new Thread(sendridle).Start();
            sendThread.Start();
            receiveThread.Start();
            /**/

            //listeningTask = new Task(() => RecieveTransmission());
            //transmissionTask = new Task(() => SendTransmission());
            //sendridletask = new Task(() => sendridle());
            listeningTask = new Task(delegate { RecieveTransmission(); });
            transmissionTask = new Task(delegate { SendTransmission(); });
            //sendridletask = new Task((delegate { sendridle(); });

            //sendridletask.Start();
            listeningTask.Start();
            transmissionTask.Start();
            //sendridletask.Wait();
            //listeningTask.Wait();
            //transmissionTask.Wait();
           /**/
        }
        public void RecieveTransmission()
        {
            while (isOnline)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    var read = new BinaryReader(netStream);

                    string message = read.ReadString();

                    switch (message)
                    {

                        case CommandImage:
                            clientwidth = read.ReadInt32();
                            clientheight = read.ReadInt32();
                            //Console.WriteLine("Received Winres {0} {1}",clientwidth,clientheight);
                            RecieveImage();
                            break;
                        case WinRes:
                            clientwidth = read.ReadInt32();
                            clientheight = read.ReadInt32();
                            break;
                        case PING:
                            PrintMsg("Client Ping Received !");
                            break;
                        case RIDLE:
                            //new Thread(() => sendridle()).Start();
                            PrintMsg("RIDLE from client..");
                            break;
                        default:

                            break;

                    }

                }
                catch (Exception ex)
                {
                    PrintError(ex);
                }
            }
        }
        public void sendridle()
        {
            while (isOnline)
            {
                try
                {
                    PrintMsg("send Mouse Input : " + sendMouseInput);
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream, Encoding.UTF8);
                    var mouseinput = sendMouseInput;
                    if (mouseinput)
                    {
                        write.Write(CommandMouseMove);
                        write.Write(true);
                        write.Flush();

                    }
                    else
                    {
                        write.Write(CommandMouseMove);
                        write.Write(false);
                        write.Flush();

                    }

                    Thread.Sleep(1);
                    
                }
                catch { }
            }
            

        }
        public void SendTransmission()
        {
            int tempx = 1;
            int tempy = 1;
            while (isOnline)
            {

                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream, Encoding.UTF8);

                    
                    if (sendMouseInput == true)
                    {
                        write.Write(CommandMouseMove);
                        write.Write(true);
                        write.Flush();
                        Console.WriteLine("Mouse Input : {0}", sendMouseInput);
                        /*
                        if (clientheight>0 && clientwidth>0)
                        {
                            int X = xmove * clientwidth / pbwidth;
                            int Y = ymove * clientheight / pbheight;
                            if(X!=tempx && Y != tempy)
                            {
                                write.Write(CommandCursor);
                                write.Write(X);
                                write.Write(Y);
                                write.Flush();
                                Console.WriteLine("Move :" + X + " " + Y);
                            }

                            tempx = X;
                            tempy = Y;
                        }
                        /**/
                    }
                    else
                    {
                        write.Write(CommandMouseMove);
                        write.Write(false);
                        write.Flush();
                        Console.WriteLine("Mouse Input : {0}", false);

                    }
                    Thread.Sleep(1);
                    /**/
                }

                catch (Exception ex)
                {
                    PrintError(ex);
                }
            }
        }
        public void updateCursor()
        {
            int tempx = 1;
            int tempy = 1;
            try
            {
                var netStream = ServerSocket.GetStream();
                var write = new BinaryWriter(netStream, Encoding.UTF8);
                int X = xmove * clientwidth / pbwidth;
                int Y = ymove * clientheight / pbheight;

//                if (sendMouseInput == true)
  //              {
    //                if (clientheight > 0 && clientwidth > 0)
      //              {
        //                if (X != tempx && Y != tempy)
          //              {
                            write.Write(CommandCursor);
                            write.Write(X);
                            write.Write(Y);
                            write.Flush();
                            Console.WriteLine("Move :" + X + " " + Y);
                //            }
                //Thread.Sleep(30);
                tempx = X;
                        tempy = Y;
              //      }
                //}
            }

            catch (Exception ex)
            {
                PrintError(ex);
            }
            
        }


        private static void RecieveImage()
        {
            var netStream = ServerSocket.GetStream();
            var write = new BinaryWriter(netStream);
            var read = new BinaryReader(netStream);
            /*
            // read how big the image buffer is
            int ctBytes = read.ReadInt32();

            // read the image buffer into a MemoryStream
            MemoryStream ms = new MemoryStream(read.ReadBytes(ctBytes));

            // get the image from the MemoryStream
            Image img = Image.FromStream(ms);
            //Image screenshot = (Image)DesktopScreen.DeserializeScreen(netStream);
            */
            Image img = (Image)DesktopScreen.DeserializeScreen(netStream);
            
            OnEventImageRecieved(new ServerEventArgs() { Image = img });
            //Console.WriteLine("Received Image..");

        }



        public static void OnEventImageRecieved(ServerEventArgs args)
        {
            if (EventImageRecieved != null)
            {
                EventImageRecieved(null, args);
            }
        }

        public void SendCAD()
        {
            var netStream = ServerSocket.GetStream();
            var write = new BinaryWriter(netStream);
            var read = new BinaryReader(netStream);
            
            write.Write(CommandCtrlAltDelete);
            write.Flush();

        }
        public static void PrintMsg(String msg)
        {
            Console.WriteLine("Remote : " + msg);
        }
        public static void PrintError(Exception msg)
        {
            Console.WriteLine("Error : " + msg);
        }
        public void PINGING()
        {
            lock (this)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream);

                    write.Write(PING);
                    write.Flush();
                    PrintMsg("Send PING !");

                }
                catch (Exception e)
                {
                    PrintError(e);
                }
            }
        }
        
        public void InputMouseClicked(string mouse)
        {
            lock (this)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    write.Write(CommandMousePressed);
                    write.Write(mouse);
                    write.Flush();
                    Console.WriteLine("Clicked {0} ", mouse);
                }
                catch (Exception ex)
                {

                }
            }
            
        }

        public void InputKeyPressed(int input)
        {
            lock (this)
            {
                try
                {

                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    write.Write(CommandKeyPressed);
                    write.Write(input);
                    write.Flush();
                    Console.WriteLine("Input {0} ", input);
                }
                catch (Exception ex)
                {

                }
            }

            
        }
        public void InputKeyDown(int input)
        {
            lock (this)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    write.Write(CommandKeyDown);
                    write.Write(input);
                    write.Flush();
                    Console.WriteLine("Input {0} ", input);
                }
                catch (Exception ex)
                {

                }
            }
            
        }
        public void InputKeyUp(int input)
        {
            lock (this)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream);
                    write.Write(CommandKeyUp);
                    write.Write(input);
                    write.Flush();
                }
                catch (Exception ex)
                {
                }
            }

            
        }
    }
}
