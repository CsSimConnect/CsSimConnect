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

namespace CsSimConnect.LiveTests
{
    /// <summary>
    /// Defines the shared "Simulator" xUnit collection. All test classes decorated
    /// with [Collection("Simulator")] share ONE <see cref="SimulatorFixture"/> that
    /// connects once before any tests run and disconnects after all of them finish.
    ///
    /// This prevents multiple fixtures from racing to connect the same SimConnect
    /// singleton concurrently.
    /// </summary>
    [CollectionDefinition("Simulator")]
    public class SimulatorCollection : ICollectionFixture<SimulatorFixture>
    {
        // This class has no code; it serves only as the collection definition.
    }
}
