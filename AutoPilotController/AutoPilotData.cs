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

namespace AutoPilotController
{
    public class AutoPilotData
    {
        [DataDefinition("NAV ACTIVE FREQUENCY:1", Units = "MHz", Type = DataType.Int32)]
        public int FreqNav1 { get; set; }
        [DataDefinition("NAV ACTIVE FREQUENCY:2", Units = "MHz", Type = DataType.Int32)]
        public int FreqNav2 { get; set; }
        [DataDefinition("ADF ACTIVE FREQUENCY:1", Units = "Frequency ADF BCD32", Type = DataType.Int32)]
        public int FreqAdf { get; set; }

        [DataDefinition("AUTOPILOT MASTER", Units = "Bool", Type = DataType.Int32)]
        public bool AutoPilotMaster { get; set; }

        [DataDefinition("AUTOPILOT HEADING LOCK DIR", Units ="Degrees", Type = DataType.Int32)]
        public int Heading { get; set; }
        [DataDefinition("AUTOPILOT HEADING LOCK", Units = "Bool", Type = DataType.Int32)]
        public bool HeadingHold { get; set; }

        [DataDefinition("AUTOPILOT ALTITUDE LOCK VAR", Units = "Feet", Type = DataType.Int32)]
        public int Altitude { get; set; }
        [DataDefinition("AUTOPILOT ALTITUDE LOCK", Units = "Bool", Type = DataType.Int32)]
        public bool AltitudeHold { get; set; }

        [DataDefinition("AUTOPILOT VERTICAL HOLD VAR", Units = "Feet/minute", Type = DataType.Int32)]
        public int VerticalSpeed { get; set; }

        [DataDefinition("AUTOPILOT AIRSPEED HOLD VAR", Units = "Knots", Type = DataType.Int32)]
        public int IndicatedAirSpeed { get; set; }
        [DataDefinition("AUTOPILOT AIRSPEED HOLD", Units = "Bool", Type = DataType.Int32)]
        public bool SpeedHold { get; set; }

        [DataDefinition("NAV OBS:1", Units = "Feet/minute", Type = DataType.Int32)]
        public int CourseNav1 { get; set; }
        [DataDefinition("AUTOPILOT NAV1 LOCK", Units = "Bool", Type = DataType.Int32)]
        public bool Nav1Hold { get; set; }
        [DataDefinition("NAV OBS:2", Units = "Feet/minute", Type = DataType.Int32)]
        public int CourseNav2 { get; set; }

        [DataDefinition("AUTOPILOT BACKCOURSE HOLD", Units = "Bool", Type = DataType.Int32)]
        public bool BackCourseHold { get; set; }
        [DataDefinition("AUTOPILOT APPROACH HOLD", Units = "Bool", Type = DataType.Int32)]
        public bool ApproachHold { get; set; }
    }
}
