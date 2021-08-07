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
using System.Windows.Controls;
using System.Windows.Input;
using Window = System.Windows.Window;
using Visibility = System.Windows.Visibility;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using CsSimConnect.UIComponents.Domain;
using System.Windows.Media;
using System;
using System.Text;

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

        private static string NormalizeFreq(string input, int fracDigits =2)
        {
            string[] freqParts = input.Split('.');
            if (freqParts.Length > 1)
            {
                return freqParts[0].Substring(0, 3) + "." + (freqParts[1] + "00").Substring(0, fracDigits);
            }
            return (fracDigits > 0) ? (freqParts[0] + ".00".Substring(0,fracDigits+1)) : freqParts[0];
        }

        private static uint FreqToBCD(string freq)
        {
            uint result = 0;
            foreach (char c in freq)
            {
                if (char.IsDigit(c))
                {
                    result = (uint)((result << 4) + (c - '0'));
                }
            }
            return result;
        }

        private static string BCDToFreq(int freq)
        {
            if (freq < 0x100)
            {
                return "0.00";
            }
            string result = "";
            while (freq > 0)
            {
                result = ((char)('0' + (freq & 0xf))) + result;
                if (result.Length == 4)
                {
                    result = "." + result;
                }
                freq >>= 4;
            }
            return result.Substring(0, result.Length-2);
        }

        private static string NormalizeHeading(uint heading)
        {
            uint hdg = heading % 360;
            if (hdg == 0) hdg = 360;
            return $"{hdg:000}";
        }

        private static Visibility Indicator(bool? isVisible) => (isVisible ?? false) ? Visibility.Visible : Visibility.Hidden;

        private void UpdateUI()
        {
            Nav1Freq.Text = $"{currentState?.FreqNav1 ?? 0:000.00}";
            Nav2Freq.Text = $"{currentState?.FreqNav2 ?? 0:000.00}";
            AdfFreq.Text = BCDToFreq(currentState?.FreqAdf ?? 0);

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

        // NAV1 Frequency

        private readonly ClientEvent nav1RadioSet = EventManager.GetEvent("nav1_radio_set");

        private void Nav1KeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string freq = NormalizeFreq(Nav1Freq.Text);
                Nav1Freq.Text = freq;
                nav1RadioSet.Send(data: FreqToBCD(freq));
                Keyboard.ClearFocus();
            }
        }

        // NAV2 Frequency

        private readonly ClientEvent nav2RadioSet = EventManager.GetEvent("nav2_radio_set");

        private void Nav2KeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string freq = NormalizeFreq(Nav2Freq.Text);
                Nav2Freq.Text = freq;
                nav2RadioSet.Send(data: FreqToBCD(freq));
                Keyboard.ClearFocus();
            }
        }

        // ADF Frequency

        private readonly ClientEvent adfRadioSet = EventManager.GetEvent("adf_complete_set");

        private void AdfKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string freq = NormalizeFreq(AdfFreq.Text, 1);
                AdfFreq.Text = freq;
                adfRadioSet.Send(data: FreqToBCD(freq+"000"));
                Keyboard.ClearFocus();
            }
        }

        // AutoPilot Master switch

        private readonly ClientEvent apMasterEvent = EventManager.GetEvent("ap_master");

        private void ToggleAutoPilot(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle AP master");
                apMasterEvent.Send();
            }
        }

        // Heading Hold switch

        private readonly ClientEvent hdgHoldEvent = EventManager.GetEvent("ap_hdg_hold");

        private void ToggleHeading(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Heading Hold");
                hdgHoldEvent.Send();
            }
        }

        // Heading bug

        private readonly ClientEvent headingSet = EventManager.GetEvent("heading_bug_set");

        private void HeadingKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string heading = NormalizeHeading(uint.Parse(Heading.Text));
                Heading.Text = heading;
                headingSet.Send(data: uint.Parse(heading));
                Keyboard.ClearFocus();
            }
        }

        private readonly ClientEvent hdgInc = EventManager.GetEvent("heading_bug_inc");
        private readonly ClientEvent hdgDec = EventManager.GetEvent("heading_bug_dec");

        private void HdgWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                hdgInc.Send();
            }
            else if (e.Delta < 0)
            {
                hdgDec.Send();
            }
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

        private readonly ClientEvent altSet = EventManager.GetEvent("ap_alt_var_set_english");

        private void AltKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                Altitude.ReFormat();
                altSet.Send(data: Altitude.AsUInt());
                Keyboard.ClearFocus();
            }
        }

        private readonly ClientEvent altInc = EventManager.GetEvent("ap_alt_var_inc");
        private readonly ClientEvent altDec = EventManager.GetEvent("ap_alt_var_dec");

        private void AltWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                altInc.Send();
            }
            else if (e.Delta < 0)
            {
                altDec.Send();
            }
        }

        // VS Speed Hold

        private readonly ClientEvent vsHoldEvent = EventManager.GetEvent("ap_vs_hold");
        private void ToggleVerticalSpeed(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Vertical Speed Hold");
                vsHoldEvent.Send();
            }
            UpdateUI();
        }

        private readonly ClientEvent vsSet = EventManager.GetEvent("ap_vs_var_set_english");
        private void VSKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                VerticalSpeed.ReFormat();
                vsSet.SendSigned(data: VerticalSpeed.AsInt());
                Keyboard.ClearFocus();
            }
        }

        private readonly ClientEvent vsInc = EventManager.GetEvent("ap_vs_var_inc");
        private readonly ClientEvent vsDec = EventManager.GetEvent("ap_vs_var_dec");

        private void VSWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                vsInc.Send();
            }
            else if (e.Delta < 0)
            {
                vsDec.Send();
            }
        }

        // SPEED Hold

        private readonly ClientEvent speedHoldEvent = EventManager.GetEvent("ap_panel_speed_hold_toggle");
        private void ToggleSpeed(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Speed/IAS Hold");
                speedHoldEvent.Send();
            }
            UpdateUI();
        }

        private readonly ClientEvent speedSet = EventManager.GetEvent("ap_spd_var_set");
        private void SpeedKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                Speed.ReFormat();
                speedSet.SendSigned(data: Speed.AsInt());
                Keyboard.ClearFocus();
            }
        }

        private readonly ClientEvent speedInc = EventManager.GetEvent("ap_spd_var_inc");
        private readonly ClientEvent speedDec = EventManager.GetEvent("ap_spd_var_dec");

        private void SpeedWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                speedInc.Send();
            }
            else if (e.Delta < 0)
            {
                speedDec.Send();
            }
        }

        // NAV1 Hold

        private readonly ClientEvent nav1HoldEvent = EventManager.GetEvent("ap_nav1_hold");
        private void ToggleNav1Hold(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle NAV1 Hold");
                nav1HoldEvent.Send();
            }
            UpdateUI();
        }

        // NAV1 Course set

        private readonly ClientEvent nav1CourseSet = EventManager.GetEvent("vor1_set");
        private void Nav1CourseKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                Nav1Course.ReFormat();
                nav1CourseSet.Send(data: Nav1Course.AsUInt());
                Keyboard.ClearFocus();
            }
        }

        private readonly ClientEvent vor1Inc = EventManager.GetEvent("vor1_obi_inc");
        private readonly ClientEvent vor1Dec = EventManager.GetEvent("vor1_obi_dec");

        private void Vor1Wheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                vor1Inc.Send();
            }
            else if (e.Delta < 0)
            {
                vor1Dec.Send();
            }
        }

        // Nav2 Course set

        private readonly ClientEvent nav2CourseSet = EventManager.GetEvent("vor2_set");
        private void Nav2CourseKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                Nav2Course.ReFormat();
                nav2CourseSet.Send(data: Nav2Course.AsUInt());
                Keyboard.ClearFocus();
            }
        }

        private readonly ClientEvent vor2Inc = EventManager.GetEvent("vor2_obi_inc");
        private readonly ClientEvent vor2Dec = EventManager.GetEvent("vor2_obi_dec");

        private void Vor2Wheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                vor2Inc.Send();
            }
            else if (e.Delta < 0)
            {
                vor2Dec.Send();
            }
        }

        // APProach hold

        private readonly ClientEvent appHoldEvent = EventManager.GetEvent("ap_apr_hold");
        private void ToggleApproach(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Approach Hold");
                appHoldEvent.Send();
            }
            UpdateUI();
        }

        // BackCourse Hold

        private readonly ClientEvent bcHoldEvent = EventManager.GetEvent("ap_bc_hold");
        private void ToggleBackCourseHold(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle BackCourse Hold");
                bcHoldEvent.Send();
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
