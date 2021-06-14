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

namespace CsSimConnect.Events
{

    public class EventGroup
    {
        public const uint PriorityHighest = 1;
        public const uint PriorityHighestMaskable = 10000000;
        public const uint PriorityStandard = 1900000000;
        public const uint PriorityDefault = 2000000000;
        public const uint PriorityLowest = 4000000000;

        public string Name { get; init; }
        public uint Id { get; init; }
        public uint Priority { get; init; }

        private readonly SortedSet<ClientEvent> events = new(new ClientEvent.Comparer());
        public SortedSet<ClientEvent> Events => events;

        public EventGroup(string name, uint priority)
        {
            Name = name;
            Id = EventManager.Instance.NextGroupId();
            Priority = priority;
        }
    }
}
