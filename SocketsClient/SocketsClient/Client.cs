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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Web;

namespace Sockets
{
    public partial class frmMain : Form
    {
        // private UdpClient udpClient;     // клиентский сокет
        private IPAddress IP;                           // IP-адрес клиента
        // private UdpClient localClient;
        private int ServerPort;
        private int ClientPort;

        private TcpClient Client;

        private StreamReader streamReader = null;
        private StreamWriter streamWriter = null;

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());    // информация об IP-адресах и имени машины, на которой запущено приложение
            IP = hostEntry.AddressList[0];                                  // IP-адрес, который будет указан в заголовке окна для идентификации клиента

            IP = IPAddress.Parse("127.127.127.127");

            this.Text += "Клиент";
        }

        // Tcp sending messages
        async Task SendMessageAsync(string message)
        {
            await streamWriter.WriteLineAsync(message);
            await streamWriter.FlushAsync();
        }

        // Recieve messages via tcp
        async Task ReceiveMessageAsync(StreamReader reader)
        {
            while (true)
            {
                try
                {
                    // считываем ответ в виде строки
                    string message = await reader.ReadLineAsync();
                    // если пустой ответ, ничего не выводим на консоль
                    if (string.IsNullOrEmpty(message)) continue;

                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                        if (message.Replace("\0", "") != "")
                            rtbMessages.Text += "\n >> " + message;             // выводим полученное сообщение на форму
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        //// Recieve messages via udp broadcasting
        //async Task RecieveMessages()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            var result = await localClient.ReceiveAsync();
        //            string message = Encoding.UTF8.GetString(result.Buffer);

        //            rtbMessages.Invoke((MethodInvoker)delegate
        //            {
        //                if (message.Replace("\0", "") != "")
        //                    rtbMessages.Text += "\n >> " + message;             // выводим полученное сообщение на форму
        //            });
        //        }
                
        //        catch
        //        {

        //        }
        //    }
        //}

        //async Task SendMessageAsync(string message)
        //{
        //    byte[] data = Encoding.UTF8.GetBytes(message);

        //    await udpClient.SendAsync(data, data.Length, new IPEndPoint(IP, ServerPort));
        //}

        // подключение к серверному сокету
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ServerPort = 8080;
                ClientPort = 8010;

                //udpClient = new UdpClient();
                //localClient = new UdpClient(ClientPort);
                //localClient.JoinMulticastGroup(IP);

                Client = new TcpClient();
                Client.Connect(IP, ServerPort);
                streamReader = new StreamReader(Client.GetStream());
                streamWriter = new StreamWriter(Client.GetStream());

                btnConnect.Enabled = false;
                tbNickName.Enabled = false;
                btnSend.Enabled = true;

                Task.Run(() => ReceiveMessageAsync(streamReader));
            }
            catch
            {
                MessageBox.Show("Введен некорректный IP-адрес");
            }
        }

        // отправка сообщения
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessageAsync(tbNickName.Text + " >> " + tbMessage.Text);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //udpClient.Close();
            //localClient.Close();    
        }
    }
}