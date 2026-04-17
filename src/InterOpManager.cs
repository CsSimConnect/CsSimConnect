/*
 * Copyright (c) 2024. Bert Laverman
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

using CsSimConnect.Exc;
using CsSimConnect.Sim;
using Rakis.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public sealed class InterOpManager
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(InterOpManager));

        private static readonly Lazy<InterOpManager> lazyInstance = new(() => new InterOpManager());

        public static InterOpManager Instance => lazyInstance.Value;

        public delegate void DispatchProc(ref ReceiveStruct structData, UInt32 wordData, IntPtr context);


        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool FreeLibrary(IntPtr hModule);

        public const string InterOpDllName = "CsSimConnectInterOp.dll";

        public static FlightSimType InterOpType { get; private set; } = FlightSimType.Unknown;

        /// <summary>
        /// Gets the full path to the InterOp DLL.
        /// </summary>
        /// <remarks>
        /// The InterOp DLL is the native DLL that acts as a bridge between the .NET world and the SimConnect world.
        /// In order of preference, it is located:
        /// - If the environment variable CSSC_INTEROP_PATH is set and the file exists, use that path.
        /// - If the environment variable CSSC_INTEROP_DIR is set and the file exists in that directory, use that path.
        /// - Otherwise, the file is expected to be in the same directory as the .NET DLL.
        /// </remarks>
        /// <returns>The full path to the InterOp DLL.</returns>
        public static string InterOpPath()
        {
            if ((Environment.GetEnvironmentVariable("CSSC_INTEROP_PATH") is { } path) && File.Exists(path))
            {
                return path;
            }

            if ((Environment.GetEnvironmentVariable("CSSC_INTEROP_DIR") is not { } dir) || !Directory.Exists(dir))
                return InterOpDllName;

            path = Path.Combine(dir, InterOpDllName);
            return File.Exists(path) ? path : InterOpDllName;
        }


        /// <summary>
        /// Gets the full path to the InterOp DLL for the given
        /// <paramref name="fs"/>.
        /// </summary>
        /// <param name="fs">The flight simulator version to get the path for.</param>
        /// <returns>The full path to the InterOp DLL.</returns>
        public static string InterOpPath(FlightSimVersion? fs)
        {
            // If we don't have a version, fall back to the default path.
            if (fs is not { } notNullFs) return InterOpPath();

            if ((Environment.GetEnvironmentVariable("CSSC_INTEROP_PATH") is { } path) && File.Exists(path))
            {
                return path;
            }
            if ((Environment.GetEnvironmentVariable("CSSC_INTEROP_DIR") is { } dir) && Directory.Exists(dir))
            {
                path = dir;
            }
            else
            {
                path = ".";
            }
            return Path.Combine(path, notNullFs.ToString(), InterOpDllName);
        }


        /// <summary>
        /// The InterOp DLL.
        /// </summary>
        /// <remarks>
        /// This is a handle to the InterOp DLL.
        /// </remarks>
        private static IntPtr _interOpDll = IntPtr.Zero;

        /// <summary>
        /// Load the InterOp DLL from the given path.
        /// </summary>
        /// <param name="path">The path to the InterOp DLL.</param>
        /// <remarks>
        /// If the InterOp DLL is already loaded, it will be unloaded first.
        /// </remarks>
        private static void LoadInterOpLibrary(string path)
        {
            // Unload the DLL if it is already loaded.
            UnloadInterOpLibrary();
            try
            {
                _interOpDll = LoadLibrary(path);
                if (_interOpDll == IntPtr.Zero)
                {
                    log.Fatal?.Log("Unable to load '{0}'", path);
                }
            }
            catch (Exception e) {
                log.Error?.Log("Exception caught in LoadInterOpLibrary('{0}'): {1}", path, e.Message);
            }
        }

        /// <summary>
        /// Unload the InterOp DLL from memory.
        /// </summary>
        /// <remarks>
        /// If the InterOp DLL is already unloaded, this function does nothing.
        /// </remarks>
        private static void UnloadInterOpLibrary()
        {
            if (_interOpDll != IntPtr.Zero)
            {
                try
                {
                    log.Info?.Log("Unloading InterOp DLL");
                    // Keep calling FreeLibrary until we get a failure.
                    // This is to make sure that the DLL is actually unloaded.
                    while (FreeLibrary(_interOpDll))
                    {
                        log.Debug?.Log("Reduced link count of InterOp DLL by one");
                    }
                    _interOpDll = IntPtr.Zero;
                }
                catch (Exception e)
                {
                    log.Error?.Log("Exception caught in UnloadInterOpLibrary('{0}'): {1}", e.Message);
                }
            }
        }

        /// <summary>
        /// Sets the target InterOp type and loads the InterOp DLL.
        /// </summary>
        /// <param name="fs">The target InterOp type.</param>
        /// <remarks>
        /// If the target InterOp type is already set and the InterOp DLL is already loaded, this function does nothing.
        /// </remarks>
        public static void SetFlightSimType(FlightSimVersion fs)
        {
            if ((InterOpType == fs.Type) && (_interOpDll != IntPtr.Zero)) {
                // The target InterOp type is already set and the InterOp DLL is already loaded.
                return;
            }

            InterOpType = fs.Type;
            var interOpPath = InterOpPath(fs);

            if (interOpPath != null)
            {
                log.Info?.Log("Loading InterOp DLL for '{0}'.", fs.Type.ToString());
                LoadInterOpLibrary(interOpPath);
            }
            else if (fs.Type == FlightSimType.Unknown)
            {
                log.Fatal?.Log("Target InterOp type not set!");
            }
            else
            {
                log.Fatal?.Log("Unknown FlightSimType '{0}'", fs.Type.ToString());
            }
        }

    }
}