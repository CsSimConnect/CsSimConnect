/*
 * Copyright (c) 2022. Bert Laverman
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

namespace XmlGauge.Model
{
    public class Gauge : Component
    {
        public string Name { get; internal set; }
        public string Version { get; internal set; }

        public Dimension Dimension = new(320, 240);

        public List<Image> background = new();
        public List<Image> Background => background;

        private List<Element> elements = new();
        public List<Element> Elements => elements;
    }
}
