using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    //Handle with connections
    class Connection
    {
        TcpClient tcpClient;

        //Thread that will sent information to the client
        private Thread thrSender;
        private StreamWriter swSender;
        private StreamReader srSender;
        private string currentUser;
        private string response;

        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            thrSender = new Thread(AcceptClient);
            thrSender.IsBackground = true;
            thrSender.Start();
        }

        private void CloseConnections()
        {
            tcpClient.Close();
            swSender.Close();
            srSender.Close();
        }

        private void AcceptClient()
        {
            srSender = new StreamReader(tcpClient.GetStream());
            swSender = new StreamWriter(tcpClient.GetStream());

            //Read info about the user
            currentUser = srSender.ReadLine();

            if (currentUser != "")
            {
                //Save user in the hash table
                if (Server.htUsers.Contains(currentUser))
                {
                    //0 == not connected
                    swSender.WriteLine("0 | User already exists");
                    swSender.Close();
                    CloseConnections();
                    return;
                }
                else if (currentUser == "Server")
                {
                    swSender.WriteLine("0 | User already exists");
                }
                else
                {
                    //1 == connected
                    swSender.WriteLine("1");
                    swSender.Flush();

                    Server.AddUser(tcpClient, currentUser);
                }

                try
                {
                    while ((response = srSender.ReadLine()) != "")
                    {
                        if (response == null)
                        {
                            Server.RemoveUser(tcpClient);
                        }
                        else
                        {
                            Server.SendMessageToAll(currentUser, response);
                        }
                    }
                }
                catch
                {
                    Server.RemoveUser(tcpClient);
                }
            }
            else
            {
                CloseConnections();
            }

        }
    }
}
