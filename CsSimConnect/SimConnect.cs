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
using System.Runtime.InteropServices;
using System.Text;

namespace CsSimConnect
{

    public sealed class SimConnect
    {

        private static readonly Lazy<SimConnect> lazyInstance = new Lazy<SimConnect>(() => new SimConnect());

        public static SimConnect Instance {  get { return lazyInstance.Value; } }

        public delegate void ConnectionStateHandler(bool willAutoConnect, bool isConnected);

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsCharArrayToStringBuilder(StringBuilder str, byte[] bytebuf, UInt64 bufLen);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsConnect([MarshalAs(UnmanagedType.LPStr)] string appName, ref IntPtr handle);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsDisconnect(IntPtr handle);

        internal IntPtr handle = IntPtr.Zero;
        public bool UseAutoConnect { get; set; }

        public event ConnectionStateHandler OnConnectionStateChange;

        private SimConnect()
        {
            UseAutoConnect = false;
            SimName = "";
        }

        public bool IsConnected()
        {
            return handle != IntPtr.Zero;
        }

        internal void InvokeConnectionStateChanged()
        {
            OnConnectionStateChange(UseAutoConnect, IsConnected());
        }

        public void Connect()
        {
            EventManager.Instance.Init();

            CsConnect("test", ref handle);
            MessageDispatcher.Instance.Init();
            InvokeConnectionStateChanged();
        }

        public void Disconnect()
        {
            CsDisconnect(handle);
            handle = IntPtr.Zero;
            InvokeConnectionStateChanged();
        }

        public string SimName { get; set; }
        public UInt32 ApplicationVersionMajor { get; set; }
        public UInt32 ApplicationVersionMinor { get; set; }
        public UInt32 ApplicationBuildMajor { get; set; }
        public UInt32 ApplicationBuildMinor { get; set; }

        public string GetSimVersion()
        {
            return String.Format("{0}.{1})", ApplicationVersionMajor, ApplicationVersionMinor);
        }

        public string GetSimNameAndVersion()
        {
            return String.Format("{0} {1}.{2}", SimName, ApplicationVersionMajor, ApplicationVersionMinor);
        }

        public string GetSimNameAndFullVersion()
        {
            return String.Format("{0} {1}.{2} (build {3}.{4})", SimName, ApplicationVersionMajor, ApplicationVersionMinor, ApplicationBuildMajor, ApplicationBuildMinor);
        }

        public UInt32 SimConnectVersionMajor { get; set; }
        public UInt32 SimConnectVersionMinor { get; set; }
        public UInt32 SimConnectBuildMajor { get; set; }
        public UInt32 SimConnectBuildMinor { get; set; }

        public string GetSimConnectVersion()
        {
            return String.Format("{0}.{1}", SimConnectVersionMajor, SimConnectVersionMinor);
        }

        public string GetSimConnectFullVersion()
        {
            return String.Format("{0}.{1} (build {2}.{3})", SimConnectVersionMajor, SimConnectVersionMinor, SimConnectBuildMajor, SimConnectBuildMinor);
        }

        public string StringFromBytes(byte[] bytes, UInt64 len)
        {
            StringBuilder bld = new((int)len);
            CsCharArrayToStringBuilder(bld, bytes, len);
            return bld.ToString();
        }
    }
}
