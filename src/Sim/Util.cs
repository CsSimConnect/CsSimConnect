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

using Microsoft.Win32;
using Rakis.Logging;
using System;
using System.IO;

namespace CsSimConnect.Sim
{
    public static class Util
    {

        private static readonly ILogger log = Logger.GetLogger(typeof(Util));

        private const string P3DRegistryBase = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Lockheed Martin\\";

        private const string P3Dv4Name = "Prepar3D v4";
        public const string P3Dv4Key = "P3Dv4";

        private const string P3Dv5Name = "Prepar3D v5";
        public const string P3Dv5Key = "P3Dv5";

        private const string MSFSName = "MSFS 2020";
        public const string MSFSKey = "MSFS";

        private const string InstallPathPrefix = "InstalledPackagesPath ";

        private static string IsOrIsnt(bool isOrIsnt) => isOrIsnt ? "is" : "is not";

        private static string HaveOrHaveno(bool haveOrHavent) => haveOrHavent ? "have" : "have no";

        public static Simulator GetPrepar3Dv4()
        {
            log.Trace?.Log("Gathering information on Prepar3D v4 installation.");

            Simulator result = new()
            {
                InstallationPath = (string)Registry.GetValue(P3DRegistryBase + P3Dv4Name, "SetupPath", null),
                Name = P3Dv4Name,
                Key = P3Dv4Key,
                Fs = new FlightSimVersion() { Type = FlightSimType.Prepar3D, Version = "v4" }
            };
            result.Installed = result.InstallationPath != null;
            result.DllAvailable = File.Exists(InterOpManager.InterOpPath(result.Fs));

            log.Trace?.Log($"Prepar3D v4 {IsOrIsnt(result.Installed)} installed and we {HaveOrHaveno(result.DllAvailable)} DLL to load.");

            return result;
        }

        public static Simulator GetPrepar3Dv5()
        {
            log.Trace?.Log("Gathering information on Prepar3D v5 installation.");

            Simulator result = new()
            {
                InstallationPath = (string)Registry.GetValue(P3DRegistryBase + P3Dv5Name, "SetupPath", null),
                Name = P3Dv5Name,
                Key = P3Dv5Key,
                Fs = new FlightSimVersion() { Type = FlightSimType.Prepar3D, Version = "v5" }
            };
            result.Installed = result.InstallationPath != null;
            result.DllAvailable = File.Exists(InterOpManager.InterOpPath(result.Fs));

            log.Trace?.Log($"Prepar3D v5 {IsOrIsnt(result.Installed)} installed and we {HaveOrHaveno(result.DllAvailable)} DLL to load.");

            return result;
        }

        public static Simulator GetMSFS2020()
        {
            log.Trace?.Log("Gathering information on MS Flight Simulator 2020 installation.");

            var configFile = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + "\\AppData\\Local\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalCache\\UserCfg.opt";

            string path = null;
            if (File.Exists(configFile))
            {
                using StreamReader f = new(configFile);
                while ((path = f.ReadLine()) != null)
                {
                    if (path.StartsWith(InstallPathPrefix))
                    {
                        path = path.Substring(InstallPathPrefix.Length).Replace("\"", "").Trim();
                        break;
                    }
                }
            }
            Simulator result = new()
            {
                InstallationPath = path,
                Name = MSFSName,
                Key = MSFSKey,
                Fs = new FlightSimVersion() { Type = FlightSimType.MSFlightSimulator, Version = "2020" }
            };
            result.Installed = result.InstallationPath != null;
            result.DllAvailable = File.Exists(InterOpManager.InterOpPath(result.Fs));

            log.Trace?.Log($"MS Flight Simulator 2020 {IsOrIsnt(result.Installed)} installed and we {HaveOrHaveno(result.DllAvailable)} DLL to load.");

            return result;
        }

        public static Simulator FromKey(string key)
        {
            return key switch
            {
                P3Dv4Key => GetPrepar3Dv4(),
                P3Dv5Key => GetPrepar3Dv5(),
                MSFSKey => GetMSFS2020(),
                _ => throw new ArgumentOutOfRangeException($"Unknow Simulator key '{key}'."),
            };
        }

    }
}
