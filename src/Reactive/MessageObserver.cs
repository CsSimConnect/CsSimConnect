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

using System;
using System.Collections;
using System.Collections.Generic;

namespace CsSimConnect.Reactive
{
    public class MessageVoidObserver : IMessageVoidObserver
    {

        private readonly bool streamable;
        public bool IsStreamable() => streamable;

        public bool Completed { set; private get; }
        public bool IsCompleted => Completed;


        protected Action callback = null;
        private event Action OnCompleteActions;
        private event Action<Exception> OnErrorActions;
        private event Action CleanupActions;

        public Exception Error { get; private set; }

        internal MessageVoidObserver(bool streamable)
        {
            Completed = false;
            this.streamable = streamable;
        }

        public void OnComplete(Action action)
        {
            OnCompleteActions += action;
        }

        public void OnError(Action<Exception> action)
        {
            OnErrorActions += action;
        }

        public void OnNext(object msg)
        {
            OnNext();
        }

        public virtual void OnNext()
        {
            callback?.Invoke();
        }

        public virtual void OnCompleted()
        {
            Completed = true;
            OnCompleteActions?.Invoke();
        }

        public virtual void OnError(Exception error)
        {
            Completed = true;
            Error = error;
            OnErrorActions?.Invoke(error);
        }

        public virtual IEnumerator Subscribe()
        {
            throw new NotImplementedException();
        }

        public virtual void Subscribe(Action callback, Action<Exception> onError = null, Action onCompleted = null)
        {
            this.callback = callback;
            this.OnErrorActions += onError;
            this.OnCompleteActions += onCompleted;
        }

        public virtual void Dispose()
        {
            CleanupActions?.Invoke();
        }
    }

    public class MessageObserver<T> : IMessageObserver, IMessageObserver<T>
        where T : class
    {

        private readonly bool streamable;
        public bool IsStreamable() => streamable;

        public bool Completed { set; private get; }
        public bool IsCompleted => Completed;


        protected Action<T> callback = null;
        private event Action OnCompleteActions;
        private event Action<Exception> OnErrorActions;
        private event Action CleanupActions;

        public Exception Error { get; private set; }

        internal MessageObserver(bool streamable)
        {
            Completed = false;
            this.streamable = streamable;
        }

        public void OnComplete(Action action)
        {
            OnCompleteActions += action;
        }

        public void OnError(Action<Exception> action)
        {
            OnErrorActions += action;
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
            OnCompleteActions?.Invoke();
        }

        public virtual void OnError(Exception error)
        {
            Completed = true;
            Error = error;
            OnErrorActions?.Invoke(error);
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

        public virtual void Subscribe(Action<T> callback, Action<Exception> onError = null, Action onCompleted = null)
        {
            this.callback = callback;
            this.OnErrorActions += onError;
            this.OnCompleteActions += onCompleted;
        }

        public virtual void Dispose()
        {
            CleanupActions?.Invoke();
        }

    }
}
