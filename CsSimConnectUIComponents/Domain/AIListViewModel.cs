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
using static CsSimConnect.Util.StringUtil;
using NSwag.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rakis.Logging;
using CsSimConnect.Reflection;
using CsSimConnect.DataDefs.Standard;

namespace CsSimConnect.UIComponents.Domain
{

    public class VehicleTitle
    {

        [DataDefinition("TITLE", Type = DataType.String256)]
        public string Title { get; set; }
    }

    public class VehicleData
    {
        [DataObjectId]
        public uint ObjectId { get; set; }

        [DataDefinition("TITLE", Type = DataType.String256)]
        public string Title { get; set; }

        [DataDefinition("ATC ID", Type = DataType.String32)]
        public string Id { get; set; }

        [DataDefinition("ATC AIRLINE", Type = DataType.String32)]
        public string Airline { get; set; }

        [DataDefinition("ATC FLIGHT NUMBER", Type = DataType.String8)]
        public string FlightNumber { get; set; }
    }

    public class SelectionFilter
    {
        private AIListViewModel model;
        public string Name { get; init; }
        private bool isSelected;
        public bool IsSelected {
            get => isSelected;
            set { isSelected = value; model.MarkSelectionChanged(); }
        }

        public SelectionFilter(AIListViewModel model, string name, bool isSelected = true)
        {
            this.model = model;
            Name = name;
            this.isSelected = isSelected;
        }
    }

    public class AIListViewModel : ViewModelBase
    {

        private static readonly Logger log = Logger.GetLogger(typeof(AIListViewModel));

        private readonly Dictionary<UInt32, SimulatedObject> _dictionary = new();
        public ObservableDictionary<UInt32, SimulatedObject> AIList { get; init; }

        public SelectionFilter ShowAircraft { get; init; }
        public SelectionFilter ShowHelicopters { get; init; }
        public SelectionFilter ShowBoats { get; init; }
        public SelectionFilter ShowGroundVehicles { get; init; }
        public SelectionFilter ShowAnimals { get; init; }
        public SelectionFilter ShowAvatars { get; init; }
        public SelectionFilter ShowBlimps { get; init; }
        public SelectionFilter ShowViewers { get; init; }
        public bool SelectionChanged { get; private set; }

        private Action<Action> uiUpdater;

        public bool SimConnected => SimConnect.Instance.IsConnected;

        public AIListViewModel(Action<Action> updater)
        {
            AIList = new(_dictionary);

            ShowAircraft = new(this, "Aircraft");
            ShowHelicopters = new(this, "Helicopters");
            ShowBoats = new(this, "Boats", false);
            ShowGroundVehicles = new(this, "GroundVehicles", false);
            ShowAnimals = new(this, "Animals", false);
            ShowAvatars = new(this, "Avatars", false);
            ShowBlimps = new(this, "Blimps", false);
            ShowViewers = new(this, "Viewers", false);

            uiUpdater = updater;

            SimConnect.Instance.OnOpen += FillList;
            SimConnect.Instance.OnDisconnect += ClearList;
        }

        public void MarkSelectionChanged()
        {
            SelectionChanged = true;
            NotifyPropertyChanged(nameof(SelectionChanged));
        }

        public void RefreshList()
        {
            if (SimConnect.Instance.IsConnected)
            {
                AIList.Clear();
                FillList();
                SelectionChanged = false;
                uiUpdater(() => NotifyPropertyChanged(nameof(SelectionChanged)));
            }
        }

        private void OnObjectAdded(SimulatedObject obj)
        {
            log.Info?.Log("A {0} was added with id {1}.", obj.GetType().Name, obj.ObjectId);
            Add(obj);
        }

        private void OnObjectRemoved(SimulatedObject obj)
        {
            log.Info?.Log("A {0} was removed with id {1}.", obj.GetType().Name, obj.ObjectId);
            Remove(obj);
        }

        private void ClearList(bool _)
        {
            log.Debug?.Log("Clearing AI-list");
            uiUpdater(() =>
            {
                AIList.Clear();
                NotifyPropertyChanged(nameof(SimConnected));
            });
        }

        private void LoadObjects(ObjectType type)
        {
            DataManager.Instance.RequestDataOnObjectType<VehicleData>(type).Subscribe(data =>
            {
                if (type == ObjectType.Aircraft)
                {
                    SimulatedAircraft aircraft = new($"{data.Title} ({data.ObjectId})", objectId: data.ObjectId);
                    if (IsEmpty(data.Airline))
                    {
                        aircraft.Details = data.Id;
                    }
                    else
                    {
                        aircraft.Details = $"{data.Id} ({data.Airline} flight {data.FlightNumber})";
                    }
                    log.Debug?.Log($"Adding aircraft {aircraft.ObjectId}. Title='{aircraft.Title}', Details='{aircraft.Details}'.");
                    Add(aircraft);
                }
                else
                {
                    SimulatedObject vehicle = new(type, $"{data.Title} ({data.ObjectId})", objectId: data.ObjectId);
                    log.Debug?.Log($"Adding object {vehicle.ObjectId}. Title='{vehicle.Title}'.");
                    Add(vehicle);
                }
            });
        }

        private static ObjectType[] interestingTypes = { ObjectType.Aircraft, ObjectType.Helicopter, ObjectType.Boat, ObjectType.GroundVehicle, ObjectType.Animal, ObjectType.Avatar, ObjectType.Blimp, ObjectType.Viewer };

        private bool IsSelected(ObjectType objectType)
        {
            switch (objectType)
            {
                case ObjectType.Aircraft: return ShowAircraft.IsSelected;
                case ObjectType.Helicopter: return ShowHelicopters.IsSelected;
                case ObjectType.Boat: return ShowBoats.IsSelected;
                case ObjectType.GroundVehicle: return ShowGroundVehicles.IsSelected;
                case ObjectType.Animal: return ShowAnimals.IsSelected;
                case ObjectType.Avatar: return ShowAvatars.IsSelected;
                case ObjectType.Blimp: return ShowBlimps.IsSelected;
                case ObjectType.Viewer: return ShowViewers.IsSelected;
            }
            return false;
        }

        private void FillList(AppInfo _ = null)
        {
            EventManager.Instance.SubscribeToObjectAddedEvent().Subscribe(OnObjectAdded);
            EventManager.Instance.SubscribeToObjectRemovedEvent().Subscribe(OnObjectRemoved);
            foreach (ObjectType objectType in interestingTypes)
            {
                if (IsSelected(objectType))
                {
                    LoadObjects(objectType);
                }
            }
            uiUpdater(() => NotifyPropertyChanged(nameof(SimConnected)));
        }

        public void Add(SimulatedObject obj)
        {
            if (IsSelected(obj.ObjectType))
            {
                uiUpdater(() => {
                    if (!AIList.ContainsKey(obj.ObjectId))
                    {
                        AIList.Add(obj.ObjectId, obj);
                    }
                    else
                    {
                        obj = AIList[obj.ObjectId];
                    }
                    if (IsEmpty(obj.Title))
                    {
                        Task.Run(() => getAIData(obj.ObjectId));
                    }
                });
            }
        }

        public void Remove(SimulatedObject obj)
        {
            uiUpdater(() => AIList.Remove(obj.ObjectId));
        }

        private void getAIData(uint objectId)
        {
            SimulatedObject foundObj = GetFromList(objectId);
            if ((foundObj != null) && IsSelected(foundObj.ObjectType))
            {
                if (foundObj is SimulatedAircraft aircraft)
                {
                    var aircraftData = DataManager.Instance.RequestData<AircraftData>(objectId: objectId).Get();
                    aircraft.Title = aircraftData.Title;
                    aircraft.TailNumber = aircraftData.Id;
                    if (aircraftData.Airline == "")
                    {
                        aircraft.Details = aircraftData.Id;
                    }
                    else
                    {
                        aircraft.Details = $"{aircraftData.Id} ({aircraftData.Airline} flight {aircraftData.FlightNumber})";
                    }
                    log.Debug?.Log("Aircraft data received: {0} {1} is a '{2}', details set to '{3}'", foundObj.ObjectType.ToString(), objectId, aircraftData.Title, aircraft.Details);
                }
                else
                {
                    var title = DataManager.Instance.RequestData<VehicleTitle>(objectId: objectId).Get().Title;
                    foundObj.Title = title;
                    log.Debug?.Log("Vehicle title received: {0} {1} is a '{2}'", foundObj.ObjectType.ToString(), objectId, title);
                }
                uiUpdater?.Invoke(() => Update(objectId, foundObj));
            }
        }

        private SimulatedObject GetFromList(uint objectId) => AIList.ContainsKey(objectId) ? AIList[objectId] : null;

        public void Update(uint objectId, SimulatedObject update)
        {
            AIList.Remove(objectId);
            if (IsSelected(update.ObjectType))
            {
                AIList.Add(objectId, update);
            }
            NotifyPropertyChanged(nameof(AIList));
        }
    }
}