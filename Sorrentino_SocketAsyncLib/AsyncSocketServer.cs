using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sorrentino_SocketAsyncLib
{
    public class AsyncSocketServer
    {
        IPAddress mIP;
        int mPort;
        TcpListener mServer;
        bool keep;

        List<TcpClient> mClients;
        public AsyncSocketServer()
        {
            mClients = new List<TcpClient>();
        }

        // Metodo che mette in ascolto il server
        public async void StartListening(IPAddress ipaddr = null, int port = 23000)
        {
            // Controlli sull'ip address e sulla porta
            if (ipaddr == null)
            {
                ipaddr = IPAddress.Any;
            }

            if (port < 0 || port > 65535)
            {
                port = 23000;
            }

            mIP = ipaddr;
            mPort = port;

            Debug.WriteLine($"Avvio il server, IP: {mIP.ToString()} - Porta: {mPort.ToString()}");

            // Creo l'oggetto server
            mServer = new TcpListener(mIP, mPort);

            // Avvio del server
            mServer.Start();
            keep = true;
            while (keep)
            {
                // Mi metto in ascolto di connessioni in entrata
                TcpClient client = await mServer.AcceptTcpClientAsync();
                mClients.Add(client);

                Debug.WriteLine($"Client connessi: {mClients.Count}. Client connesso: {client.Client.RemoteEndPoint}.");

                ReceiveMessage(client);
            }
        }

        public async void ReceiveMessage(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[512];

                // Ricezione effettiva
                while (keep)
                {
                    Debug.WriteLine("Pronto ad ascoltare...");
                    int nBytes = await reader.ReadAsync(buff, 0, buff.Length);
                    if (nBytes == 0)
                    {
                        RemoveClient(client);
                        Debug.WriteLine("Client disconnesso.");
                        break;
                    }
                    string recvMessage = new string(buff, 0, nBytes);
                    Debug.WriteLine($"Returned bytes {nBytes}, Messaggio: {recvMessage}");

                    DoWork(recvMessage, client);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void DoWork(string recvMessage, TcpClient client)
        {
            if (recvMessage.ToUpper().Trim() == "QUIT")
            {
                return;
            }
            else if (recvMessage.ToUpper().Trim() == "TIME")
            {
                SendToOne(DateTime.Now.ToShortTimeString() + " ", client);
            }
            else if (recvMessage.ToUpper().Trim() == "DATE")
            {
                SendToOne(DateTime.Now.ToShortDateString() + " ", client);
            }
            else
            {
                SendToOne("Non ho capito" + " ", client);
            }
        }

        public void SendToAll(string messaggio)
        {
            try
            {
                if (string.IsNullOrEmpty(messaggio))
                {
                    return;
                }

                byte[] buff = Encoding.ASCII.GetBytes(messaggio);

                foreach (TcpClient client in mClients)
                {
                    client.GetStream().WriteAsync(buff, 0, buff.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }

        public void SendToOne(string messaggio, TcpClient client)
        {
            try
            {
                if (string.IsNullOrEmpty(messaggio))
                {
                    return;
                }

                byte[] buff = Encoding.ASCII.GetBytes(messaggio);

                client.GetStream().WriteAsync(buff, 0, buff.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }

        private void RemoveClient(TcpClient client)
        {
            if (mClients.Contains(client))
            {
                mClients.Remove(client);
            }
        }

        public void CloseConnection()
        {
            try
            {
                foreach (TcpClient client in mClients)
                {
                    client.Close();
                    RemoveClient(client);
                }

                mServer.Stop();
                mServer = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Errore:" + ex.Message);
            }
        }
    }
}
