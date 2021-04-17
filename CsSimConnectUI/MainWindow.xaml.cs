using CsSimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CsSimConnectUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SimConnect simConnect = SimConnect.Instance;

        public MainWindow()
        {
            InitializeComponent();
            simConnect.OnConnectionStateChange += (bool useAutoConnect, bool connected) =>
            {
                if (!connected && !useAutoConnect)
                {
                    iconSim.Source = new BitmapImage(new Uri("Images/dark-slider-off-64.png", UriKind.Relative));
                }
                else if (!connected)
                {
                    iconSim.Source = new BitmapImage(new Uri("Images/dark-slider-on-notok-64.png", UriKind.Relative));
                }
                else
                {
                    iconSim.Source = new BitmapImage(new Uri("Images/dark-slider-on-ok-64.png", UriKind.Relative));
                }
                if (connected)
                {
                    if (simConnect.SimName.Length == 0)
                    {
                        lStatus.Content = "Connected.";
                    }
                    else
                    {
                        lStatus.Content = String.Format("Connected to {0}, SimConnect version {1}", simConnect.SimName, simConnect.GetSimConnectVersion());
                    }
                }
                else
                {
                    lStatus.Content = "Disconnected.";
                }
            };
        }

        private void ToggleConnection(object sender, RoutedEventArgs e)
        {
            if (simConnect.IsConnected())
            {
                simConnect.Disconnect();
            }
            else
            {
                simConnect.Connect();
            }
        }
    }
}
