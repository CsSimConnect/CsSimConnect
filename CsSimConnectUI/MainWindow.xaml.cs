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
        private SimConnect simConnect = new SimConnect();

        public MainWindow()
        {
            InitializeComponent();
            simConnect.OnConnectionStateChange += (bool connected) => { lStatusValue.Content = connected ? "Connected" : "Not connected"; };
        }

        private void DoConnect(object sender, RoutedEventArgs e)
        {
            simConnect.Connect();
            bConnect.IsEnabled = !simConnect.IsConnected();
            bDisconnect.IsEnabled = simConnect.IsConnected();
            if (simConnect.IsConnected())
            {
                //simConnect.Events.addSystemEventHandler(SystemEvent.SIM, (evt) => { });
            }
        }

        private void DoDisconnect(object sender, RoutedEventArgs e)
        {
            simConnect.Disconnect();
            bConnect.IsEnabled = !simConnect.IsConnected();
            bDisconnect.IsEnabled = simConnect.IsConnected();
        }
    }
}
