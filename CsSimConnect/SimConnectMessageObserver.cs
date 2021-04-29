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

namespace CsSimConnect
{
    public class SimConnectMessageObserver : MessageObserver<SimConnectMessage>
    {
        public uint SendID { get; init; }

        internal SimConnectMessageObserver(UInt32 sendID, bool streamable = false) : base(streamable)
        {
            SendID = sendID;
        }

        public static SimConnectMessageObserver ErrorResult(UInt32 sendId, Exception error)
        {
            SimConnectMessageObserver result = new(sendId);
            result.OnError(error);
            return result;
        }

    }

    public abstract class SimConnectObserver<T> : SimConnectMessageObserver, IMessageObserver<T>
        where T : SimConnectMessage
    {

        internal SimConnectObserver(UInt32 sendID, bool streamable = false) : base(sendID, streamable)
        {
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public virtual void OnNext(T value)
        {
            throw new NotImplementedException();
        }
    }

}
