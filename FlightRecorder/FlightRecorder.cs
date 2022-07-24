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
using CsSimConnect.AI;
using CsSimConnect.DataDefs;
using CsSimConnect.DataDefs.Dynamic;
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
        [DataDefinition("ATC ID", Type = DataType.String32)]
        public string TailNumber;
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

        public AircraftData(string tailNumber, string title, double latitude, double longitude, double altitude, double pitch, double bank, double heading, bool onGround, int airSpeed)
        {
            TailNumber = tailNumber;
            Title = title;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Pitch = pitch;
            Bank = bank;
            Heading = heading;
            OnGround = onGround;
            AirSpeed = airSpeed;
        }
    }
    public class FlightData
    {
        public DateTime ts;

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

        public FlightData()
        {

        }

        public FlightData(DateTime ts, double latitude, double longitude, double altitude, double pitch, double bank, double heading, bool onGround, int airSpeed)
        {
            this.ts = ts;
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Pitch = pitch;
            Bank = bank;
            Heading = heading;
            OnGround = onGround;
            AirSpeed = airSpeed;
        }
    }

    public class FlightRecorder
    {
        private const string OPT_P3DV5 = "p3dv5";
        private const string OPT_MSFS = "msfs";
        private const char OPT_O = 'c';
        private const string OPT_OUTPUT = "output";
        private const string OPT_OBS_DELAY = "observation-delay";
        private const string OPT_REPLAY = "replay";
        private const string OPT_DYNAMIC = "dynamic";

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
            Console.WriteLine("  --dynamic        Use a dynamic request rather than an annotated record.");
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
                .WithOption(OPT_DYNAMIC)
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

            if (!parsedArgs.Has(OPT_REPLAY))
            {
                // Check recording parameters
                try
                {
                    startDelay = int.Parse(parsedArgs.Parameters[0]);
                    duration = int.Parse(parsedArgs.Parameters[1]);
                }
                catch (FormatException)
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
                    catch (FormatException)
                    {
                        Console.WriteLine($"Bad observation delay '{parsedArgs[OPT_OBS_DELAY]}'.");
                        Usage();
                        return;
                    }
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
                RecordAnnotated();
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

            JsonSerializer serializer = new();

            string line = f.ReadLine();
            AircraftData aircraftData = JsonConvert.DeserializeObject<AircraftData>(line);

            Console.WriteLine($"Creating AI aircraft of type '{aircraftData.Title}'");
            SimulatedAircraft aircraft = AircraftBuilder.Builder(aircraftData.Title)
                .WithTailNumber(aircraftData.TailNumber)
                .AtPosition(aircraftData.Latitude, aircraftData.Longitude, aircraftData.Altitude)
                .WithPBH(aircraftData.Pitch, aircraftData.Bank, aircraftData.Heading)
                .OnGround()
                .WithAirSpeed(aircraftData.AirSpeed)
                .Build();
            SimulatedAircraft ai = AIManager.Instance.Create(aircraft).Get();

            Console.WriteLine($"Starting {startDelay} second(s) of wait.");
            Thread.Sleep(new TimeSpan(0, 0, startDelay));

            line = f.ReadLine();
            TimeSpan diff = TimeSpan.Zero;
            while (line != null)
            {
                FlightData data = JsonConvert.DeserializeObject<FlightData>(line);
                if (data == null)
                {
                    Console.WriteLine("Unable to deserialize object.");
                    break;
                }
                DateTime now = DateTime.Now;
                Console.WriteLine($"Read data with ts = {data.ts}, now = {now}.");
                if ((diff == TimeSpan.Zero) || ((data.ts + diff) > now))
                {
                    if (diff == TimeSpan.Zero)
                    {
                        diff = now - data.ts;
                        Console.WriteLine($"Going to correct for {diff} in time difference");
                    }
                    Thread.Sleep(data.ts + diff - now);
                    DataManager.Instance.SetData(ai.ObjectId, data);
                }
                else
                {
                    Console.WriteLine("Skipping old data.");
                }

                line = f.ReadLine();
            }

            Thread.Sleep(10000);
        }

        private static void RecordAnnotated()
        {
            AircraftData aircraft = DataManager.Instance.RequestData<AircraftData>().Get();

            using StreamWriter f = new(output, true);

            f.WriteLine(JsonConvert.SerializeObject(aircraft));

            FlightData data = new FlightData();
            bool haveData = false;
            ulong seqNr = 0;
            DateTime timestamp = DateTime.Now;

            DataManager.Instance.RequestData(data, period: ObjectDataPeriod.PerSimFrame, onlyWhenChanged: true, onNext: () => { haveData = true; seqNr++; timestamp = DateTime.Now; });

            Console.WriteLine($"Starting {startDelay} second(s) of wait.");
            Thread.Sleep(new TimeSpan(0, 0, startDelay));

            Console.WriteLine($"Starting recording: {duration} second(s)");

            DateTime limit = DateTime.Now + new TimeSpan(0, 0, duration);

            DateTime update = DateTime.Now + new TimeSpan(0, 0, 10);
            int count = 0;

            ulong lastSeqNr = 0;
            while (DateTime.Now < limit)
            {
                if (haveData && (seqNr > lastSeqNr))
                {
                    lastSeqNr = seqNr;
                    data.ts = timestamp;
                    f.WriteLine(JsonConvert.SerializeObject(data));
                }
                Thread.Sleep(obsDelay);
                if (DateTime.Now > update)
                {
                    count += 10;
                    Console.WriteLine($"Recorded {count} second(s).");
                    update = DateTime.Now + new TimeSpan(0, 0, 10);
                }
            }
            DataManager.Instance.ClearDefinition(data);
        }

        private static void RecordDynamic()
        {
            SimObjectData data = new();

            data.AddField("ATC ID", type: DataType.String32);
            data.AddField("TITLE", type: DataType.String256);
            data.AddField("PLANE LATITUDE", units: "DEGREES", type: DataType.Float64, epsilon: 1.0f);
            data.AddField("PLANE LONGITUDE", units: "DEGREES", type: DataType.Float64, epsilon: 1.0f);
            data.AddField("PLANE ALTITUDE", units: "FEET", type: DataType.Float64, epsilon: 1.0f);
            data.AddField("PLANE PITCH DEGREES", units: "DEGREES", type: DataType.Float64, epsilon: 1.0f);
            data.AddField("PLANE BANK DEGREES", units: "DEGREES", type: DataType.Float64, epsilon: 1.0f);
            data.AddField("PLANE HEADING DEGREES TRUE", units: "DEGREES", type: DataType.Float64, epsilon: 1.0f);
            data.AddField("SIM ON GROUND", units: "BOOL", type: DataType.Int32);
            data.AddField("AIRSPEED TRUE", units: "KNOTS", type: DataType.Int32);

            var aircraft = data.Request().Get();

            using StreamWriter f = new(output, true);

            f.WriteLine(JsonConvert.SerializeObject(new AircraftData(aircraft.GetString("ATC ID"), aircraft.GetString("TITLE"),
                aircraft.GetDouble("PLANE LATITUDE"), aircraft.GetDouble("PLANE LONGITUDE"), aircraft.GetDouble("PLANE ALTITUDE"),
                aircraft.GetDouble("PLANE PITCH DEGREES"), aircraft.GetDouble("PLANE BANK DEGREES"), aircraft.GetDouble("PLANE HEADING DEGREES"),
                aircraft.GetBoolean("SIM ON GROUND"), aircraft.GetInt32("AIRSPEED TRUE"))));

            bool haveData = false;
            ulong seqNr = 0;
            DateTime timestamp = DateTime.Now;

            Console.WriteLine($"Starting {startDelay} second(s) of wait.");
            Thread.Sleep(new TimeSpan(0, 0, startDelay));

            Console.WriteLine($"Starting recording: {duration} second(s)");

            DateTime limit = DateTime.Now + new TimeSpan(0, 0, duration);

            DateTime update = DateTime.Now + new TimeSpan(0, 0, 10);
            int count = 0;

            ulong lastSeqNr = 0;

            data.RequestData(ObjectDataPeriod.PerSimFrame, onlyWhenChanged: true)
                .Subscribe(record => {
                    haveData = true;
                    seqNr++;
                    timestamp = DateTime.Now;
                });

            while (DateTime.Now < limit)
            {
                if (haveData && (seqNr > lastSeqNr))
                {
                    lastSeqNr = seqNr;
                    f.WriteLine(JsonConvert.SerializeObject(new FlightData(timestamp,
                        aircraft.GetDouble("PLANE LATITUDE"), aircraft.GetDouble("PLANE LONGITUDE"), aircraft.GetDouble("PLANE ALTITUDE"),
                        aircraft.GetDouble("PLANE PITCH DEGREES"), aircraft.GetDouble("PLANE BANK DEGREES"), aircraft.GetDouble("PLANE HEADING DEGREES"),
                        aircraft.GetBoolean("SIM ON GROUND"), aircraft.GetInt32("AIRSPEED TRUE"))));
                }
                Thread.Sleep(obsDelay);
                if (DateTime.Now > update)
                {
                    count += 10;
                    Console.WriteLine($"Recorded {count} second(s).");
                    update = DateTime.Now + new TimeSpan(0, 0, 10);
                }
            }
        }
    }
}
