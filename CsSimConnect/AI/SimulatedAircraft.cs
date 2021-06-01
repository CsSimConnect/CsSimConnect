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

namespace CsSimConnect.AI
{
    public class SimulatedAircraft : SimulatedObject
    {
        public string TailNumber { get; set; }

        public SimulatedAircraft(string tailNumber = null, string title = null, uint objectId = RequestManager.SimObjectUser)
            : base(ObjectType.Aircraft, title : title, objectId : objectId)
        {
            TailNumber = tailNumber;
        }
    }
}
