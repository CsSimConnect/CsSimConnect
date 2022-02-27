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

using CsSimConnect.DataDefs;
using System;

namespace CsSimConnect.Reflection
{
    /// <summary>Annotate a field or property to receive Simulator variables</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataDefinition : DefinitionAttribute
    {
        /// <value>The units for the variable. The simulator must be able to provide a default conversion.</value>
        public string Units { get; set; }

        /// <value>The <c>SIMCONNECT_DATA_TYPE</c> to be used for transferring the variable's value.</value>
        public DataType Type { get; set; }

        /// <value>The smallest change that should be considered when setting <c>onlyWhenChanged</c> to <c>true</c>.</value>
        public float Epsilon { get; set; }

        /// <value>The size of the variable, if relevant.</value>
        public uint Size { get; set; }

        public DataDefinition(string name) : base(name)
        {
            Name = name;
            Units = "NULL"; // Default for strings and structs
            Type = DataType.Float64;
            Epsilon = 0.0f;
        }
    }
}
