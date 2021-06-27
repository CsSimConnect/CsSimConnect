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

namespace AutoPilotController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger log = Logger.GetLogger(typeof(MainWindow));

        private readonly AIListViewModel aiList;

        public MainWindow()
        {
            aiList = new(action => Dispatcher.Invoke(action));
            DataContext = aiList;
            InitializeComponent();

            if (SimConnect.Instance.IsConnected)
            {
                log.Info?.Log("We're already connected, let's subscribe to the data.");
                OnOpen();
                aiList.RefreshList();
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
        private static string NormalizeHeading(int? hdg) => NormalizeHeading((uint)(hdg ?? 0));

        private static string NormalizeAltitude(int? alt) => (alt == null) ? "0" : $"{alt:##,###}";

        private static string NormalizeSpeed(int? speed) => (speed == null) ? "0" : $"{speed}";

        private static int ParseNumber(string vs) => int.Parse(vs.Replace(",", ""));

        private static string NormalizeVerticalSpeed(int? vs) => (vs == null) ? "0" : $"{vs}";

        private Visibility Indicator(bool? isVisible) => (isVisible ?? false) ? Visibility.Visible : Visibility.Hidden;

        private void UpdateUI()
        {
            Nav1Freq.Text = $"{currentState?.FreqNav1 ?? 0:000.00}";
            Nav2Freq.Text = $"{currentState?.FreqNav2 ?? 0:000.00}";
            AdfFreq.Text = BCDToFreq(currentState?.FreqAdf ?? 0);

            IndicatorAP.Visibility = Indicator(currentState?.AutoPilotMaster);
            Heading.Text = NormalizeHeading(currentState?.Heading);
            IndicatorHDG.Visibility = Indicator(currentState?.HeadingHold);
            Altitude.Text = NormalizeAltitude(currentState?.Altitude);
            IndicatorALT.Visibility = Indicator(currentState?.AltitudeHold);
            VerticalSpeed.Text = NormalizeVerticalSpeed(currentState?.VerticalSpeed);
            IndicatorVS.Visibility = Indicator(currentState?.VerticalSpeedHold);
            Speed.Text = NormalizeSpeed(currentState?.IndicatedAirSpeed);
            IndicatorIAS.Visibility = Indicator(currentState?.SpeedHold);
            Nav1Course.Text = NormalizeHeading(currentState?.CourseNav1);
            IndicatorNAV1.Visibility = Indicator(currentState?.Nav1Hold);
            Nav2Course.Text = NormalizeHeading(currentState?.CourseNav2);
            IndicatorAPP.Visibility = Indicator(currentState?.ApproachHold);
            IndicatorBC.Visibility = Indicator(currentState?.BackCourseHold);
        }

        private readonly ClientEvent nav1RadioSet = EventManager.GetEvent("nav1_radio_set");
        private void Nav1KeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string freq = NormalizeFreq(Nav1Freq.Text);
                Nav1Freq.Text = freq;
                nav1RadioSet.Send(data: FreqToBCD(freq), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent nav2RadioSet = EventManager.GetEvent("nav2_radio_set");
        private void Nav2KeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string freq = NormalizeFreq(Nav2Freq.Text);
                Nav2Freq.Text = freq;
                nav2RadioSet.Send(data: FreqToBCD(freq), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent adfRadioSet = EventManager.GetEvent("adf_complete_set");
        private void AdfKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string freq = NormalizeFreq(AdfFreq.Text, 1);
                AdfFreq.Text = freq;
                adfRadioSet.Send(data: FreqToBCD(freq+"000"), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent apMasterEvent = EventManager.GetEvent("ap_master");
        private void ToggleAutoPilot(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle AP master");
                apMasterEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent hdgHoldEvent = EventManager.GetEvent("ap_hdg_hold");
        private void ToggleHeading(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Heading Hold");
                hdgHoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent headingSet = EventManager.GetEvent("heading_bug_set");
        private void HeadingKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string heading = NormalizeHeading(uint.Parse(Heading.Text));
                Heading.Text = heading;
                headingSet.Send(data: uint.Parse(heading), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent altHoldEvent = EventManager.GetEvent("ap_alt_hold");
        private void ToggleAltitude(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Altitude Hold");
                altHoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent altSet = EventManager.GetEvent("ap_alt_var_set_english");
        private void AltKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string altitude = NormalizeAltitude(ParseNumber(Altitude.Text));
                Altitude.Text = altitude;
                altSet.Send(data: (uint)ParseNumber(altitude), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent vsHoldEvent = EventManager.GetEvent("ap_vs_hold");
        private void ToggleVerticalSpeed(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Vertical Speed Hold");
                vsHoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent vsSet = EventManager.GetEvent("ap_vs_var_set_english");
        private void VSKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string vs = NormalizeVerticalSpeed(ParseNumber(VerticalSpeed.Text));
                VerticalSpeed.Text = vs;
                vsSet.SendSigned(data: ParseNumber(vs), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent speedHoldEvent = EventManager.GetEvent("ap_panel_speed_hold_toggle");
        private void ToggleSpeed(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Speed/IAS Hold");
                speedHoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent speedSet = EventManager.GetEvent("ap_spd_var_set");
        private void SpeedKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string speed = NormalizeSpeed(ParseNumber(Speed.Text));
                Speed.Text = speed;
                speedSet.SendSigned(data: ParseNumber(speed), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent nav1HoldEvent = EventManager.GetEvent("ap_nav1_hold");
        private void ToggleNav1Hold(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle NAV1 Hold");
                nav1HoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent nav1CourseSet = EventManager.GetEvent("vor1_set");
        private void Nav1CourseKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string course = NormalizeHeading(uint.Parse(Nav1Course.Text));
                log.Debug?.Log($"NAV1 Course normalized from {Nav1Course.Text} to {course}");
                Nav1Course.Text = course;
                nav1CourseSet.Send(data: uint.Parse(course), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent nav2CourseSet = EventManager.GetEvent("vor2_set");
        private void Nav2CourseKeyDown(object sender, KeyEventArgs evt)
        {
            if (evt.Key == Key.Enter)
            {
                string course = NormalizeHeading(uint.Parse(Nav2Course.Text));
                log.Debug?.Log($"NAV1 Course normalized from {Nav2Course.Text} to {course}");
                Nav2Course.Text = course;
                nav2CourseSet.Send(data: uint.Parse(course), onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
        }

        private readonly ClientEvent appHoldEvent = EventManager.GetEvent("ap_apr_hold");
        private void ToggleApproach(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle Approach Hold");
                appHoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }

        private readonly ClientEvent bcHoldEvent = EventManager.GetEvent("ap_bc_hold");
        private void ToggleBackCourseHold(object sender, RoutedEventArgs e)
        {
            if (sender is Button apButton)
            {
                log.Info?.Log("Toggle BackCourse Hold");
                bcHoldEvent.Send(onError: exc => log.Error?.Log($"Exception: {exc.Message}"));
            }
            UpdateUI();
        }
    }
}
