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
using SimScanner.Sim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using static SimScanner.Sim.SimUtil;

namespace SimScanner.AddOns
{
    public static class AddOnManager
    {

        public static List<string> FindAddOnConfigFiles(Simulator sim)
        {
            List<string> result = new();

            string path = GetProgramData("Lockheed Martin", sim.Name, "add-ons.cfg");
            if (File.Exists(path))
            {
                result.Add(path);
            }
            path = GetLocalAppData("Lockheed Martin", sim.Name, "add-ons.cfg");
            if (File.Exists(path))
            {
                result.Add(path);
            }
            path = GetRoamingAppData("Lockheed Martin", sim.Name, "add-ons.cfg");
            if (File.Exists(path))
            {
                result.Add(path);
            }
            return result;
        }

        private static void ReadAddOnXml(AddOn addOn)
        {
            var config = XElement.Load(addOn.ConfigFile);
            addOn.Name = config.Element("AddOn.Name").Value.Trim();
            addOn.Description = config.Element("AddOn.Description")?.Value.Trim();

            foreach (XElement el in config.Elements("AddOn.Component")) {
                Component comp = new();

                comp.Category = (ComponentCategory)Enum.Parse(typeof(ComponentCategory), el.Element("Category").Value.Trim());
                comp.Path = el.Element("Path")?.Value.Trim();
                comp.Name = el.Element("Name")?.Value.Trim();
                string value = el.Element("Type")?.Value.Trim();
                if (value != null)
                {
                    comp.Type = (ComponentType)Enum.Parse(typeof(ComponentType), value);
                }
                value = el.Element("Layer")?.Value.Trim();
                if (value != null)
                {
                    comp.Layer = int.Parse(value);
                }
                comp.CommandLine = el.Element("CommandLine")?.Value.Trim();
                value = el.Element("DLLType")?.Value.Trim();
                if (value != null)
                {
                    comp.DLLType = (ComponentDLLType)Enum.Parse(typeof(ComponentDLLType), value);
                }
                comp.DLLStartName = el.Element("DLLStartName")?.Value.Trim();
                comp.DLLStopName = el.Element("DLLStopName")?.Value.Trim();
                value = el.Element("NewConsole")?.Value.Trim();
                if (value != null)
                {
                    comp.NewConsole = value.ToLower().Equals("true");
                }
                addOn.Components.Add(comp);
            }
        }

        public static List<AddOn> FindAddOns(Simulator sim)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<AddOn> result = new();

            foreach (string cfg in FindAddOnConfigFiles(sim))
            {
                Console.WriteLine($"[Reading \"{cfg}\"]");
                var parser = new FileIniDataParser();
                parser.Parser.Configuration.AllowCreateSectionsOnFly = false;
                parser.Parser.Configuration.CaseInsensitive = true;
                parser.Parser.Configuration.CommentString = "#";

                IniData data = parser.ReadFile(cfg);

                uint index = 0;
                string sectionTitle = $"Package.{index}";
                while (data[sectionTitle] != null)
                {
                    AddOn package = new();
                    package.Name = data[sectionTitle]["TITLE"];
                    package.Path = data[sectionTitle]["PATH"];
                    package.IsActive = data[sectionTitle]["ACTIVE"]?.ToLower().Equals("true") ?? false;
                    package.IsRequired = data[sectionTitle]["REQUIRED"]?.ToLower().Equals("true") ?? false;

                    if (!package.IsActive)
                    {
                        continue;
                    }

                    if (package.Path != null)
                    {
                        string xmlFile = Path.Combine(package.Path, "add-on.xml");
                        if (File.Exists(xmlFile))
                        {
                            package.ConfigFile = xmlFile;
                            ReadAddOnXml(package);
                        }
                    }
                    result.Add(package);

                    index++;
                    sectionTitle = $"Package.{index}";
                }

                index = 0;
                sectionTitle = $"DiscoveryPath.{index}";
                while (data[sectionTitle] != null)
                {
                    AddOn package = new();
                    package.Name = data[sectionTitle]["TITLE"];
                    package.Path = data[sectionTitle]["PATH"];
                    package.IsActive = data[sectionTitle]["ACTIVE"]?.ToLower().Equals("true") ?? false;
                    package.IsRequired = data[sectionTitle]["REQUIRED"]?.ToLower().Equals("true") ?? false;

                    if (!package.IsActive || !Directory.Exists(package.Path))
                    {
                        continue;
                    }

                    DiscoverPackages(result, package.Path, package.IsActive, package.IsRequired);

                    index++;
                    sectionTitle = $"DiscoveryPath.{index}";
                }
            }
            return result;
        }

        private static void DiscoverPackages(List<AddOn> result, string path, bool isActive =true, bool isRequired =true)
        {
            foreach (string subdir in Directory.EnumerateFiles(path))
            {
                AddOn subPackage = new();
                subPackage.Name = subdir;
                subPackage.Path = Path.Combine(path, subdir);
                subPackage.IsActive = isActive;
                subPackage.IsRequired = isRequired;

                string xmlFile = Path.Combine(subPackage.Path, "add-on.xml");
                if (File.Exists(xmlFile))
                {
                    subPackage.ConfigFile = xmlFile;
                    ReadAddOnXml(subPackage);
                }
                result.Add(subPackage);
            }
        }
    }
}
