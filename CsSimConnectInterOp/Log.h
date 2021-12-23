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
		enum class Type { Char, Int, UInt, Long, ULong, LLong, ULLong, Float, Double, String, WString };
		any(char e) { m_data.char_ = e; m_type = Type::Char; }
		any(const int   e) { m_data.int_ = e; m_type = Type::Int; }
		any(const unsigned int   e) { m_data.uint_ = e; m_type = Type::UInt; }
		any(const long   e) { m_data.long_ = e; m_type = Type::Long; }
		any(const unsigned long   e) { m_data.ulong_ = e; m_type = Type::ULong; }
		any(const long long   e) { m_data.llong_ = e; m_type = Type::LLong; }
		any(const unsigned long long   e) { m_data.ullong_ = e; m_type = Type::ULLong; }
		any(const float e) { m_data.float_ = e; m_type = Type::Float; }
		any(const double e) { m_data.double_ = e; m_type = Type::Double; }
		any(const char* e) { m_data.str_ = e; m_type = Type::String; }
		any(const wchar_t* e) { m_data.wstr_ = e; m_type = Type::WString; }
		any(std::string const& s) { m_data.str_ = s.c_str(); m_type = Type::String; }
		Type type() const { return m_type; }
		int asChar() const { return m_data.char_; }
		int asInt() const { return m_data.int_; }
		int asUInt() const { return m_data.uint_; }
		long asLong() const { return m_data.long_; }
		unsigned long asULong() const { return m_data.ulong_; }
		long long asLLong() const { return m_data.llong_; }
		unsigned long long asULLong() const { return m_data.ullong_; }
		float asFloat() const { return m_data.float_; }
		double asDouble() const { return m_data.double_; }
		const char* asString() const { return m_data.str_; }
		const wchar_t* asWString() const { return m_data.wstr_; }
	private:
		Type m_type;
		union {
			char char_;
			int   int_;
			unsigned int uint_;
			long long_;
			unsigned long ulong_;
			long long llong_;
			unsigned long long ullong_;
			float float_;
			double double_;
			const char* str_;
			const wchar_t* wstr_;
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
		void stream(std::ostream& str) { str << std::endl << std::flush; }

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
						switch (argVec[arg].type()) {
						case any::Type::Char: s << argVec[arg].asChar(); break;
						case any::Type::Int: s << argVec[arg].asInt(); break;
						case any::Type::UInt: s << argVec[arg].asUInt(); break;
						case any::Type::Long: s << argVec[arg].asLong(); break;
						case any::Type::ULong: s << argVec[arg].asULong(); break;
						case any::Type::LLong: s << argVec[arg].asLLong(); break;
						case any::Type::ULLong: s << argVec[arg].asULLong(); break;
						case any::Type::Float: s << argVec[arg].asFloat(); break;
						case any::Type::Double: s << argVec[arg].asDouble(); break;
						case any::Type::String: s << argVec[arg].asString(); break;
						case any::Type::WString: s << argVec[arg].asWString(); break;
						}
						arg++;
					}
				}
				else {
					s.put(fmt[i]);
				}
			}
			s << std::endl << std::flush;
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
