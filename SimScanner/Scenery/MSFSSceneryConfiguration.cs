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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rakis.Logging;
using SimScanner.Sim;
using System;
using System.IO;

namespace SimScanner.Scenery
{
    public class Package : SceneryEntry
    {
        public string ContentType { get; set; }
        public string Version { get; set; }
    }

    public class MSFSSceneryConfiguration : SceneryConfiguration
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MSFSSceneryConfiguration));

        public MSFSSceneryConfiguration(Simulator simulator) : base(simulator)
        {

        }

        private void ScanPackages(string path)
        {
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                string packageDir = Path.Combine(path, dir);
                string jsonManifest = Path.Combine(packageDir, "manifest.json");
                string jsonLayout = Path.Combine(packageDir, "layout.json");

                if (!File.Exists(jsonManifest) || !File.Exists(jsonLayout))
                {
                    ScanPackages(packageDir);
                }
                else
                {
                    Package package = LoadPackage(packageDir, jsonManifest, jsonLayout, filename => Path.GetExtension(filename) == ".bgl");
                    if (package.Files.Count > 0)
                    {
                        Entries.Add(package);
                    }
                }
            }
        }

        public static Package LoadPackage(string packagePath, string manifestPath, string layoutPath, Func<string,bool> filenameFilter)
        {
            Package result = null;

            try
            {
                var manifest = File.ReadAllText(manifestPath);
                var loadedManifest = JsonConvert.DeserializeObject<JObject>(manifest);
                var layout = File.ReadAllText(layoutPath);
                var loadedLayout = JsonConvert.DeserializeObject<JObject>(layout);

                if ((loadedManifest != null) && (loadedLayout != null))
                {
                    result = new();
                    result.Title = loadedManifest.GetValue("title").ToString();
                    if ((result.Title == null) || (result.Title.Length == 0))
                        result.Title = Path.GetFileName(packagePath);
                    result.Version = loadedManifest.GetValue("package_version").ToString();
                    result.ContentType = loadedManifest.GetValue("content_type").ToString();
                    result.Layer = (result.ContentType.ToLower() == "core") ? 1 : 2;
                    result.LocalPath = packagePath;

                    log.Debug?.Log($"Collecting files from '{result.Title}'");
                    result.Files.Clear();
                    JProperty content = loadedLayout.Property("content");
                    if (content.Value is JArray array)
                    {
                        foreach (JObject prop in array)
                        {
                            string filename = prop.GetValue("path").ToString().ToLower();
                            if (filenameFilter(filename))
                                result.Files.Add(Path.Combine(packagePath, filename));
                        }
                    }
                    log.Debug?.Log($"- Collected {result.Files.Count} files");
                    result.Active = result.Files.Count > 0;
                }
                else
                {
                    log.Error?.Log($"Failed to parse package files in '{packagePath}'");
                }
            }
            catch (Exception e)
            {
                log.Error?.Log($"Failed to parse package in '{packagePath}': {e.Message}");
            }
            return result;
        }

        public override void LoadSceneryConfig()
        {
            ScanPackages(Simulator.InstallationPath);
        }

        public override void LoadAddOnScenery()
        {
            // DONOTHING
        }

        public override void SortEntries()
        {
            entries.Sort((SceneryEntry e1, SceneryEntry e2) => e1.Layer.CompareTo(e2.Layer));
            log.Trace?.Log($"Sorted {entries.Count} scenery entries.");
        }
    }
}
