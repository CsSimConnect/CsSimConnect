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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CsSimConnect
{

    public sealed class MessageDispatcher
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MessageDispatcher));

        public string Name { get; init; }

        private readonly Dictionary<UInt32, IMessageObserver> MessageObservers = new();
        private readonly Dictionary<UInt32, ArrayList> MessageObserverLobby = new();
        private readonly object observerLock = new();

        public MessageDispatcher(string name)
        {
            Name = name;
        }

        public bool TryGetObserver<T>(UInt32 id, [MaybeNullWhen(false)] out MessageObserver<T> observer, bool removeIfFound =false)
            where T : SimConnectMessage
        {
            log.Debug("Trying to find observer for {0} {1}.", Name, id);

            lock (observerLock)
            {
                IMessageObserver messageObserver;
                bool found = MessageObservers.TryGetValue(id, out messageObserver);
                if (found && !messageObserver.IsStreamable())
                {
                    MessageObservers.Remove(id, out _);
                }
                observer = messageObserver as MessageObserver<T>;
                return found;
            }
        }

        public bool DispatchToObserver<T>(UInt32 id, T msg)
            where T : SimConnectMessage
        {
            log.Debug("Dispatching {0} with {1} {2}.", ((RecvId)msg.Id).ToString(), Name, id);

            lock (observerLock)
            {
                bool found = MessageObservers.TryGetValue(id, out IMessageObserver observer);
                if (found)
                {
                    if (!observer.IsStreamable())
                    {
                        MessageObservers.Remove(id, out _);
                    }

                    log.Trace("Dispatching {0} {1} to observer", Name, id);
                    observer.OnNext(msg);

                    if (!observer.IsStreamable())
                    {
                        observer.OnCompleted();
                    }
                }
                else
                {
                    log.Trace("No observer yet for {0} {1}", Name, id);
                    if (!MessageObserverLobby.TryGetValue(id, out ArrayList waitingValues))
                    {
                        waitingValues = new();
                        MessageObserverLobby.Add(id, waitingValues);
                    }
                    waitingValues.Add(msg);
                }
                return found;
            }
        }

        public void AddObserver<T>(UInt32 id, MessageObserver<T> observer)
            where T : SimConnectMessage
        {
            log.Debug("Adding observer for {0} {1}.", Name, id);

            lock (observerLock)
            {
                if (MessageObservers.ContainsKey(id))
                {
                    log.Error("Attempted to add second observer for {0} {1}", Name, id);
                    return;
                }
                MessageObservers.Add(id, observer);
                if (MessageObserverLobby.Remove(id, out ArrayList waitingList) && (waitingList.Count != 0))
                {
                    foreach (SimConnectMessage msg in waitingList) {
                        log.Trace("Dispatching {0} {1} from lobby to observer", Name, id);
                        observer.OnNext(msg);
                        if (!observer.IsStreamable())
                        {
                            MessageObservers.Remove(id);
                            if (waitingList.Count > 1)
                            {
                                log.Error("Received multiple messages for {0} {1}", Name, id);
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
