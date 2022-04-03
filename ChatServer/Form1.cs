using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private delegate void UpdateStatusCallback(string message);

        bool connected = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                Application.Exit();
                return;
            }

            if (txtAddress.Text == "")
            {
                MessageBox.Show("Insert a ip address");
                txtAddress.Focus();
                return;
            }

            try
            {
                IPAddress iPAddress = IPAddress.Parse(txtAddress.Text);
                int port = (int)txtPort.Value;

                Server mainServer = new Server(iPAddress, port);

                Server.StatusChanged += new StatusChangedEventHandler(MainServerStatusChanged);

                mainServer.StartAttendance();

                listLogs.Items.Add("Server is up ! \n Waiting for connections...\n");
                listLogs.SetSelected(listLogs.Items.Count - 1, true);
            }
            catch (Exception ex)
            {
                listLogs.Items.Add("Erro while trying to connect: " + ex.Message);
                listLogs.SetSelected(listLogs.Items.Count - 1, true);
                return;
            }

            connected = true;
            txtAddress.Enabled = false;
            txtPort.Enabled = false;
            
            btnStartServer.ForeColor = Color.Red;
            btnStartServer.Text = "Exit";
        }

        public void MainServerStatusChanged(object sender, StatusChangedEventArgs e)
        {
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string message)
        {
            //Update log messages
            listLogs.Items.Add(message);
            listLogs.SetSelected(listLogs.Items.Count - 1, true);
        }

    }
}
