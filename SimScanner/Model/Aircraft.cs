﻿/*
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

namespace SimScanner.Model
{
    public class Aircraft
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string Category { get; set; }

        private static string Cleanup(string s)
        {
            if ((s != null) && s.StartsWith("\"") && s.EndsWith("\""))
            {
                return s.Substring(1, s.Length - 2);
            }
            return s;
        }

        public Aircraft() { }
        public Aircraft(string title, string type, string model, string category)
        {
            Title = Cleanup(title);
            Type = Cleanup(type);
            Model = Cleanup(model);
            Category = Cleanup(category);
        }
    }
}
