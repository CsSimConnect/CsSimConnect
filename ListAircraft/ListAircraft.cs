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

using SimScanner.AircraftCfg;
using SimScanner.Model;
using SimScanner.Sim;
using System;

namespace ListAircraft
{
    class ListAircraft
    {
        static void Main(string[] args)
        {
            using AircraftManager mgr = new();

            if ((args == null) || (args.Length == 0))
            {
                foreach (Aircraft aircraft in mgr.BuildDB())
                {
                    Console.WriteLine($"Added '{aircraft.Title}'");
                }
            }
            else
            {
                foreach (string title in args)
                {
                    var aircraft = mgr.GetAircraft(title);
                    if (aircraft == null)
                    {
                        Console.WriteLine($"No aircraft found with title '{title}'");
                    }
                    else
                    {
                        Console.WriteLine($"Found '{title}':");
                        Console.WriteLine($"- Model:    '{aircraft.Model}'");
                        Console.WriteLine($"- Type:     '{aircraft.Type}'");
                        Console.WriteLine($"- Category: '{aircraft.Category}'");
                    }
                }
            }
        }
    }
}
