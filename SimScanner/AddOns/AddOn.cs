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

namespace SimScanner.AddOns
{

    public enum ComponentCategory
    {
        Autogen,
        DLL,
        EXE,
        Effects,
        Fonts,
        Gauges,
        Sound,
        Scaleform,
        Scenarios,
        Scenery,
        Scripts,
        ShadersHLSL,
        SimObjects,
        Texture,
        Weather,
    }
    public enum ComponentType
    {
        UI,
        GLOBAL,
        WORLD,
    }
    public enum ComponentDLLType
    {
        Default,
        SimConnect,
        PDK,
    }

    public class Component
    {
        public ComponentCategory Category { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public ComponentType Type { get; set; }
        public int Layer { get; set; }
        public string CommandLine { get; set; }
        public ComponentDLLType DLLType { get; set; }
        public string DLLStartName { get; set; }
        public string DLLStopName { get; set; }
        public bool NewConsole { get; set; }
    }

    public class AddOn
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string ConfigFile { get; set; }
        public bool IsActive { get; set; }
        public bool IsRequired { get; set; }

        private readonly List<Component> components = new();
        public List<Component> Components { get => components; }
    }
}
