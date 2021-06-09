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
                using var reader = section.file.File.Section(section.SubSectionHeaderOffset(index), BglSubSectionHeader16.Size);
                reader.Read(out BglSubSectionHeader16 header, BglSubSectionHeader16.Size);

                QMID1 = header.QMID1;
                QMID2 = header.QMID2;
                DataOffset = header.DataOffset;
                DataSize = header.DataSize;
            }
            else if (section.Header.SubSectionSize == BglSubSectionHeader20.Size)
            {
                using var reader = section.file.File.Section(section.SubSectionHeaderOffset(index), BglSubSectionHeader20.Size);
                reader.Read(out BglSubSectionHeader20 header, BglSubSectionHeader20.Size);
                QMID1 = header.QMID1;
                QMID2 = header.QMID2;
                NumRecords = header.NumRecords;
                DataOffset = header.DataOffset;
                DataSize = header.DataSize;
            }
        }

        public BglAirport Airport => section.Type != SectionType.Airport ? null : (new(this));
    }
}
