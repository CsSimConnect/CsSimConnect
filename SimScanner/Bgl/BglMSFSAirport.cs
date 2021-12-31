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
using System.Text;

namespace SimScanner.Bgl
{
    public struct BglMSFSAirportHeader
    {
        public const uint Size = 0x44;

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
        public uint Unknown0,Unknown1,Unknown2;
        public float MagneticVariance;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public uint FuelAvailability;
        public byte Unknown3;
        public byte INT;
        public byte Flags;
        public ushort Unknown4;
        public byte OnlyAddIfReplace;
        public byte Unknown5;
        public byte ApplyFlatten;
        public uint Unknown6, Unknown7;

        public int NumAprons => EncodedNumAprons & 0x7f;
        public bool Deleted => (EncodedNumAprons & 0x80) != 0;
    }

    public enum MSFSAirportRecordId
    {
        Null = 0x0000,
        Start = 0x0011,
        Com = 0x0012,
        Name = 0x0019,
        TaxiwayPoint = 0x001a,
        TaxiName = 0x001d,
        Waypoint = 0x0022,
        Approach = 0x0024,
        Helipad = 0x0026,
        ApronEdgeLights = 0x0031,
        DeleteAirport = 0x0033,
        BlastFence = 0x0038,
        BoundaryFence = 0x0039,
        Departure = 0x0042,
        Arrival = 0x0048,
        LightSupport = 0x0057,
        Tower = 0x0066,
        Runway = 0x00ce,
        PaintedLine = 0x00cf,
        Apron = 0x00d3,
        TaxiwayPath = 0x00d4,
        PaintedHatchedArea = 0x00d8,
        TaxiwaySign = 0x00d9,
        TaxiwayParkingMfgrName = 0x00dd,
        Jetway = 0x00de,
        TaxiwayParking = 0x00e7,
        ProjectedMesh = 0x00e8,
        GroundMergingTransfer = 0x00e9,
    }

    public class BglMSFSAirport : BglAirport
    {
        private static readonly Logger log = Logger.GetLogger(typeof(BglMSFSAirport));

        private BglMSFSAirportHeader header;
        public BglMSFSAirportHeader Header => header;

        public override string ICAO => DecodeName(header.EncodedICAO);
        public override string RegionCode => DecodeName(header.EncodedRegionIdent, false);
        public override double Latitude => 90.0 - header.Latitude * (180.0 / (2 * 0x10000000));
        public override double Longitude => -180.0 + (header.Longitude * (360.0 / (3 * 0x10000000)));
        public override double Altitude => header.Altitude / 1000.0;

        internal BglMSFSAirport(BglSubSection subSection, long pos)
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
                .Read(out header.Unknown0)
                .Read(out header.Unknown1)
                .Read(out header.Unknown2)
                .Read(out header.MagneticVariance)
                .Read(out header.EncodedICAO)
                .Read(out header.EncodedRegionIdent)
                .Read(out header.FuelAvailability)
                .Read(out header.Unknown3)
                .Read(out header.INT)
                .Read(out header.Flags)
                .Read(out header.Unknown4)
                .Read(out header.OnlyAddIfReplace)
                .Read(out header.Unknown5)
                .Read(out header.ApplyFlatten)
                .Read(out header.Unknown6)
                .Read(out header.Unknown7);

            log.Debug?.Log($"Reading MSFS airport record for {ICAO}");
            log.Trace?.Log($"Airport Record has id 0x{header.Id:X4}, size {header.TotalSize}.");

            uint recNum = 0;
            pos += BglMSFSAirportHeader.Size;
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

                MSFSAirportRecordId recordType = (MSFSAirportRecordId)id;
                switch (recordType)
                {
                    case MSFSAirportRecordId.Name:
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

                    case MSFSAirportRecordId.Tower:
                        break;

                    case MSFSAirportRecordId.DeleteAirport:
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

                    case MSFSAirportRecordId.Com:
                        break;

//                    case SectionType.ApronDetail:
//                        NumApronDetailRecords += 1;
//                        reader.Skip(size - 6);
//                        break;

                    case MSFSAirportRecordId.ApronEdgeLights:
                        NumApronEdgeLightRecords += 1;
                        break;

//                    case SectionType.ApronSurface:
//                        NumApronSurfaceRecords += 1;
//                        reader.Skip(size - 6);
//                        break;

                    case MSFSAirportRecordId.TaxiwayPoint:
                        break;

                    case MSFSAirportRecordId.TaxiwayPath:
                        break;

                    case MSFSAirportRecordId.TaxiName:
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

                    case MSFSAirportRecordId.TaxiwayParking:
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

                            const uint numberMarkingSkip = 0x14;
                            reader.Skip(numberMarkingSkip); // Number marking
                            recordPos += numberMarkingSkip;
                        }
                        break;

                    case MSFSAirportRecordId.Helipad:
                        NumHelipads += 1;
                        break;

                    case MSFSAirportRecordId.Jetway:
                        NumJetways += 1;
                        break;

                    case MSFSAirportRecordId.Approach:
                        NumApproaches += 1;
                        break;

                    case MSFSAirportRecordId.Waypoint:
                        break;

                    case MSFSAirportRecordId.BlastFence:
                    case MSFSAirportRecordId.BoundaryFence:
                        break;

                    case MSFSAirportRecordId.Start:
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
