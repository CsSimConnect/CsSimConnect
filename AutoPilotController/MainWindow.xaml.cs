﻿/*
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
using CsSimConnect.Events;
using System.Windows.Controls;
using System.Windows.Input;
using Window = System.Windows.Window;
using Visibility = System.Windows.Visibility;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using CsSimConnect.UIComponents.Domain;
using System.Windows.Media;

namespace AutoPilotController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger log = Logger.GetLogger(typeof(MainWindow));

        private readonly AIListViewModel aiList;
        private readonly SimConnect simConnect = SimConnect.Instance;

        public MainWindow()
        {
            aiList = new(action => Dispatcher.Invoke(action));
            DataContext = aiList;
            InitializeComponent();

            if (simConnect.IsConnected)
            {
                log.Info?.Log("We're already connected, let's subscribe to the data.");
                OnOpen();
                aiList.RefreshList();
            }
            Dispatcher.Invoke(SetButtons);

            simConnect.OnOpen += OnOpen;
            simConnect.OnConnectionStateChange += (_, _) => Dispatcher.Invoke(SetButtons);
        }

        private void OnOpen(AppInfo obj = null)
        {
            log.Info?.Log("Subscribing to the data.");
            DataManager.Instance.RequestData<AutoPilotData>().Subscribe(SetData);
            DataManager.Instance.RequestData<AutoPilotData>(ObjectDataPeriod.PerSimFrame, onlyWhenChanged: true).Subscribe(SetData);
        }

        private AutoPilotData currentState;
        private void SetData(AutoPilotData data)
        {
            currentState = data;
            Dispatcher.Invoke(UpdateUI);
        }

        private static Visibility Indicator(bool? isVisible) => (isVisible ?? false) ? Visibility.Visible : Visibility.Hidden;

        private void UpdateUI()
        {
            Nav1Freq.Set($"{currentState?.FreqNav1 ?? 0:000.00}");
            Nav2Freq.Set($"{currentState?.FreqNav2 ?? 0:000.00}");
            AdfFreq.FromBCD(currentState?.FreqAdf ?? 0);

            IndicatorAP.Visibility = Indicator(currentState?.AutoPilotMaster);
            Heading.Set(currentState?.Heading);
            IndicatorHDG.Visibility = Indicator(currentState?.HeadingHold);
            Altitude.Set(currentState?.Altitude);
            IndicatorALT.Visibility = Indicator(currentState?.AltitudeHold);
            VerticalSpeed.Set(currentState?.VerticalSpeed);
            //IndicatorVS.Visibility = Indicator(currentState?.VerticalSpeedHold);
            Speed.Set(currentState?.IndicatedAirSpeed);
            IndicatorIAS.Visibility = Indicator(currentState?.SpeedHold);
            Nav1Course.Set(currentState?.CourseNav1);
            IndicatorNAV1.Visibility = Indicator(currentState?.Nav1Hold);
            Nav2Course.Set(currentState?.CourseNav2);
            IndicatorAPP.Visibility = Indicator(currentState?.ApproachHold);
            IndicatorBC.Visibility = Indicator(currentState?.BackCourseHold);
        }

        // Altitude Hold Switch

        private bool useAltHold;
        private readonly ClientEvent altHoldEvent = EventManager.GetEvent("ap_alt_hold");
        private readonly ClientEvent altSelOnEvent = EventManager.GetEvent("ap_panel_altitude_on");
        private readonly ClientEvent altSelOffEvent = EventManager.GetEvent("ap_panel_altitude_off");

        private void ToggleAltitude(object sender, RoutedEventArgs e)
        {
            if (sender is Button altButton)
            {
                if (useAltHold || (currentState ==  null))
                {
                    log.Info?.Log("Toggle Altitude Hold");
                    altHoldEvent.Send();
                }
                else if (currentState?.AltitudeHold ?? true)
                {
                    log.Info?.Log("Set Altitude Hold OFF");
                    altSelOffEvent.Send();
                }
                else
                {
                    log.Info?.Log("Set Altitude Hold ON");
                    altSelOnEvent.Send();
                }
            }
            UpdateUI();
        }

        private void DoSettings(object sender, RoutedEventArgs e)
        {
            new SettingsDialog().ShowDialog();
        }

        private void DoClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private readonly SolidColorBrush blackBrush = new(Colors.Black);
        private readonly SolidColorBrush grayBrush = new(Colors.DarkGray);

        private void SetButtons()
        {
            iLink.Foreground = simConnect.IsConnected ? blackBrush : grayBrush;
            iLinkOff.Visibility = simConnect.IsConnected ? Visibility.Hidden : Visibility.Visible;

            iRenew.Foreground = simConnect.UseAutoConnect ? blackBrush : grayBrush;
            iNoRenew.Visibility = simConnect.UseAutoConnect ? Visibility.Hidden : Visibility.Visible;
        }

        private void DoConnect(object sender, RoutedEventArgs e)
        {
            if (simConnect.IsConnected)
            {
                simConnect.Disconnect();
            }
            else
            {
                simConnect.Connect();
            }
            Dispatcher.Invoke(SetButtons);
        }

        private void DoAutoConnect(object sender, RoutedEventArgs e)
        {
            simConnect.UseAutoConnect = !simConnect.UseAutoConnect;
            Dispatcher.Invoke(SetButtons);
        }

        private void StartDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
