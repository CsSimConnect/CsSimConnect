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

        private int isConnected = 0; // Because we don't van `Interlocked.Exchange()` for `bool`s.

        private void Run(Action action) {
            this.Dispatcher.Invoke(action);
        }

        private void SetPausedStatus(bool running) => Run(() => lPaused.Style = (Style)FindResource(running ? "StatusOn" : "StatusOff"));
        private void SetStoppedStatus(bool running) => Run(() => lStopped.Style = (Style)FindResource(running ? "StatusOff" : "StatusOn"));

        public MainWindow()
        {
            Logger.Configure();
            SimConnect.InterOpType = FlightSimType.Prepar3Dv5;
            SimConnect.Instance.OnOpen += OnOpen;
            SimConnect.Instance.OnClose += OnClose;

            InitializeComponent();

            log.Info("Registering connectionstate listener");
            SimConnect.Instance.OnConnectionStateChange += (bool useAutoConnect, bool connected) => Run(() =>
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
            });
        }

        private void OnOpen(AppInfo info)
        {
            log.Info("Connected");
            if (Interlocked.Exchange(ref isConnected, 1) == 0)
            {
                // Haven't registered these yet
                CsSimConnect.EventManager.Instance.SubscribeToSystemEventBool(SystemEvent.Pause, SetPausedStatus);
                CsSimConnect.EventManager.Instance.SubscribeToSystemEventBool(SystemEvent.Sim, SetStoppedStatus);
                RequestManager.Instance.RequestSystemStateBool(SystemState.Sim, SetStoppedStatus);
            }
            if (SimConnect.Instance.Info.Name.Length == 0)
            {
                lStatus.Content = "Connected.";
            }
            else
            {
                lStatus.Content = String.Format("Connected to {0}, SimConnect version {1}", info.Name, info.SimConnectVersion());
            }
            new Task(TestGetSimState).Start();
        }

        private void OnClose()
        {
            log.Info("Not connected");

            isConnected = 0;
            lStatus.Content = "Disconnected.";
            lPaused.Style = (Style)FindResource("StatusOff");
            lStopped.Style = (Style)FindResource("StatusOff");
        }

        private void ToggleConnection(object sender, RoutedEventArgs e)
        {
            if (SimConnect.Instance.IsConnected())
            {
                SimConnect.Instance.Disconnect();
            }
            else
            {
                SimConnect.Instance.Connect();
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
