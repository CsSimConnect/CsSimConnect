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
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace SimScanner.Bgl
{

    public struct BglHeader
    {
        public const uint Size = 56;
        public const uint Magic1Value = 0x19920201;
        public const uint Magic2Value = 0x08051803;

        public readonly uint Magic1;
        public readonly uint HeaderSize;
        public readonly uint DateTimeLow;
        public readonly uint DateTimeHigh;
        public readonly uint Magic2;
        public readonly uint SectionCount;
        public readonly uint QMID1;
        public readonly uint QMID2;
        public readonly uint QMID3;
        public readonly uint QMID4;
        public readonly uint QMID5;
        public readonly uint QMID6;
        public readonly uint QMID7;
        public readonly uint QMID8;
    }

    public class BglFile : IDisposable
    {
        public string Name { get; init; }
        public BinFile File { get; init; }

        private readonly BglHeader bglHeader;
        public BglHeader Header => bglHeader;

        public bool Valid => (bglHeader.Magic1 == BglHeader.Magic1Value) && (bglHeader.Magic2 == BglHeader.Magic2Value) && (bglHeader.HeaderSize == BglHeader.Size);
        public uint NumSections => bglHeader.SectionCount;
        public DateTime FileTime => DateTime.FromFileTime((((long)bglHeader.DateTimeHigh) << 32) | ((long)bglHeader.DateTimeLow));

        public readonly List<BglSection> Sections = new();

        internal long SectionHeaderOffset(uint index)
        {
            return BglHeader.Size + (index * BglSectionHeader.Size);
        }

        public BglFile(string filename)
        {
            Name = filename;
            File = new BinFile(filename);

            using (BinSection header = File.Section(0, BglHeader.Size))
            {
                header.Read(out bglHeader, BglHeader.Size);
            }

            for (uint index = 0; index < NumSections; index++)
            {
                Sections.Add(new(this, index));
            }
        }

        public void Dispose()
        {
            File.Dispose();
        }
    }

}
