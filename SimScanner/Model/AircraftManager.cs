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
using SimScanner.AircraftCfg;
using SimScanner.Sim;
using System;
using System.Collections.Generic;

namespace SimScanner.Model
{
    public class AircraftManager : IDisposable
    {
        private static readonly Logger log = Logger.GetLogger(typeof(AircraftManager));

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
                        description = "Creting table 'aircraft'",
                        command = @"CREATE TABLE IF NOT EXISTS aircraft (
                                    air_title    TEXT NOT NULL PRIMARY KEY,
                                    air_type     TEXT NOT NULL,
                                    air_model    TEXT NOT NULL,
                                    air_category TEXT NOT NULL
                                  )"
                    },
                    new
                    {
                        description = "Creating index 'ind_air_type'",
                        command = @"CREATE INDEX IF NOT EXISTS ind_air_type ON aircraft(air_type)"
                    },
                    new
                    {
                        description = "Creating index 'ind_air_cat'",
                        command = @"CREATE INDEX IF NOT EXISTS ind_air_cat ON aircraft(air_category)"
                    },
                };
                var filename = new SettingsDir(Context, type: SettingsType.AppDataLocal).SettingFile(Simulator.Key.ToLower() + "-aircraft.db");
                log.Info?.Log($"Opening connection to {filename} for aircraft data.");

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

        public AircraftManager(Context context = null, Simulator simulator = null)
        {
            Context = context ?? new Context("CsSimConnect", "SimScanner");
            Simulator = simulator ?? SimUtil.GetPrepar3Dv5();
        }

        public List<Aircraft> BuildDB()
        {
            AircraftConfiguration cfg = Simulator.Aircraft;
            cfg.ScanSimObjects();
            foreach (Aircraft aircraft in cfg.Entries)
            {
                if (aircraft.Category == null)
                {
                    log.Error?.Log($"Not storing '{aircraft.Title}': No category set!");
                    continue;
                }
                PutAircraft(aircraft);
            }
            return cfg.Entries;
        }

        public Aircraft GetAircraft(string title)
        {
            Aircraft result = null;

            var query = DB().CreateCommand();
            query.CommandText = @"SELECT air_title,air_type,air_model,air_category FROM aircraft WHERE air_title=$title";
            query.Parameters.AddWithValue("$title", title);

            using (var reader = query.ExecuteReader())
            {
                if (reader.Read())
                {
                    result = new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                }
            }
            return result;
        }

        public bool PutAircraft(Aircraft aircraft)
        {
            Aircraft oldAircraft = GetAircraft(aircraft.Title);
            if (oldAircraft == null)
            {
                log.Debug?.Log($"Adding '{aircraft.Title}'");

                var cmd = DB().CreateCommand();
                cmd.CommandText = @"INSERT INTO aircraft(air_title,air_type,air_model,air_category) VALUES($title,$type,$model,$category)";
                cmd.Parameters.AddWithValue("$title", aircraft.Title);
                cmd.Parameters.AddWithValue("$type", aircraft.Type ?? "");
                cmd.Parameters.AddWithValue("$model", aircraft.Model ?? "");
                cmd.Parameters.AddWithValue("$category", aircraft.Category);

                if (cmd.ExecuteNonQuery() != 1)
                {
                    return false;
                }
            }
            else log.Debug?.Log($"Keeping '{aircraft.Title}'");
            return true;
        }

        public List<string> ListAllAircraft()
        {
            List<string> result = new();


            var query = DB().CreateCommand();
            query.CommandText = @"SELECT air_title FROM aircraft";

            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }
            return result;
        }

        public List<string> ListAircraftByCategory(string category)
        {
            List<string> result = new();


            var query = DB().CreateCommand();
            query.CommandText = @"SELECT air_title FROM aircraft WHERE air_category=$category";
            query.Parameters.AddWithValue("$category", category.ToLower());

            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }
            return result;
        }

        public void Dispose()
        {
            db?.Dispose();
        }
    }
}
