
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;

namespace RemoteControlV1
{
    class ClientConnections
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

        public static TcpClient ServerSocket;
        public static Task listeningTask;
        public static Task transmissionTask;
        public static Boolean sendMouseInput { get; set; }

        public static Boolean screenRemote { get; set; }
        public static Boolean isOnline { get; set; }

        public static Form parentForm;

        public static event EventHandler<ServerEventArgs> EventCursorUpdate;
        public static BinaryFormatter binaryFormatter;
        public ClientConnections(TcpClient server)
        {
            ServerSocket = server;
        }
        public void StartSendReceive()
        {
            isOnline = true;
            /*
            var receiveThread = new Thread(ClientReceived);
            var sendThread = new Thread(ClientSend);
            //new Thread(sendridle).Start(); 
            receiveThread.Start();
            sendThread.Start();
            /**/

            //listeningTask = new Task(() => ClientSend());
            //transmissionTask = new Task(() => ClientReceived());
            //var ridletask = new Task(() => sendridle()); 
            listeningTask = new Task(delegate { ClientSend(); });
            transmissionTask = new Task(delegate { ClientReceived(); });

            transmissionTask.Start();
            //ridletask.Start();
            listeningTask.Start();
            //transmissionTask.Wait();
            //ridletask.Wait();
            //listeningTask.Wait();
            /**/
        }
        public static void ClientReceived()
        {
            
            while (isOnline)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var read = new BinaryReader(netStream);
                    var write = new BinaryWriter(netStream);

                    string message = read.ReadString();
                    Console.WriteLine(message);
                    switch (message)
                    {
                        case CommandCursor:
                            UpdateCursorPosition(read.ReadInt32(), read.ReadInt32());
                            break;
                        case CommandMousePressed:
                            InputMouseClicked(read.ReadString());
                            break;
                        case CommandKeyPressed:
                            InputKeyPressed(read.ReadInt32());
                            break;
                        case CommandKeyDown:
                            InputKeyDown(read.ReadInt32());
                            break;
                        case CommandKeyUp:
                            InputKeyUp(read.ReadInt32());
                            break;
                        case CommandShutdown:
                            Process.Start("shutdown", "/s /t 0");
                            break;
                        case CommandMouseMove:
                            mouseMove(read.ReadBoolean());
                            break;
                        case CommandCtrlAltDelete:
                            InputCAD();
                            break;
                        case PING:
                            PrintMsg("Ping Masuk!");
                            break;
                        case HANDLER:
                            PrintMsg(read.ReadString());
                            break;
                        case RIDLE:
                            //write.Write(CIDLE);
                            //write.Flush();
                            //PrintMsg("RIDLE from Server... ");
                            break;
                        case GETWINRES:
                            SendWinRes();
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
        public static void sendridle()
        {
            while (isOnline)
            {
                try
                {
                    var netStream = ServerSocket.GetStream();
                    var write = new BinaryWriter(netStream, Encoding.UTF8);
                    write.Write(RIDLE);
                    write.Flush();
                    PrintMsg("send RIDLE...");
                }
                catch { }
            }


        }
        public void ClientSend()
        {
            int ww = Screen.PrimaryScreen.Bounds.Width;
            int hh = Screen.PrimaryScreen.Bounds.Height;
            int tmpw = 0;
            int tmph = 0;
            while (isOnline)
            {

                lock (this)
                {
                    try
                    {
                        var netStream = ServerSocket.GetStream();
                        var write = new BinaryWriter(netStream);


                        
                        write.Write(CommandImage);
                        write.Write(ww);
                        write.Write(hh);
                        write.Flush();

                        Bitmap screenshot = DesktopScreen.CaptureScreen(true);

                        DesktopScreen.SerializeScreen(netStream, screenshot);
                        netStream.Flush();

                        //`Console.WriteLine("Send image..");
                        /**/
                       
                        //Thread.Sleep(1);

                    }
                    catch (Exception e)
                    {
                        PrintError(e);

                    }

                }


            }

        }
        private static void mouseMove(bool mouseInput)
        {
            if (mouseInput == true)
            {
                EventCursorUpdate += NewCursorPosition;
                Console.WriteLine("Moving!");
            }
        }
        private static void NewCursorPosition(object source, ServerEventArgs args)
        {
            Cursor.Position = new Point(args.CursorPosition.X, args.CursorPosition.Y);
            Console.WriteLine("New cursor pos! " + args.CursorPosition.X + ", " + args.CursorPosition.Y);
        }

        private static void UpdateCursorPosition(int x, int y)
        {
            //var netStream = ServerSocket.GetStream();
            //var read = new BinaryReader(netStream);
            OnEventCursorUpdate(new ServerEventArgs() { CursorPosition = new Point(x, y) });
            Console.WriteLine("Update cursor pos! {0} {1}",x,y);
        }

        public static void OnEventCursorUpdate(ServerEventArgs args)
        {
            if (EventCursorUpdate != null)
            {
                EventCursorUpdate(null, args);
            }
        }

        public static void InputMouseClicked(string mouse)
        {
            switch (mouse)
            {
                case CommandLeftMouseDOWN:
                    Mouse.mouse_event(Mouse.MOUSEEVENTF_LEFTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;

                case CommandRightMouseDOWN:
                    Mouse.mouse_event(Mouse.MOUSEEVENTF_RIGHTDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case CommandMiddleMouseDOWN:
                    Mouse.mouse_event(Mouse.MOUSEEVENTF_MIDDLEDOWN, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case CommandLeftMouseUP:
                    Mouse.mouse_event(Mouse.MOUSEEVENTF_LEFTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case CommandRightMouseUP:
                    Mouse.mouse_event(Mouse.MOUSEEVENTF_RIGHTUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;
                case CommandMiddleMouseUP:
                    Mouse.mouse_event(Mouse.MOUSEEVENTF_MIDDLEUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                    break;

                default:
                    break;

            }
        }

        public static void InputKeyPressed(Int32 key)
        {
            VirtualKeyCode keyCode = (VirtualKeyCode)key;
            InputSimulator.SimulateKeyPress(keyCode);
        }
        public static void InputKeyDown(Int32 key)
        {
            VirtualKeyCode keyCode = (VirtualKeyCode)key;
            InputSimulator.SimulateKeyDown(keyCode);
        }
        public static void InputKeyUp(Int32 key)
        {
            VirtualKeyCode keyCode = (VirtualKeyCode)key;
            InputSimulator.SimulateKeyUp(keyCode);
        }
        public static void InputCAD()
        {
            InputSimulator.SimulateKeyDown(VirtualKeyCode.CONTROL);
            InputSimulator.SimulateKeyDown(VirtualKeyCode.MENU);
            InputSimulator.SimulateKeyDown(VirtualKeyCode.DELETE);
            InputSimulator.SimulateKeyUp(VirtualKeyCode.CONTROL);
            InputSimulator.SimulateKeyUp(VirtualKeyCode.MENU);
            InputSimulator.SimulateKeyUp(VirtualKeyCode.DELETE);

        }

        private static void SendWinRes()
        {
            var netStream = ServerSocket.GetStream();
            var write = new BinaryWriter(netStream);

            Console.WriteLine("Send Winres...");
            write.Write(WinRes);
            write.Write(Screen.PrimaryScreen.Bounds.Width);
            write.Write(Screen.PrimaryScreen.Bounds.Height);
            write.Flush();
            Console.WriteLine("Sent Winres : " + Screen.PrimaryScreen.Bounds.Width + ", " + Screen.PrimaryScreen.Bounds.Height);


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
                    PrintMsg("PINGING!!");

                }
                catch (Exception e)
                {
                    PrintError(e);
                }
            }
            

        }

        public static void PrintMsg(String msg)
        {
            Console.WriteLine("Client : " + msg);
        }
        public static void PrintError(Exception msg)
        {
            Console.WriteLine("Error : " + msg);
        }

    }

}