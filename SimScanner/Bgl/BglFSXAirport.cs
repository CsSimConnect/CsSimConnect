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
using System;
using System.Collections.Generic;
using System.Text;

namespace SimScanner.Bgl
{
    public struct BglFSXAirportHeader
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
        public int Longitude;
        public int Latitude;
        public int Altitude;
        public uint TowerLongitude;
        public uint TowerLatitude;
        public uint TowerAltitude;
        public float MagneticVariance;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public uint FuelAvailability;
        public byte UnKnown0;
        public byte INT;
        public ushort Unknown1;

        public int NumAprons => EncodedNumAprons & 0x7f;
        public bool Deleted => (EncodedNumAprons & 0x80) != 0;
    }

    public class BglFSXAirport : BglAirport
    {
        private static readonly Logger log = Logger.GetLogger(typeof(BglFSXAirport));

        private BglFSXAirportHeader header;
        public BglFSXAirportHeader Header => header;

        public override string ICAO => DecodeName(header.EncodedICAO);
        public override string RegionCode => DecodeName(header.EncodedRegionIdent, false);
        public override double Latitude => 90.0 - header.Latitude * (180.0 / (2 * 0x10000000));
        public override double Longitude => -180.0 + (header.Longitude * (360.0 / (3 * 0x10000000)));
        public override double Altitude => header.Altitude / 1000.0;

        internal BglFSXAirport(BglSubSection subSection, long pos)
        {
            this.subSection = subSection;

            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader
                .Seek(pos)
                .Read(out header.Id)
                .Read(out header.TotalSize)
                .Read(out header.NumRunways)
                .Read(out header.NumComs)
                .Read(out header.NumStarts)
                .Read(out header.NumApproaches)
                .Read(out header.EncodedNumAprons)
                .Read(out header.NumHeliPads)
                .Read(out header.Longitude)
                .Read(out header.Latitude)
                .Read(out header.Altitude)
                .Read(out header.TowerLongitude)
                .Read(out header.TowerLatitude)
                .Read(out header.TowerAltitude)
                .Read(out header.MagneticVariance)
                .Read(out header.EncodedICAO)
                .Read(out header.EncodedRegionIdent)
                .Read(out header.FuelAvailability)
                .Read(out header.UnKnown0)
                .Read(out header.INT)
                .Read(out header.Unknown1);

            log.Debug?.Log($"Reading FSX/P3D airport record for {ICAO}");
            log.Trace?.Log($"Airport Record has id 0x{header.Id:X4}, size {header.TotalSize}.");
            uint recNum = 0;
            pos += BglFSXAirportHeader.Size;
            log.Trace?.Log($"Starting to parse subrecords, pos=0x{pos:X4}, dataSize=0x{subSection.DataSize:X4}, reader position=0x{reader.Position:X4}.");

            while (pos < (subSection.DataSize-6))
            {
                recNum += 1;
                log.Trace?.Log($"Starting to look at subrecord {recNum}, pos=0x{pos:X4}, dataSize=0x{subSection.DataSize:X4}, reader position=0x{reader.Position:X4}.");

                ushort id;
                uint size;

                try
                {
                    reader.Seek(pos).Read(out id).Read(out size);
                }
                catch (Exception e)
                {
                    log.Fatal?.Log($"Failed to read start of subrecord {recNum}, subsection {subSection.Index} of section {subSection.section.Index}, airport {DecodeName(header.EncodedICAO)}.");
                    throw e;
                }
                log.Trace?.Log($"Reading subrecord {recNum} (pos=0x{pos:X4}, id=0x{id:X4} ({((MSFSAirportRecordId)id).ToString()}), size={size} (0x{size:X4}) byte(s) {subSection.DataSize - pos} byte(s) left)");
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
                            reader.Read(out b);
                            i++;
                            if (b == 0)
                            {
                                break;
                            }
                            bld.Append((char)b);
                        }
                        Name = bld.ToString();
                        break;

                    case SectionType.AirportTowerScenery:
                        break;

                    case SectionType.DeleteAirport:
                        ushort flags;
                        byte numRunways;
                        byte numStarts;
                        byte numFrequencies;
                        reader.Read(out flags)
                            .Read(out numRunways)
                            .Read(out numStarts)
                            .Read(out numFrequencies)
                            .Skip(sizeof(byte))
                            .Skip(size - 0x0c);
                        break;

                    case SectionType.Com:
                        break;

                    case SectionType.ApronDetail:
                        NumApronDetailRecords += 1;
                        break;

                    case SectionType.ApronEdgeLight:
                        NumApronEdgeLightRecords += 1;
                        break;

                    case SectionType.ApronSurface:
                        NumApronSurfaceRecords += 1;
                        break;

                    case SectionType.TaxiwayPointFSX:
                        break;

                    case SectionType.TaxiwayPath:
                        break;

                    case SectionType.TaxiwayName:
                        ushort numNames;
                        reader.Read(out numNames);
                        uint recordPos = 8;
                        for (i = 0; i < numNames; i++)
                        {
                            byte[] b = new byte[8];
                            for (uint j = 0; j < b.Length; j++) reader.Read(out b[j]);
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
                                Taxiways.Add(name);
                            }
                        }
                        break;

                    case SectionType.TaxiwayParkingFSX:
                        reader.Read(out ushort numParkings);
                        log.Trace?.Log($"We have {numParkings} parking entries");

                        recordPos = 8;
                        for (i = 0; i < numParkings; i++)
                        {
                            reader
                                .Read(out uint info)
                                .Read(out float radius)
                                .Read(out float heading)
                                .Read(out float teeOffset1)
                                .Read(out float teeOffset2)
                                .Read(out float teeOffset3)
                                .Read(out float teeOffset4)
                                .Read(out int lon)
                                .Read(out int lat);
                            recordPos += 9 * 4;
                            log.Trace?.Log($"info=0x{info:X8}, heading={heading:###.##}");

                            Parking parking = new();
                            parking.Number = (info >> 12) & 0x0fff;
                            uint numAirlineDesignators = (info >> 24);

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
                            parking.Type = ((info >> 8) & 0x0f) switch
                            {
                                0 => "None",
                                1 => "Ramp GA",
                                2 => "Ramp GA small",
                                3 => "Ramp GA medium",
                                4 => "Ramp GA large",
                                5 => "Ramp cargo",
                                6 => "Ramp mil cargo",
                                7 => "Ramp mil combat",
                                8 => "Gate small",
                                9 => "Gate medium",
                                10 => "Gate heavy",
                                11 => "Docker GA",
                                12 => "Fuel",
                                13 => "Vehicle",
                                14 => "Ramp GA extra",
                                15 => "Gate extra",
                                _ => "Unknown"
                            };
                            log.Trace?.Log($"name='{parking.Name}', number={parking.Number}");
                            parking.Heading = heading;
                            parking.Latitude = 90.0 - lat * (180.0 / (2 * 0x10000000));
                            parking.Longitude = -180.0 + (lon * (360.0 / (3 * 0x10000000)));

                            Parkings.Add(parking);

                            uint airlineDesignatorsSkip = 4 * numAirlineDesignators;
                            reader.Skip(airlineDesignatorsSkip);
                            recordPos += airlineDesignatorsSkip;
                        }
                        break;

                    case SectionType.TaxiwayParkingP3D:
                        reader.Read(out numParkings);
                        log.Trace?.Log($"We have {numParkings} parking entries");

                        recordPos = 8;
                        for (i = 0; i < numParkings; i++)
                        {
                            reader
                                .Read(out uint info)
                                .Read(out float radius)
                                .Read(out float heading)
                                .Read(out float teeOffset1)
                                .Read(out float teeOffset2)
                                .Read(out float teeOffset3)
                                .Read(out float teeOffset4)
                                .Read(out int lon)
                                .Read(out int lat);
                            recordPos += 9 * 4;
                            log.Trace?.Log($"info=0x{info:X8}, heading={heading:###.##}");

                            Parking parking = new();
                            parking.Number = (info >> 12) & 0x0fff;
                            uint numAirlineDesignators = (info >> 24);

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
                                _ => "Gate " + ((char)(53 + gateName))
                            };
                            parking.Type = ((info >> 8) & 0x0f) switch
                            {
                                0 => "None",
                                1 => "Ramp GA",
                                2 => "Ramp GA small",
                                3 => "Ramp GA medium",
                                4 => "Ramp GA large",
                                5 => "Ramp cargo",
                                6 => "Ramp mil cargo",
                                7 => "Ramp mil combat",
                                8 => "Gate small",
                                9 => "Gate medium",
                                10 => "Gate heavy",
                                11 => "Docker GA",
                                12 => "Fuel",
                                13 => "Vehicle",
                                14 => "Ramp GA extra",
                                15 => "Gate extra",
                                _ => "Unknown"
                            };
                            log.Trace?.Log($"name='{parking.Name}', number={parking.Number}");
                            parking.Heading = heading;
                            parking.Latitude = 90.0 - lat * (180.0 / (2 * 0x10000000));
                            parking.Longitude = -180.0 + (lon * (360.0 / (3 * 0x10000000)));

                            Parkings.Add(parking);

                            uint airlineDesignatorsSkip = 4 * numAirlineDesignators;
                            reader.Skip(airlineDesignatorsSkip);
                            recordPos += airlineDesignatorsSkip;

                            reader.Skip(4); // unknown
                        }
                        break;

                    case SectionType.Helipad:
                        NumHelipads += 1;
                        break;

                    case SectionType.SceneryObject:
                        break;

                    case SectionType.Jetway:
                        NumJetways += 1;
                        break;

                    case SectionType.Approach:
                        NumApproaches += 1;
                        break;

                    case SectionType.Waypoint:
                        break;

                    case SectionType.BlastFence:
                    case SectionType.BoundaryFence:
                        break;

                    case SectionType.RunwayStart:
                        NumRunwayStarts += 1;
                        break;

                    default:
                        break;
                }
                log.Trace?.Log($"Updating pos {size} byte(s) (0x{size:X4}) from 0x{pos:X4}, reader position=0x{reader.Position:X4}");
                pos += size;
            }
        }
    }
}
