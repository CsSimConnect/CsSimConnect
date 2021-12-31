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
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SimScanner.Bgl
{
    public class BinSection : IDisposable
    {
        private static readonly Logger log = Logger.GetLogger(typeof(BinFile));

        private MemoryMappedViewAccessor accessor;
        private long pos = 0;
        public long Position => pos;

        public BinSection(BinFile reader, long offset, long size)
        {
            accessor = reader.file.CreateViewAccessor(offset, size);
        }

        public BinSection Read<T>(out T value, uint size, bool readAhead =false)
            where T : struct
        {
            if (accessor.Capacity < pos+size)
            {
                throw new ArgumentOutOfRangeException($"Trying to read 0x{size:X4} byte(s) starting at 0x{pos:X4}, Capacity = 0x{accessor.Capacity:X4}.");
            }
            log.Trace?.Log($"Reading 0x{size:X4} bytes starting at 0x{pos:X4}, Capacity = 0x{accessor.Capacity:X4}.");

            accessor.Read(pos, out value);
            if (!readAhead)
            {
                pos += size;
            }
            return this;
        }

        public BinSection Read(out byte value) => Read(out value, 1);
        public BinSection Read(out short value) => Read(out value, 2);
        public BinSection Read(out ushort value) => Read(out value, 2);
        public BinSection Read(out int value) => Read(out value, 4);
        public BinSection Read(out uint value) => Read(out value, 4);
        public BinSection Read(out long value) => Read(out value, 8);
        public BinSection Read(out ulong value) => Read(out value, 8);
        public BinSection Read(out float value) => Read(out value, 4);
        public BinSection Read(out double value) => Read(out value, 8);

        public BinSection ReadAhead(out ushort value) => Read(out value, sizeof(ushort), true);

        public BinSection Skip(uint size)
        {
            pos += size;
            return this;
        }

        public BinSection Seek(long pos)
        {
            if ((pos < 0) || (pos >= accessor.Capacity))
            {
                throw new ArgumentOutOfRangeException($"Trying to seek to pos {pos}, Capacity = {accessor.Capacity}.");
            }
            this.pos = pos;
            return this;
        }

        public BinSection Read(out string value, int maxSize =-1)
        {
            StringBuilder bld = new();
            long i = (maxSize >= 0) ? maxSize : (accessor.Capacity - pos);
            while (i-- > 0)
            {
                Read(out byte b);
                if (b == 0)
                {
                    break;
                }
                bld.Append((char)b);
            }
            value = bld.ToString();
            return this;
        }

        public string HexDump(long offset, uint size)
        {
            StringBuilder bld = new();
            for (uint i = 0; i < size; i++)
            {
                accessor.Read(offset + i, out byte b);
                bld.Append($"{b:X2} ");
            }
            bld.Append("| '");
            for (uint i = 0; i < size; i++)
            {
                accessor.Read(offset + i, out byte b);
                bld.Append(((b >= 0x20) && (b <= 0x7f)) ? ((char)b) : '.');
            }
            return bld.ToString();
        }

        public void Dispose()
        {
            accessor?.Dispose();
        }
    }

    public class BinFile : IDisposable
    {
        public string Name { get; private set; }
        internal MemoryMappedFile file;

        public BinFile(string filename)
        {
            Name = filename;
            file = MemoryMappedFile.CreateFromFile(filename, FileMode.Open);
        }

        public BinSection Section(long offset, long size) => new BinSection(this, offset, size);

        public void Dispose()
        {
            file?.Dispose();
        }
    }
}
