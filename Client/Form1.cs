using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        //Handle with user nickname
        private string userName;
        private StreamWriter swWriter;
        private StreamReader srReader;
        private TcpClient tcpClient;

        //Needed to update log messages
        private delegate void UpdateLogCallback(string message);

        //Needed to define form state "disconnected" from another thread
        private delegate void CloseConnectionCallback(string reason);
        
        private Thread messageThread;
        private IPAddress iPAddress;
        private int port;
        private bool connected = false;

        public Form1()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!connected)
            {
                initConnection();
            }
            else
            {
                CloseConnection("Desconnected by the user");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtChat_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }

        private void initConnection()
        {
            try
            {
                iPAddress = IPAddress.Parse(txtAddress.Text);
                port = (int)txtPort.Value;

                tcpClient = new TcpClient();
                tcpClient.Connect(iPAddress, port);

                connected = true;

                txtAddress.Enabled = false;
                txtPort.Enabled = false;
                txtNickname.Enabled = false;

                txtChat.Enabled = true;
                btnSend.Enabled = true;

                btnConnect.ForeColor = Color.Red;
                btnConnect.Text = "Disconnect";

                swWriter = new StreamWriter(tcpClient.GetStream());
                swWriter.WriteLine(txtNickname.Text);
                swWriter.Flush();

                messageThread = new Thread(new ThreadStart(receiveMessage));
                messageThread.IsBackground = true;
                messageThread.Start();

                labelStatus.Invoke(new Action(() =>
                {
                    labelStatus.ForeColor = Color.Green;
                    labelStatus.Text = $"Connected ! \n\n Host:{iPAddress} \n {port} \n";
                }));
            }
            catch (Exception ex)
            {
                labelStatus.Invoke(new Action(() =>
                {
                    labelStatus.ForeColor = Color.Red;
                    labelStatus.Text = "Error while trying to connect " + ex.Message;
                }));
            }
        }

        private void receiveMessage()
        {
            srReader = new StreamReader(tcpClient.GetStream());
            string conResponse = srReader.ReadLine();

            //0 == disconnected
            if (conResponse[0] == '1')
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected with success !" });
            }
            else
            {
                string reason = "Not connected: ";
                reason += conResponse.Substring(2, conResponse.Length - 2);
                this.Invoke(new CloseConnectionCallback(this.UpdateLog), new object[] { reason });
                return;
            }

            while (connected)
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReader.ReadLine() });
            }
        }

        private void UpdateLog(string message)
        {
            txtLog.AppendText(message + "\r\n");
        }

        private void SendMessage()
        {
            if (txtChat.Lines.Length >= 1)
            {
                swWriter.WriteLine(txtChat.Text);
                swWriter.Flush();

                txtChat.Lines = null;
            }

            txtChat.Text = "";
        }

        private void CloseConnection(string reason)
        {
            txtLog.AppendText(reason + "\r\n");

            txtAddress.Enabled = true;
            txtPort.Enabled = true;
            txtChat.Enabled = false;
            btnSend.Enabled = false;

            btnConnect.ForeColor = Color.Black;
            btnConnect.Text = "Connect";

            connected = false;

            swWriter.Close();
            srReader.Close();
            tcpClient.Close();

            labelStatus.Invoke(new Action(() =>
            {
                labelStatus.ForeColor = Color.Green;
                labelStatus.Text = $"Deconnecting from... \n\n Server: {iPAddress} \n Port: {port} \n";
            }));
        }

        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (connected)
            {
                connected = false;

                swWriter.Close();
                srReader.Close();
                tcpClient.Close();

                labelStatus.Invoke(new Action(() =>
                {
                    labelStatus.ForeColor = Color.Green;
                    labelStatus.Text = $"Deconnected from... \n\n Server: {iPAddress} \n Port: {port} \n";
                }));
            }
        }
    }
}
