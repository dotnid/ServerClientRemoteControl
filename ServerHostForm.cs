using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RemoteControlV1
{
    public partial class ServerHostForm : Form
    {
        private const string CommandLeftMouseUP = "LFU";
        private const string CommandLeftMouseDOWN = "LFD";
        private const string CommandRightMouseUP = "RFU";
        private const string CommandRightMouseDOWN = "RFD";
        private const string CommandMiddleMouseUP = "MFU";
        private const string CommandMiddleMouseDOWN = "MFD";
        private static List<Socket> sockets;
        //private static ServerHost.SocketAccepted server;
        Form parentForm;
        public ServerHostForm()
        {
            InitializeComponent();
        }
        public ServerHostForm(Form parentForm)
        {
            this.parentForm = parentForm;
            
            InitializeComponent();
        }

        private void ServerHostForm_Load(object sender, EventArgs e)
        { 
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2, 0);
            //ServerHost.Start();
            
            //Load += new EventHandler(mainload);
                
            //new Task(() => ServerHost.Listen()).Start();
            
            try
            {
                Server server = new Server();
                //new Thread(() => server.Start()).Start();
                server.Start();
                //new Task(() => server.Start()).Start();
                /*
                server = new Listener(2212);
                server.Start();
                server.SocketAccepted += new Listener.SocketAcceptedHandler(server.listener_SocketAccepted);
                */
                //sockets = new List<Socket>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
           
            this.Capture = true;

        }
        
        
        public void ServerHostForm_UpdatePicture(object source, ServerEventArgs args)
        {
            pictureBox.Image = args.Image;
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            //ServerHost.Stop(new ServerErrorHandler("Server termination unsuccessful"));
            CeaseConnection("Client transmission terminate");
            this.Close();
        }


        private void ServerHostForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //CeaseConnection("Client transmission terminate");
            parentForm.Show();
        }

        public void UpdateButtonText(string text)
        {
            btnDisconnect.Text = text;
        }

        private void CeaseConnection(string message)
        {
           
        }

        private void ServerHostForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                Console.WriteLine("Try Sending ...");
            }
            catch (Exception ex)
            {
               
            }
            
        }

        private void Server_senderacc(Socket e)
        {
            throw new NotImplementedException();
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            //textBox1.Text = "Mouse Up";
            /*
            if (ServerHost.isOnline)
            {
                ServerHost.binaryWriter.Write(ServerHost.CommandMousePressed);
                if (e.Button == MouseButtons.Left)
                {
                    ServerHost.binaryWriter.Write(CommandLeftMouseUP);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    ServerHost.binaryWriter.Write(CommandRightMouseUP);
                }
                else
                {
                    ServerHost.binaryWriter.Write(CommandMiddleMouseUP);
                }
                ServerHost.binaryWriter.Flush();
            }
            */

        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            /*
            if (ServerHost.isOnline)
            {
                ServerHost.binaryWriter.Write(ServerHost.CommandMousePressed);
                if (e.Button == MouseButtons.Left)
                {
                    ServerHost.binaryWriter.Write(CommandLeftMouseDOWN);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    ServerHost.binaryWriter.Write(CommandRightMouseDOWN);
                }
                else
                {
                    ServerHost.binaryWriter.Write(CommandMiddleMouseDOWN);
                }
                ServerHost.binaryWriter.Flush();
            }
            */
        }

        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            //ServerHost.sendMouseInput = false;
            //Console.WriteLine("Mouse Leave");
        }
        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            //ServerHost.sendMouseInput = true;
            //Console.WriteLine("Mouse Enter");
        }
        private void pictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /*
            var pbwidth = pictureBox.Width;
            var pbheight = pictureBox.Height;
            var cwidth = ServerHost.clientwidth;
            var cheight = ServerHost.clientheight;
            var LocalMousePosition = pictureBox.PointToClient(Cursor.Position);
            int mouseX = e.X;
            int mouseY = e.Y;
            int xx = mouseX * cwidth / pbwidth;
            int yy = mouseY * cheight / pbheight;
            ServerHost.xmove = xx;
            ServerHost.ymove = yy;
            */
            //Console.WriteLine("Mouse :" + xx + " " + yy);


            //mousePath.AddLine(mouseX, mouseY, mouseX, mouseY);
            //textBox1.Text = " X=" + mouseX + "," + "Y= " + mouseY;
            //textBox1.Text = "Mouse Move";
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            new ViewerForm(this).ShowDialog();
        }
    }
}
