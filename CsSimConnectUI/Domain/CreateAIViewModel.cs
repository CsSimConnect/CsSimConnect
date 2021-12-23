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
using Rakis.Settings;
using SimScanner.Model;
using SimScanner.Sim;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static CsSimConnect.Util.StringUtil;

namespace CsSimConnectUI.Domain
{
    public class ObjectTypeSelector : ViewModelBase
    {
        private ObjectType objectType;
        public ObjectType ObjectType {
            get => objectType;
            set
            {
                objectType = value;
                NotifyPropertyChanged(nameof(ObjectType));
            }
        }

        private string name;
        public string Name {
            get => name;
            set
            {
                name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        private static readonly Dictionary<ObjectType, string> IconNames = new()
        {
            [ObjectType.User] = "Account",
            [ObjectType.Aircraft] = "Airplane",
            [ObjectType.Helicopter] = "Helicopter",
            [ObjectType.Boat] = "Ferry",
            [ObjectType.GroundVehicle] = "Truck",
            [ObjectType.Animal] = "Cow",
            [ObjectType.Avatar] = "Account",
            [ObjectType.Blimp] = "Airballoon",
            [ObjectType.ControlTower] = "TowerFire",
            [ObjectType.Submersible] = "Submarine",
            [ObjectType.Viewer] = "AccountEye",
        };

        public static string GetIconName(ObjectType objectType)
        {
            return IconNames.GetValueOrDefault(objectType, "Help");
        }

        public string IconName => GetIconName(ObjectType);

        public ObjectTypeSelector(ObjectType type)
        {
            ObjectType = type;
            Name = type.ToString();
        }
    }

    public class CreateAIViewModel : ViewModelBase
    {

        private ObjectType objectType;
        public ObjectType ObjectType
        {
            get => objectType;
            set
            {
                objectType = value;
                NotifyPropertyChanged(nameof(ObjectType));
            }
        }

        public List<string> Titles { get; }
        public List<string> ICAOList { get; }
        public List<string> Parkings { get; } = new();
        public Airport Airport { get; set; }
        public Parking Parking { get; set; }

        public ObservableCollection<ObjectTypeSelector> ObjectTypes { get; init; }

        public static readonly  Context DBContext = new("CsSimConnect", "SimScanner");

        private SceneryManager sceneryManager;

        public void LoadParkings(string icao)
        {
            List<int> layers = sceneryManager.GetLayersForICAO(icao);
            if (layers.Count == 0)
            {
                return;
            }
            Parkings.Clear();

            Airport = sceneryManager.GetAirport(layers[0], icao);
            if (Airport.Parkings.Count != 0)
            {
                foreach (string name in Airport.Parkings.Keys)
                {
                    Parkings.Add(name);
                }
            }
            NotifyPropertyChanged(nameof(Parkings));
        }

        public CreateAIViewModel()
        {
            ObjectType = ObjectType.Aircraft;

            ObjectTypes = new();
            ObjectTypes.Add(new(ObjectType.Boat));
            ObjectTypes.Add(new(ObjectType.Helicopter));
            ObjectTypes.Add(new(ObjectType.Aircraft));
            ObjectTypes.Add(new(ObjectType.GroundVehicle));

            Simulator simulator = SimUtil.FromInterOpType(SimConnect.InterOpType);
            AircraftManager aircraftMgr = new(DBContext, simulator);
            Titles = aircraftMgr.ListAllAircraft();

            sceneryManager = new(DBContext, simulator);
            ICAOList = sceneryManager.GetICAOList();
        }

        public void SetSelectedParking(string name)
        {
            Parking = Airport?.Parkings[name];
        }
    }
}
