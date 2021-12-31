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

using IniParser;
using IniParser.Model;
using Rakis.Logging;
using SimScanner.AddOns;
using SimScanner.Sim;
using System.IO;
using static SimScanner.Sim.SimUtil;

namespace SimScanner.Scenery
{
    public class P3DSceneryConfiguration : SceneryConfiguration
    {
        private static readonly Logger log = Logger.GetLogger(typeof(SceneryConfiguration));

        public P3DSceneryConfiguration(Simulator simulator) : base(simulator)
        {
        }

        public override void LoadSceneryConfig()
        {
            string path = GetProgramData("Lockheed Martin", Simulator.Name, "scenery.cfg");
            if (File.Exists(path))
            {
                log.Debug?.Log($"Loading '{path}'");

                var parser = new FileIniDataParser();
                parser.Parser.Configuration.AllowCreateSectionsOnFly = false;
                parser.Parser.Configuration.CaseInsensitive = true;
                parser.Parser.Configuration.CommentString = "#";

                IniData data = parser.ReadFile(path);
                var generalData = data["General"];

                Title = generalData["title"];
                Description = generalData["description"];
                CleanOnExit = bool.Parse(generalData["clean_on_exit"]);

                int count = entries.Count;

                foreach (SectionData collection in data.Sections)
                {
                    string name = collection.SectionName;
                    if (name.ToLower().StartsWith("area."))
                    {
                        entries.Add(SceneryEntry.FromIniFile(Simulator, data[name]));
                        count++;
                    }
                }

                log.Info?.Log($"Added {entries.Count - count} entries from SCENERY.CFG.");
                SortEntries();
            }
            else
            {
                log.Warn?.Log($"No Scenery.CFG found for '{Simulator.Name}'");
            }
        }

        public override void LoadAddOnScenery()
        {
            foreach (AddOn addOn in AddOnManager.FindAddOns(Simulator))
            {
                foreach (Component addOnComponent in addOn.Components)
                {
                    if (addOnComponent.Category == ComponentCategory.Scenery)
                    {
                        log.Info?.Log($"Adding {addOnComponent.Name} from Add-on {addOn.Name}.");
                        entries.Add(SceneryEntry.FromComponent(addOn, addOnComponent));
                    }
                }
            }
            SortEntries();
        }

        public override void SortEntries()
        {
            entries.Sort((SceneryEntry e1, SceneryEntry e2) => e1.Layer.CompareTo(e2.Layer));
            log.Trace?.Log($"Sorted {entries.Count} scenery entries.");
        }
    }
}
