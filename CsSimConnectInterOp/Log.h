#pragma once
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


#include "File.h"

#include <map>
#include <vector>
#include <string>
#include <ostream>
#include <fstream>
#include <cwchar>

namespace nl {
namespace rakis {

	struct any {
		enum class Type { Int, Long, Float, String };
		any(const int   e) { m_data.INT = e; m_type = Type::Int; }
		any(const long   e) { m_data.LONG = e; m_type = Type::Long; }
		any(const float e) { m_data.FLOAT = e; m_type = Type::Float; }
		any(const char* e) { m_data.STRING = e; m_type = Type::String; }
		any(std::string const& s) { m_data.STRING = s.c_str(); m_type = Type::String; }
		Type get_type() const { return m_type; }
		int get_int() const { return m_data.INT; }
		unsigned long get_long() const { return m_data.LONG; }
		float get_float() const { return m_data.FLOAT; }
		const char* get_string() const { return m_data.STRING; }
	private:
		Type m_type;
		union {
			int   INT;
			long LONG;
			float FLOAT;
			const char* STRING;
		} m_data;
	};

	class Logger {
	public:
		enum Level {
			LOGLVL_TRACE,
			LOGLVL_DEBUG,
			LOGLVL_INFO,
			LOGLVL_WARN,
			LOGLVL_ERROR,
			LOGLVL_FATAL,
			LOGLVL_INIT
		};
		typedef std::array<char, 256> LineBuffer;
		typedef std::array<std::ofstream, 16> TargetArray;

		static void configure(const File& file);
		inline static Logger getLogger(const std::string& name) { return Logger(name); }

	private:
		std::string name_;
		Level level_;

		Logger(const std::string& name);

		void startLine(std::ostream& s, Level level);
		void stream(std::ostream& str) { str << std::endl; }

		template <class T, class... Types>
		void stream(std::ostream& str, const T& arg, const Types&... args) {
			str << arg;
			stream(str, args...);
		}
		template <class... Types>
		void stream(std::ostream& str, const std::wstring& arg, const Types&... args) {
			for (auto i = arg.begin(); i != arg.end(); i++) {
				str << char(wctob(*i));
			}
			stream(str, args...);
		}

		static Level rootLevel_;
		static std::map<std::string, Level> levels_;
		static std::map<std::string, size_t> logTargets_;
		static size_t numTargets_;
		static TargetArray targets_;
		static std::ofstream rootLog_;
		static bool configDone_;

		static bool parseLine(std::string& name, std::string& value, const LineBuffer& buf, const File& file);

		static Level getLevel(const std::string& name);
		static std::ostream& getStream(const std::string& name);

		template <class... Args>
		void log(Level level, const std::string& fmt, const Args&... args) {
			std::vector<any> argVec{ args... };
			unsigned arg{ 0 };

			std::ostream& s(getStream(name_));
			startLine(s, level);
			for (unsigned i{ 0 }; i < fmt.size(); i++) {
				if ((fmt[i] == '\\') && ((i + 1) < fmt.size())) {
					s.put(fmt[++i]);
				}
				else if ((fmt[i] == '{') && ((i + 1) < fmt.size()) && (fmt[i + 1] == '}')) {
					i++;
					if (arg < argVec.size()) {
						switch (argVec[arg].get_type()) {
						case any::Type::Int: s << argVec[arg].get_int(); break;
						case any::Type::Long: s << argVec[arg].get_long(); break;
						case any::Type::Float: s << argVec[arg].get_float(); break;
						case any::Type::String: s << argVec[arg].get_string(); break;
						}
						arg++;
					}
				}
				else {
					s.put(fmt[i]);
				}
			}
			s << std::endl;
			s.flush();
		}

	public:
		Logger(const Logger& log) : name_(log.name_), level_(log.level_) { }
		~Logger();

		const std::string& getName() const { return name_; }
		const char* getNameC() const { return name_.c_str(); }

		Level getLevel();
		void setLevel(Level level) { level_ = level; }

		bool isTraceEnabled() { return getLevel() <= LOGLVL_TRACE; }
		bool isDebugEnabled() { return getLevel() <= LOGLVL_DEBUG; }
		bool isInfoEnabled() { return getLevel() <= LOGLVL_INFO; }
		bool isWarnEnabled() { return getLevel() <= LOGLVL_WARN; }
		bool isErrorEnabled() { return getLevel() <= LOGLVL_ERROR; }
		bool isFatalEnabled() { return getLevel() <= LOGLVL_FATAL; }

		template <class... Args>
		void trace(const std::string& fmt, const Args&... args) {
			if (isTraceEnabled()) {
				log(LOGLVL_TRACE, fmt, args...);
			}
		}
		template <class... Args>
		void debug(const std::string& fmt, const Args&... args) {
			if (isDebugEnabled()) {
				log(LOGLVL_DEBUG, fmt, args...);
			}
		}
		template <class... Args>
		void info(const std::string& fmt, const Args&... args) {
			if (isInfoEnabled()) {
				log(LOGLVL_INFO, fmt, args...);
			}
		}
		template <class... Args>
		void warn(const std::string& fmt, const Args&... args) {
			if (isWarnEnabled()) {
				log(LOGLVL_WARN, fmt, args...);
			}
		}
		template <class... Args>
		void error(const std::string& fmt, const Args&... args) {
			if (isErrorEnabled()) {
				log(LOGLVL_ERROR, fmt, args...);
			}
		}
		template <class... Args>
		void fatal(const std::string& fmt, const Args&... args) {
			if (isFatalEnabled()) {
				log(LOGLVL_FATAL, fmt, args...);
			}
		}

		Logger& operator=(const Logger& log) {
			name_ = log.name_;
			level_ = log.level_;
		}

	private: // deleted and/or disallowed stuff
		Logger() =delete;
	};
}
}
