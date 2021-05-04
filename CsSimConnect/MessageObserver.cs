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

namespace CsSimConnect
{
    public class MessageObserver<T> : IMessageObserver, IMessageObserver<T>
        where T : class
    {

        private readonly bool streamable;
        public bool IsStreamable() => streamable;

        public bool Completed { set; private get; }
        public bool IsCompleted() => Completed;

        protected Action<T> callback = null;

        public Exception Error { get; private set; }

        public Action Cleanup { private get; set; }

        internal MessageObserver(bool streamable)
        {
            Completed = false;
            this.streamable = streamable;
            Cleanup = null;
        }

        public virtual void OnNext(T msg)
        {
            callback?.Invoke(msg);
        }

        public void OnNext(object msg)
        {
            OnNext((T)msg);
        }

        public virtual void OnCompleted()
        {
            Completed = true;
        }

        public virtual void OnError(Exception error)
        {
            Completed = true;
            Error = error;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<T> Subscribe()
        {
            throw new NotImplementedException();
        }

        public virtual void Subscribe(Action<T> callback)
        {
            this.callback = callback;
        }

        public virtual void Dispose()
        {
            Cleanup?.Invoke();
        }

    }
}
