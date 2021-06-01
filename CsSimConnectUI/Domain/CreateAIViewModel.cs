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
                Validate();
                NotifyPropertyChanged(nameof(ObjectType));
            }
        }

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                Validate();
                NotifyPropertyChanged(nameof(Title));
            }
        }

        private string tailNumber;
        public string TailNumber
        {
            get => tailNumber;
            set
            {
                tailNumber = value;
                Validate();
                NotifyPropertyChanged(nameof(TailNumber));
            }
        }

        private string airportId;
        public string AirportId
        {
            get => airportId;
            set
            {
                airportId = value;
                Validate();
                NotifyPropertyChanged(nameof(AirportId));
            }
        }

        public bool Validated { get; private set; }
        private void Validate()
        {
            bool oldOk = Validated;
            Validated = !IsEmpty(Title) && !IsEmpty(TailNumber) && !IsEmpty(AirportId);
            if (oldOk != Validated)
            {
                NotifyPropertyChanged(nameof(Validated));
            }
        }

        public ObservableCollection<ObjectTypeSelector> ObjectTypes { get; init; }

        public CreateAIViewModel()
        {
            ObjectType = ObjectType.Aircraft;

            ObjectTypes = new();
            ObjectTypes.Add(new(ObjectType.Boat));
            ObjectTypes.Add(new(ObjectType.Helicopter));
            ObjectTypes.Add(new(ObjectType.Aircraft));
            ObjectTypes.Add(new(ObjectType.GroundVehicle));
        }
    }
}
