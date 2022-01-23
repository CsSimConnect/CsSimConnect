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
using SimScanner.Bgl;
using SimScanner.Model;
using SimScanner.Scenery;
using SimScanner.Sim;
using System;
using System.Collections.Generic;

namespace BglReader
{
    class BglReader
    {
        private const string OPT_P3D = "p3dv5";
        private const string OPT_MSFS = "msfs";
        private const string OPT_LIST = "list";
        private const string OPT_LIST_BGL = "list-bgl";
        private const string OPT_LIST_SUBSECTIONS = "list-subsections";
        private const string OPT_LIST_PARKINGS = "list-parkings";
        private const string OPT_FILTER = "filter";
        private const string OPT_ICAO = "icao";
        private const string OPT_BUILD_DB = "build-db";
        private const string OPT_DUMP_HEADER = "dump-header";


        static void Main(string[] args)
        {
            Logger.Configure();
            var parsedArgs = new ArgParser(args)
                .WithOption(OPT_P3D)
                .WithOption(OPT_MSFS)
                .WithOption(OPT_LIST)
                .WithOption(OPT_LIST_BGL)
                .WithOption(OPT_LIST_SUBSECTIONS)
                .WithOption(OPT_LIST_PARKINGS)
                .WithOption(OPT_FILTER, true)
                .WithOption(OPT_ICAO, true)
                .WithOption(OPT_BUILD_DB)
                .WithOption(OPT_DUMP_HEADER)
                .Parse();

            if (parsedArgs.Parameters.Count > 0)
            {
                SortedSet<string> filters = new();
                if (parsedArgs.ArgOpts.ContainsKey(OPT_FILTER))
                {
                    string filter = parsedArgs.ArgOpts[OPT_FILTER];
                    Console.WriteLine($"Filter = \"{filter}\"");
                    foreach (string f in filter.Split(','))
                    {
                        filters.Add(f);
                    }
                }

                foreach (string filename in parsedArgs.Parameters)
                {
                    BglFile file = AnalyzeFile(filename, filters, parsedArgs);
                }
            }
            else
            {
                Simulator simulator = parsedArgs.Has(OPT_MSFS) ? SimUtil.GetMSFS2020() : SimUtil.GetPrepar3Dv5();

                if (parsedArgs.Has(OPT_LIST) || parsedArgs.Has(OPT_LIST_BGL))
                {
                    simulator.Scenery.LoadSceneryConfig();
                    foreach (SceneryEntry entry in simulator.Scenery.Entries)
                    {
                        Console.WriteLine($"- {entry.Title} ({entry.LocalPath})");
                        if (parsedArgs.Has(OPT_LIST_BGL))
                        {
                            foreach (string f in entry.Files)
                            {
                                Console.WriteLine($"  {f})");
                            }
                        }
                    }
                }
                if (parsedArgs.Has(OPT_BUILD_DB))
                {
                    using (SceneryManager mgr = simulator.SceneryManager())
                    {
                        mgr.BuildDb();
                    }
                }
            }

        }

        private static BglFile AnalyzeFile(string filename, SortedSet<string> filters, Args args)
        {
            var file = new BglFile(filename);
            var status = file.Valid ? "VALID" : "NOT VALID";
            Console.WriteLine($"BglFile(\"{filename}\") is {status}");
            Console.WriteLine($"Magic1 = 0x{file.Header.Magic1:x}, Magic2 = 0x{file.Header.Magic2:x}");
            Console.WriteLine($"FileTime = {file.FileTime}");
            Console.WriteLine($"File size = {file.Size} (0x{file.Size:X8})");
            Console.WriteLine($"Number of sections = {file.Header.SectionCount}");

            bool listParkings = args.Has(OPT_LIST_PARKINGS);
            bool listSubsections = args.Has(OPT_LIST_SUBSECTIONS);
            bool dumpHeader = args.Has(OPT_DUMP_HEADER);
            string icao = args.ArgOpts.ContainsKey(OPT_ICAO) ? args.ArgOpts[OPT_ICAO] : null;

            foreach (BglSection section in file.Sections)
            {
                Console.WriteLine($"Section {section.Index}: {section.Header.Type} ({section.Header.SubSectionCount} subsection(s), {section.Header.SubSectionSize} byte(s) each), starts at 0x{file.SectionHeaderOffset(section.Index):X8}");

                if (listSubsections)
                {
                    foreach (BglSubSection subSection in section.SubSections)
                    {
                        Console.WriteLine($"- Subsection {subSection.Index:D2}: 0x{subSection.DataSize:X8} byte(s) of data, {subSection.NumRecords} record(s), starting pos 0x{subSection.DataOffset:X8}");
                    }
                }

                if ((filters.Count > 0) && !filters.Contains(section.Header.Type.ToString()))
                {
                    continue;
                }

                if (section.IsAirport)
                {
                    if (dumpHeader)
                    {
                        uint dataOffset = section.GetSubSection(0).DataOffset;
                        using (var reader = section.file.MappedFile.Section(dataOffset, section.GetSubSection(0).DataSize))
                        {
                            uint i = 0;
                            while (i <= 256)
                            {
                                Console.WriteLine($"0x{(dataOffset + i):X8}: {reader.HexDump(i, 16)}");
                                i += 16;
                            }
                        }
                    }
                    foreach (BglAirport bglAirport in section.Airports)
                    {
                        if ((icao == null) || (icao == bglAirport.ICAO))
                        {
                            Console.WriteLine($"  ==> name {bglAirport.Name}, ICAO code {bglAirport.ICAO}, region ident {bglAirport.RegionCode}, {bglAirport.NumRunwayStarts} runway(s), {bglAirport.Taxiways.Count} named taxiway(s), {bglAirport.NumJetways} jetway(s), {bglAirport.Parkings.Count} parking(s).");
                            Console.WriteLine($"      Latitude  {bglAirport.Latitude:###.###}");
                            Console.WriteLine($"      Longitude {bglAirport.Longitude:###.###}");
                            Console.WriteLine($"      Altitude : {bglAirport.Altitude:#####} meter(s)");
                            if (listParkings)
                            {
                                foreach (Parking p in bglAirport.Parkings)
                                {
                                    Console.WriteLine($"      - Parking {p.FullName} ({p.Type})");
                                    Console.WriteLine($"        Latitude : {p.Latitude:###.###}");
                                    Console.WriteLine($"        Longitude: {p.Longitude:###.###}");
                                    Console.WriteLine($"        Heading  : {p.Heading:###}");
                                }
                            }
                        }
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
                else if (section.IsAirportSummary)
                {
                    foreach (BglAirportSummary summary in section.AirportSummaries)
                    {
                        Console.WriteLine($"  ==> Summary for {summary.ICAO}: region {summary.RegionCode}.");
                        Console.WriteLine($"      Latitude : {summary.Latitude:###.###}");
                        Console.WriteLine($"      Longitude: {summary.Longitude:###.###}");
                        Console.WriteLine($"      Elevation: {summary.Elevation:#####.##} meter(s)");
                    }
                }
                else
                {
                    for (uint i = 0; i < section.SubSectionCount; i++)
                    {
                        var subSection = section.GetSubSection(i);
                        Console.WriteLine($"  ==> Subsection[{i:D3}] has {subSection.NumRecords} record(s) in {subSection.DataSize} byte(s).");
                    }
                }
            }

            return file;
        }
    }
}
