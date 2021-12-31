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

using Rakis.Args;
using Rakis.Logging;
using SimScanner.Model;
using SimScanner.Sim;
using System;

namespace ListAircraft
{
    class ListAircraft
    {
        private const string OPT_P3D = "p3dv5";
        private const string OPT_MSFS = "msfs";
        private const string OPT_LIST = "list";
        private const string OPT_FILTER = "filter";
        private const string OPT_BUILD_DB = "build-db";

        static void Main(string[] args)
        {
            Logger.Configure();
            var parsedArgs = new ArgParser(args)
                .WithOption(OPT_P3D)
                .WithOption(OPT_MSFS)
                .WithOption(OPT_LIST)
                .WithOption(OPT_FILTER, true)
                .WithOption(OPT_BUILD_DB)
                .Parse();

            Simulator simulator = parsedArgs.Has(OPT_MSFS) ? SimUtil.GetMSFS2020() : SimUtil.GetPrepar3Dv5();
            using AircraftManager mgr = simulator.AircraftManager();

            if (parsedArgs.Has(OPT_LIST))
            {

            }
            else if (parsedArgs.Has(OPT_BUILD_DB))
            {
                foreach (Aircraft aircraft in mgr.BuildDB())
                {
                    Console.WriteLine($"Added '{aircraft.Title}'");
                }
            }
            else if (parsedArgs.Parameters.Count > 0)
            {
                foreach (string title in parsedArgs.Parameters)
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
