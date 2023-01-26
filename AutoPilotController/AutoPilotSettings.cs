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

using CsSimConnect.UIComponents.Domain;
using Rakis.Logging;
using Rakis.Settings;
using Rakis.Settings.Files;
using System;
using System.Collections.Generic;
using System.Windows;

namespace AutoPilotController
{
    public class AutoPilotSettings : ViewModelBase
    {

        private readonly static ILogger log = Logger.GetLogger(typeof(AutoPilotSettings));

        private static readonly Lazy<AutoPilotSettings> lazyInstance = new(() => new AutoPilotSettings());
        public static AutoPilotSettings Instance { get { return lazyInstance.Value; } }

        private const string Group = "RakisSoftware";

        private const string Application = "AutoPilotController";

        private const string SectionSpeedHold = "SpeedHold";

        private bool autoConnect;
        public bool AutoConnect { get => autoConnect; set => autoConnect = value; }

        private bool useSpeedHoldToggle;
        public bool UseSpeedHoldToggle
        {
            get => useSpeedHoldToggle;
            set
            {
                useSpeedHoldToggle = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(PanelSpeedStyleVisibility));
                NotifyPropertyChanged(nameof(SpeedHoldEventVisibility));
                NotifyPropertyChanged(nameof(SpeedHoldEventsVisibility));
            }
        }
        public Visibility PanelSpeedStyleVisibility => UseSpeedHoldToggle ? Visibility.Collapsed : Visibility.Visible;

        private bool useSpeedHoldPanelEvents;
        public bool UseSpeedHoldPanelEvents { get => useSpeedHoldPanelEvents; set { useSpeedHoldPanelEvents = value; NotifyPropertyChanged(); } }

        private bool useCustomEvents;
        public bool UseCustomEvents
        {
            get => useCustomEvents;
            set
            {
                useCustomEvents = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SpeedHoldEventVisibility));
                NotifyPropertyChanged(nameof(SpeedHoldEventsVisibility));
            }
        }
        public Visibility SpeedHoldEventVisibility => (UseSpeedHoldToggle && UseCustomEvents) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SpeedHoldEventsVisibility => (!UseSpeedHoldToggle && UseCustomEvents) ? Visibility.Visible : Visibility.Collapsed;

        private string speedHoldToggleEvent;
        public string SpeedHoldToggleEvent { get => speedHoldToggleEvent; set { speedHoldToggleEvent = value; NotifyPropertyChanged(); } }
        private string speedHoldOnEvent;
        public string SpeedHoldOnEvent { get => speedHoldOnEvent; set { speedHoldOnEvent = value; NotifyPropertyChanged(); } }
        private string speedHoldOffEvent;
        public string SpeedHoldOffEvent { get => speedHoldOffEvent; set { speedHoldOffEvent = value; NotifyPropertyChanged(); } }

        private List<string> speedHoldToggleEventNames = new() { "ap_airspeed_hold", "ap_panel_speed_hold", "ap_panel_speed_hold_toggle" };
        public List<string> SpeedHoldToggleEventNames => speedHoldToggleEventNames;
        private List<string> speedHoldOnEventNames = new() { "ap_airspeed_on", "ap_panel_speed_on" };
        public List<string> SpeedHoldOnEventNames => speedHoldOnEventNames;
        private List<string> speedHoldOffEventNames = new() { "ap_airspeed_off", "ap_panel_speed_off" };
        public List<string> SpeedHoldOffEventNames => speedHoldOffEventNames;

        public string SimulatorKey { get; private set; }

        private JsonFileSettings settings;

        private AutoPilotSettings()
        {
            log.Debug?.Log($"Instantiating settings.");
        }

        public void Load()
        {
            log.Info?.Log($"Loading settings from the Roaming Profile.");
            settings = new(new(Group, Application), SettingsType.AppDataRoaming);
            settings.Load();

            AutoConnect = settings[nameof(AutoConnect)].AsBool ?? true;
            SimulatorKey = settings[nameof(SimulatorKey)].AsString ?? CsSimConnect.Sim.Util.P3Dv5Key;

            var speedSettings = settings[SectionSpeedHold].AsSettings;

            UseSpeedHoldToggle = speedSettings[nameof(UseSpeedHoldToggle)].AsBool ?? false;
            SpeedHoldToggleEvent = speedSettings[nameof(SpeedHoldToggleEvent)].AsString ?? "";
            UseSpeedHoldPanelEvents = speedSettings[nameof(UseSpeedHoldPanelEvents)].AsBool ?? false;
            SpeedHoldOnEvent = speedSettings[nameof(SpeedHoldOnEvent)].AsString ?? "";
            SpeedHoldOffEvent = speedSettings[nameof(SpeedHoldOffEvent)].AsString ?? "";

            log.Info?.Log($"- AutoConnect set to {AutoConnect}");
        }

        public void Store()
        {
            log.Info?.Log($"Updating settings.");

            settings[nameof(AutoConnect)] = AutoConnect;
            settings[nameof(SimulatorKey)] = SimulatorKey;

            var speedSettings = settings[SectionSpeedHold].AsSettings;

            speedSettings[nameof(UseSpeedHoldToggle)] = UseSpeedHoldToggle;
            speedSettings[nameof(SpeedHoldToggleEvent)] = SpeedHoldToggleEvent;

            speedSettings[nameof(UseSpeedHoldPanelEvents)] = UseSpeedHoldPanelEvents;
            speedSettings[nameof(SpeedHoldOnEvent)] = SpeedHoldOnEvent;
            speedSettings[nameof(SpeedHoldOffEvent)] = SpeedHoldOffEvent;

            settings.Save();
        }
    }
}
