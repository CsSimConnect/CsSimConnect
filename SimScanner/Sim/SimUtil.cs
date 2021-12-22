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

using Microsoft.Win32;
using System;
using System.IO;

namespace SimScanner.Sim
{
    public static class SimUtil
    {
        private const string P3DRegistryBase = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Lockheed Martin\\";

        private const string P3Dv4Name = "Prepar3D v4";
        private const string P3Dv4Key = "P3Dv4";

        private const string P3Dv5Name = "Prepar3D v5";
        private const string P3Dv5Key = "P3Dv5";

        private const string MSFSName = "MSFS 2020";
        private const string MSFSKey = "MSFS";
        private const string InstallPathPrefix = "InstalledPackagesPath ";

        public static Simulator GetPrepar3Dv4()
        {
            Simulator result = new();
            result.InstallationPath = (string)Registry.GetValue(P3DRegistryBase + P3Dv4Name, "SetupPath", null);
            result.Installed = result.InstallationPath != null;
            result.Name = P3Dv4Name;
            result.Key = P3Dv4Key;
            result.Type = FlightSimType.Prepar3Dv4;
            result.DllAvailable = File.Exists(P3Dv4Key + "\\CsSimConnectInterOp.dll");

            return result;
        }

        public static Simulator GetPrepar3Dv5()
        {
            Simulator result = new();
            result.InstallationPath = (string)Registry.GetValue(P3DRegistryBase + P3Dv5Name, "SetupPath", null);
            result.Installed = result.InstallationPath != null;
            result.Name = P3Dv5Name;
            result.Key = P3Dv5Key;
            result.Type = FlightSimType.Prepar3Dv5;
            result.DllAvailable = File.Exists(P3Dv5Key + "\\CsSimConnectInterOp.dll");

            return result;
        }

        public static string GetLocalAppData(string path)
        {
            return Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "AppData", "Local", path);
        }

        public static string GetLocalAppData(string author, string app, string path)
        {
            return Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "AppData", "Local", author, app, path);
        }

        public static string GetRoamingAppData(string path)
        {
            return Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "AppData", "Roaming", path);
        }

        public static string GetRoamingAppData(string author, string app, string path)
        {
            return Path.Combine(Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH"), "AppData", "Roaming", author, app, path);
        }

        public static string GetProgramData(string path)
        {
            return Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), path);
        }

        public static string GetProgramData(string author, string app, string path)
        {
            return Path.Combine(Environment.GetEnvironmentVariable("ProgramData"), author, app, path);
        }

        public static Simulator GetMSFS2020()
        {
            string configFile = GetLocalAppData("Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalCache\\UserCfg.opt");

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
            result.Type = FlightSimType.MSFS2020;
            result.DllAvailable = File.Exists(P3Dv5Key + "\\CsSimConnectInterOp.dll");

            return result;

        }
    }
}
