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
    public class Parking
    {
        public Airport Airport { get; set; }

        public uint Number { get; set; } = 0;
        public string Type { get; set; } = "";
        public bool HasPushbackLeft { get; set; }
        public bool HasPushbackRight { get; set; }
        public string Name { get; set; } = "";
        public float Radius { get; set; }
        public float Heading { get; set; }
        public float TeeOffset1 { get; set; }
        public float TeeOffset2 { get; set; }
        public float TeeOffset3 { get; set; }
        public float TeeOffset4 { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        private readonly List<string> airlineDesignators = new();
        public List<string> AirlineDesignators => airlineDesignators;

        public string FullName => Name + (((Name != "") && (Number != 0)) ? " " : "") + Number;
    }
}
