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

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double Pitch { get; set; }
        public double Bank { get; set; }
        public double Heading { get; set; }

        public bool IsOnGround { get; set; } = true;        // By default, we want the aircraft on the ground.
        public bool EnginesRunning { get; set; } = true;    // By default, engines are running.
        public int AirSpeed { get; set; } = 0;              // By default, we want the aircraft to stand still.

        public string AirportId { get; set; } = null;       // If we select an airport ICAO id, we want a parked, ATC-controlled aircraft

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

        public AircraftBuilder AtPosition(double latitude, double longitude, double altitude =0.0)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;

            return this;
        }

        public AircraftBuilder WithPBH(double pitch, double bank, double heading)
        {
            Pitch = pitch;
            Bank = bank;
            Heading = heading;

            return this;
        }

        public AircraftBuilder WithPitch(double pitch)
        {
            Pitch = pitch;

            return this;
        }

        public AircraftBuilder WithBank(double bank)
        {
            Bank = bank;

            return this;
        }

        public AircraftBuilder WithHeading(double heading)
        {
            Heading = heading;

            return this;
        }

        public AircraftBuilder OnGround()
        {
            Altitude = 0.0;

            Pitch = 0.0;
            Bank = 0.0;

            IsOnGround = true;
            return this;
        }

        public AircraftBuilder WithAirSpeed(int speed)
        {
            AirSpeed = speed;

            return this;
        }

        public AircraftBuilder Static()
        {
            AirSpeed = 0;

            return this;
        }

        public AircraftBuilder WithEnginesStopped()
        {
            EnginesRunning = false;
            return this;
        }

        public AircraftBuilder WithEnginesRunning()
        {
            EnginesRunning = true;
            return this;
        }

        public SimulatedAircraft Build()
        {
            SimulatedAircraft result;
            if (!IsEmpty(AirportId))
            {
                result = new ParkedAircraft(airportId: AirportId, title: Title, tailNumber: TailNumber);
            }
            else
            {
                result = new SimulatedAircraft(title: Title, tailNumber: TailNumber);
                result.Latitude = Latitude;
                result.Longitude = Longitude;
                result.Altitude = Altitude;
            }
            result.Pitch = Pitch;
            result.Bank = Bank;
            result.Heading = Heading;
            result.OnGround = IsOnGround;
            result.AirSpeed = AirSpeed;
            return result;
        }

    }
}
