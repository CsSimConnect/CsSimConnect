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

using Rakis.Logging;
using SimScanner.Bgl;
using SimScanner.Model;
using SimScanner.Sim;
using System;

namespace BglReader
{
    class BglReader
    {
        static void Main(string[] args)
        {
            Logger.Configure();

            if (args.Length > 0)
            {
                foreach (string filename in args)
                {
                    BglFile file = AnalyzeFile(args);
                }
            }
            else
            {
                using (SceneryManager mgr = new(simulator: SimUtil.GetPrepar3Dv5()))
                {
                    mgr.BuildDb();
                }
            }

        }

        private static BglFile AnalyzeFile(string[] args)
        {
            var file = new BglFile(args[0]);
            var status = file.Valid ? "VALID" : "NOT VALID";
            Console.WriteLine($"BglFile(\"{args[0]}\") is {status}");
            Console.WriteLine($"Magic1 = 0x{file.Header.Magic1:x}, Magic2 = 0x{file.Header.Magic2:x}");
            Console.WriteLine($"FileTime = {file.FileTime}");
            Console.WriteLine($"Number of sections = {file.Header.SectionCount}");

            foreach (BglSection section in file.Sections)
            {
                Console.WriteLine($"Section {section.Index}: {section.Header.Type} ({section.Header.SubSectionCount} subsection(s))");

                if (section.IsAirport)
                {
                    Console.WriteLine($"  --> Section has {section.SubSectionCount} subsection(s)");
                    foreach (BglSubSection subSection in section.SubSections)
                    {
                        Console.WriteLine($"      SubSection {subSection.Index} has {subSection.DataSize} byte(s) of data.");
                    }

                    foreach (BglAirport bglAirport in section.Airports)
                    {
                        Console.WriteLine($"  ==> name {bglAirport.Name}, ICAO code {bglAirport.ICAO}, region ident {bglAirport.RegionCode}, {bglAirport.NumRunwayStarts} runway(s), {bglAirport.Taxiways.Count} named taxiway(s), {bglAirport.NumJetways} jetway(s), {bglAirport.Parkings.Count} parking(s).");
                        Console.WriteLine($"      Latitude  {bglAirport.Latitude:###.###}");
                        Console.WriteLine($"      Longitude {bglAirport.Longitude:###.###}");
                        Console.WriteLine($"      Altitude : {bglAirport.Header.Altitude:#####} meter(s) (tower alt {bglAirport.Header.TowerAltitude:#####})");
                    }
                }
                else if (section.IsNameList)
                {
                    foreach (BglNameList nameList in section.NameLists)
                    {
                        if (nameList?.Names == null)
                        {
                            Console.WriteLine("  ==> Skipping empty list.");
                            continue;
                        }
                        foreach (BglName name in nameList.Names)
                        {
                            Console.WriteLine($"  ==> {name.ICAO}: '{name.Airport}', city {name.City}, state {name.State}, country {name.Country}, region {name.Region}.");
                        }
                    }
                }
                else
                {
                    for (uint i = 0; i < section.SubSectionCount; i++)
                    {
                        var subSection = section.GetSubSection(i);
                        Console.WriteLine($"  ==> Subsection[{i:D3}] has {subSection.NumRecords} record(s) in {subSection.DataSize} byte(s).");
                        for (uint j = 0; j < subSection.NumRecords; j++)
                        {
                            Console.WriteLine($"      Subsection [{i:D3}][{j:D3}]: type = 0x{subSection.BglRecordType(j)}:X2");
                        }
                    }
                }
            }

            return file;
        }
    }
}
