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

using Microsoft.Data.Sqlite;
using Rakis.Logging;
using Rakis.Settings;
using SimScanner.Bgl;
using SimScanner.Scenery;
using SimScanner.Sim;
using System;
using System.Collections.Generic;

namespace SimScanner.Model
{

    public class SceneryManager : IDisposable
    {
        private static readonly Logger log = Logger.GetLogger(typeof(SceneryManager));

        public Context Context { get; init; }
        public Simulator Simulator { get; init; }

        private SqliteConnection db;

        public SqliteConnection DB()
        {
            if (db == null)
            {
                var setupCommands = new[]
                {
                    new
                    {
                        description = "Creting table 'airports'",
                        command = @"CREATE TABLE IF NOT EXISTS airports (
                                    apt_name     TEXT       NOT NULL,
                                    bgl_filename TEXT       NOT NULL,
                                    bgl_layer    INTEGER    NOT NULL,
                                    apt_icao     VARCHAR(4) NOT NULL,
                                    apt_lat      REAL       NOT NULL,
                                    apt_lon      REAL       NOT NULL,
                                    apt_alt_mtr  REAL       NOT NULL,
                                    apt_alt_feet REAL       NOT NULL,
                                    PRIMARY KEY (bgl_layer,apt_icao)
                                  )"
                    },
                    new
                    {
                        description = "Creating index 'ind_apt_icao'",
                        command = @"CREATE INDEX IF NOT EXISTS ind_apt_icao ON airports(apt_icao)"
                    },
                    new
                    {
                        description = "Creating index 'ind_apt_bglfile'",
                        command = @"CREATE INDEX IF NOT EXISTS ind_apt_bglfile ON airports(bgl_filename)"
                    },
                    new
                    {
                        description = "Creating index 'ind_apt_bgllayer'",
                        command = @"CREATE INDEX IF NOT EXISTS ind_apt_bgllayer ON airports(bgl_layer)"
                    },
                    new
                    {
                        description = "Creating table 'airport_names'",
                        command = @"CREATE TABLE IF NOT EXISTS airport_names (
                                    bgl_filename TEXT NOT NULL,
                                    bgl_layer INTEGER NOT NULL,
                                    apt_icao VARCHAR(4) NOT NULL,
                                    apt_region TEXT NOT NULL,
                                    apt_country TEXT NOT NULL,
                                    apt_state TEXT NOT NULL,
                                    apt_city TEXT NOT NULL,
                                    apt_name TEXT NOT NULL,
                                    PRIMARY KEY (bgl_layer,apt_icao)
                                  )"
                    },
                    new
                    {
                        description = "Creating table 'parkings'",
                        command = @"CREATE TABLE IF NOT EXISTS parkings (
                                    bgl_layer INTEGER NOT NULL,
                                    apt_icao VARCHAR(4) NOT NULL,
                                    prk_displayname TEXT NOT NULL,
                                    prk_number INT NULL,
                                    prk_name TEXT NULL,
                                    prk_lat REAL NOT NULL,
                                    prk_lon REAL NOT NULL,
                                    prk_heading REAL NOT NULL,
                                    PRIMARY KEY (bgl_layer,apt_icao,prk_displayname),
                                    FOREIGN KEY (bgl_layer,apt_icao) REFERENCES airports(bgl_layer,apt_icao) ON DELETE CASCADE
                                  )"
                    },
                    new
                    {
                        description = "Creating index 'ind_prk_apt'",
                        command = @"CREATE INDEX IF NOT EXISTS ind_prk_apt ON parkings(bgl_layer,apt_icao)"
                    }
                };
                var filename = new SettingsDir(Context, type: SettingsType.AppDataLocal).SettingFile(Simulator.Key.ToLower() + "-airports.db");
                log.Info?.Log($"Opening connection to {filename} for airport data.");

                db = new($"Data Source={filename}");
                db.Open();

                foreach (var setup in setupCommands)
                {
                    var cmd = db.CreateCommand();
                    cmd.CommandText = setup.command;
                    int result = cmd.ExecuteNonQuery();
                    log.Info?.Log($"{setup.description} resulted in {result}");
                }
            }
            return db;
        }

        public SceneryManager(Context context = null, Simulator simulator = null)
        {
            Context = context ?? new Context("CsSimConnect", "SimScanner");
            Simulator = simulator ?? SimUtil.GetPrepar3Dv5();
        }

        public void BuildDb()
        {
            SceneryConfiguration config = Simulator.Scenery;
            config.LoadSceneryConfig();
            config.LoadAddOnScenery();

            foreach (SceneryEntry entry in config.Entries)
            {
                if (!entry.Active)
                {
                    log.Debug?.Log($"Skipping inactive entry '{entry.Title}'.");
                    continue;
                }
                log.Debug?.Log($"Scanning '{entry.Title}'");
                if (entry.Files.Count == 0)
                {
                    log.Warn?.Log($"No Scenery files for {entry.Title} in {entry.LocalPath}.");
                }
                foreach (string bglFile in entry.Files)
                {
                    try
                    {
                        log.Trace?.Log($"- Reading '{bglFile}'");
                        BglFile file = new(bglFile);
                        foreach (BglSection section in file.Sections)
                        {
                            if (section.IsAirport)
                            {
                                foreach (BglAirport bglAirport in section.Airports)
                                {
                                    if (bglAirport == null)
                                    {
                                        continue;
                                    }

                                    Airport airport = new(bglAirport.Name, entry.Layer, file.Name, bglAirport.ICAO, bglAirport.Latitude, bglAirport.Longitude);
                                    airport.AltitudeMeters = bglAirport.Altitude;
                                    airport.AltitudeFeet = airport.AltitudeMeters * 3.28084;
                                    foreach (Parking parking in bglAirport.Parkings)
                                    {
                                        if (airport.Parkings.ContainsKey(parking.FullName))
                                        {
                                            log.Debug?.Log($"Replacing '{parking.FullName}' at {bglAirport.ICAO}");
                                            airport.Parkings[parking.FullName] = parking;
                                        }
                                        else
                                        {
                                            airport.Parkings.Add(parking.FullName, parking);
                                        }
                                    }

                                    PutAirport(airport);
                                }
                            }
                            else if (section.IsNameList)
                            {
                                foreach (BglNameList bglNameList in section.NameLists)
                                {
                                    foreach (BglName name in bglNameList.Names)
                                    {
                                        PutAirportName(entry.Layer, file.Name, name.ICAO, name.Region, name.Country, name.State, name.City, name.Airport);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error?.Log($"Exception while parsing '{bglFile}': {e.Message}");
                    }
                }
            }
        }

        private bool HaveAirportName(int layer, string icao)
        {
            var query = DB().CreateCommand();
            query.CommandText = @"SELECT apt_name FROM airport_names WHERE (bgl_layer=$layer) AND (apt_icao=$icao)";
            query.Parameters.AddWithValue("$layer", layer);
            query.Parameters.AddWithValue("$icao", icao);
            using var reader = query.ExecuteReader();

            return reader.Read();
        }

        private bool PutAirportName(int layer, string filename, string icao, string region, string country, string state, string city, string airport)
        {
            if (HaveAirportName(layer, icao))
            {
                log.Warn?.Log($"Not replacing names for {icao} in layer {layer}.");
                return true;
            }

            var cmd = DB().CreateCommand();
            cmd.CommandText = @"INSERT INTO airport_names(bgl_filename,bgl_layer,apt_icao,apt_region,apt_country,apt_state,apt_city,apt_name) VALUES($filename,$layer,$icao,$region,$country,$state,$city,$name)";
            cmd.Parameters.AddWithValue("$filename", filename);
            cmd.Parameters.AddWithValue("$layer", layer);
            cmd.Parameters.AddWithValue("$icao", icao);
            cmd.Parameters.AddWithValue("$region", region ?? "");
            cmd.Parameters.AddWithValue("$country", country ?? "");
            cmd.Parameters.AddWithValue("$state", state ?? "");
            cmd.Parameters.AddWithValue("$city", city ?? "");
            cmd.Parameters.AddWithValue("$name", airport ?? "");
            if (cmd.ExecuteNonQuery() != 1)
            {
                return false;
            }
            return true;
        }

        public Airport GetAirport(int layer, string icao)
        {
            Airport result = null;

            var query = DB().CreateCommand();
            query.CommandText = @"SELECT apt_name,bgl_layer,bgl_filename,apt_icao,apt_lat,apt_lon,apt_alt_mtr,apt_alt_feet FROM airports WHERE (bgl_layer=$layer) AND (apt_icao=$icao)";
            query.Parameters.AddWithValue("$layer", layer);
            query.Parameters.AddWithValue("$icao", icao);
            using (var reader = query.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = new(reader.GetString(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetDouble(4), reader.GetDouble(5));
                    result.AltitudeMeters = reader.GetDouble(6);
                    result.AltitudeFeet = reader.GetDouble(7);

                    query = DB().CreateCommand();
                    query.CommandText = @"SELECT prk_number,prk_name,prk_lat,prk_lon,prk_heading FROM parkings WHERE (bgl_layer=$layer) AND (apt_icao=$icao)";
                    query.Parameters.AddWithValue("$layer", layer);
                    query.Parameters.AddWithValue("$icao", icao);
                    using (var prkReader = query.ExecuteReader())
                    {
                        while (prkReader.Read())
                        {
                            var parking = new Parking();
                            parking.Airport = result;
                            parking.Number = (uint)prkReader.GetInt32(0);
                            parking.Name = prkReader.GetString(1);
                            parking.Latitude = prkReader.GetFloat(2);
                            parking.Longitude = prkReader.GetFloat(3);
                            parking.Heading = prkReader.GetFloat(4);

                            result.Parkings.Add(parking.FullName, parking);
                        }
                    }
                }
            }
            if ((result != null) && HaveAirportName(layer, icao))
            {
                var nameQuery = DB().CreateCommand();
                nameQuery.CommandText = @"SELECT apt_region,apt_country,apt_state,apt_city,apt_name FROM airport_names WHERE bgl_layer=$layer AND apt_icao=$icao";
                nameQuery.Parameters.AddWithValue("$layer", layer);
                nameQuery.Parameters.AddWithValue("$icao", icao);
                using (var nameReader = nameQuery.ExecuteReader())
                {
                    if (nameReader.Read())
                    {
                        result.Region = nameReader.GetString(0);
                        result.Country = nameReader.GetString(1);
                        result.State = nameReader.GetString(2);
                        result.City = nameReader.GetString(3);
                        result.Name = nameReader.GetString(4) ?? result.Name;
                    }
                }
            }
            return result;
        }

        public List<int> GetLayersForICAO(string icao)
        {
            List<int> result = new();

            var query = DB().CreateCommand();
            query.CommandText = @"SELECT bgl_layer FROM airports WHERE apt_icao=$icao ORDER BY bgl_layer DESC";
            query.Parameters.AddWithValue("$icao", icao);
            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetInt32(0));
                }
            }
            return result;
        }

        public List<string> GetICAOList()
        {
            List<string> result = new();

            var query = DB().CreateCommand();
            query.CommandText = @"SELECT DISTINCT apt_icao FROM airports ORDER BY apt_icao ASC";

            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }
            return result;
        }

        private static SqliteParameter addParameter(SqliteCommand cmd, string name)
        {
            var parm = cmd.CreateParameter();
            parm.ParameterName = name;
            cmd.Parameters.Add(parm);
            return parm;
        }

        public bool PutAirport(Airport airport)
        {
            Airport oldAirport = GetAirport(airport.Layer, airport.ICAO);
            if (oldAirport == null)
            {
                log.Debug?.Log($"Adding {airport.ICAO} (layer {airport.Layer})");

                var cmd = DB().CreateCommand();
                cmd.CommandText = @"INSERT INTO airports(apt_name,bgl_layer,bgl_filename,apt_icao,apt_lat,apt_lon,apt_alt_mtr,apt_alt_feet) VALUES($name,$layer,$filename,$icao,$lat,$lon,$altMtr,$altFeet)";
                cmd.Parameters.AddWithValue("$name", airport.Name ?? "");
                cmd.Parameters.AddWithValue("$layer", airport.Layer);
                cmd.Parameters.AddWithValue("$filename", airport.Filename);
                cmd.Parameters.AddWithValue("$icao", airport.ICAO);
                cmd.Parameters.AddWithValue("$lat", airport.Latitude);
                cmd.Parameters.AddWithValue("$lon", airport.Longitude);
                cmd.Parameters.AddWithValue("$altMtr", airport.AltitudeMeters);
                cmd.Parameters.AddWithValue("$altFeet", airport.AltitudeFeet);
                if (cmd.ExecuteNonQuery() != 1)
                {
                    return false;
                }
            }
            else log.Debug?.Log($"Keeping {airport.ICAO} (layer {airport.Layer})");

            if (airport.Parkings.Count > 0)
            {
                var cmd = DB().CreateCommand();
                cmd.CommandText = @"DELETE FROM parkings WHERE (bgl_layer=$layer) AND (apt_icao=$icao)";
                cmd.Parameters.AddWithValue("$layer", airport.Layer);
                cmd.Parameters.AddWithValue("$icao", airport.ICAO);
                var count = cmd.ExecuteNonQuery();
                if (count > 0)
                {
                    log.Debug?.Log($"Removed {count} parkings of {airport.ICAO} (layer {airport.Layer})");
                }

                log.Debug?.Log($"Adding {airport.Parkings.Count} parkings to {airport.ICAO} (layer {airport.Layer})");
                cmd = DB().CreateCommand();
                cmd.CommandText = @"INSERT INTO parkings(bgl_layer,apt_icao,prk_displayname,prk_number,prk_name,prk_lat,prk_lon,prk_heading) VALUES($aptLayer,$aptIcao,$prkDisplayName,$prkNumber,$prkName,$prkLat,$prkLon,$prkHeading)";
                var aptLayer = addParameter(cmd, "$aptLayer");
                var aptIcao = addParameter(cmd, "$aptIcao");
                var prkDisplayName = addParameter(cmd, "$prkDisplayName");
                var prkNumber = addParameter(cmd, "$prkNumber");
                var prkName = addParameter(cmd, "$prkName");
                var prkLat = addParameter(cmd, "$prkLat");
                var prkLon = addParameter(cmd, "$prkLon");
                var prkHeading = addParameter(cmd, "$prkHeading");

                foreach (Parking parking in airport.ParkingValues)
                {
                    if (parking.FullName == null)
                    {
                        log.Error?.Log($"Parking {parking.Number} (name='{parking.Name ?? ""}') has no displayname?");
                        continue;
                    }
                    aptLayer.Value = airport.Layer;
                    aptIcao.Value = airport.ICAO;
                    prkDisplayName.Value = parking.FullName;
                    prkNumber.Value = parking.Number;
                    prkName.Value = parking.Name;
                    prkLat.Value = parking.Latitude;
                    prkLon.Value = parking.Longitude;
                    prkHeading.Value = parking.Heading;
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }

        public void Dispose()
        {
            db?.Dispose();
        }
    }
}