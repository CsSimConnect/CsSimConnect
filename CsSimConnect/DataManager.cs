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

using CsSimConnect.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public sealed class DataManager
    {

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsAddToDataDefinition(IntPtr handle, UInt32 defId, [MarshalAs(UnmanagedType.LPStr)] string datumName, [MarshalAs(UnmanagedType.LPStr)] string UnitsName, uint datumType, float epsilon, UInt32 datumId);

        private static readonly Logger log = Logger.GetLogger(typeof(DataManager));

        private static readonly Lazy<DataManager> lazyInstance = new(() => new DataManager(SimConnect.Instance));

        public static DataManager Instance { get { return lazyInstance.Value; } }

        private readonly SimConnect simConnect;

        private DataManager(SimConnect simConnect)
        {
            this.simConnect = simConnect;
        }

        private UInt32 nextId = 1;

        public UInt32 NextId()
        {
            return Interlocked.Increment(ref nextId);
        }

        private readonly Dictionary<Type, UInt32> registeredTypes = new();
        private readonly Dictionary<UInt32, ObjectDefinition> definedTypes = new();

        internal ObjectDefinition GetObjectDefinition(Type type)
        {
            if (registeredTypes.TryGetValue(type, out UInt32 defId) && definedTypes.TryGetValue(defId, out ObjectDefinition def))
            {
                return def;
            }
            def = new(type);
            definedTypes.Add(def.DefinitionId, def);
            registeredTypes.Add(type, def.DefinitionId);
            return def;
        }

        private void BuildDataDefinition(ObjectDefinition def)
        {
            if (def.IsDefined)
            {
                return;
            }

        }
        public T RequestData<T>()
            where T : class
        {
            Type t = typeof(T);
            ObjectDefinition def = GetObjectDefinition(t);
            return null;
        }

        internal void AddToDefinition(UInt32 defId, ObjectDefinition.DataDefInfo info)
        {
            log.Debug("AddToDataDefinition(..., {0}, '{1}', '{2}', {3}, {4}, {5})", defId, info.Definition.Name, info.Definition.Units, info.Definition.Type.ToString(), info.Definition.Epsilon, info.Definition.Tag);
            //CsAddToDataDefinition(simConnect.handle, defId, info.Definition.Name, info.Definition.Units, (uint)info.Definition.Type, info.Definition.Epsilon, info.Definition.Tag);
        }
    }
}
