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

namespace SimScanner.Bgl
{

    public struct BglSubSectionHeader16
    {
        public const uint Size = 16;

        public readonly uint QMID1;
        public readonly uint QMID2;
        public readonly uint DataOffset;
        public readonly uint DataSize;
    }
    public struct BglSubSectionHeader20
    {
        public const uint Size = 20;

        public readonly uint QMID1;
        public readonly uint QMID2;
        public readonly uint NumRecords;
        public readonly uint DataOffset;
        public readonly uint DataSize;
    }

    public class BglSubSection
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(BglSubSection));

        internal BglSection section;
        public uint Index { get; init; }

        public uint Size { get; init; }
        public uint QMID1 { get; init; }
        public uint QMID2 { get; init; }
        public uint NumRecords { get; init; }
        public uint DataOffset { get; init; }
        public uint DataSize { get; init; }

        internal BglSubSection(BglSection section, uint index)
        {
            this.section = section;
            Index = index;

            if (section.Header.SubSectionSize == BglSubSectionHeader16.Size)
            {
                using var reader = section.file.MappedFile.Section(section.SubSectionHeaderOffset(index), BglSubSectionHeader16.Size);
                long pos = reader.Position;
                reader.Read(out BglSubSectionHeader16 header, BglSubSectionHeader16.Size);

                QMID1 = header.QMID1;
                QMID2 = header.QMID2;
                DataOffset = header.DataOffset;
                DataSize = header.DataSize;
                log.Trace?.Log($"SubSection Header at 0x{pos:X4}: NumRecords={NumRecords}, DataOffset=0x{DataOffset:X4}, DataSize=0x{DataSize:X4}");
            }
            else if (section.Header.SubSectionSize == BglSubSectionHeader20.Size)
            {
                using var reader = section.file.MappedFile.Section(section.SubSectionHeaderOffset(index), BglSubSectionHeader20.Size);
                long pos = reader.Position;
                reader.Read(out BglSubSectionHeader20 header, BglSubSectionHeader20.Size);
                QMID1 = header.QMID1;
                QMID2 = header.QMID2;
                NumRecords = header.NumRecords;
                DataOffset = header.DataOffset;
                DataSize = header.DataSize;
                log.Trace?.Log($"SubSection Header at 0x{pos:X}: NumRecords={NumRecords}, DataOffset={DataOffset}, DataSize={DataSize}");
            }
            else
            {
                log.Error?.Log($"SubSectionSize == {section.Header.SubSectionSize}?");
            }
            CollectAirports();
            CollectAirportSummaries();
        }

        // Airport

        public List<BglAirport> Airports { get; init; } = new();

        // Lookahead...

        private ushort GetIdAndSize(long pos, out uint size)
        {
            using var reader = section.file.MappedFile.Section(DataOffset, DataSize);
            reader.Seek(pos).Read(out ushort id).Read(out size);
            log.Trace?.Log($"Found a record at 0x{pos:X8} with id 0x{id:X4} of size 0x{size:X8}");

            return id;
        }

        private RecordId BglRecordId(long pos, out uint size)
        {
            return (RecordId)GetIdAndSize(pos, out size);
        }

        private void CollectAirports()
        {
            if (!section.IsAirport)
            {
                return;
            }
            long pos = 0;
            while (pos < DataSize)
            {
                RecordId type = BglRecordId(pos, out uint recordSize);
                if (type == RecordId.AirportMSFS)
                {
                    Airports.Add(new BglMSFSAirport(this, pos));
                }
                else if (type == RecordId.AirportFSX)
                {
                    Airports.Add(new BglFSXAirport(this, pos));
                }
                else if (type == RecordId.AirportP3D)
                {
                    Airports.Add(new BglP3DAirport(this, pos));
                }
                else
                {
                    log.Error?.Log($"Found a {type} record, id 0x{((uint)type):X4}, while expecting only airports");
                }
                pos += recordSize;
            }
        }

        // NameList

        public BglNameList NameList => !section.IsNameList ? null : (new(this));

        // AirportSummary

        public List<BglAirportSummary> AirportSummaries { get; init; } = new();

        private void CollectAirportSummaries()
        {
            if (!section.IsAirportSummary)
            {
                return;
            }
            long pos = 0;
            while (pos < DataSize)
            {
                SectionType type = (SectionType)GetIdAndSize(pos, out uint recordSize);
                if (type == SectionType.AirportSummaryFSX)
                {
                    AirportSummaries.Add(new BglFSXAirportSummary(this, pos));
                }
                else if (type == SectionType.AirportSummaryP3D)
                {
                    AirportSummaries.Add(new BglP3DAirportSummary(this, pos));
                }
                else if (type == SectionType.AirportSummaryMSFS)
                {
                    AirportSummaries.Add(new BglMSFSAirportSummary(this, pos));
                }
                else
                {
                    log.Error?.Log($"Found a {type} record, id 0x{((uint)type):X4}, while expecting only AirportSummaries");
                }
                pos += recordSize;
            }
        }

    }
}
