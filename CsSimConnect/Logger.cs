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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly string name;

        public static Logger GetLogger(string name)
        {
            return new(name);
        }

        public static Logger GetLogger(Type type)
        {
            return new(type.Name);
        }

        private static string logPath = "cssimconnect.log";
        private static bool configDone = false;
        private static LogLevel rootLevel = LogLevel.INFO;

        private Logger(string name)
        {
            this.name = name;
        }

        private static LogLevel toLevel(string level)
        {
            try
            {
                return (LogLevel)Enum.Parse(typeof(LogLevel), level);
            }
            catch (Exception _)
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

        public static void Configure(string configPath ="rakisLog.properties")
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
                        if (elems[0].Equals("rootLogger"))
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
                        // TODO
                    }
                }
            }
            using FileStream fs = new(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter fw = new(fs);
            fw.WriteLine(String.Format("Logger initialized with root level '{0}'", rootLevel.ToString()));

            configDone = true;
        }

        public LogLevel GetThreshold()
        {
            return rootLevel; // TODO
        }

        public bool IsTraceEnabled()
        {
            return GetThreshold() <= LogLevel.TRACE;
        }

        public bool IsDebugEnabled()
        {
            return GetThreshold() <= LogLevel.DEBUG;
        }

        public bool IsInfoEnabled()
        {
            return GetThreshold() <= LogLevel.INFO;
        }

        public bool IsWarnEnabled()
        {
            return GetThreshold() <= LogLevel.WARN;
        }

        public bool IsErrorEnabled()
        {
            return GetThreshold() <= LogLevel.ERROR;
        }

        public bool IsFatalEnabled()
        {
            return GetThreshold() <= LogLevel.FATAL;
        }

        public void Log(LogLevel level, string fmt, params object[] msg)
        {
            if (GetThreshold() <= level)
            {
                lock (this)
                {
                    using FileStream fs = new(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    using StreamWriter f = new(fs);
                    f.Write(String.Format("{0,19} {1,7} {2} ", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), "[" + level.ToString() + "]", name));
                    f.WriteLine(String.Format(fmt, msg));
                }
            }
        }

        public void Trace(string fmt, params object[] msg)
        {
            Log(LogLevel.TRACE, fmt, msg);
        }
        public void Debug(string fmt, params object[] msg)
        {
            Log(LogLevel.DEBUG, fmt, msg);
        }
        public void Info(string fmt, params object[] msg)
        {
            Log(LogLevel.INFO, fmt, msg);
        }
        public void Warn(string fmt, params object[] msg)
        {
            Log(LogLevel.WARN, fmt, msg);
        }
        public void Error(string fmt, params object[] msg)
        {
            Log(LogLevel.ERROR, fmt, msg);
        }
        public void Fatal(string fmt, params object[] msg)
        {
            Log(LogLevel.FATAL, fmt, msg);
        }
    }
}
