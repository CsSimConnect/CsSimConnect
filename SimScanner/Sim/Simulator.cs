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

namespace SimScanner.Sim
{

    public enum FlightSimType
    {
        Unknown,
        Prepar3Dv4,
        Prepar3Dv5,
        MSFS2020
    }

    public class Simulator
    {
        public FlightSimType Type { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string InstallationPath { get; set; }
        public bool Installed { get; set; }
        public bool DllAvailable { get; set; }
    }
}
