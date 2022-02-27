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

using System;
using System.Xml.Linq;
using XmlGauge.Model;

namespace XmlGauge.Xml
{
    public static class GaugeParser
    {

        private static int toInt(string s)
        {
            return int.Parse(s);
        }

        private static double toDouble(string s)
        {
            return double.Parse(s);
        }

        private static double toDouble(XElement elem, string name)
        {
            double result = 0.0;

            if (elem.Attribute(name) is XAttribute attr)
            {
                result = double.Parse((string)attr);
            }
            return result;
        }

        private static bool IsYes(XElement elem, string name) {
            bool result = false;

            if (elem.Attribute(name) is XAttribute attr)
            {
                result = ((string)attr).ToLower().Equals("yes");
            }
            return result;
        }

        public static Image ParseImage(XElement xml)
        {
            Image result = new();

            result.Name = (string)xml.Attribute("Name");
            if (xml.Attribute("ImageSizes") is XAttribute sizes)
            {
                string[] numbers = ((string)sizes).Split(",");
                if (numbers.Length > 0)
                {
                    result.Dimension.Width = toInt(numbers[0]);
                }
                if (numbers.Length > 1)
                {
                    result.Dimension.Height = toInt(numbers[1]);
                }
                if (numbers.Length > 2)
                {
                    result.DimensionRes1024.Width = toInt(numbers[2]);
                }
                if (numbers.Length > 3)
                {
                    result.DimensionRes1024.Height = toInt(numbers[3]);
                }
            }
            if (xml.Element("Axis") is XElement axis)
            {
                result.Axis = new(toDouble(axis, "X"), toDouble(axis, "Y"));
            }
            result.Bright = IsYes(xml, "Bright");
            result.UseTransparency = IsYes(xml, "UseTransparency");
            result.Luminous = IsYes(xml, "Luminous");
            result.Alpha = IsYes(xml, "Alpha");
            result.NoBilinear = IsYes(xml, "NoBilinear");

            return result;
        }

        private static Element ParseElement(XElement element)
        {
            throw new NotImplementedException();
        }

        public static Gauge Parse(XElement xml)
        {
            Gauge result = new();

            result.Name = (string)xml.Attribute("Name");
            result.Version = (string)xml.Attribute("Version");

            if (xml.Element("Size") is XElement size)
            {
                result.Dimension.Width = toInt((string)size.Attribute("X"));
                result.Dimension.Height = toInt((string)size.Attribute("Y"));
            }
            foreach (XElement background in xml.Elements("Image"))
            {
                result.Background.Add(ParseImage(background));
            }
            foreach (XElement element in xml.Elements("Element"))
            {
                result.Elements.Add(ParseElement(element));
            }
            return result;
        }
    }
}
