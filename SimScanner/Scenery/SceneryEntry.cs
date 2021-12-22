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

using IniParser.Model;
using SimScanner.AddOns;
using SimScanner.Sim;
using System;
using System.Collections.Generic;
using System.IO;

namespace SimScanner.Scenery
{
    public class SceneryEntry
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int TextureId { get; set; }
        public string RemotePath { get; set; }
        public string LocalPath { get; set; }
        public int Layer { get; set; }
        public bool Active { get; set; }
        public bool Required { get; set; }
        public string Exclude { get; set; }

        public List<string> SceneryFiles => new(Directory.GetFiles(Path.Combine(LocalPath, "scenery"), "*.bgl"));

        public static SceneryEntry FromIniFile(Simulator sim, KeyDataCollection section)
        {
            SceneryEntry result = new();

            result.Title = section["Title"];
            result.LocalPath = Path.Combine(sim.InstallationPath, section["local"]);
            result.Active = Boolean.Parse(section["active"]);
            result.Required = Boolean.Parse(section["active"]);
            result.Layer = Int32.Parse(section["Layer"]);

            return result;
        }

        public static SceneryEntry FromComponent(AddOn addOn, Component comp)
        {
            SceneryEntry result = new();

            result.Title = comp.Name;
            result.LocalPath = Path.Combine(addOn.Path, comp.Path);
            result.Active = true;
            result.Layer = comp.Layer;

            return result;
        }
    }
}
