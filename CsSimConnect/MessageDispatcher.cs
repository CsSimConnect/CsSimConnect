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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect
{

    public sealed class MessageDispatcher
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MessageDispatcher));

        private readonly string name;

        private readonly Dictionary<UInt32, SimConnectObserver> MessageObservers = new();
        private readonly Dictionary<UInt32, LinkedList<SimConnectMessage>> MessageObserverLobby = new();
        private readonly object observerLock = new();

        public MessageDispatcher(string name)
        {
            this.name = name;
        }

        public bool TryGetObserver(UInt32 id, [MaybeNullWhen(false)] out SimConnectObserver<SimConnectMessage> observer, bool removeIfFound =false)
        {
            log.Debug("Trying to find observer for {0} {1}.", name, id);

            lock (observerLock)
            {
                SimConnectObserver messageObserver;
                bool found = MessageObservers.TryGetValue(id, out messageObserver);
                if (found && !messageObserver.IsStreamable)
                {
                    MessageObservers.Remove(id, out _);
                }
                observer = messageObserver as SimConnectObserver<SimConnectMessage>;
                return found;
            }
        }

        public bool DispatchToObserver(UInt32 id, SimConnectMessage msg)
        {
            log.Debug("Dispatching {0} with {1} {2}.", ((RecvId)msg.Id).ToString(), name, id);

            lock (observerLock)
            {
                bool found = MessageObservers.TryGetValue(id, out SimConnectObserver observer);
                if (found)
                {
                    if (!observer.IsStreamable)
                    {
                        MessageObservers.Remove(id, out _);
                    }

                    observer.OnNext(msg);

                    if (!observer.IsStreamable)
                    {
                        observer.OnCompleted();
                    }
                }
                else
                {
                    if (!MessageObserverLobby.TryGetValue(id, out LinkedList<SimConnectMessage> waitingValues))
                    {
                        waitingValues = new();
                        MessageObserverLobby.Add(id, waitingValues);
                    }
                    waitingValues.AddLast(msg);
                }
                return found;
            }
        }

        public void AddObserver(UInt32 id, SimConnectObserver observer)
        {
            log.Debug("Adding observer for {0} {1}.", name, id);

            lock (observerLock)
            {
                if (MessageObservers.ContainsKey(id))
                {
                    log.Error("Attempted to add second observer for {0} {1}", name, id);
                    return;
                }
                MessageObservers.Add(id, observer);
                if (MessageObserverLobby.Remove(id, out LinkedList<SimConnectMessage> waitingList) && (waitingList.Count != 0))
                {
                    var messageObserver = observer as SimConnectObserver<SimConnectMessage>;
                    foreach (SimConnectMessage msg in waitingList) {
                        messageObserver.OnNext(msg);
                        if (!messageObserver.IsStreamable)
                        {
                            MessageObservers.Remove(id);
                            if (waitingList.Count > 1)
                            {
                                log.Error("Received multiple messages for {0} {1}", name, id);
                            }
                            break;
                        }
                    }
                }
            }
        }

        internal void Remove(uint id)
        {
            lock (observerLock)
            {
                MessageObservers.Remove(id);
                MessageObserverLobby.Remove(id);
            }
        }
    }
}
