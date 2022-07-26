namespace RemoteControlV1
{
    partial class SetupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnHost = new System.Windows.Forms.Button();
            this.labelIPAdress = new System.Windows.Forms.Label();
            this.tbIPAdress = new System.Windows.Forms.TextBox();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.labelPort = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.ClientName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(9, 97);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(163, 24);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnHost
            // 
            this.btnHost.Location = new System.Drawing.Point(9, 156);
            this.btnHost.Name = "btnHost";
            this.btnHost.Size = new System.Drawing.Size(162, 23);
            this.btnHost.TabIndex = 1;
            this.btnHost.Text = "Host";
            this.btnHost.UseVisualStyleBackColor = true;
            this.btnHost.Click += new System.EventHandler(this.btnHost_Click);
            // 
            // labelIPAdress
            // 
            this.labelIPAdress.AutoSize = true;
            this.labelIPAdress.Location = new System.Drawing.Point(10, 9);
            this.labelIPAdress.Name = "labelIPAdress";
            this.labelIPAdress.Size = new System.Drawing.Size(52, 13);
            this.labelIPAdress.TabIndex = 2;
            this.labelIPAdress.Text = "IP Adress";
            // 
            // tbIPAdress
            // 
            this.tbIPAdress.Location = new System.Drawing.Point(72, 6);
            this.tbIPAdress.Name = "tbIPAdress";
            this.tbIPAdress.Size = new System.Drawing.Size(100, 20);
            this.tbIPAdress.TabIndex = 3;
            this.tbIPAdress.Text = "192.168.100.72";
            this.tbIPAdress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbIPAdress.TextChanged += new System.EventHandler(this.tbIPAdress_TextChanged);
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(72, 32);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(100, 20);
            this.tbPort.TabIndex = 4;
            this.tbPort.Text = "2212";
            this.tbPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbPort.TextChanged += new System.EventHandler(this.tbPort_TextChanged);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(12, 35);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(26, 13);
            this.labelPort.TabIndex = 5;
            this.labelPort.Text = "Port";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 127);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(162, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Remote";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ClientName
            // 
            this.ClientName.Location = new System.Drawing.Point(71, 58);
            this.ClientName.Name = "ClientName";
            this.ClientName.Size = new System.Drawing.Size(100, 20);
            this.ClientName.TabIndex = 7;
            this.ClientName.Text = "Client";
            this.ClientName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Name";
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(192, 191);
            this.Controls.Add(this.ClientName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.tbPort);
            this.Controls.Add(this.tbIPAdress);
            this.Controls.Add(this.labelIPAdress);
            this.Controls.Add(this.btnHost);
            this.Controls.Add(this.btnConnect);
            this.Name = "SetupForm";
            this.Text = "myRemote";
            this.Load += new System.EventHandler(this.ConnectForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnHost;
        private System.Windows.Forms.Label labelIPAdress;
        private System.Windows.Forms.TextBox tbIPAdress;
        private System.Windows.Forms.TextBox tbPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox ClientName;
        private System.Windows.Forms.Label label1;
    }
}

