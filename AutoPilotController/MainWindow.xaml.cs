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
using CsSimConnect.Events;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoPilotController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger log = Logger.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            if (SimConnect.Instance.IsConnected)
            {
                log.Info?.Log("We're already connected, let's subscribe to the data.");
                OnOpen();
            }
            SimConnect.Instance.OnOpen += OnOpen;
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

        private void UpdateUI()
        {
            IndicatorAP.Visibility = (currentState?.AutoPilotMaster ?? false) ? Visibility.Visible : Visibility.Hidden;
            IndicatorHDG.Visibility = (currentState?.HeadingHold ?? false) ? Visibility.Visible : Visibility.Hidden;
            IndicatorALT.Visibility = (currentState?.AltitudeHold ?? false) ? Visibility.Visible : Visibility.Hidden;
            IndicatorIAS.Visibility = (currentState?.SpeedHold ?? false) ? Visibility.Visible : Visibility.Hidden;
            IndicatorNAV1.Visibility = (currentState?.Nav1Hold ?? false) ? Visibility.Visible : Visibility.Hidden;
            IndicatorAPP.Visibility = (currentState?.ApproachHold ?? false) ? Visibility.Visible : Visibility.Hidden;
            IndicatorBC.Visibility = (currentState?.BackCourseHold ?? false) ? Visibility.Visible : Visibility.Hidden;

        }

        private readonly ClientEvent apMasterEvent = new("ap_master");
        private void ToggleAutoPilot(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle AP master");
                CsSimConnect.EventManager.Instance.SendEvent(apMasterEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent hdgHoldEvent = new("ap_hdg_hold");
        private void ToggleHeading(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Heading Hold");
                CsSimConnect.EventManager.Instance.SendEvent(hdgHoldEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent altHoldEvent = new("ap_alt_hold");
        private void ToggleAltitude(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Altitude Hold");
                CsSimConnect.EventManager.Instance.SendEvent(altHoldEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent speedHoldEvent = new("ap_panel_speed_hold_toggle");
        private void ToggleSpeed(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Speed/IAS Hold");
                CsSimConnect.EventManager.Instance.SendEvent(speedHoldEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent nav1HoldEvent = new("ap_nav1_hold");
        private void ToggleNav1Hold(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle NAV1 Hold");
                CsSimConnect.EventManager.Instance.SendEvent(nav1HoldEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent appHoldEvent = new("ap_apr_hold");
        private void ToggleApproach(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Approach Hold");
                CsSimConnect.EventManager.Instance.SendEvent(appHoldEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent bcHoldEvent = new("ap_bc_hold");
        private void ToggleBackCourseHold(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle BackCourse Hold");
                CsSimConnect.EventManager.Instance.SendEvent(bcHoldEvent, onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }
    }
}
