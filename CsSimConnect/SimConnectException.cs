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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public class SimConnectException : Exception
    {
        public UInt32 Code { get; init; }
        public UInt32? SendID { get; init; }
        public UInt32? Index { get; init; }

        public SimConnectException(string msg) : base(msg)
        {
            Code = 1;
            SendID = null;
            Index = null;
        }

        public SimConnectException(UInt32 code, UInt32 sendId) : base(ExceptionMessage[code])
        {
            Code = code;
            SendID = sendId;
            Index = null;
        }

        public SimConnectException(UInt32 code, UInt32 sendId, UInt32 index) : base(ExceptionMessage[code])
        {
            Code = code;
            SendID = sendId;
            Index = (index == 0) ? null : index;
        }

        private static readonly string[] ExceptionMessage = {
                "No error",
                "Error",
                "Size mismatch",
                "Unrecognized Id",
                "Unopened",
                "Version mismatch",
                "Too many groups",
                "Name unrecognized",
                "Too many event names",
                "Event Id already in use",
                "Too many maps",
                "Too many objects",
                "Too many requests",
                "Weather: Invalid port",
                "Weather: Invalid METAR",
                "Weather: Unable to get observation",
                "Weather: Unable to create station",
                "Weather: Unable to remove station",
                "Invalid data type",
                "Invalid data size",
                "Data error",
                "Invalid array",
                "Create object failed",
                "Load flightplan failed",
                "Operation invalid for object type",
                "Illegal operation",
                "Already subscribed",
                "Invalid Enum",
                "Definition error",
                "Duplicate Id",
                "Datum Id",
                "Out of bounds",
                "Already created",
                "Object outside reality bubble",
                "Object container",
                "Object AI",
                "Object ATC",
                "Object schedule",
                "Block timeout",
        };

    }
}
