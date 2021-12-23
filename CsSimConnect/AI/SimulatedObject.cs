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

using System.Collections.Generic;
using CsSimConnect.DataDefs;

namespace CsSimConnect.AI
{
    public class SimulatedObject
    {
        public ObjectType ObjectType { get; init; }
        public string Title { get; set; }
        public string Details { get; set; }
        public uint ObjectId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Pitch { get; set; }
        public double Bank { get; set; }
        public double Heading { get; set; }

        public bool OnGround { get; set; }
        public int AirSpeed { get; set; }

        public SimulatedObject(ObjectType objectType, string title = null, uint objectId = RequestManager.SimObjectUser)
        {
            ObjectType = objectType;
            Title = title;
            ObjectId = objectId;
        }

        private static readonly Dictionary<ObjectType, string> iconNames = new(){
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
    }
}
