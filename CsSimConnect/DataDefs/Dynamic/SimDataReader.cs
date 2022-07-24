/*
 * Copyright (c) 2022. Bert Laverman
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
using System.Data;

namespace CsSimConnect.DataDefs.Dynamic
{
    /**
     * <summary>Provide a reader.</summary>
     */
    public class SimDataReader : SimDataRecord, IDataReader
    {
        public SimDataReader(SimObjectData data) : base(data)
        {
        }

        public int Depth => 0;

        public bool IsClosed => simObjectData.Completed;

        public int RecordsAffected => -1;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // IGNORE
        }

        public DataTable GetSchemaTable() => simObjectData.GetSchemaTable();

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            // TODO: this won't work right with the last data in the stream.
            simObjectData.sync.WaitOne();
            return !simObjectData.Completed;
        }
    }
}
