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
        public UInt32 SendID { get; init; }
        public UInt32? Index { get; init; }

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
