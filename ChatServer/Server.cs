using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    //Needed to specify the parameters that we are sending with our event
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
    class Server
    {
        //This hash keep values from users
        public static Hashtable htUsers = new Hashtable(30);
        //This hash keep values from connections
        public static Hashtable htConnections = new Hashtable(30);

        private IPAddress ipAddress;
        private int serverPort;

        private TcpClient tcpClient;

        //Handle with all status change
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs e;

        public Server(IPAddress address, int port)
        {
            ipAddress = address;
            serverPort = port;
        }

        //Handle with connections
        private Thread thredListener;

        //Listen to connections
        private TcpListener tlsClient;

        bool ServerRunning = false;

        //Add user to the hash table
        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            Server.htUsers.Add(strUsername, tcpUser);
            Server.htConnections.Add(tcpUser, strUsername);

            //Emit a message to the server -- All users connected can hear it
            SendGlobalMessage(htConnections[tcpUser] + " connected to the server !");
        }

        //Remove user from hash table
        public static void RemoveUser(TcpClient tcpUser)
        {
            if (htConnections[tcpUser] != null)
            {
                SendGlobalMessage(htConnections[tcpUser] + " has left the room.");
                Server.htUsers.Remove(Server.htConnections[tcpUser]);
                Server.htConnections.Remove(Server.htConnections[tcpUser]);
            }
        }

        //This event is called when we want to emit a StatusChanged Event
        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChangedEventHandler statusHandler = StatusChanged;

            if (statusHandler != null)
            {
                statusHandler(null, e);
            }
        }

        //Send messages from server to everyone connected in the room
        public static void SendGlobalMessage(string message)
        {
            StreamWriter StreamWriterSender;
            e = new StatusChangedEventArgs("Server: " + message);
            OnStatusChanged(e);

            //TCP Client array with the length equals to the number of users connected
            TcpClient[] tcpClients = new TcpClient[Server.htUsers.Count];
            Server.htUsers.Values.CopyTo(tcpClients, 0);

            for (int i = 0; i < tcpClients.Length; i++)
            {
                //Try to send the message to all clients
                try
                {
                    if (message.Trim() == "" || tcpClients[i] != null)
                    {
                        continue;
                    }

                    StreamWriterSender = new StreamWriter(tcpClients[i].GetStream());
                    StreamWriterSender.WriteLine("Server: " + message);
                    StreamWriterSender.Flush();
                    StreamWriterSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public static void SendMessageToAll(string origin, string message)
        {
            StreamWriter StreamWriterSender;

            e = new StatusChangedEventArgs(origin + " says: " + message);
            OnStatusChanged(e);

            TcpClient[] tcpClients = new TcpClient[Server.htUsers.Count];
            Server.htUsers.Values.CopyTo(tcpClients, 0);

            for (int i = 0; i < tcpClients.Length; i++)
            {
                //Try to send the message to all clients
                try
                {
                    if (message.Trim() == "" || tcpClients[i] != null)
                    {
                        continue;
                    }

                    StreamWriterSender = new StreamWriter(tcpClients[i].GetStream());
                    StreamWriterSender.WriteLine("Server: " + message);
                    StreamWriterSender.Flush();
                    StreamWriterSender = null;
                }
                catch
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public void StartAttendance()
        {
            try
            {
                IPAddress localIP = ipAddress;
                int localPort = serverPort;

                //Create a object tcp listener
                tlsClient = new TcpListener(localIP, localPort);

                //Start to listen to connections
                tlsClient.Start();

                ServerRunning = true;

                //Start a new thread
                thredListener = new Thread(KeepAlive);
                thredListener.IsBackground = true;
                thredListener.Start();
            }
            catch (Exception ex)
            {

            }
        }

        private void KeepAlive()
        {
            while (ServerRunning)
            {
                //Accept a new connection
                tcpClient = tlsClient.AcceptTcpClient();

                //Create a new instance of a connection
                Connection newConnection = new Connection(tcpClient);
            }
        }
    }
}
