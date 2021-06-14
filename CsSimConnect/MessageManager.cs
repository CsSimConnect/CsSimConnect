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

using CsSimConnect.Exc;
using CsSimConnect.Reactive;
using System;
using System.Threading;

namespace CsSimConnect
{
    public class MessageManager
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MessageManager));

        private readonly MessageDispatcher dispatcher;
        private uint nextId;
        protected readonly SimConnect simConnect;

        protected MessageManager(string dispatcherType, uint firstId, SimConnect simConnect)
        {
            dispatcher = new(dispatcherType);
            nextId = firstId;
            this.simConnect = simConnect;
            simConnect.OnDisconnect += ClearDispatcher;
        }

        protected void ClearDispatcher(bool connectionLost)
        {
            dispatcher.Clear(connectionLost);
        }

        public uint NextId()
        {
            return Interlocked.Increment(ref nextId);
        }

        public void DispatchResult(uint id, SimConnectMessage msg)
        {
            if (!dispatcher.DispatchToObserver(id, msg))
            {
                log.Error?.Log("Received a message with unknown {0} {1}.", dispatcher.Name, id);
            }
        }

        protected MessageStream<T> RegisterStreamObserver<T>(uint id, long sendId, string api)
            where T : SimConnectMessage
        {
            MessageStream<T> result;
            if (sendId > 0)
            {
                result = new MessageStream<T>(1);
                result.OnComplete(() => NormalCleanup(api, id));
                dispatcher.AddObserver(id, result);
                simConnect.AddCleanup((uint)sendId, (SimConnectException exc) => ErrorCleanup(api, id, exc));
            }
            else
            {
                var msg = String.Format("Call to {0} failed. (HRETURN=0x{1:X8})", api, sendId);
                log.Error?.Log(msg);
                result = MessageStream<T>.ErrorResult(0, new SimConnectException(msg));
            }
            return result;
        }

        protected MessageResult<T> RegisterResultObserver<T>(uint id, long sendId, string api)
            where T : SimConnectMessage
        {
            MessageResult<T> result;
            if (sendId > 0)
            {
                result = new MessageResult<T>();
                result.OnComplete(() => NormalCleanup(api, id));
                dispatcher.AddObserver(id, result);
                simConnect.AddCleanup((uint)sendId, (SimConnectException exc) => { ErrorCleanup(api, id, exc); result.OnError(exc); });
            }
            else
            {
                string msg = String.Format("Call to {0} failed. (HRETURN=0x{1:X8})", api, sendId);
                log.Error?.Log(msg);
                result = MessageResult<T>.ErrorResult(0, new SimConnectException(msg));
            }
            return result;
        }

        protected void RegisterCleanup(long sendId, string api, Action<SimConnectException> callback)
        {
            if (sendId > 0)
            {
                simConnect.AddCleanup((uint)sendId, callback);
            }
            else
            {
                log.Error?.Log($"Call to {api} failed. (HRETURN={sendId})");
            }
        }

        private void NormalCleanup(string api, uint id)
        {
            dispatcher.Remove(id);
            log.Debug?.Log("Handling of {0} {1} after calling '{2}' completed", dispatcher.Name, id, api);
        }

        private void ErrorCleanup(string api, uint id, SimConnectException exc)
        {
            dispatcher.Remove(id);
            log.Error?.Log("Exception returned for {0} {1} after calling '{2}': {3}", dispatcher.Name, id, api, exc.Message);
        }
    }
}
