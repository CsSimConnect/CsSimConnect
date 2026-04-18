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

using System;

namespace CsSimConnect.LiveTests
{
    /// <summary>
    /// Marks a test that requires a live simulator connection.
    /// The test is skipped unless the environment variable CSTESTS_LIVE=1 is set
    /// AND a simulator connection can be established.
    /// Run live tests with: $env:CSTESTS_LIVE=1; dotnet test CsSimConnect.LiveTests
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LiveFactAttribute : FactAttribute
    {
        public LiveFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("CSTESTS_LIVE") != "1")
            {
                Skip = "Live tests opt-in required: set CSTESTS_LIVE=1 and start a simulator.";
            }
        }
    }

    /// <summary>
    /// Parameterised variant of <see cref="LiveFactAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class LiveTheoryAttribute : TheoryAttribute
    {
        public LiveTheoryAttribute()
        {
            if (Environment.GetEnvironmentVariable("CSTESTS_LIVE") != "1")
            {
                Skip = "Live tests opt-in required: set CSTESTS_LIVE=1 and start a simulator.";
            }
        }
    }
}
