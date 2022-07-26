using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

using WindowsInput;
using System.Net.Sockets;
using System.IO;

namespace RemoteControlV1
{
    public partial class ViewerForm : Form
    {
        
        private const string CommandLeftMouseUP = "LFU";
        private const string CommandLeftMouseDOWN = "LFD";
        private const string CommandRightMouseUP = "RFU";
        private const string CommandRightMouseDOWN = "RFD";
        private const string CommandMiddleMouseUP = "MFU";
        private const string CommandMiddleMouseDOWN = "MFD";
        public static string cipaddr { get; set; }
        public static int cport { get; set; }
        public static string cname { get; set; }
        public static TcpClient client { get; set; }
        RemoteConnections ser;
        Form parentForm;
        public ViewerForm()
        {
            InitializeComponent();
        }
        public ViewerForm(Form parentForm)
        {

            InitializeComponent();
            this.parentForm = parentForm;
            
            
        }

        private void ViewerForm_Load(object writeer, EventArgs e)
        {
            
            try
            {
                var name = cname;
                var addr = "123456";
                var role = "remote";
                if (string.IsNullOrEmpty(name)) return;

                var client = new TcpClient();
                client.NoDelay = true;
                //client.ExclusiveAddressUse = false;
                client.Connect(cipaddr, cport);
                var netStream = client.GetStream();
                var write = new BinaryWriter(netStream);
                write.Write(name + "Remote");
                write.Write(addr);
                write.Write(role);
                ser = new RemoteConnections(client);
                RemoteConnections.isOnline = true;
                //new Task(() => ser.StartSendReceive()).Start();
                ser.StartSendReceive();
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                RemoteConnections.EventImageRecieved += ViewerForm_UpdatePicture;
                RemoteConnections.parentForm = this;
                this.Capture = true;
                RemoteConnections.screenRemote = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //AllocConsole();
            //ServerHost.Start();
            //new Task(() => ServerHost.Listen()).Start();
 
            //int imgWidth = pictureBox.Image.Width;
            //int imgHeight = pictureBox.Image.Height;

        }
        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();
        private void ViewerForm_FormClosed(object sender, FormClosedEventArgs e )
        {
            //CeaseConnection();
            parentForm.Show();
            RemoteConnections.screenRemote=false;
        }

        public void ViewerForm_UpdatePicture(object source, ServerEventArgs args)
        {
            //pictureBox.Dock = DockStyle.Fill;
            //pictureBox.Paint += pic;
            pictureBox.Image = args.Image;
            //pictureBox.Refresh();

        }

        private void ViewerForm_Shown(object sender, EventArgs e)
        {
        }
        
        private void ViewerForm_KeyDown(object sender, KeyEventArgs e)
        {

            if (RemoteConnections.isOnline)
            {
                var netStream = RemoteConnections.ServerSocket.GetStream();
                var read = new BinaryReader(netStream);
                var write = new BinaryWriter(netStream);

                if (e.KeyCode == Keys.End)
                {
                    write.Write(RemoteConnections.CommandShutdown);
                    write.Flush();
                    Console.WriteLine("Input {0}",RemoteConnections.CommandShutdown);
                }
                else if(e.KeyCode == Keys.Pause)
                {
                    if (ser.sendMouseInput == true) ser.sendMouseInput = false;
                    else ser.sendMouseInput = true;
                    
                }
                else
                {
                    Console.WriteLine("Input {0}", RemoteConnections.CommandKeyDown);
                    new Task(delegate { ser.InputKeyDown((Int32)e.KeyCode); }).Start();
                    //ser.InputKeyDown((Int32)e.KeyCode);
                }
                

            }
        }
        private void ViewerForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (RemoteConnections.isOnline)
            {
                var netStream = RemoteConnections.ServerSocket.GetStream();
                var read = new BinaryReader(netStream);
                var write = new BinaryWriter(netStream);

                if (e.KeyCode == Keys.End)
                {
                    write.Write(RemoteConnections.CommandShutdown);
                    write.Flush();
                }
                else if (e.KeyCode == Keys.Pause)
                {
                    if (ser.sendMouseInput == true) ser.sendMouseInput = false;
                    else ser.sendMouseInput = true;

                }
                else
                {
                    new Task(delegate { ser.InputKeyUp((Int32)e.KeyCode); }).Start();
                    //ser.InputKeyUp((Int32)e.KeyCode);
                }
                

            }
        }
        
        private void CeaseConnection()
        {

            RemoteConnections.isOnline = false;
            //RemoteConnections.Stop(new ServerErrorHandler("Connection Closed!"));

        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            
            if (RemoteConnections.isOnline)
            {
                //var netStream = ser.ServerSocket.GetStream();
                //var read = new BinaryReader(netStream);
                //var write = new BinaryWriter(netStream);

                //write.Write(RemoteConnections.CommandMousePressed);
                if (e.Button == MouseButtons.Left)
                {
                    new Task(delegate { ser.InputMouseClicked(CommandLeftMouseUP); }).Start();
                    
                    //ser.InputMouseClicked(CommandLeftMouseUP);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    new Task(delegate { ser.InputMouseClicked(CommandRightMouseUP); }).Start();
                    
                    //ser.InputMouseClicked(CommandRightMouseUP);
                }
                else
                {
                    new Task(delegate { ser.InputMouseClicked(CommandMiddleMouseUP); }).Start();
                    
                    //ser.InputMouseClicked(CommandMiddleMouseUP);
                }
                //write.Flush();
            }

        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (RemoteConnections.isOnline)
            {
                var netStream = RemoteConnections.ServerSocket.GetStream();
                var read = new BinaryReader(netStream);
                var write = new BinaryWriter(netStream);

                //write.Write(RemoteConnections.CommandMousePressed);
                if (e.Button == MouseButtons.Left)
                {
                    new Task(delegate { ser.InputMouseClicked(CommandLeftMouseDOWN); }).Start();
                    //ser.InputMouseClicked(CommandLeftMouseDOWN);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    new Task(delegate { ser.InputMouseClicked(CommandRightMouseDOWN); }).Start();
                    
                    //ser.InputMouseClicked(CommandRightMouseDOWN);
                }
                else
                {
                    //new Thread(new ThreadStart(() => ser.InputMouseClicked(CommandMiddleMouseDOWN))).Start();
                    new Task(delegate { ser.InputMouseClicked(CommandMiddleMouseDOWN); }).Start();

                    //ser.InputMouseClicked(CommandMiddleMouseDOWN);
                }
                //write.Flush();
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            ser.pbwidth = pictureBox.Width;
            ser.pbheight = pictureBox.Height;
            int tmpx = 1;
            int tmpy = 1;
            var pbwidth = pictureBox.Width;
            var pbheight = pictureBox.Height;
            var cwidth = ser.clientwidth;
            var cheight = ser.clientheight;
            var LocalMousePosition = pictureBox.PointToClient(Cursor.Position);
            int mouseX = e.X;
            int mouseY = e.Y;
            int xx = mouseX * cwidth / pbwidth;
            int yy = mouseY * cheight / pbheight;
            ser.xmove = mouseX;
            ser.ymove = mouseY;
            //ser.updateCursor();
            new Task(delegate { ser.updateCursor(); }).Start();
            //new Task(() => ser.updateCursor()).Start();
            //ser.sendMouseInput = true;
            //var updatecursor = new Task(() => ser.updateCursor(xx, yy));
            //updatecursor.Start();
            //updatecursor.Wait();
            //ser.updateCursor(xx, yy);

        }
        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            ser.sendMouseInput = true;
        }
        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            ser.sendMouseInput = false;
        }

        private void ctrlAltDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RemoteConnections.isOnline)
            {
                ser.SendCAD();
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CeaseConnection();
            this.Close();
        }
        static string GetChatName()
        {
            var form = new Form { Text = "Enter name:", StartPosition = FormStartPosition.CenterScreen };
            var tb = new TextBox { Parent = form, Top = 8, Left = 8, Width = form.ClientSize.Width - 16, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            var okButton = new Button { Parent = form, Text = "OK", DialogResult = DialogResult.OK, Left = 8 };
            var cancelButon = new Button { Parent = form, Text = "Cancel", Left = okButton.Right + 8 };
            okButton.Top = cancelButon.Top = form.ClientSize.Height - okButton.Height - 8;
            okButton.Anchor = cancelButon.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            form.AcceptButton = okButton;
            form.CancelButton = cancelButon;
            var dr = form.ShowDialog();
            return dr == DialogResult.OK ? tb.Text : null;
        }


        private void pingToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            //var pinging = new Task(() => ser.PINGING());
            //pinging.Start();
            //pinging.Wait();
            new Task(delegate { ser.PINGING(); }).Start();
        }
    }
}
