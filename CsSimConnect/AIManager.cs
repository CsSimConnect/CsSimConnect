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

using CsSimConnect.AI;
using System;
using System.Runtime.InteropServices;

namespace CsSimConnect
{
    public enum ObjectType
    {
        User,
        All,
        Aircraft,
        Helicopter,
        Boat,
        GroundVehicle,
        Weapon,
        Countermeasure,
        Animal,
        Avatar,
        Blimp,
        ControlTower,
        ExternalSim,
        SimpleObject,
        Submersible,
        Viewer,
    }

    public class AIManager : MessageManager
    {

        private static readonly Logger log = Logger.GetLogger(typeof(AIManager));

        private static readonly Lazy<AIManager> lazyInstance = new(() => new AIManager(SimConnect.Instance));

        public static AIManager Instance { get { return lazyInstance.Value; } }

        private AIManager(SimConnect simConnect) : base("ObjectID", 0, simConnect)
        {
        }

        public void Create(ParkedAircraft aircraft)
        {
            log.Info?.Log("Creating a '{0}' at {1}.", aircraft.Title, aircraft.AirportId);
            RequestManager.Instance.CreateParkedAircraft(aircraft).Subscribe(obj => aircraft.ObjectId = obj.ObjectId);
        }
    }
}
