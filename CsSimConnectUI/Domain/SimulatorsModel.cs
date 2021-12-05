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
using CsSimConnect.Sim;
using static CsSimConnect.Sim.Util;
using NSwag.Collections;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using Rakis.Logging;

namespace CsSimConnectUI.Domain
{
    public class SelectableSimulator : INotifyPropertyChanged
    {
        private SimulatorsModel model;
        public Simulator Sim { get; set; }
        internal bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                model.NotifySelectionChanged(Sim.Key); 
            }
        }
        public string Name
        {
            get => Sim.Name;
        }
        public string InstallationPath
        {
            get => Sim.InstallationPath;
        }
        public bool Installed
        {
            get => Sim.DllAvailable;
        }

        public SelectableSimulator(SimulatorsModel model, Simulator sim)
        {
            this.model = model;
            Sim = sim;
        }
    }

    public class SimulatorsModel : ViewModelBase
    {

        private static readonly Logger log = Logger.GetLogger(typeof(AIListViewModel));

        private Dictionary<string, SelectableSimulator> _dictionary = new();
        public ObservableDictionary<string, SelectableSimulator> Simulators { get; init; }

        private void addSimulator(Simulator sim)
        {
            if (sim.Installed)
            {
                Simulators.Add(sim.Key, new(this, sim));
            }
        }

        public SimulatorsModel()
        {
            Simulators = new(_dictionary);
            addSimulator(GetPrepar3Dv4());
            addSimulator(GetPrepar3Dv5());
            addSimulator(GetMSFS2020());
        }

        internal void NotifySelectionChanged(string selectedKey)
        {
            foreach (string key in Simulators.Keys)
            {
                if (!key.Equals(selectedKey))
                {
                    Simulators[key].isSelected = false;
                }
            }
            NotifyPropertyChanged(nameof(Simulators));
        }
    }
}
