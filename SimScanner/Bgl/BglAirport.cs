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
using SimScanner.Model;
using System.Collections.Generic;
using System.Text;

namespace SimScanner.Bgl
{
    public struct BglAirportHeader
    {
        public const uint Size = 0x38;

        public ushort Id;
        public uint TotalSize;
        public byte NumRunways;
        public byte NumComs;
        public byte NumStarts;
        public byte NumApproaches;
        public byte EncodedNumAprons;
        public byte NumHeliPads;
        public uint Longitude;
        public uint Latitude;
        public uint Altitude;
        public uint TowerLongitude;
        public uint TowerLatitude;
        public uint TowerAltitude;
        public float MagneticVariance;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public uint FuelAvailability;

        public int NumAprons => EncodedNumAprons & 0x7f;
        public bool Deleted => (EncodedNumAprons & 0x80) != 0;
    }

    public class BglAirport
    {
        private static readonly Logger log = Logger.GetLogger(typeof(BglAirport));

        internal BglSubSection subSection;
        private BglAirportHeader header;
        public BglAirportHeader Header => header;

        public string Name { get; init; }

        public uint NumRunwayStarts { get; init; }
        public uint NumHelipads { get; init; }
        public uint NumJetways { get; set; }

        public uint NumApronSurfaceRecords { get; init; }
        public uint NumApronDetailRecords { get; init; }
        public uint NumApronEdgeLightRecords { get; init; }

        public uint NumApproaches { get; set; }

        public string ICAO => DecodeName(header.EncodedICAO);
        public string RegionCode => DecodeName(header.EncodedRegionIdent, false);
        public double Latitude => 90.0 - header.Latitude * (180.0 / (2 * 0x10000000));
        public double Longitude => -180.0 + (header.Longitude * (360.0 / (3 * 0x10000000)));

        private readonly List<string> taxiways = new();
        public List<string> Taxiways => taxiways;
        private readonly List<Parking> parkings = new();
        public List<Parking> Parkings => parkings;

        internal BglAirport(BglSubSection subSection)
        {
            this.subSection = subSection;

            using var reader = subSection.section.file.File.Section(subSection.DataOffset, subSection.DataSize);

            reader
                .Read(out header.Id)
                .Read(out header.TotalSize, sizeof(uint))
                .Read(out header.NumRunways, sizeof(byte))
                .Read(out header.NumComs, sizeof(byte))
                .Read(out header.NumStarts, sizeof(byte))
                .Read(out header.NumApproaches, sizeof(byte))
                .Read(out header.EncodedNumAprons, sizeof(byte))
                .Read(out header.NumHeliPads, sizeof(byte))
                .Read(out header.Longitude, sizeof(uint))
                .Read(out header.Latitude, sizeof(uint))
                .Read(out header.Altitude, sizeof(uint))
                .Read(out header.TowerLongitude, sizeof(uint))
                .Read(out header.TowerLatitude, sizeof(uint))
                .Read(out header.TowerAltitude, sizeof(uint))
                .Read(out header.MagneticVariance, sizeof(float))
                .Read(out header.EncodedICAO, sizeof(uint))
                .Read(out header.EncodedRegionIdent, sizeof(uint))
                .Read(out header.FuelAvailability, sizeof(uint))
                .Skip(sizeof(byte)).Skip(sizeof(byte)).Skip(sizeof(short));

            uint pos = 0x0038;
            while (pos < (subSection.DataSize-6))
            {
                log.Trace?.Log($"Reading subrecord ({subSection.DataSize - pos} byte(s) left)");
                ushort id;
                uint size;

                reader.Read(out id, sizeof(ushort)).Read(out size, sizeof(uint));
                log.Trace?.Log($"Reading subrecord (id={id}, size={size} byte(s))");
                if (id == 0)
                {
                    log.Trace?.Log($"Section 0, stopping scan.");
                    break;
                }
                SectionType recordType = (SectionType)id;
                switch (recordType)
                {
                    case SectionType.AirportName:
                        StringBuilder bld = new();
                        uint i = 6;
                        while (i < size)
                        {
                            byte b;
                            reader.Read(out b, sizeof(byte));
                            i++;
                            if (b == 0)
                            {
                                break;
                            }
                            bld.Append((char)b);
                        }
                        Name = bld.ToString();
                        reader.Skip(size - i);
                        break;

                    case SectionType.AirportTowerScenery:
                        reader.Skip(size-6);
                        break;

                    case SectionType.DeleteAirport:
                        ushort flags;
                        byte numRunways;
                        byte numStarts;
                        byte numFrequencies;
                        reader.Read(out flags, sizeof(ushort))
                            .Read(out numRunways, sizeof(byte))
                            .Read(out numStarts, sizeof(byte))
                            .Read(out numFrequencies, sizeof(byte))
                            .Skip(sizeof(byte))
                            .Skip(size - 0x0c);
                        break;

                    case SectionType.Com:
                        reader.Skip(size - 6);
                        break;

                    case SectionType.ApronDetail:
                        NumApronDetailRecords += 1;
                        reader.Skip(size - 6);
                        break;

                    case SectionType.ApronEdgeLight:
                        NumApronEdgeLightRecords += 1;
                        reader.Skip(size - 6);
                        break;

                    case SectionType.ApronSurface:
                        NumApronSurfaceRecords += 1;
                        reader.Skip(size - 6);
                        break;

                    case SectionType.TaxiwayPoint:
                        reader.Skip(size - 6);
                        break;

                    case SectionType.TaxiwayPath:
                        reader.Skip(size - 6);
                        break;

                    case SectionType.TaxiwayName:
                        ushort numNames;
                        reader.Read(out numNames, sizeof(ushort));
                        uint recordPos = 8;
                        for (i = 0; i < numNames; i++)
                        {
                            byte[] b = new byte[8];
                            for (uint j = 0; j < b.Length; j++) reader.Read(out b[j], sizeof(byte));
                            recordPos += 8;

                            bld = new();
                            for (uint j = 0; j < b.Length; j++)
                            {
                                if (b[j] == 0)
                                {
                                    break;
                                }
                                bld.Append((char)b[j]);
                            }

                            string name = bld.ToString();
                            if (name != "")
                            {
                                taxiways.Add(name);
                            }
                        }
                        break;

                    case SectionType.TaxiwayParking:
                        ushort numParkings;
                        reader.Read(out numParkings, sizeof(ushort));

                        recordPos = 8;
                        for (i = 0; i < numParkings; i++)
                        {
                            uint info;
                            float radius, heading, teeOffset1, teeOffset2, teeOffset3, teeOffset4;
                            uint lon, lat;
                            reader.Read(out info, sizeof(uint))
                                .Read(out radius, sizeof(float))
                                .Read(out heading, sizeof(float))
                                .Read(out teeOffset1, sizeof(float))
                                .Read(out teeOffset2, sizeof(float))
                                .Read(out teeOffset3, sizeof(float))
                                .Read(out teeOffset4, sizeof(float))
                                .Read(out lon, sizeof(uint))
                                .Read(out lat, sizeof(uint));
                            recordPos += 9 * 4;
                            Parking parking = new();
                            parking.Number = (info >> 12) & 0x0fff;
                            parking.HasPushbackLeft = (info & 0x40) != 0;
                            parking.HasPushbackRight = (info & 0x80) != 0;
                            uint gateName = (info & 0x3f);
                            parking.Name = gateName switch
                            {
                                0 => "",
                                1 => "Parking",
                                2 => "N Parking",
                                3 => "NE Parking",
                                4 => "E Parking",
                                5 => "SE Parking",
                                6 => "S Parking",
                                7 => "SW Parking",
                                8 => "W Parking",
                                9 => "NW Parking",
                                10 => "Gate",
                                11 => "Dock",
                                _ => "Gate " + ((char) (53 + gateName))
                            };
                            parking.Heading = heading;
                            parking.Latitude = 90.0 - lat * (180.0 / (2 * 0x10000000));
                            parking.Longitude = -180.0 + (lon * (360.0 / (3 * 0x10000000)));

                            Parkings.Add(parking);

                            uint numAirlineDesignators = (info >> 24);
                            uint airlineDesignatorsSkip = 4 * numAirlineDesignators;
                            reader.Skip(airlineDesignatorsSkip);
                            recordPos += airlineDesignatorsSkip;
                        }
                        reader.Skip(size - recordPos);
                        break;

                    case SectionType.Helipad:
                        NumHelipads += 1;
                        reader.Skip(size - 6);
                        break;

                    case SectionType.SceneryObject:
                        reader.Skip(size - 6);
                        break;

                    case SectionType.Jetway:
                        NumJetways += 1;
                        reader.Skip(size - 6);
                        break;

                    case SectionType.Approach:
                        NumApproaches += 1;
                        reader.Skip(size - 6);
                        break;

                    case SectionType.Waypoint:
                        reader.Skip(size - 6);
                        break;

                    case SectionType.BlastFence:
                    case SectionType.BoundaryFence:
                        reader.Skip(size - 6);
                        break;

                    case SectionType.RunwayStart:
                        NumRunwayStarts += 1;
                        reader.Skip(size - 6);
                        break;

                    default:
                        reader.Skip(size - 6);
                        break;
                }
                pos += size;
            }
        }

        public static string DecodeName(uint encoded, bool shift5 =true)
        {
            string result = "";

            if (shift5)
            {
                encoded >>= 5;
            }
            while (encoded != 0)
            {
                uint oneChar = encoded % 38;
                if (oneChar == 0)
                {
                    result = " " + result;
                }
                else if ((oneChar >= 2) && (oneChar <= 11))
                {
                    result = ((char)('0' + oneChar - 2)) + result;
                }
                else if ((oneChar >= 12) && (oneChar <= 37))
                {
                    result = ((char)('A' + oneChar - 12)) + result;
                }
                encoded /= 38;
            }
            return result;
        }
    }
}
