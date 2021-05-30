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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CsSimConnectUI
{
    public class ObjectTypeSelector
    {
        public ObjectType ObjectType { get; set; }
        public string Name { get; set; }
        private static readonly Dictionary<ObjectType, string> iconNames = new()
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
        public string IconName => iconNames.GetValueOrDefault(ObjectType, "Help");

        public ObjectTypeSelector(ObjectType type)
        {
            ObjectType = type;
            Name = type.ToString();
        }
    }

    public class CreateAIViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }
        public ObjectType ObjectType;
        public ObservableCollection<ObjectTypeSelector> ObjectTypes { get; init; }

        public CreateAIViewModel()
        {
            ObjectTypes = new();
            ObjectTypes.Add(new(ObjectType.Boat));
            ObjectTypes.Add(new(ObjectType.Helicopter));
            ObjectTypes.Add(new(ObjectType.Aircraft));
            ObjectTypes.Add(new(ObjectType.GroundVehicle));
        }
    }
}
