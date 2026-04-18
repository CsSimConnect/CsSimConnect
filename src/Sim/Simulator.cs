/*
 * Copyright (c) 2021-2024. Bert Laverman
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


namespace CsSimConnect.Sim
{
    
    public enum FlightSimType
    {
        Unknown,
        Test,
        Prepar3D,
        MSFlightSimulator
    }

    public struct FlightSimVersion
    {
        public FlightSimType Type;
        public string Version;

        public override readonly string ToString() =>
            Type switch
            {
                FlightSimType.Test => "Test",
                FlightSimType.Prepar3D => "P3D",
                FlightSimType.MSFlightSimulator => "MSFS",
                _ => null
            } + (Version ?? "");

        public static FlightSimVersion FromAppInfo(string name) =>
            name switch
            {
                "Test" => new FlightSimVersion{ Type = FlightSimType.Test, Version = "" },
                "KittyHawk" => new FlightSimVersion{ Type = FlightSimType.MSFlightSimulator, Version = "2020" },
                "SunRise" => new FlightSimVersion{ Type = FlightSimType.MSFlightSimulator, Version = "2024" },
                "Lockheed Martin® Prepar3D® v4" => new FlightSimVersion{ Type = FlightSimType.Prepar3D, Version = "4" },
                "Lockheed Martin® Prepar3D® v5" => new FlightSimVersion{ Type = FlightSimType.Prepar3D, Version = "5" },
                "Lockheed Martin® Prepar3D® v6" => new FlightSimVersion{ Type = FlightSimType.Prepar3D, Version = "6" },
                _ => new FlightSimVersion{ Type = FlightSimType.Unknown, Version = "" }
            };
    }

    public class Simulator
    {
        public FlightSimVersion Fs { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string InstallationPath { get; set; }
        public bool Installed { get; set; }
        public bool DllAvailable { get; set; }
    }
}
