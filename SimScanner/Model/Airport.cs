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

using System.Collections.Generic;

namespace SimScanner.Model
{
    public class Airport
    {
        public string Name { get; set; }
        public int Layer { get; set; }
        public string Filename { get; set; }
        public string ICAO { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public IDictionary<string, Parking> Parkings { get; } = new Dictionary<string, Parking>();
        public ICollection<string> ParkingNames => Parkings.Keys;
        public ICollection<Parking> ParkingValues => Parkings.Values;

        public Airport() { }
        public Airport(string name, int layer, string filename, string icao, double lat, double lon)
        {
            Name = name;
            Layer = layer;
            Filename = filename;
            ICAO = icao;
            Latitude = lat;
            Longitude = lon;
        }
    }
}
