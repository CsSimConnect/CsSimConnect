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

using SimScanner.Model;
using System.Collections.Generic;

namespace SimScanner.Bgl
{
    public class BglAirport
    {
        internal BglSubSection subSection;

        public string Name { get; init; }

        public uint NumRunwayStarts { get; init; }
        public uint NumHelipads { get; init; }
        public uint NumJetways { get; set; }

        public uint NumApronSurfaceRecords { get; init; }
        public uint NumApronDetailRecords { get; init; }
        public uint NumApronEdgeLightRecords { get; init; }

        public uint NumApproaches { get; set; }

        public virtual string ICAO => null;
        public virtual string RegionCode => null;
        public virtual double Latitude => 0.0;
        public virtual double Longitude => 0.0;
        public virtual double Altitude => 0.0;

        private readonly List<string> taxiways = new();
        public List<string> Taxiways => taxiways;
        private readonly List<Parking> parkings = new();
        public List<Parking> Parkings => parkings;

        public static string DecodeName(uint encoded, bool shift5 = true)
        {
            string result = "";

            if (shift5)
            {
                encoded >>= 5;
            }
            while (encoded != 0)
            {
                uint oneChar = encoded % 38;
                if (oneChar == 0)
                {
                    result = " " + result;
                }
                else if ((oneChar >= 2) && (oneChar <= 11))
                {
                    result = ((char)('0' + oneChar - 2)) + result;
                }
                else if ((oneChar >= 12) && (oneChar <= 37))
                {
                    result = ((char)('A' + oneChar - 12)) + result;
                }
                encoded /= 38;
            }
            return result;
        }
    }
}