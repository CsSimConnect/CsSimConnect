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
using CsSimConnect.AI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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

        private readonly SimConnect simConnect;
        private bool isPaused = false;
        private bool isRunning = false;

        public readonly List<SimulatedObject> AIList = new();

        public MainWindow()
        {
            Logger.Configure();
            SimConnect.InterOpType = FlightSimType.Prepar3Dv5;
            simConnect = SimConnect.Instance;

            simConnect.OnOpen += OnOpen;
            simConnect.OnClose += OnClose;

            BuildDemoList();

            InitializeComponent();
            SetButtons();

            log.Info("Registering connectionstate listener");
            simConnect.OnConnectionStateChange += (bool useAutoConnect, bool connected) => {
                if (!connected)
                {
                    Run(() => status.MessageQueue.Enqueue("Disconnected."));
                }
            };
        }

        private void BuildDemoList()
        {
            AIList.Add(new(ObjectType.Boat));
            AIList.Add(new(ObjectType.Helicopter));
            AIList.Add(new(ObjectType.Aircraft));
            AIList.Add(new(ObjectType.GroundVehicle));
            DataContext = AIList;
        }


        private void NewAI(object sender, RoutedEventArgs e)
        {
            log.Info("NewAI requested");
            CreateAIDialog dlg = new();
            dlg.ShowDialog();
        }

        private void OnOpen(AppInfo info)
        {
            log.Info("Connected");
            if (simConnect.Info.Name.Length != 0)
            {
                Run(() => status.MessageQueue.Enqueue(String.Format("Connected to {0}, SimConnect version {1}", info.Name, info.SimConnectVersion())));
            }
            new Task(Subscribe).Start();
        }

        private void Subscribe()
        {
            if (Interlocked.Exchange(ref isConnected, 1) == 0)
            {
                CsSimConnect.EventManager evtMgr = CsSimConnect.EventManager.Instance;
                evtMgr.SubscribeToSystemEventBool(SystemEvent.Pause, OnPause);
                evtMgr.SubscribeToSystemEventBool(SystemEvent.Sim, OnStop);
                evtMgr.SubscribeToObjectAddedEvent().Subscribe(OnObjectAdded);
                evtMgr.SubscribeToObjectRemovedEvent().Subscribe(OnObjectRemoved);

                RequestManager.Instance.RequestSystemStateBool(SystemState.Sim, OnStop);
            }
        }

        private void OnObjectAdded(SimulatedObject obj)
        {
            log.Info("A '{0}' was added with id '{1}.", obj.ObjectType.ToString(), obj.ObjectId);
        }

        private void OnObjectRemoved(SimulatedObject obj)
        {
            log.Info("A '{0}' was removed with id '{1}.", obj.ObjectType.ToString(), obj.ObjectId);
        }

        private void UpdateStatus()
        {
            Run(SetButtons);
        }

        private void OnClose()
        {
            log.Info("Not connected");

            isConnected = 0;
            Run(() => status.MessageQueue.Enqueue("Disconnected."));
        }

        private void OnPause(bool paused)
        {
            isPaused = paused;
            Run(() =>
            {
                UpdateStatus();
                if (isPaused)
                {
                    status.MessageQueue.Enqueue("Simulator paused.");
                }
                else
                {
                    status.MessageQueue.Enqueue("Simulator unpaused.");
                }
            });
        }

        private void OnStop(Boolean running)
        {
            isRunning = running;
            Run(() =>
            {
                UpdateStatus();
                if (isRunning)
                {
                    status.MessageQueue.Enqueue("Simulator resumed.");
                }
                else
                {
                    status.MessageQueue.Enqueue("Simulator stopped.");
                }
            });
        }

        private void SetButtons()
        {
            iLink.Visibility = simConnect.IsConnected() ? Visibility.Visible : Visibility.Collapsed;
            iLinkOff.Visibility = simConnect.IsConnected() ? Visibility.Collapsed : Visibility.Visible;

            iRenew.Foreground = simConnect.UseAutoConnect ? iNoRenew.Foreground : new SolidColorBrush(Colors.DarkGray);
            iNoRenew.Visibility = simConnect.UseAutoConnect ? Visibility.Hidden : Visibility.Visible;

            bPaused.IsEnabled = isPaused;
            bStopped.IsEnabled = !isRunning;
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            if (simConnect.IsConnected())
            {
                simConnect.Disconnect();
                if (simConnect.IsConnected())
                {
                    Run(() => status.MessageQueue.Enqueue("Disconnect failed."));
                }
            }
            else
            {
                simConnect.Connect();
                if (!simConnect.IsConnected())
                {
                    Run(() => status.MessageQueue.Enqueue("Connection failed."));
                }
            }
            Run(SetButtons);
        }

        private void AutoConnect(object sender, RoutedEventArgs e)
        {
            log.Debug("Togggle autoconnect.");
            simConnect.UseAutoConnect = !simConnect.UseAutoConnect;
            log.Debug("Autoconnect is now {0}.", simConnect.UseAutoConnect ? "ON" : "OFF");
            Run(SetButtons);
        }

    }
}
