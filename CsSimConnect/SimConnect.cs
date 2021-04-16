using System;
using System.Runtime.InteropServices;

namespace CsSimConnect
{

    public class SimConnect
    {

        public delegate void ConnectionStateHandler(bool isConnected);

        [DllImport("CsSimConnectInterOp.dll", EntryPoint ="#2")]
        private static extern bool CsConnect([MarshalAs(UnmanagedType.LPStr)] string appName, ref IntPtr handle);
        [DllImport("CsSimConnectInterOp.dll", EntryPoint = "#3")]
        private static extern bool CsDisconnect(IntPtr handle);

        private IntPtr handle = IntPtr.Zero;
        public IntPtr Handle { get; }

//        private MessageDispatcher dispatcher = null;
        public MessageDispatcher Dispatcher { get; set; }
//        private EventManager events = null;
        public EventManager Events { get; set; }
        public event ConnectionStateHandler OnConnectionStateChange;

        public void Connect()
        {
            if (CsConnect("test", ref handle))
            {
                Dispatcher = new MessageDispatcher(this);
                Events = new EventManager(this);
                OnConnectionStateChange.Invoke(true);
            }
        }

        public void Disconnect()
        {
            CsDisconnect(handle);
            handle = IntPtr.Zero;
            OnConnectionStateChange.Invoke(false);
        }

        public bool IsConnected()
        {
            return handle != IntPtr.Zero;
        }
    }
}
