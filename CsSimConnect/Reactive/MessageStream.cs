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

using Rakis.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsSimConnect.Reactive
{
    public class MessageStream<T> : MessageObserver<T>, IMessageStream<T>
        where T : class
    {

        private static ILogger log = Logger.GetLogger(typeof(MessageStream<T>));

        public uint MaxSize { get; set; }

        private ConcurrentQueue<T> queue = new();
        private T current;
        private bool disposedValue;

        public MessageStream(uint queueSize) : base(true)
        {
            MaxSize = queueSize;
        }

        override public void OnNext(T msg)
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

        public static MessageStream<T> ErrorResult(UInt32 sendId, Exception error)
        {
            MessageStream<T> result = new(0);
            result.OnError(error);
            return result;
        }

    }
}
