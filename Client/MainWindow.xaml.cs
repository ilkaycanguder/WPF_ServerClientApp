using Client.ViewModel;
using System.Text;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Server.ViewModel;

namespace Client
{
    public partial class MainWindow : Window
    {

        ClientViewModel clientViewModel;
        bool isLamb = false;
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            
            clientViewModel = new ClientViewModel();
            DataContext = clientViewModel;

            SetBinding(LampPropertyProperty, new Binding("LampState")
            {
                Source = clientViewModel,
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
                if (originator.LampProperty)
                {
                    originator.OnOffButton.Content = "Açık";
                    originator.OnOffButton.Background = Brushes.Yellow;

                }
                else
                {
                    originator.OnOffButton.Content = "Kapalı";
                    originator.OnOffButton.Background = Brushes.LightGray;
                }
            }
        }

        private void GenerateNumber_Click(object sender, RoutedEventArgs e)
        {
            int randomNumber = random.Next(999999);
            GenerateNumber.Content = randomNumber.ToString();
            var message = $"TextChanged:{randomNumber}";
            clientViewModel.SendMessageServer(message);
        }

        private void OnOffButton_Click(object sender, RoutedEventArgs e)
        {
            isLamb = !isLamb;
            var message = $"LampChanged:{isLamb}";

            if (isLamb)
            {
                OnOffButton.Content = "Kapalı";
                OnOffButton.Background = Brushes.LightGray;
            }
            else
            {
                OnOffButton.Content = "Açık";
                OnOffButton.Background = Brushes.Yellow;
            }

            clientViewModel.SendMessageServer(message);
        }

    
    }
}