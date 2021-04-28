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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public class MessageStream<T> : SimConnectObserver<T>, IObserver<T>, IAsyncEnumerator<T>
        where T : SimConnectMessage
    {

        private static Logger log = Logger.GetLogger(typeof(MessageStream<T>));

        public uint MaxSize { get; set; }

        private ConcurrentQueue<T> queue = new();
        private T current = null;
        private bool disposedValue;

        public MessageStream(uint sendID, uint queueSize) : base(sendID, true)
        {
            MaxSize = queueSize;
        }

        override public void OnNext(SimConnectMessage msg)
        {
            base.OnNext(msg);
        }

        public void OnNext(T msg)
        {
            base.OnNext(msg);
        }

        public T Current => throw new NotImplementedException();

        ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            throw new NotImplementedException();
        }

    }
}
