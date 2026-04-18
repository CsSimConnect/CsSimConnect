/*
 * Copyright (c) 2021-2024. Bert Laverman
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

namespace CsSimConnect.Tests.Helpers
{
    /// <summary>
    /// Minimal <see cref="SimConnectMessage"/> subclass usable in unit tests
    /// without a native DLL or live simulator connection.
    /// </summary>
    internal class TestMessage(uint payload = 42) : SimConnectMessage(RecvId.Null, 0)
    {
        public uint Payload { get; } = payload;
    }
}
