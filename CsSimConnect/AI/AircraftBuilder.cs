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
using static CsSimConnect.Util.StringUtil;

namespace CsSimConnect.AI
{
    public class AircraftBuilder
    {

        public static AircraftBuilder Builder(string title)
        {
            return new(title);
        }

        public string Title { get; set; }
        public string TailNumber { get; set; }
        public InitPosition InitPosition { get; set; }
        public string AirportId { get; set; }

        private AircraftBuilder(string title)
        {
            Title = title;
        }

        public AircraftBuilder WithTailNumber(string tailNumber)
        {
            TailNumber = tailNumber;
            return this;
        }

        public AircraftBuilder AtAirport(string airportId)
        {
            AirportId = airportId;
            return this;
        }

        public SimulatedAircraft Build()
        {
            if (!IsEmpty(AirportId))
            {
                return new ParkedAircraft(airportId: AirportId, title: Title, tailNumber: TailNumber);
            }
            return new SimulatedAircraft(title: Title);
        }

    }
}
