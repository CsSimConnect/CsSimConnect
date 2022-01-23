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
using System.Runtime.InteropServices;
using System.Text;

namespace SimScanner.Bgl
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BglMSFSAirportHeader
    {
        public const uint Size = 0x44;

        public ushort Id;                   // 0x0000
        public uint TotalSize;              // 0x0002
        public byte NumRunways;             // 0x0006
        public byte NumComs;                // 0x0007
        public byte NumStarts;              // 0x0008
        public byte NumApproaches;          // 0x0009
        public byte LegacyNumAprons;       // 0x000A
        public byte NumHeliPads;            // 0x000B
        public int Longitude;               // 0x000C
        public int Latitude;                // 0x0010
        public int Altitude;                // 0x0014
        public int TowerLongitude;          // 0x0018
        public int TowerLatitude;           // 0x001C
        public int TowerAltitude;           // 0x0020
        public float MagneticVariance;      // 0x0024
        public uint EncodedICAO;            // 0x0028
        public uint EncodedRegionIdent;     // 0x002C
        public uint FuelAvailability;       // 0x0030
        public byte Unknown0;
        public byte INT;                    // 0x0035
        public byte Flags;                  // 0x0036
        public byte NumDepartures;          // 0x0037
        public byte OnlyAddIfReplace;       // 0x0038
        public byte NumArrivals;            // 0x0039
        public byte Unknown1;               // 0x003A
        public byte ApplyFlatten;           // 0x003B
        public ushort NumAprons;            // 0x003C
        public ushort NumPaintedLines;      // 0x003E
        public ushort NumPaintedPolygons;   // 0x0040
        public ushort NumPaintedHatchedAreas; // 0x0042
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

            reader.Seek(pos).Read(out header, BglMSFSAirportHeader.Size);

            log.Debug?.Log($"Airport Record has id 0x{header.Id:X4}, pos 0x{pos:X8}, size 0x{header.TotalSize:X8}, DataOffset 0x{subSection.DataOffset:X8}, DataSize 0x{subSection.DataSize:X8}.");
            log.Debug?.Log($"Reading MSFS airport record for {ICAO} (encoded 0x{header.EncodedICAO:X8})");

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
                log.Trace?.Log($"Reading subrecord {recNum} (pos=0x{pos:X4}, id=0x{id:X4} ({((RecordId)id).ToString()}), size={size} (0x{size:X4}) byte(s) {subSection.DataSize - pos} byte(s) left)");
                if (id == 0)
                {
                    log.Trace?.Log($"Section 0, stopping scan.");
                    break;
                }

                RecordId recordType = (RecordId)id;
                switch (recordType)
                {
                    case RecordId.AirportName:
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

                    case RecordId.DeleteAirport:
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

                    case RecordId.ApronEdgeLight:
                        NumApronEdgeLightRecords += 1;
                        break;

                    case RecordId.TaxiwayName:
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

                    case RecordId.TaxiwayParkingMSFS:
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

                    case RecordId.Helipad:
                        NumHelipads += 1;
                        break;

                    case RecordId.JetwayMSFS:
                        NumJetways += 1;
                        break;

                    case RecordId.Approach:
                        NumApproaches += 1;
                        break;

                    case RecordId.RunwayStart:
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
