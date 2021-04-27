using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public class DoubleResultException : Exception
    {
        public DoubleResultException() : base("Attempted to complete a MessageResult twice")
        {

        }
    }

    public class MessageResult<T> : SimConnectObserver<T>, IObserver<T>
        where T : SimConnectMessage
    {

        private T result;
        private readonly TaskCompletionSource<T> future = new();

        public MessageResult(UInt32 sendID) : base(sendID)
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

        public void OnNext(T value)
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

        public class ResultEnumerator<R> : SimConnectObserverEnumerator<R>, IDisposable
            where R : SimConnectMessage
        {

            private readonly MessageResult<R> result;

            public ResultEnumerator(MessageResult<R> result)
            {
                this.result = result;
            }

            public override R Current => result.result;

            public override void Dispose()
            {
                // DONOTHING
            }

            public override bool MoveNext()
            {
                if (result.IsCompleted)
                {
                    return false;
                }
                result.future.Task.Wait();
                return result.IsCompleted;
            }

            public override void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
