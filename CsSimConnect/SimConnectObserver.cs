using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Collections;

namespace CsSimConnect
{
    public abstract class SimConnectObserver
    {
        public UInt32 SendID { get; init; }

        internal SimConnectObserver(UInt32 sendID)
        {
            SendID = sendID;
        }

        public abstract void OnCompleted();
        public abstract void OnError(Exception error);
    }

}
