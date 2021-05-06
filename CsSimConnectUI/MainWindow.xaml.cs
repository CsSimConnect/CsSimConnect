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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

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

        private void SetPausedStatus(bool running) => Run(() => lPaused.Style = (Style)FindResource(running ? "StatusOn" : "StatusOff"));
        private void SetStoppedStatus(bool running) => Run(() => lStopped.Style = (Style)FindResource(running ? "StatusOff" : "StatusOn"));

        public MainWindow()
        {
            Logger.Configure();
            InitializeComponent();

            log.Info("Registering connectionstate listener");
            simConnect.OnConnectionStateChange += (bool useAutoConnect, bool connected) => Run(() =>
            {
                if (!connected && !useAutoConnect)
                {
                    log.Info("Not connected, no autoconnect");
                    iconSim.Source = new BitmapImage(new Uri("Images/dark-slider-off-64.png", UriKind.Relative));
                }
                else if (!connected)
                {
                    log.Info("Not connected, autoconnect");
                    iconSim.Source = new BitmapImage(new Uri("Images/dark-slider-on-notok-64.png", UriKind.Relative));
                }
                else
                {
                    iconSim.Source = new BitmapImage(new Uri("Images/dark-slider-on-ok-64.png", UriKind.Relative));
                }
                if (connected)
                {
                    log.Info("Connected");
                    if (Interlocked.Exchange(ref isConnected, 1) == 0)
                    {
                        // Haven't registered these yet
                        events.SubscribeToSystemEventBool(SystemEvent.Pause, SetPausedStatus);
                        events.SubscribeToSystemEventBool(SystemEvent.Sim, SetStoppedStatus);
                        requests.RequestSystemStateBool(SystemState.Sim, SetStoppedStatus);
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
                    log.Info("Not connected");

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
            log.Info("The simulator is {0}.", RequestManager.Instance.RequestSystemState(SystemState.Sim).Get().AsBoolean() ? "Running" : "Stopped");
            AircraftData data = DataManager.Instance.RequestData<AircraftData>().Get();
            log.Info("Currently selected aircraft is '{0}'.", data.Title);

            log.Info("Starting stream");
            DataManager.Instance.RequestData<AircraftData>(ObjectDataPeriod.PerSecond, onlyWhenChanged: true)
                                .Subscribe((AircraftData data) => log.Info("[stream] Currently selected aircraft is '{0}'.", data.Title));
        }
    }
}
