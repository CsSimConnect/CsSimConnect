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
using System.Collections.Generic;

namespace ShowAirport
{
    class ShowAirport
    {
        private const string OPT_P3D = "p3dv5";
        private const string OPT_MSFS = "msfs";
        private const string OPT_LIST_PARKINGS = "list-parkings";

        static bool HaveValue(string s) => (s != null) && (s.Trim().Length != 0);

        static void Main(string[] args)
        {
            Logger.DefaultConfiguration().Build();
            var parsedArgs = new ArgParser(args)
                .WithOption(OPT_P3D)
                .WithOption(OPT_MSFS)
                .WithOption(OPT_LIST_PARKINGS)
                .Parse();

            bool showParkings = parsedArgs.Has(OPT_LIST_PARKINGS);
            Simulator simulator = parsedArgs.Has(OPT_MSFS) ? SimUtil.GetMSFS2020() : SimUtil.GetPrepar3Dv5();

            using (SceneryManager mgr = simulator.SceneryManager())
            {
                Console.WriteLine($"Showing {parsedArgs.Parameters.Count} airports from {simulator.Name}");
                foreach (string arg in parsedArgs.Parameters)
                {
                    string icao = arg;
                    int layer = -1;
                    int index = arg.IndexOf(':');
                    if (index >= 0)
                    {
                        icao = arg.Substring(0, index);
                        layer = Int32.Parse(arg.Substring(index + 1));
                    }

                    if (layer == -1)
                    {
                        var layers = mgr.GetLayersForICAO(icao);
                        if (layers.Count == 0)
                        {
                            Console.WriteLine($"ICAO code '{icao}' not found.");
                            continue;
                        }
                        foreach (int l in layers)
                        {
                            Console.WriteLine($"- {icao} is present in layer {l}.");
                        }
                        layer = layers[0];
                    }
                    var airport = mgr.GetAirport(layer, icao);
                    if (airport == null)
                    {
                        Console.WriteLine($"Failed to retrieve airport with ICAO '{icao}' at layer {layer}.");
                        continue;
                    }
                    Console.WriteLine($"  ==> name {airport.Name} (layer {layer}, file '{airport.Filename}'), ICAO code {airport.ICAO}, {airport.Parkings.Count} parking(s).");
                    if (HaveValue(airport.Region))  Console.WriteLine($"      Region   : {airport.Region}");
                    if (HaveValue(airport.Country)) Console.WriteLine($"      Country  : {airport.Country}");
                    if (HaveValue(airport.State))   Console.WriteLine($"      State    : {airport.State}");
                    if (HaveValue(airport.City))    Console.WriteLine($"      City     : {airport.City}");
                    Console.WriteLine($"      Latitude : {airport.Latitude:###.###}");
                    Console.WriteLine($"      Longitude: {airport.Longitude:###.###}");
                    Console.WriteLine($"      Altitude : {airport.AltitudeMeters:#####} meter(s) ({airport.AltitudeFeet:######} feet)");
                    if (showParkings)
                    {
                        foreach (Parking p in airport.ParkingValues)
                        {
                            Console.WriteLine($"      - Parking {p.FullName}");
                            Console.WriteLine($"        Latitude : {p.Latitude:###.###}");
                            Console.WriteLine($"        Longitude: {p.Longitude:###.###}");
                            Console.WriteLine($"        Heading  : {p.Heading:###}");
                        }
                    }
                }
            }
        }
    }
}
