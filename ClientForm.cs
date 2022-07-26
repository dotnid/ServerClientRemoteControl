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
using System.IO;

namespace RemoteControlV1
{
    public partial class ClientForm : Form
    {
        public static string cipaddr { get; set; }
        public static int cport { get; set; }
        public static string cname { get; set; }
        private const string TEXT = "TEXT";
        ClientConnections ser;

        Form parentForm;
        public ClientForm(Form parentForm)
        {            
            InitializeComponent();
            this.parentForm = parentForm;

        }

        private void HostForm_Load(object sender, EventArgs e)
        { 
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2, 0);
            ClientConnections.parentForm = this;
            var name = cname;
            var addr = "123456";
            var role = "client";
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                var client = new TcpClient();
                client.NoDelay = true;
                //client.ExclusiveAddressUse = false;
                client.Connect(cipaddr, cport);
                var netStream = client.GetStream();
                var write = new BinaryWriter(netStream);
                write.Write(name + "Client");
                write.Write(addr);
                write.Write(role);
                ser = new ClientConnections(client);
                ClientConnections.isOnline = true;
                // new Task (()=>ser.StartSendReceive()).Start();
                ser.StartSendReceive();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            
            //CeaseConnection("Client transmission terminate");
            
            this.Close();
        }


        private void HostForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //ServerConnect.Disconnect(new ServerErrorHandler("Server termination successful"));
            //CeaseConnection("Client transmission terminate");
            parentForm.Show();
        }

        public void UpdateButtonText(string text)
        {
            btnDisconnect.Text = text;
        }

        private void CeaseConnection(string message)
        {
            ClientConnections.isOnline = false;
            //ClientConnections.Stop(new ServerErrorHandler(message));
        }

        private void HostForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
               var pinging = new Task(() => ser.PINGING());
                pinging.Start();
                pinging.Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            
        }
    }
}
