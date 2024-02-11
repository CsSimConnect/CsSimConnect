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

        private static string isOrIsnt(Boolean isOrIsnt)
        {
            return isOrIsnt ? "is" : "is not";
        }

        private static string haveOrHaveno(Boolean haveOrHavent)
        {
            return haveOrHavent ? "have" : "have no";
        }

        public static Simulator GetPrepar3Dv4()
        {
            log.Trace?.Log("Gathering information on Prepar3D v4 installation.");

            Simulator result = new();
            result.InstallationPath = (string)Registry.GetValue(P3DRegistryBase + P3Dv4Name, "SetupPath", null);
            result.Installed = result.InstallationPath != null;
            result.Name = P3Dv4Name;
            result.Key = P3Dv4Key;
            result.Fs = new FlightSimVersion() { Type = FlightSimType.Prepar3D, Version = "v4" };
            result.DllAvailable = File.Exists(SimConnect.InterOpPath(result.Fs));

            log.Trace?.Log($"Prepar3D v4 {isOrIsnt(result.Installed)} installed and we {haveOrHaveno(result.DllAvailable)} DLL to load.");

            return result;
        }

        public static Simulator GetPrepar3Dv5()
        {
            log.Trace?.Log("Gathering information on Prepar3D v5 installation.");

            Simulator result = new();
            result.InstallationPath = (string)Registry.GetValue(P3DRegistryBase + P3Dv5Name, "SetupPath", null);
            result.Installed = result.InstallationPath != null;
            result.Name = P3Dv5Name;
            result.Key = P3Dv5Key;
            result.Fs = new FlightSimVersion() { Type = FlightSimType.Prepar3D, Version = "v5" };
            result.DllAvailable = File.Exists(SimConnect.InterOpPath(result.Fs));

            log.Trace?.Log($"Prepar3D v5 {isOrIsnt(result.Installed)} installed and we {haveOrHaveno(result.DllAvailable)} DLL to load.");

            return result;
        }

        public static Simulator GetMSFS2020()
        {
            log.Trace?.Log("Gathering information on MS Flight Simulator 2020 installation.");

            string configFile = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + "\\AppData\\Local\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalCache\\UserCfg.opt";

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
            Simulator result = new();
            result.InstallationPath = path;
            result.Installed = result.InstallationPath != null;
            result.Name = MSFSName;
            result.Key = MSFSKey;
            result.Fs = new FlightSimVersion() { Type = FlightSimType.MSFlightSimulator, Version = "2020" };
            result.DllAvailable = File.Exists(SimConnect.InterOpPath(result.Fs));

            log.Trace?.Log($"MS Flight Simulator 2020 {isOrIsnt(result.Installed)} installed and we {haveOrHaveno(result.DllAvailable)} DLL to load.");

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
