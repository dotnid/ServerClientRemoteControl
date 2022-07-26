using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

using WindowsInput;

namespace RemoteControlV1
{

    class ServerConnect
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
        private const string IDLE = "IDLE";
        private const string PING = "PING";
        private const string HANDLER = "HANDLER";
        private const string REMOTESET = "REMOTESET";
        private const string CLIENT = "CLIENT";
        private const string REMOTE = "REMOTE";


        private static TcpListener server;
        private static TcpClient client;
        
        public static Boolean isOnline = false;

        private static Boolean connectionTerminated = true;
        private static BinaryWriter binaryWriter;
        private static BinaryReader binaryReader;
        private static BinaryFormatter binaryFormatter;
        private static NetworkStream netStream;
        
        private static Task listeningTask;
        private static Task transmissionTask;
        private static String ClientName { get; set; }
        private static bool ConnectedToRemote = false;

        public static Form parentForm;
        

        public static event EventHandler<ServerEventArgs> EventCursorUpdate;
        public void ConnectIP(string ipAdress, int port, string clientname)
        {
            ClientName = clientname;
            SetupFields(new ServerErrorHandler("Error connecting to server"), ipAdress, port);
        }

        private void SetupFields(IErrorLogger log, string ipAdress, int port)
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
                PrintError(e);
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


            Disconnect(new ServerErrorHandler("Connection ending."));

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

        }

        private static void RecieveTransmission()
        {

            if (client != null)
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
                                InputMouseClicked(binaryReader.ReadString());
                                break;
                            case CommandKeyPressed:
                                InputKeyPressed(binaryReader.ReadInt32());
                                break;
                            case CommandKeyDown:
                                InputKeyDown(binaryReader.ReadInt32());
                                break;
                            case CommandKeyUp:
                                InputKeyUp(binaryReader.ReadInt32());
                                break;
                            case CommandShutdown:
                                Process.Start("shutdown", "/s /t 0");
                                break;
                            case CommandMouseMove:
                                mouseMove(binaryReader.ReadBoolean());
                                break;
                            case CommandCtrlAltDelete:
                                InputCAD();
                                break;
                            case PING:
                                PrintMsg("Ping Masuk!");
                                break;
                            case HANDLER:
                                PrintMsg(binaryReader.ReadString());
                                break;
                            case IDLE:
                                PrintMsg("IDLE OK!");
                                binaryWriter.Write("IDLE");
                                binaryWriter.Flush();
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

        private static void UpdateCursorPosition()
        {
            OnEventCursorUpdate(new ServerEventArgs() { CursorPosition = new Point(binaryReader.ReadInt32(), binaryReader.ReadInt32()) });
            Console.WriteLine("Update cursor pos!");
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

        private static void SendTransmission()
        {
            int ww = Screen.PrimaryScreen.Bounds.Width;
            int hh = Screen.PrimaryScreen.Bounds.Height;
            int tmpw = 0;
            int tmph = 0;
            while (isOnline)
            {
                try
                {
                    if (ClientName != null && ConnectedToRemote == false)
                    {
                        //Console.WriteLine("Get Remote support.");
                        binaryWriter.Write(CLIENT);
                        binaryWriter.Write(ClientName);
                        binaryWriter.Write(ClientName);
                        binaryWriter.Flush();

                    }

                    if (ConnectedToRemote)
                    {
                        if (tmpw != ww || tmph != hh)
                        {
                            SendWinRes();
                        }
                        tmpw = ww;
                        tmph = hh;

                        binaryWriter.Write(CommandImage);
                        binaryWriter.Flush();
                        Bitmap screenshot = DesktopScreen.CaptureScreen(true);

                        DesktopScreen.SerializeScreen(netStream, screenshot);

                        netStream.Flush();

                    }


                }
                catch (Exception e)
                {
                    PrintError(e);
                    isOnline = false;

                }

                //Thread.Sleep(10);
            }
        }
        private static void SendWinRes()
        {
            Console.WriteLine("Send Winres...");
            binaryWriter.Write(WinRes);
            binaryWriter.Write(Screen.PrimaryScreen.Bounds.Width);
            binaryWriter.Write(Screen.PrimaryScreen.Bounds.Height);
            binaryWriter.Flush();
            Console.WriteLine("Is Online : " + isOnline + ", Sent Winres : " + Screen.PrimaryScreen.Bounds.Width + ", " + Screen.PrimaryScreen.Bounds.Height);


        }
        public static void PINGING()
        {
            try
            {
                binaryWriter.Write(PING);
                binaryWriter.Flush();

            }
            catch (Exception e)
            {
                PrintError(e);
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
        public static void PrintMsg(String msg)
        {
            Console.WriteLine("Msg : " + msg);
        }
        public static void PrintError(Exception msg)
        {
            Console.WriteLine("Error : " + msg);
        }

    }
    
}