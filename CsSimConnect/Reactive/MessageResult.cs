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
using System.Threading.Tasks;

namespace CsSimConnect.Reactive
{
    public class DoubleResultException : Exception
    {
        public DoubleResultException() : base("Attempted to complete a MessageResult twice")
        {

        }
    }

    public class MessageResult<T> : MessageObserver<T>, IMessageResult<T>
        where T : class
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MessageResult<T>));

        private readonly TaskCompletionSource<T> future = new();

        public MessageResult() : base(false)
        {
        }

        override public void OnCompleted()
        {
        }

        override public void OnNext(T value)
        {
            if (future.TrySetResult(value))
            {
                base.OnNext(value);
                base.OnCompleted();
            }
            else
            {
                OnError(new DoubleResultException());
            }
        }

        override public void OnError(Exception error)
        {
            if (!future.TrySetException(error))
            {
                log.Error?.Log("Ignoring Exception '{0}', because we are already in an exceptional state.", error.Message);
            }
            base.OnError(error);
        }

        public T Get()
        {
            return future.Task.Result;
        }

        public static MessageResult<T> ErrorResult(UInt32 sendId, Exception error)
        {
            MessageResult<T> result = new();
            result.OnError(error);
            return result;
        }

    }
}
