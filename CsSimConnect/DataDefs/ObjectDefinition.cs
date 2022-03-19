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

using Rakis.Logging;
using System.Threading;

namespace CsSimConnect.DataDefs
{

    public enum DataType
    {
        Invalid,

        Int32,
        Int64,
        Float32,
        Float64,

        String8,
        String32,
        String64,
        String128,
        String256,
        String260,
        StringV,

        InitPosition,
        MarkerState,
        Waypoint,
        LatLonAlt,
        XYZ,
        PBH,
        Observer,
        VideoStreamInfo,

        WString8,
        WString32,
        WString64,
        WString128,
        WString256,
        WString260,
        WStringV,

        Max,
    }

    public class ObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(ObjectDefinition));

        public const uint UNSET_DATASIZE = 0;

        internal static readonly uint[] DataSize =
        {
            UNSET_DATASIZE, // Invalid,

            4, // Int32,
            8, // Int64,
            4, // Float32,
            8, // Float64,

            8, // String8,
            32, // String32,
            64, // String64,
            128, // String128,
            256, // String256,
            260, // String260,
            0, // StringV,

            56, // InitPosition,
            68, // MarkerState,
            44, // Waypoint,
            24, // LatLonAlt,
            24, // XYZ,
            24, // PBH,
            80, // Observer, 32 + sizeof(LatLonAlt) + sizeof(PBH)
            120, // VideoStreamInfo,

            16, // WString8,
            64, // WString32,
            128, // WString64,
            256, // WString128,
            512, // WString256,
            520, // WString260,
            0, // WStringV,

            0, // Max,

        };

        /// <value>The <c>SIMCONNECT_DATA_DEFINITION_ID</c> for the data block described by this <c>ObjectDefinition</c></ObjectDefinition></value>
        public uint DefinitionId { get; private set; }

        /// <value>The size of the SimConnect Data Definition block.</value>
        public uint TotalSize { get; set; }

        private uint isDefined = 0;
        public bool Defined => isDefined != 0;

        public ObjectDefinition()
        {
            DefinitionId = DataManager.Instance.NextId();
        }

        /**
         * <summary>Override this method with the code to add all known fields for the data block.</summary>
         */
        public virtual void DefineFields()
        {
            // No fields
        }

        /**
         * <summary>Register this data block with the simulator by calling <see cref="DefineFields"/> at most once.</summary>
         */
        public void DefineObject()
        {
            if (1 != Interlocked.Exchange(ref isDefined, 1))
            {
                DefineFields();
            }
        }

        /**
         * <summary>Mark this data block for definition, usually because fields have been added.</summary>
         */
        public void ReDefineObject()
        {
            isDefined = 0;
        }
    }
}