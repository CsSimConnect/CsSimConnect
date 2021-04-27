/*
 * Copyright (c) 2021. Bert Laverman
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using CsSimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        private static readonly Logger log = Logger.GetLogger(typeof(MainWindow));

        private readonly SimConnect simConnect = SimConnect.Instance;
        private readonly RequestManager requests = RequestManager.Instance;
        private readonly CsSimConnect.EventManager events = CsSimConnect.EventManager.Instance;

        private int isConnected = 0; // Because we don't van `Interlocked.Exchange()` for `bool`s.

        private void Run(Action action) {
            this.Dispatcher.Invoke(action);
        }

        public MainWindow()
        {
            Logger.Configure();
            InitializeComponent();

            log.Info("Registering connectionstate listener");
            simConnect.OnConnectionStateChange += (bool useAutoConnect, bool connected) => Run(() =>
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
                    if (Interlocked.Exchange(ref isConnected, 1) == 0)
                    {
                        // Haven't registered these yet
                        events.SubscribeToSystemStateBool(CsSimConnect.EventManager.ToString(SystemEvent.Pause),
                            (bool running) => Run(() => lPaused.Style = (Style)FindResource(running ? "StatusOn" : "StatusOff")));
                        events.SubscribeToSystemStateBool(CsSimConnect.EventManager.ToString(SystemEvent.Sim),
                            (bool running) => Run(() => lStopped.Style = (Style)FindResource(running ? "StatusOff" : "StatusOn")),
                            true);
                    }
                    if (simConnect.Info.Name.Length == 0)
                    {
                        lStatus.Content = "Connected.";
                    }
                    else
                    {
                        lStatus.Content = String.Format("Connected to {0}, SimConnect version {1}", simConnect.Info.Name, simConnect.Info.SimConnectVersion());
                    }
                    new Task(TestGetSimState).Start();
                }
                else
                {
                    isConnected = 0;
                    lStatus.Content = "Disconnected.";
                    lPaused.Style = (Style)FindResource("StatusOff");
                    lStopped.Style = (Style)FindResource("StatusOff");
                }
            });
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

        private void TestGetSimState()
        {
            log.Info("The simulator is {0}.", RequestManager.Instance.RequestSystemState("Sim").Get().AsBoolean() ? "Running" : "Stopped");
        }
    }
}
