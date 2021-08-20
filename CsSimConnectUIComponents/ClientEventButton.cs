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
using System.Windows.Controls;

namespace CsSimConnect.UIComponents
{
    public class ClientEventButton : Button
    {
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

        protected override void OnClick()
        {
            clientEvent?.Send();
            base.OnClick();
        }

    }
}
