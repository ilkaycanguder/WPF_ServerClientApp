using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModel
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        //List<TcpClient> tcpClients = new List<TcpClient>();
        TcpClient client;
        private bool _lampState = false;
        public bool LampState
        {
            get => _lampState;
            set
            {
                if (_lampState != value)
                {
                    _lampState = value;
                    OnPropertyChanged(nameof(LampState));
                    SendMessageServer($"LampChanged:{value}");
                }
            }
        }
        private string _randomValue = "";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string RandomValue
        {
            get => _randomValue;
            set
            {
                if (_randomValue != value)
                {
                    _randomValue = value;
                    OnPropertyChanged(nameof(RandomValue));
                    SendMessageServer($"TextChanged:{value}");
                }
            }
        }

        public ClientViewModel()
        {
            ConnectToTcpServer();
        }

        public async void ConnectToTcpServer()
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5050);

            var thread = new Thread(ListenServer);
            thread.Start();
        }

        public async void ListenServer()
        {
            var stream = client.GetStream();
            byte[] buffer = new byte[256];
            int bytesRead;

            while (true)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (message.StartsWith("TextChanged:"))
                {
                    RandomValue = message.Split(':')[1];
                }
                else if (message.StartsWith("LampState:"))
                {
                    LampState = message.Split(':')[1] == "Açık";
                 
                }
            }
        }

        public void SendMessageServer(string message)
        {
            var stream = client.GetStream();
            var byteMessage = Encoding.UTF8.GetBytes(message);
            stream.Write(byteMessage, 0, byteMessage.Length);
            //foreach (var client in tcpClients)
            //{
            //    var bytemessage = Encoding.UTF8.GetBytes(message);
            //    client.GetStream().Write(bytemessage, 0, bytemessage.Length);
            //}
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
