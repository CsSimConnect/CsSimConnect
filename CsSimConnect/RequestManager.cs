using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public class RequestManager
    {

        private static readonly Lazy<RequestManager> lazyInstance = new Lazy<RequestManager>(() => new RequestManager(SimConnect.Instance));

        public static RequestManager Instance { get { return lazyInstance.Value; } }

        private SimConnect simConnect;

        private RequestManager(SimConnect simConnect)
        {
            this.simConnect = simConnect;
        }

        private UInt32 nextRequest = 0;

        public UInt32 NextRequest()
        {
            return Interlocked.Increment(ref nextRequest);
        }

    }
}
