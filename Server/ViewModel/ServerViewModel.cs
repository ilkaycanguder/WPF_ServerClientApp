using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server.ViewModel
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        TcpListener _listener;
        List<TcpClient> tcpClients = new List<TcpClient>();
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _lambState = false;
        public bool LampState
        {
            get => _lambState;
            set
            {
                if (_lambState != value)
                {
                    _lambState = value;
                    OnPropertyChanged(nameof(LampState));
                    SendMessageClient($"LampState:{(value ? "Açık" : "Kapalı")}");
                }
            }
        }

        private string _randomValue = "";
        public string RandomValue
        {
            get => _randomValue;
            set
            {
                if (_randomValue != value)
                {
                    _randomValue = value;
                    OnPropertyChanged(nameof(RandomValue));
                    SendMessageClient($"TextChanged:{value}");
                }
            }
        }

        public string LampImage => LampState ? "/Resources/Images/onlamb.png" : "/Resources/Images/offlamb.png";

        public ServerViewModel()
        {
            StartTcpServer();
        }

        public async void StartTcpServer()
        {
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5050);
            _listener.Start();

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                tcpClients.Add(client);
                var thread = new Thread(new ParameterizedThreadStart(ListenClients));
                thread.Start(client);
            }
        }

        public async void ListenClients(object client)
        {
            TcpClient paramClient = client as TcpClient;
            var stream = paramClient.GetStream();
            byte[] buffer = new byte[256];
            int bytesRead;

            while (true)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (message.StartsWith("LampChanged:"))
                {
                    LampState = message.Split(':')[1] == "True";
                }
                else if (message.StartsWith("TextChanged:"))
                {
                    RandomValue = message.Split(':')[1];
                }
            }
        }

        public void SendMessageClient(string message)
        {
            foreach (var client in tcpClients)
            {
                var bytemessage = Encoding.UTF8.GetBytes(message);
                client.GetStream().Write(bytemessage, 0, bytemessage.Length);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
