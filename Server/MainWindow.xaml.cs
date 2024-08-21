using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Server.ViewModel;
using System.Windows.Data;

namespace Server
{
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private bool isLampOn = false;
        private ServerViewModel server;
        private List<TcpClient> connectedClients = new List<TcpClient>();

        public MainWindow()
        {
            InitializeComponent();
            server = new ServerViewModel();
            DataContext = server;

            SetBinding(LampPropertyProperty, new Binding("LampState")
            {
                Source = server,
                Mode = BindingMode.OneWay
            });
        }

        public bool LampProperty
        {
            get { return (bool)GetValue(LampPropertyProperty); }
            set { SetValue(LampPropertyProperty, value); }  
        }

        // Using a DependencyProperty as the backing store for LampProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LampPropertyProperty =
            DependencyProperty.Register("LampProperty", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false, LampPropertyChanged));

        private static void LampPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var originator = d as MainWindow;

            if (originator != null)
            {
                originator.myImage.Source = new BitmapImage(new Uri(originator.LampProperty ? "pack://application:,,,/Resources/Images/onlamb.png" : "pack://application:,,,/Resources/Images/offlamb.png"));
            }
        }

        private void GenerateNumber_Click(object sender, RoutedEventArgs e)
        {
            int randomNumber = random.Next(999999);
            NumberTextBox.Text = randomNumber.ToString();
            var message = $"TextChanged:{randomNumber}";
            server.SendMessageClient(message);
        }

        private void OnOffButton_Click(object sender, RoutedEventArgs e)
        {
            isLampOn = !isLampOn;
            myImage.Source = new BitmapImage(new Uri(isLampOn ? "pack://application:,,,/Resources/Images/onlamb.png" : "pack://application:,,,/Resources/Images/offlamb.png"));
            var message = $"LampState:{(isLampOn ? "Açık" : "Kapalı")}";
            server.SendMessageClient(message);
        }

        private void BroadcastMessage(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            List<TcpClient> disconnectedClients = new List<TcpClient>();

            foreach (var client in connectedClients)
            {
                try
                {
                    if (client.Connected)
                    {
                        var stream = client.GetStream();
                        stream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);
                    }
                    else
                    {
                        disconnectedClients.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error broadcasting message to client: {ex.Message}");
                    disconnectedClients.Add(client); // Add to disconnected list if there's an error
                }
            }

            foreach (var client in disconnectedClients)
            {
                connectedClients.Remove(client);
            }
        }
    }
}
