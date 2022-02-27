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
    public struct Dimension
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }

        public Dimension(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    public struct Position
    {
        public double X { get; internal set; }
        public double Y { get; internal set; }

        public Position(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Component
    {
        public string Id { get; internal set; }
        public bool Bright { get; internal set; } = false;
    }
}
