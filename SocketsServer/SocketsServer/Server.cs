using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using SocketsServer;

namespace Sockets
{
    public partial class frmMain : Form
    {
        private TcpListener Listener;                   // сокет сервера
        // private UdpClient udpClient;
        private IPAddress IP;
        private int ServerPort;
        private int ClientPort;

        private List<CustomClient> clients = new List<CustomClient>();      // список клиентов

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());    // информация об IP-адресах и имени машины, на которой запущено приложение
            IP = hostEntry.AddressList[0];                        // IP-адрес, который будет указан при создании сокета
            ServerPort = 8080;                                                // порт, который будет указан при создании сокета
            ClientPort = 8010;

            IP = IPAddress.Parse("127.127.127.127");

            // вывод IP-адреса машины и номера порта в заголовок формы, чтобы можно было его использовать для ввода имени в форме клиента, запущенного на другом вычислительном узле
            this.Text += "     " + IP.ToString() + "  :  " + ServerPort.ToString();

            //udpClient= new UdpClient(ServerPort);

            //udpClient.JoinMulticastGroup(IP);

            //Task.Run(() => RecieveMessages());

            // создаем серверный сокет (Listener для приема заявок от клиентских сокетов)
            Listener = new TcpListener(IP, ServerPort);
            Listener.Start();

            ListenAsync();
        }

        //// Recieve messages via udp broadcasting
        //async Task RecieveMessages()
        //{
        //    while (true)
        //    {
        //        var result = await udpClient.ReceiveAsync();
        //        string message = Encoding.UTF8.GetString(result.Buffer);

        //        rtbMessages.Invoke((MethodInvoker)delegate
        //        {
        //            if (message.Replace("\0", "") != "")
        //                rtbMessages.Text += "\n >> " + message;             // выводим полученное сообщение на форму
        //        });

        //        UdpClient localClient = new UdpClient();
        //        await localClient.SendAsync(result.Buffer, result.Buffer.Length, new IPEndPoint(IP, ClientPort));
        //        localClient.Close();
        //    }
        //}

        // прослушивание входящих подключений
        private async Task ListenAsync()
        {
            try
            {
                Listener.Start();

                while (true)
                {
                    TcpClient tcpClient = await Listener.AcceptTcpClientAsync();

                    CustomClient clientObject = new CustomClient(tcpClient);
                    clients.Add(clientObject);
                    Task.Run(() => ProcessAsync(clientObject));
                }
            }
            catch
            {

            }
            finally
            {
                Disconnect();
            }
        }

        public async Task ProcessAsync(CustomClient customClient)
        {
            try
            {
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        string message = await customClient.Reader.ReadLineAsync();
                        if (message == null) continue;

                        rtbMessages.Invoke((MethodInvoker)delegate
                        {
                            if (message.Replace("\0", "") != "")
                                rtbMessages.Text += "\n >> " + message;             // выводим полученное сообщение на форму
                        });

                        await BroadcastMessageAsync(message);
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                RemoveConnection(customClient.Id);
            }
        }

        // трансляция сообщения подключенным клиентам
        private async Task BroadcastMessageAsync(string message)
        {
            foreach (var client in clients)
            {
                await client.Writer.WriteLineAsync(message); //передача данных
                await client.Writer.FlushAsync();
            }
        }

        // Отключение отсоединившегося клиента
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            CustomClient client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null) clients.Remove(client);
            client?.Close();
        }

        private void Disconnect()
        {
            foreach (var client in clients)
            {
                client.Close(); //отключение клиента
            }
            Listener.Stop(); //остановка сервера
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();

            //udpClient.DropMulticastGroup(IP);

            //udpClient.Close();
        }
    }
}