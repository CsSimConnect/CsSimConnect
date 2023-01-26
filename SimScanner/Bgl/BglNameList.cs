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
using System.Collections.Generic;
using System.Text;

namespace SimScanner.Bgl
{
    public struct BglNameListHeader
    {
        public ushort Id;
        public uint Size;

        public ushort NumRegions;
        public ushort NumCountries;
        public ushort NumStates;
        public ushort NumCities;
        public ushort NumAirports;
        public ushort NumICAOs;

        public uint RegionOffset;
        public uint CountryOffset;
        public uint StateOffset;
        public uint CityOffset;
        public uint AirportOffset;
        public uint ICAOOffset;
    }

    public struct BglICAO
    {
        public const uint Size = 20;

        public byte RegionIndex;
        public byte CountryIndex;
        public ushort StateIndex;
        public ushort CityIndex;
        public ushort AirportIndex;
        public uint ICAO;
        public uint RegionIdent;
        public ushort LonQMID;
        public ushort LatQMID;
    }

    public class BglName
    {
        public string Region { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Airport { get; set; }
        public string ICAO { get; set; }

        public BglName() { }
        public BglName(string region, string country, string state, string city, string airport, string icao)
        {
            Region = region;
            Country = country;
            State = state;
            City = city;
            Airport = airport;
            ICAO = icao;
        }
    }

    public class BglNameList
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(BglNameList));

        internal BglSubSection subSection;
        private BglNameListHeader header;
        public BglNameListHeader Header => header;

        public List<string> Regions { get; init; } = new();
        public List<string> Countries { get; init; } = new();
        public List<string> Cities { get; init; } = new();
        public List<string> States { get; init; } = new();
        public List<string> Airports { get; init; } = new();
        public List<BglName> Names { get; init; } = new();

        public static string ToString<T>(List<T> list)
        {
            StringBuilder bld = new();

            bld.Append('[');
            foreach (var elem in list)
            {
                if (bld.Length > 1)
                {
                    bld.Append(", '");
                }
                else
                {
                    bld.Append("'");
                }
                bld.Append(elem.ToString()).Append("'");
            }
            return bld.Append(']').ToString();
        }

        internal BglNameList(BglSubSection subSection)
        {
            this.subSection = subSection;

            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader.Read(out header.Id).Read(out header.Size)
                .Read(out header.NumRegions)
                .Read(out header.NumCountries)
                .Read(out header.NumStates)
                .Read(out header.NumCities)
                .Read(out header.NumAirports)
                .Read(out header.NumICAOs)
                .Read(out header.RegionOffset)
                .Read(out header.CountryOffset)
                .Read(out header.StateOffset)
                .Read(out header.CityOffset)
                .Read(out header.AirportOffset)
                .Read(out header.ICAOOffset);

            if (log.IsTraceEnabled)
            {
                for (uint x = 0; x < 0x0200; x += 16)
                {
                    log.Trace?.Log($"{x:X4}: {reader.HexDump(x, 16)}");
                }
            }

            ReadList(reader, Regions, header.NumRegions, header.RegionOffset);
            log.Trace?.Log($"Read {header.NumRegions} region name(s): {ToString(Regions)}.");
            ReadList(reader, Countries, header.NumCountries, header.CountryOffset);
            log.Trace?.Log($"Read {header.NumCountries} country name(s): {ToString(Countries)}.");
            ReadList(reader, States, header.NumStates, header.StateOffset);
            log.Trace?.Log($"Read {header.NumStates} state name(s): {ToString(States)}.");
            ReadList(reader, Cities, header.NumCities, header.CityOffset);
            log.Trace?.Log($"Read {header.NumCities} city name(s): {ToString(Cities)}.");
            ReadList(reader, Airports, header.NumAirports, header.AirportOffset);
            log.Trace?.Log($"Read {header.NumAirports} airport name(s): {ToString(Airports)}.");

            if (header.NumICAOs > 0)
            {
                reader.Seek(header.ICAOOffset);
                BglICAO icao;
                for (uint i = header.NumICAOs; i > 0; i--)
                {
                    reader.Read(out icao, BglICAO.Size);
                    BglName name = new(Regions[icao.RegionIndex],
                                       Countries[icao.CountryIndex],
                                       States[(icao.StateIndex & 0xfff0) >> 4],
                                       Cities[icao.CityIndex],
                                       Airports[icao.AirportIndex],
                                       BglAirport.DecodeName(icao.ICAO));
                    Names.Add(name);
                    log.Trace?.Log($"  ==> {name.ICAO}: '{name.Airport}', city {name.City}, state {name.State}, country {name.Country}, region {name.Region}.");
                }
            }
            else
            {
                log.Trace?.Log($"No ICAO entries in NameList.");
            }
        }

        private void ReadList(BinSection reader, List<string> list, ushort num, uint offset)
        {
            if (num > 0)
            {
                reader.Seek(offset);
                uint[] offsets = new uint[num];
                for (uint i = 0; i < num; i++)
                {
                    reader.Read(out offsets [i]);
                }
                long start = reader.Position;
                for (uint i = 0; i < num; i++)
                {
                    log.Trace?.Log($"Reading a string starting at offset {start+offsets[i]:X4}.");
                    reader.Seek(start+offsets[i]).Read(out string s);
                    list.Add(s);
                }
            }
        }
    }
}
