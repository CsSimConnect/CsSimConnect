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

using CsSimConnect;
using CsSimConnect.DataDefs;
using CsSimConnect.DataDefs.Dynamic;
using Rakis.Args;
using Rakis.Logging;
using System;
using System.Threading;

namespace DataTester
{
    class DataTester
    {
        private const string OPT_P3DV5 = "p3dv5";
        private const string OPT_MSFS = "msfs";

        private static void Usage()
        {
            Console.WriteLine("Usage: DataTester [OPT]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --p3dv5          Use Prepar3D SimConnect library.");
            Console.WriteLine("  --msfs           Use MSFS-2020 SimConnect library.");
            Console.WriteLine();
            Console.WriteLine("Specifying at least one of '--p3dv5' and '--msfs' is required.");
        }

        private static void WaitUntilConnected(int seconds)
        {
            DateTime limit = DateTime.Now + new TimeSpan(0, 0, seconds);
            while (DateTime.Now < limit)
            {
                Thread.Sleep(500);
                SimConnect instance = SimConnect.Instance;
                if (instance.IsConnected)
                {
                    return;
                }
            }
        }

        static void Main(string[] args)
        {
            Logger.Configure();

            var parsedArgs = new ArgParser(args)
                .WithOption(OPT_P3DV5)
                .WithOption(OPT_MSFS)
                .Parse();

            if (parsedArgs.Parameters.Count != 0)
            {
                Usage();
                return;
            }

            // Check and set Simulator type
            if (parsedArgs.Has(OPT_MSFS))
            {
                SimConnect.SetFlightSimType(FlightSimType.MSFS2020);
                SimConnect.Instance.UseAutoConnect = true;
            }
            else if (parsedArgs.Has(OPT_P3DV5))
            {
                SimConnect.SetFlightSimType(FlightSimType.Prepar3Dv5);
                SimConnect.Instance.UseAutoConnect = true;
            }
            else
            {
                Usage();
                return;
            }

            WaitUntilConnected(10);
            if (!SimConnect.Instance.IsConnected)
            {
                Console.WriteLine("Connecting to simulator taking more than 10 seconds. Is it running?");
                return;
            }

            SimObjectData data = new();
            data.AddField("SIM ON GROUND", type: DataType.Int32, valueSetter: (bool onGround) => { Console.WriteLine($"SIM ON GROUND = {onGround}"); });
            SimDataRecord record = data.Request().Get();
            Console.WriteLine($"record[\"SIM ON GROUND\"] = {record["SIM ON GROUND"]}");

            data.AddField("TITLE", type: DataType.String256);
            record = data.Request().Get();
            Console.WriteLine($"record[\"SIM ON GROUND\"] = {record["SIM ON GROUND"]}, record[\"TITLE\"] = \"{record["TITLE"]}\"");
        }
    }
}
