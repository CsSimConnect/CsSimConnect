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

namespace XmlGauge.Model
{
    public class Image : Component
    {
        public string Name { get; internal set; }
        public Dimension Dimension;
        public Dimension DimensionRes1024;
        public Position Axis;

        public bool UseTransparency { get; internal set; }
        public bool Luminous { get; internal set; }
        public bool Alpha { get; internal set; }
        public bool NoBilinear { get; internal set; }
    }
}
