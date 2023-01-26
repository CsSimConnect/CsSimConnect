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

using CsSimConnect.Events;
using Rakis.Logging;
using System.Windows.Controls;

namespace CsSimConnect.UIComponents
{
    public class ClientEventButton : Button
    {

        private readonly static ILogger log = Logger.GetLogger(typeof(ClientEventButton));

        private string clientEventName;
        private ClientEvent clientEvent;
        public string ClientEvent
        {
            get => clientEventName;
            set
            {
                clientEventName = value;
                clientEvent = EventManager.GetEvent(clientEventName);
            }
        }

        public bool CurrentState { get; set; }

        private string clientOnEventName;
        private ClientEvent clientOnEvent;
        public string ClientOnEvent
        {
            get => clientOnEventName;
            set
            {
                clientOnEventName = value;
                clientOnEvent = EventManager.GetEvent(clientOnEventName);
            }
        }

        private string clientOffEventName;
        private ClientEvent clientOffEvent;
        public string ClientOffEvent
        {
            get => clientOffEventName;
            set
            {
                clientOffEventName = value;
                clientOffEvent = EventManager.GetEvent(clientOffEventName);
            }
        }

        public void ClearEvents()
        {
            ClientEvent = null;
            ClientOnEvent = null;
            ClientOffEvent = null;
        }

        protected override void OnClick()
        {
            if (clientEvent != null)
            {
                log.Debug?.Log($"Sending '{clientEventName}'");
                clientEvent?.Send();
            }
            if (CurrentState && (clientOffEventName != null))
            {
                log.Debug?.Log($"Sending '{clientOffEventName}' to disable SPEED mode");
                clientOffEvent?.Send();
            }
            else if ((!CurrentState) && (clientOnEventName != null))
            {
                log.Debug?.Log($"Sending '{clientOnEventName}' to enable SPEED mode");
                clientOnEvent?.Send();
            }

            base.OnClick();
        }

    }
}
