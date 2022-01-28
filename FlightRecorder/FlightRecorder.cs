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
using CsSimConnect.Reflection;
using Newtonsoft.Json;
using Rakis.Args;
using Rakis.Logging;
using System;
using System.IO;
using System.Threading;

namespace FlightRecorder
{
    public class AircraftData
    {
        [DataDefinition("TITLE", Type = DataType.String256)]
        public string Title;
        [DataDefinition("PLANE LATITUDE", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Latitude;
        [DataDefinition("PLANE LONGITUDE", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Longitude;
        [DataDefinition("PLANE ALTITUDE", Units = "FEET", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Altitude;
        [DataDefinition("PLANE PITCH DEGREES", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Pitch;
        [DataDefinition("PLANE BANK DEGREES", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Bank;
        [DataDefinition("PLANE HEADING DEGREES TRUE", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Heading;
        [DataDefinition("SIM ON GROUND", Units = "BOOL", Type = DataType.Int32)]
        public bool OnGround;
        [DataDefinition("AIRSPEED TRUE", Units = "KNOTS", Type = DataType.Int32)]
        public int AirSpeed;
    }
    public class FlightData
    {
        [DataDefinition("PLANE LATITUDE", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Latitude;
        [DataDefinition("PLANE LONGITUDE", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Longitude;
        [DataDefinition("PLANE ALTITUDE", Units = "FEET", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Altitude;
        [DataDefinition("PLANE PITCH DEGREES", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Pitch;
        [DataDefinition("PLANE BANK DEGREES", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Bank;
        [DataDefinition("PLANE HEADING DEGREES TRUE", Units = "DEGREES", Type = DataType.Float64, Epsilon = 1.0f)]
        public double Heading;
        [DataDefinition("SIM ON GROUND", Units = "BOOL", Type = DataType.Int32)]
        public bool OnGround;
        [DataDefinition("AIRSPEED TRUE", Units = "KNOTS", Type = DataType.Int32)]
        public int AirSpeed;
    }

    public class FlightRecorder
    {
        private const string OPT_P3DV5 = "p3dv5";
        private const string OPT_MSFS = "msfs";
        private const char OPT_O = 'c';
        private const string OPT_OUTPUT = "output";
        private const string OPT_OBS_DELAY = "observation-delay";
        private const string OPT_REPLAY = "replay";

        private static void Usage()
        {
            Console.WriteLine("Usage: FlightRecorder [OPT] <start-delay> <duration>");
            Console.WriteLine();
            Console.WriteLine("  <start-delay>  Number of seconds to wait before the recording starts.");
            Console.WriteLine("  <duration>     Number of seconds to record.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --p3dv5          Use Prepar3D SimConnect library.");
            Console.WriteLine("  --msfs           Use MSFS-2020 SimConnect library.");
            Console.WriteLine("  --output=<path>  File to write the recording to. Default is '.\flight.csv'.");
            Console.WriteLine("  -o <path>        Shorthand options for '--output'.");
            Console.WriteLine("  --replay         Replay the flight recorded rather than recording one.");
            Console.WriteLine();
            Console.WriteLine("Specifying at least one of '--p3dv5' and '--msfs' is required.");
        }

        private static string output = ".\\flight.csv";
        private static int startDelay;
        private static int duration;
        private static int obsDelay = 200;

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
                .WithOption(OPT_O, OPT_OUTPUT, true)
                .WithOption(OPT_OBS_DELAY, true)
                .WithOption(OPT_REPLAY)
                .Parse();

            if (!parsedArgs.Has(OPT_REPLAY) && parsedArgs.Parameters.Count != 2)
            {
                Usage();
                return;
            }

            if (parsedArgs.ArgOpts.ContainsKey(OPT_OUTPUT))
            {
                output = parsedArgs.ArgOpts[OPT_OUTPUT];
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

            // Check recording parameters
            try
            {
                startDelay = int.Parse(parsedArgs.Parameters[0]);
                duration = int.Parse(parsedArgs.Parameters[1]);
            }
            catch (FormatException e)
            {
                Console.WriteLine("Bad number format.");
                Usage();
                return;
            }
            if (parsedArgs.ArgOpts.ContainsKey(OPT_OBS_DELAY))
            {
                try
                {
                    obsDelay = int.Parse(parsedArgs[OPT_OBS_DELAY]);
                }
                catch (FormatException e)
                {
                    Console.WriteLine($"Bad observation delay '{parsedArgs[OPT_OBS_DELAY]}'.");
                    Usage();
                    return;
                }
            }
            WaitUntilConnected(10);
            if (!SimConnect.Instance.IsConnected)
            {
                Console.WriteLine("Connecting to simulator taking more than 10 seconds. Is it running?");
                return;
            }

            if (parsedArgs.Has(OPT_REPLAY))
                Replay();
            else
                Record();
        }

        private static void ParseLine(string line, FlightData data)
        {
            string[] fields = line.Split(',');
            data.Latitude = double.Parse(fields[0]);
            data.Longitude = double.Parse(fields[1]);
            data.Altitude = double.Parse(fields[2]);
            data.Pitch = double.Parse(fields[3]);
            data.Bank = double.Parse(fields[4]);
            data.Heading = double.Parse(fields[5]);
        }

        private static void Replay()
        {
            using StreamReader f = new(output);
            using JsonTextReader fj = new(f);

            if (!fj.Read() || fj.TokenType != JsonToken.StartObject)
            {
                Console.WriteLine($"First token read from '{output}' is a {fj.TokenType}");
                return;
            }
            
        }

        private static void Record()
        {
            AircraftData aircraft = DataManager.Instance.RequestData<AircraftData>().Get();

            using StreamWriter f = new(output, true);
            using JsonTextWriter fj = new(f);

            fj.WriteRawValue(JsonConvert.SerializeObject(aircraft));

            FlightData data = new FlightData();
            bool haveData = false;
            DataManager.Instance.RequestData(data, period: ObjectDataPeriod.PerSimFrame, onlyWhenChanged: true, onNext: () => { haveData = true; });
            Thread.Sleep(new TimeSpan(0, 0, startDelay));
            DateTime limit = DateTime.Now + new TimeSpan(0, 0, duration);
            while (DateTime.Now < limit)
            {
                if (haveData)
                {
                    fj.WriteRawValue(JsonConvert.SerializeObject(data));
                }
                Thread.Sleep(obsDelay);
            }
        }
    }
}
