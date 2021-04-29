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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        }

        public uint NextId()
        {
            return Interlocked.Increment(ref nextId);
        }

        public void DispatchResult(uint id, SimConnectMessage msg)
        {
            if (!dispatcher.DispatchToObserver(id, msg))
            {
                log.Error("Received a message with unknown {0} {1}.", dispatcher.Name, id);
            }
        }

        protected SimConnectMessageStream<T> RegisterStreamObserver<T>(uint id, long sendId, string api)
            where T : SimConnectMessage
        {
            SimConnectMessageStream<T> result;
            if (sendId > 0)
            {
                result = new SimConnectMessageStream<T>((uint)sendId, 1);
                dispatcher.AddObserver(id, result);
                simConnect.AddCleanup((uint)sendId, (Exception _) => dispatcher.Remove(id));
            }
            else
            {
                var msg = String.Format("Call to {0} failed. (HRETURN=0x{1:X8})", api, sendId);
                log.Error(msg);
                result = (SimConnectMessageStream<T>)SimConnectMessageObserver.ErrorResult(0, new SimConnectException(msg));
            }
            return result;
        }

        protected SimConnectMessageResult<T> RegisterResultObserver<T>(uint id, long sendId, string api)
            where T : SimConnectMessage
        {
            SimConnectMessageResult<T> result;
            if (sendId > 0)
            {
                result = new SimConnectMessageResult<T>((uint)sendId);
                dispatcher.AddObserver(id, result);
                simConnect.AddCleanup((uint)sendId, (Exception _) => dispatcher.Remove(id));
            }
            else
            {
                var msg = String.Format("Call to {0} failed. (HRETURN=0x{1:X8})", api, sendId);
                log.Error(msg);
                result = (SimConnectMessageResult<T>)SimConnectMessageObserver.ErrorResult(0, new SimConnectException(msg));
            }
            return result;
        }

    }
}
