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
