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

    public class MessageResult<T> : SimConnectObserver, IObserver<T>
        where T : SimConnectMessage
    {

        private T result;
        private TaskCompletionSource<T> future;

        public MessageResult(UInt32 sendID) : base(sendID)
        {

        }

        public override void OnCompleted()
        {
            if (!future.TrySetResult(result))
            {
                throw new DoubleResultException();
            }
        }

        void IObserver<T>.OnNext(T value)
        {
            result = value;
        }

        public override void OnError(Exception error)
        {
            future.SetException(error);
        }

        public T Get()
        {
            return future.Task.Result;
        }
    }
}
