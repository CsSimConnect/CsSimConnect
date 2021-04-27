using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Collections;

namespace CsSimConnect
{
    public class SimConnectObserver : IDisposable
    {
        public UInt32 SendID { get; init; }

        private readonly bool streamable;
        public bool IsStreamable => streamable;

        private bool completed = false;
        public void Completed(bool completed) { this.completed = completed; }
        public bool IsCompleted => completed;

        public Exception Error { get; private set; }

        public Action Cleanup { private get; set; }

        internal SimConnectObserver(UInt32 sendID, bool streamable = false)
        {
            this.SendID = sendID;
            this.streamable = streamable;
            this.Cleanup = null;
        }

        public virtual void OnNext(SimConnectMessage msg)
        {
            throw new NotImplementedException();
        }

        public virtual void OnCompleted()
        {
            completed = true;
        }

        public virtual void OnError(Exception error)
        {
            completed = true;
            Error = error;
        }

        public virtual void Dispose()
        {
            Cleanup?.Invoke();
        }

        public static SimConnectObserver ErrorResult(UInt32 sendId, Exception error)
        {
            SimConnectObserver result = new(sendId);
            result.OnError(error);
            return result;
        }

    }

    public abstract class SimConnectObserver<T> : SimConnectObserver, IEnumerable<T>, IDisposable
        where T : SimConnectMessage
    {

        private Action<T> callback = null;

        internal SimConnectObserver(UInt32 sendID, bool streamable = false) : base(sendID, streamable)
        {
        }

        override public void OnNext(SimConnectMessage msg)
        {
            callback?.Invoke((T)msg);
        }

        public virtual IEnumerator<T> Subscribe()
        {
            return GetEnumerator();
        }

        public virtual void Subscribe(Action<T> callback)
        {
            this.callback = callback;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public abstract class SimConnectObserverEnumerator<R> : IEnumerator<R>, IDisposable
        {
            public abstract R Current { get; }

            object IEnumerator.Current => Current;

            public abstract bool MoveNext();

            public abstract void Reset();

            public abstract void Dispose();

        }
    }

}
