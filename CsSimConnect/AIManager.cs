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
using CsSimConnect.DataDefs;
using CsSimConnect.Reactive;
using CsSimConnect.Reflection;
using Rakis.Logging;
using System;

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

    public enum GearPosition
    {
        Unknown = 0,
        Up = 1,
        Down = 2
    }

    public class AllGearPositions
    {
        [DataDefinition("GEAR POSITION:0", Type = DataType.Int32)]
        public GearPosition Center { get; set; }

        [DataDefinition("GEAR POSITION:1", Type = DataType.Int32)]
        public GearPosition Left { get; set; }

        [DataDefinition("GEAR POSITION:2", Type = DataType.Int32)]
        public GearPosition Right { get; set; }

        [DataDefinition("GEAR POSITION:3", Type = DataType.Int32)]
        public GearPosition Aux { get; set; }

        public AllGearPositions(GearPosition center, GearPosition left, GearPosition right, GearPosition aux = GearPosition.Unknown)
        {
            Center = center;
            Left = left;
            Right = right;
            Aux = aux;
        }

        public AllGearPositions(GearPosition all)
        {
            Center = all;
            Left = all;
            Right = all;
            Aux = all;
        }
    }

    public class AllGearPercentages
    {
        [DataDefinition("GEAR CENTER POSITION", Units = "Percent over 100", Type = DataType.Int32)]
        public int Center;
        [DataDefinition("GEAR LEFT POSITION", Units = "Percent over 100", Type = DataType.Int32)]
        public int Left;
        [DataDefinition("GEAR RIGHT POSITION", Units = "Percent over 100", Type = DataType.Int32)]
        public int Right;
        [DataDefinition("GEAR TAIL POSITION", Units = "Percent over 100", Type = DataType.Int32, Usage = Usage.GetOnly)]
        public int Tail;
        [DataDefinition("GEAR AUX POSITION", Units = "Percent over 100", Type = DataType.Int32, Usage = Usage.GetOnly)]
        public int Aux;

        public AllGearPercentages(GearPosition all)
        {
            Center = (all == GearPosition.Up) ? 0 : 100;
            Left = (all == GearPosition.Up) ? 0 : 100;
            Right = (all == GearPosition.Up) ? 0 : 100;
        }

        public AllGearPercentages(int center, int left, int right)
        {
            Center = center;
            Left = left;
            Right = right;
        }

        public void Up()
        {
            Center = 0;
            Left = 0;
            Right = 0;
        }

        public void Down()
        {
            Center = 100;
            Left = 100;
            Right = 100;
        }
    }

    public class AIManager : MessageManager
    {

        private static readonly Logger log = Logger.GetLogger(typeof(AIManager));

        private static readonly Lazy<AIManager> lazyInstance = new(() => new AIManager(SimConnect.Instance));

        public static AIManager Instance { get { return lazyInstance.Value; } }

        public readonly AllGearPercentages GearsDown = new(GearPosition.Down);

        private AIManager(SimConnect simConnect) : base("ObjectID", 0, simConnect)
        {
        }

        public MessageResult<ParkedAircraft> Create(ParkedAircraft aircraft)
        {
            log.Info?.Log("Creating a '{0}' at {1}.", aircraft.Title, aircraft.AirportId);
            MessageResult<ParkedAircraft> result = new();
            RequestManager.Instance.CreateParkedAircraft(aircraft).Subscribe(obj => CleanupParkedAircraft(aircraft, obj.ObjectId, result), exc => result.OnError(exc));
            return result;
        }

        private void CleanupParkedAircraft(SimulatedAircraft aircraft, uint objectId, MessageResult<ParkedAircraft> observer)
        {
            log.Info?.Log("Assigned ObjectId {0} to '{1}'.", objectId, aircraft.Title);
            aircraft.ObjectId = objectId;
            if (aircraft.OnGround)
            {
                DataManager.Instance.SetData(objectId, GearsDown);
            }
            observer.OnNext(aircraft);
        }
    }
}
