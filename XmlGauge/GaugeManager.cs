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

using System.Xml.Linq;
using XmlGauge.Model;
using XmlGauge.Xml;

namespace XmlGauge
{
    public class GaugeManager
    {
        public string name { get; init; }

        public GaugeManager(string name)
        {
            this.name = name;
        }

        public static Gauge Load(string path)
        {
            var gaugeXml = XElement.Load(path);

            return GaugeParser.Parse(gaugeXml); ;
        }
    }
}
