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
using System.Threading.Tasks;

namespace CsSimConnect
{
    public class DoubleResultException : Exception
    {
        public DoubleResultException() : base("Attempted to complete a MessageResult twice")
        {

        }
    }

    public class SimConnectMessageResult<T> : SimConnectObserver<T>, IMessageResult<T>
        where T : SimConnectMessage
    {

        private T result;
        private readonly TaskCompletionSource<T> future = new();

        public SimConnectMessageResult(UInt32 sendID) : base(sendID)
        {
        }

        override public void OnCompleted()
        {
            if (!future.TrySetResult(result))
            {
                OnError(new DoubleResultException());
            }
        }

        override public void OnNext(SimConnectMessage value)
        {
            result = (T)value;
        }

        override public void OnNext(T value)
        {
            result = value;
        }

        override public void OnError(Exception error)
        {
            future.SetException(error);
        }

        public T Get()
        {
            return future.Task.Result;
        }

        override public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
