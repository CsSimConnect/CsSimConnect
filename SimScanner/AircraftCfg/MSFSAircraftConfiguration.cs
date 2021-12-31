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

using Rakis.Logging;
using SimScanner.Scenery;
using SimScanner.Sim;
using System.IO;

namespace SimScanner.AircraftCfg
{
    public class MSFSAircraftConfiguration : AircraftConfiguration
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MSFSAircraftConfiguration));

        public MSFSAircraftConfiguration(Simulator simulator) : base(simulator)
        {
        }

        private void ScanPackages(string path, string category)
        {
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                string packageDir = Path.Combine(path, dir);
                string jsonManifest = Path.Combine(packageDir, "manifest.json");
                string jsonLayout = Path.Combine(packageDir, "layout.json");

                if (!File.Exists(jsonManifest) || !File.Exists(jsonLayout))
                {
                    ScanPackages(packageDir, category);
                }
                else
                {
                    log.Info?.Log($"Checking package '{dir}'");
                    Package package = MSFSSceneryConfiguration.LoadPackage(packageDir, jsonManifest, jsonLayout, filename => Path.GetFileName(filename) == "aircraft.cfg");
                    foreach (string filename in package.Files)
                    {
                        AddAircraftFrom(filename, category);
                    }
                }
            }
        }

        public override void ScanSimObjects()
        {
            Entries.Clear();

            foreach (string category in Categories)
            {
                ScanPackages(Simulator.InstallationPath, category);
            }

            SortEntries();
        }
    }
}
