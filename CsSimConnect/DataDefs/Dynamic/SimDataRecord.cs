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
    public class SimDataRecord : IDataRecord
    {
        protected SimObjectData simObjectData;

        public SimDataRecord(SimObjectData simObjectData)
        {
            this.simObjectData = simObjectData;
        }

        public object this[int i] => simObjectData.values[i];

        public object this[string name] => simObjectData.values[simObjectData.nameLookup[name]];

        public int FieldCount => simObjectData.fieldDefinitions.Count;

        public bool GetBoolean(int i) => (bool)this[i];
        public bool GetBoolean(string name) => (bool)this[simObjectData.nameLookup[name]];

        public char GetChar(int i) => (char)this[i];
        public char GetChar(string name) => (char)this[simObjectData.nameLookup[name]];

        public byte GetByte(int i) => (byte)this[i];
        public byte GetByte(string name) => (byte)this[simObjectData.nameLookup[name]];

        public short GetInt16(int i) => (short)this[i];
        public short GetInt16(string name) => (short)this[simObjectData.nameLookup[name]];

        public int GetInt32(int i) => (int)this[i];
        public int GetInt32(string name) => (int)this[simObjectData.nameLookup[name]];

        public long GetInt64(int i) => (long)this[i];
        public long GetInt64(string name) => (long)this[simObjectData.nameLookup[name]];

        public float GetFloat(int i) => (float)this[i];
        public float GetFloat(string name) => (float)this[simObjectData.nameLookup[name]];

        public double GetDouble(int i) => (double)this[i];
        public double GetDouble(string name) => (double)this[simObjectData.nameLookup[name]];

        public string GetString(int i) => this[i].ToString();
        public string GetString(string name) => this[simObjectData.nameLookup[name]].ToString();

        public object GetValue(int i) => this[i];
        public object GetValue(string name) => this[simObjectData.nameLookup[name]];

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i) => simObjectData.fieldDefinitions[i].Type.ToString();

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i) => simObjectData.fieldDefinitions[i].TargetType;

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i) => simObjectData.names[i];

        public int GetOrdinal(string name) => simObjectData.nameLookup[name];

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i) => this[i] == null;
    }
}
