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
using System.Runtime.InteropServices;
using System.Text;

namespace SimScanner.Bgl
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BglP3DAirportHeader
    {
        public const uint Size = 0x3C;

        public ushort Id;
        public uint TotalSize;
        public byte NumRunways;
        public byte NumComs;
        public byte NumStarts;
        public byte Unknown0;
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
        public uint TrafficScalar;
        public ushort NumApproaches;
        public ushort Unknown1;

        public int NumAprons => EncodedNumAprons & 0x7f;
        public bool Deleted => (EncodedNumAprons & 0x80) != 0;
    }

    public class BglP3DAirport : BglAirport
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(BglP3DAirport));

        private BglP3DAirportHeader header;
        public BglP3DAirportHeader Header => header;

        public override string ICAO => DecodeName(header.EncodedICAO);
        public override string RegionCode => DecodeName(header.EncodedRegionIdent, false);
        public override double Latitude => 90.0 - header.Latitude * (180.0 / (2 * 0x10000000));
        public override double Longitude => -180.0 + (header.Longitude * (360.0 / (3 * 0x10000000)));
        public override double Altitude => header.Altitude / 1000.0;

        internal BglP3DAirport(BglSubSection subSection, long pos)
        {
            this.subSection = subSection;

            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader.Seek(pos).Read(out header, BglP3DAirportHeader.Size);

            log.Debug?.Log($"Reading FSX/P3D airport record for {ICAO}");
            log.Trace?.Log($"Airport Record has id 0x{header.Id:X4}, starting pos 0x{pos:X8}, TotalSize 0x{header.TotalSize:X8}.");
            uint recNum = 0;
            pos += BglP3DAirportHeader.Size;
            log.Trace?.Log($"Starting to parse subrecords, pos=0x{pos:X4}, dataSize=0x{subSection.DataSize:X4}, reader position=0x{reader.Position:X4}.");

            while (pos < (subSection.DataSize-6))
            {
                recNum += 1;
                log.Trace?.Log($"Starting to look at subrecord {recNum}, pos=0x{pos:X4}, dataSize=0x{subSection.DataSize:X4}, reader position=0x{reader.Position:X4}.");

                BglObjectHeader subRecord;

                try
                {
                    reader.Seek(pos).Read(out subRecord, BglObjectHeader.HeaderSize);
                }
                catch (Exception e)
                {
                    log.Fatal?.Log($"Failed to read start of subrecord {recNum}, subsection {subSection.Index} of section {subSection.section.Index}, airport {DecodeName(header.EncodedICAO)}.");
                    throw e;
                }
                log.Trace?.Log($"Reading subrecord {recNum} (pos=0x{pos:X4}, real=0x{(pos+reader.Position):X8}, id=0x{subRecord.Id:X4} ({((RecordId)subRecord.Id).ToString()}), size={subRecord.Size} (0x{subRecord.Size:X4}) byte(s) {subSection.DataSize - pos} byte(s) left)");
                if (subRecord.Id == 0)
                {
                    log.Trace?.Log($"Section 0, stopping scan.");
                    break;
                }
                RecordId recordType = (RecordId)subRecord.Id;
                switch (recordType)
                {
                    case RecordId.AirportName:
                        reader.Read(out string name, subRecord.DataSize);
                        Name = name;
                        break;

                    case RecordId.ApronDetail:
                        NumApronDetailRecords += 1;
                        break;

                    case RecordId.ApronEdgeLight:
                        NumApronEdgeLightRecords += 1;
                        break;

                    case RecordId.ApronSurface:
                        NumApronSurfaceRecords += 1;
                        break;

                    case RecordId.TaxiwayName:
                        reader.Read(out TaxiName numNames, TaxiName.HeaderSize);

                        for (uint i = 0; i < numNames.NumNames; i++)
                        {
                            reader.Read(out string taxiwayName, subRecord.DataSize);
                            if (taxiwayName != "")
                            {
                                Taxiways.Add(taxiwayName);
                            }
                        }
                        break;

                    case RecordId.TaxiwayParkingFSX:
                    case RecordId.TaxiwayParkingP3D:
                        reader.Read(out TaxiwayParkingFSX parkingFSX, TaxiwayParkingFSX.RecordSize);
                        log.Trace?.Log($"We have {parkingFSX.NumParkings} parking entries");

                        for (uint i = 0; i < parkingFSX.NumParkings; i++)
                        {
                            log.Trace?.Log($"Reading parking from pos 0x{reader.Position:X8}.");
                            reader.Read(out TaxiwayParkingRecordFSX parkingRecord, TaxiwayParkingRecordFSX.RecordSize);
                            Parking parking = new();
                            parking.Number = parkingRecord.Number;
                            uint numAirlineDesignators = parkingRecord.NumAirlineDesignators;

                            parking.HasPushbackLeft = parkingRecord.PushbackType == PushbackType.Left || parkingRecord.PushbackType == PushbackType.Both;
                            parking.HasPushbackRight = parkingRecord.PushbackType == PushbackType.Right || parkingRecord.PushbackType == PushbackType.Both;
                            parking.Name = parkingRecord.Name.ToString().Replace('_', ' ');
                            parking.Type = parkingRecord.Type.ToString().Replace('_', ' ');
                            parking.Heading = parkingRecord.Heading;
                            parking.Latitude = parkingRecord.Latitude;
                            parking.Longitude = parkingRecord.Longitude;

                            Parkings.Add(parking);

                            reader
                                .Skip(sizeof(uint) * numAirlineDesignators)
                                .Skip(4); // unknown
                        }
                        break;

                    case RecordId.Helipad:
                        NumHelipads += 1;
                        break;

                    case RecordId.Jetway:
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
                log.Trace?.Log($"Updating pos {subRecord.Size} byte(s) (0x{subRecord.Size:X4}) from 0x{pos:X4}, reader position=0x{reader.Position:X4}");
                pos += subRecord.Size;
            }
        }
    }
}
