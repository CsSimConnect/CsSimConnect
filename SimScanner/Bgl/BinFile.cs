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
using System.IO.MemoryMappedFiles;

namespace SimScanner.Bgl
{
    public class BinSection : IDisposable
    {
        private BinFile reader;
        private MemoryMappedViewAccessor accessor;
        private long pos = 0;

        public BinSection(BinFile reader, long offset, long size)
        {
            accessor = reader.file.CreateViewAccessor(offset, size);
        }

        public BinSection Read<T>(out T value, uint size)
            where T : struct
        {
            accessor.Read(pos, out value);
            pos += size;

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

        public void Dispose()
        {
            reader = null;
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
