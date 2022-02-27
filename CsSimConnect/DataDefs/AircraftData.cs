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
using CsSimConnect.Reflection;

namespace CsSimConnect
{
    public class AircraftData
    {
        [DataDefinition("ATC TYPE", Type = DataType.String32)]
        public string Type { get; set; }

		[DataDefinition("ATC MODEL", Type = DataType.String32)]
		public string Model { get; set; }

		[DataDefinition("ATC ID", Type = DataType.String32)]
		public string Id { get; set; }

		[DataDefinition("ATC AIRLINE", Type = DataType.String32)]
		public string Airline { get; set; }

		[DataDefinition("ATC FLIGHT NUMBER", Type = DataType.String8)]
		public string FlightNumber { get; set; }

		[DataDefinition("TITLE", Type = DataType.String256)]
		public string Title { get; set; }

		[DataDefinition("NUMBER OF ENGINES", Units = "Number", Type = DataType.Int32)]
		public int NumberOfEngines { get; set; }

		[DataDefinition("ENGINE TYPE", Units = "Number", Type = DataType.Int32)]
		public int EngineType { get; set; }
	}
}
