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
using System.Net.Sockets;
using System.IO;

namespace RemoteControlV1
{
    public partial class SetupForm : Form
    {
        Socket socket;
        
        public SetupForm()
        {
            InitializeComponent();    
        }

        private void ConnectForm_Load(object sender, EventArgs e)
        {
            AllocConsole();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //ServerConnect client = new ServerConnect();
            //client.ConnectIP(tbIPAdress.Text, Int32.Parse(tbPort.Text), ClientName.Text);
            
            this.Hide();

            //new ViewerForm(this).ShowDialog();
            
            ClientForm.cipaddr = tbIPAdress.Text;
            ClientForm.cport = Int32.Parse(tbPort.Text);
            ClientForm.cname = ClientName.Text;

            new ClientForm(this).ShowDialog();
            

            //var name = GetChatName();
            //if (string.IsNullOrEmpty(name)) return;

            try
            {
                /*
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(cipaddr, cport);
                ClientConnections client = new ClientConnections(socket);
                client.Received += new ClientConnections.ClientReceivedHandler(client_received);
                
                var client = new TcpClient();
                client.Connect(tbIPAdress.Text, Int32.Parse(tbPort.Text));
                netStream = client.GetStream();
                write = new BinaryWriter(netStream);
                write.Write(name);
                var form = new Form { Text = "Chat - " + name };
                var tbwrite = new TextBox { Dock = DockStyle.Bottom, Parent = form };
                var tbChat = new TextBox { Dock = DockStyle.Fill, Parent = form, Multiline = true, ReadOnly = true };
                var messages = new List<string>();
                tbwrite.KeyPress += (_s, _e) =>
                {
                    if (_e.KeyChar == 13 && !string.IsNullOrWhiteSpace(tbwrite.Text))
                    {
                        write.Write(tbwrite.Text);
                        tbwrite.Text = string.Empty;
                        _e.Handled = true;
                    }
                };
                Action<string> onMessageReceived = message =>
                {
                    if (messages.Count == 100) messages.RemoveAt(0);
                    messages.Add(message);
                    tbChat.Lines = messages.ToArray();
                };
                var listener = new Thread(() =>
                {
                    var listen = new BinaryReader(netStream);
                    while (true)
                    {
                        var message = listen.ReadString();
                        form.BeginInvoke(onMessageReceived, message);
                    }
                });
                listener.IsBackground = true;
                listener.Start();
                form.ShowDialog();
                */

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void btnHost_Click(object sender, EventArgs e)
        {
            //if (ServerHost.netStream != null)
            //{
            this.Hide();
            new ServerHostForm(this).ShowDialog();
            //new ViewerForm(this).ShowDialog();

            //}
        }

        private void tbPort_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ServerHost.ConnectIP(tbIPAdress.Text, Int32.Parse(tbPort.Text)); 
            ViewerForm.cipaddr = tbIPAdress.Text;
            ViewerForm.cport = Int32.Parse(tbPort.Text);
            ViewerForm.cname = ClientName.Text;
            this.Hide();
            new ViewerForm(this).ShowDialog();
            
        }

        private void tbIPAdress_TextChanged(object sender, EventArgs e)
        {

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
    }
}
