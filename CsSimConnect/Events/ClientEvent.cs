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

using CsSimConnect.Exc;
using Rakis.Logging;
using System;
using System.Collections.Generic;

namespace CsSimConnect.Events
{
    public class ClientEvent
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(ClientEvent));

        public class Comparer : IComparer<ClientEvent>
        {
            public int Compare(ClientEvent x, ClientEvent y)
            {
                return x.Id.CompareTo(y.Id);
            }
        }

        private readonly EventManager mgr;
        public uint Id { get; init; }
        public string MappedEvent { get; init; }
        public EventGroup Group { get; internal set; }

        internal bool IsMapped { get; set; }

        internal ClientEvent(EventManager mgr, string mappedEvent)
        {
            Id = EventManager.Instance.NextId();
            this.mgr = mgr;
            MappedEvent = mappedEvent;
            IsMapped = false;
        }

        public void Send(uint objectId = 0, uint data = 0, Action<SimConnectException> onError = null)
        {
            mgr.SendEvent(this, objectId, data, onError ?? LogError);
        }

        public void SendSigned(uint objectId = 0, int data = 0, Action<SimConnectException> onError = null)
        {
            mgr.SendEventSigned(this, objectId, data, onError ?? LogError);
        }

        private void LogError(SimConnectException exc)
        {
            log.Error?.Log($"Exception: {exc.Message}");
        }
    }
}
