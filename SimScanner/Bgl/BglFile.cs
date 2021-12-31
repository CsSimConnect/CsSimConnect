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

using System;
using System.IO;
using System.Collections.Generic;

namespace SimScanner.Bgl
{

    public struct BglHeader
    {
        public const uint Size = 56;
        public const uint Magic1Value = 0x19920201;
        public const uint Magic2Value = 0x08051803;

        public uint Magic1;
        public uint HeaderSize;
        public uint DateTimeLow;
        public uint DateTimeHigh;
        public uint Magic2;
        public uint SectionCount;
        public uint QMID1;
        public uint QMID2;
        public uint QMID3;
        public uint QMID4;
        public uint QMID5;
        public uint QMID6;
        public uint QMID7;
        public uint QMID8;
    }

    public class BglFile : IDisposable
    {
        public string Name { get; init; }
        public BinFile MappedFile { get; init; }
        public long Size { get; init; }

        private readonly BglHeader bglHeader;
        public BglHeader Header => bglHeader;

        public bool Valid => (bglHeader.Magic1 == BglHeader.Magic1Value) && (bglHeader.Magic2 == BglHeader.Magic2Value) && (bglHeader.HeaderSize == BglHeader.Size);
        public uint NumSections => bglHeader.SectionCount;
        public DateTime FileTime => DateTime.FromFileTime((((long)bglHeader.DateTimeHigh) << 32) | ((long)bglHeader.DateTimeLow));

        public readonly List<BglSection> Sections = new();

        public long SectionHeaderOffset(uint index)
        {
            return BglHeader.Size + (index * BglSectionHeader.Size);
        }

        public BglFile(string filename)
        {
            Name = filename;
            MappedFile = new BinFile(filename);
            Size = new FileInfo(filename).Length;

            using (BinSection header = MappedFile.Section(0, BglHeader.Size))
            {
                header
                .Read(out bglHeader.Magic1)
                .Read(out bglHeader.HeaderSize)
                .Read(out bglHeader.DateTimeLow)
                .Read(out bglHeader.DateTimeHigh)
                .Read(out bglHeader.Magic2)
                .Read(out bglHeader.SectionCount)
                .Read(out bglHeader.QMID1)
                .Read(out bglHeader.QMID2)
                .Read(out bglHeader.QMID3)
                .Read(out bglHeader.QMID4)
                .Read(out bglHeader.QMID5)
                .Read(out bglHeader.QMID6)
                .Read(out bglHeader.QMID7)
                .Read(out bglHeader.QMID8);
;
            }

            for (uint index = 0; index < NumSections; index++)
            {
                Sections.Add(new(this, index));
            }
        }

        public void Dispose()
        {
            MappedFile.Dispose();
        }
    }

}
