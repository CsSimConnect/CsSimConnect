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

using System;
using System.Collections.Generic;
using System.IO;

namespace CsSimConnect
{
    public enum LogLevel
    {
        TRACE,
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }

    public class Logger
    {
        public string Name { get; init; }
        public string FullName { get; init; }
        public LogLevel Threshold { get; init; }

        public static Logger GetLogger(string name)
        {
            return new(name);
        }

        public static Logger GetLogger(Type type)
        {
            return new(type.FullName);
        }

        private static Dictionary<string, LogLevel> thresholds = new();

        private static string logPath = "cssimconnect.log";
        private static bool configDone;
        private static LogLevel rootLevel = LogLevel.INFO;

        public bool IsTraceEnabled => Threshold <= LogLevel.TRACE;
        public bool IsDebugEnabled => Threshold <= LogLevel.DEBUG;
        public bool IsInfoEnabled => Threshold <= LogLevel.INFO;
        public bool IsWarnEnabled => Threshold <= LogLevel.WARN;
        public bool IsErrorEnabled => Threshold <= LogLevel.ERROR;
        public bool IsFatalEnabled => Threshold <= LogLevel.FATAL;

        public LeveledLogger Trace { get; init; }
        public LeveledLogger Debug { get; init; }
        public LeveledLogger Info { get; init; }
        public LeveledLogger Warn { get; init; }
        public LeveledLogger Error { get; init; }
        public LeveledLogger Fatal { get; init; }

        private Logger(string fullName)
        {
            FullName = fullName;
            int index = fullName.LastIndexOf('.');
            Name = (index <= 1) || ((fullName.Length - index) <= 1) ? fullName : fullName[(index + 1)..];
            Threshold = FindThreshold(fullName);

            Trace = IsTraceEnabled ? new LeveledLogger(this, LogLevel.TRACE) : null;
            Debug = IsDebugEnabled ? new LeveledLogger(this, LogLevel.DEBUG) : null;
            Info = IsInfoEnabled ? new LeveledLogger(this, LogLevel.INFO) : null;
            Warn = IsWarnEnabled ? new LeveledLogger(this, LogLevel.WARN) : null;
            Error = IsErrorEnabled ? new LeveledLogger(this, LogLevel.ERROR) : null;
            Fatal = IsFatalEnabled ? new LeveledLogger(this, LogLevel.FATAL) : null;
        }

        private LogLevel FindThreshold(string key)
        {
            if ((key == null) || key.Length == 0)
            {
                return rootLevel;
            }
            if (thresholds.TryGetValue(key, out LogLevel result))
            {
                return result;
            }
            int index = key.LastIndexOf('.');
            return (index <= 0) ? rootLevel : FindThreshold(key.Substring(0, index));
        }

        private static LogLevel toLevel(string level)
        {
            try
            {
                return (LogLevel)Enum.Parse(typeof(LogLevel), level);
            }
            catch (Exception)
            {
                return LogLevel.INFO;
            }
        }

        private static bool parseLine(string line, out string path, out string value)
        {
            path = "";
            value = "";
            line = line.Trim();
            if (line.StartsWith("#") || line.StartsWith(";"))
            {
                return false;
            }
            string[] parts = line.Split("=");
            if (parts.Length != 2)
            {
                return false;
            }
            path = parts[0].Trim();
            value = parts[1].Trim();
            return true;
        }

        public static void Configure(string configPath = "rakisLog.properties")
        {
            if (configDone)
            {
                return;
            }
            if (File.Exists(configPath))
            {
                using StreamReader f = new(configPath);
                string line;
                while ((line = f.ReadLine()) != null) {
                    string path, value;
                    if (parseLine(line, out path, out value))
                    {
                        string[] elems = path.Split(".");
                        string[] values = value.Split(",");
                        if (elems[0] == "rootLogger")
                        {
                            if (values.Length == 1)
                            {
                                logPath = values[0].Trim();
                            }
                            else
                            {
                                rootLevel = toLevel(values[0].Trim());
                                logPath = values[1].Trim();
                            }
                        }
                        else
                        {
                            thresholds.Add(path, toLevel(value));
                        }
                    }
                }
            }
            using FileStream fs = new(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter fw = new(fs);
            fw.WriteLine(String.Format("Logger initialized with root level '{0}'", rootLevel.ToString()));

            configDone = true;
        }

        public void Log(LogLevel level, string fmt, params object[] msg)
        {
            if (Threshold <= level)
            {
                lock (this)
                {
                    using FileStream fs = new(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    using StreamWriter f = new(fs);
                    f.Write(String.Format("{0,19} {1,7} {2} ", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "[" + level.ToString() + "]", Name));
                    f.WriteLine(String.Format(fmt, msg));
                }
            }
        }

    }

    public class LeveledLogger
    {

        private readonly Logger logger;
        public LogLevel Level { get; init; }

        public LeveledLogger(Logger logger, LogLevel level)
        {
            this.logger = logger;
            Level = level;
        }

        public void Log(string fmt, params object[] msg)
        {
            logger.Log(Level, fmt, msg);
        }
    }
}
