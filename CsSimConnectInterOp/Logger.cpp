#include "pch.h"
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


#include <ctime>
#include <cctype>
#include <cstring>

#include <array>
#include <chrono>
#include <fstream>
#include <iostream>

#include "Log.h"

using namespace nl::rakis;

using namespace std;

static const string CFG_SEPARATOR(".");
static const string CFG_ROOTLOGGER("rootLogger");
static const string CFG_FILENAME("filename");
static const string CFG_LOGGER("logger");

static const string CFG_STDOUT("STDOUT");
static const string CFG_STDERR("STDERR");

static const string CFG_LEVEL_TRACE("TRACE");
static const string CFG_LEVEL_DEBUG("DEBUG");
static const string CFG_LEVEL_INFO("INFO");
static const string CFG_LEVEL_WARN("WARN");
static const string CFG_LEVEL_ERROR("ERROR");
static const string CFG_LEVEL_FATAL("FATAL");
static const string CFG_LEVEL_INIT("INIT");

/*static*/ map<string, Logger::Level> Logger::levels_;
/*static*/ map<string, size_t> Logger::logTargets_;
/*static*/ size_t Logger::numTargets_(0);
/*static*/ Logger::TargetArray Logger::targets_;
/*static*/ ofstream Logger::rootLog_;
/*static*/ bool Logger::configDone_(false);


static string strip(string::const_iterator p, string::const_iterator q) {
	while ((p < q) && isblank(*p)) { p++; }
	while ((p < q) && isblank(q[-1])) { q--; }

	return string(p, q);
}

static string strip(const string& s) {
	return strip(s.begin(), s.end());
}

static void split(vector<string>& parts, string s, string sep, bool doStrip =false)
{
	parts.clear();
	const size_t len = s.size();
	const size_t sepLen = sep.size();
	string::const_iterator begin = s.begin();
	size_t p = 0;
	size_t q = s.find(sep);

	while ((p < len) && (q != string::npos)) {
		if (q != p) {
			parts.emplace_back(begin + p, begin + q);
		}
		p = q + sepLen;
		q = s.find(sep, p);
	}
	if (p < len) {
		if (doStrip) {
			parts.push_back(strip(begin + p, begin + len));
		}
		else {
			parts.emplace_back(begin + p, begin + len);
		}
	}
}

static string join(vector<string>::const_iterator begin, vector<string>::const_iterator end, string sep)
{
	string result;

	while (begin != end) {
		if (!result.empty()) {
			result.append(sep);
		}
		result.append(*begin);
		begin++;
	}
	return result;
}

Logger::Logger(const string& name)
	: name_(name)
{
	level_ = getLevel(name);
}

Logger::~Logger()
{

}

Logger::Level nl::rakis::Logger::getLevel()
{
	if (level_ == LOGLVL_INIT) {
		// Check if we're still uninitialized
		if (configDone_) {
			level_ = getLevel(name_);
		}
	}
	return level_;
}

/*static*/ Logger::Level Logger::getLevel(const string& name)
{
	if (!configDone_) { // Invalid rootLog_ => non-initialized
		return LOGLVL_INIT;
	}

	auto lvlIt = levels_.find(name);
	if (lvlIt == levels_.end()) {
		size_t pos = name.rfind("::");
		size_t dotPos = name.rfind(".");

		if ((pos == string::npos) && (dotPos != string::npos)) {
			pos = dotPos;
		}
		else if ((pos != string::npos) && (dotPos != string::npos) && (dotPos > pos)) {
			pos = dotPos;
		}

		if ((pos == string::npos) || (pos == 0)) {
			return LOGLVL_INFO;
		}

		string next(name.begin(), name.begin() + pos);
		return getLevel(next);
	}
	return lvlIt->second;
}

/*static*/ ostream& Logger::getStream(const string& name)
{
	auto tgtIt = logTargets_.find(name);
	if (tgtIt == logTargets_.end()) {
		size_t pos = name.rfind("::");

		size_t dotPos = name.rfind(".");
		if ((dotPos != string::npos) && (dotPos > pos)) {
			pos = dotPos;
		}

		if ((pos == string::npos) || (pos == 0)) {
			return rootLog_;
		}

		string next(name.begin(), name.begin() + (pos - 1));
		return getStream(next);
	}
	return targets_[tgtIt->second];
}

/*static*/ bool Logger::parseLine(string&name, string& value, const LineBuffer& buf, const File& file)
{
	name.clear();
	value.clear();

	auto p = buf.begin();

	// Skip leading blanks
	while ((p != buf.end()) && (*p != '\0') && isblank(*p)) { p++; }

	// skip line if at end of comment-start found
	if ((p == buf.end()) || (*p == '\0') || (*p == '#') || (*p == ';')) {
		return false;
	}

	// find end of name, being words ([A-Za-z0-9\-_]) separated by dots or colons
	auto q = p;
	while ((q != buf.end()) && (*q != '\0') && (isalnum(*q) || (*q == '.') || (*q == '_') || (*q == '-') || (*q == ':'))) { q++; }

	name.assign(p, q); // we got a name

	p = q; // restart scanning

	// skip any blanks after name
	while ((p != buf.end()) && (*p != '\0') && isblank(*p)) { p++; }

	if ((p == buf.end()) || (*p == '\0') || (*p != '=')) {
		rootLog_ << "configure(\"" << file << "\"): no '=' in \"" << buf.data() << "\"" << endl;
		return false;
	}
	p++; // skip '='

		 // skip any blanks after '='
	while ((p != buf.end()) && (*p != '\0') && isblank(*p)) { p++; }

	q = p;
	while ((q != buf.end()) && (*q != '\0') && (*q != '#') && (*q != ';')) { q++; }

	//strip spaces at end
	while ((q > p) && isblank(q[-1])) { q--; }

	value.assign(p, q);

	return true;
}

static const string LOGLVL_NAME[] = {
	CFG_LEVEL_TRACE, CFG_LEVEL_DEBUG, CFG_LEVEL_INFO, CFG_LEVEL_WARN, CFG_LEVEL_ERROR, CFG_LEVEL_FATAL, CFG_LEVEL_INIT
};

/*static*/ void Logger::configure(const File& file)
{
	if (file.exists()) {
		ifstream cfg(file);
		LineBuffer buf;
		string name;
		string value;
		vector<string> parts;

		while (cfg.getline(buf.data(), buf.size())) {
			if (parseLine(name, value, buf, file)) {
				split(parts, name, CFG_SEPARATOR);

				if (parts[0] == CFG_ROOTLOGGER) {
					rootLog_.open(value, ios_base::out|ios_base::app);
					if (parts.size() > 1) {
						rootLog_ << "configure(): ignoring name after \"rootLogger\"" << endl;
					}
				}
				else if (parts[0] == CFG_LOGGER) {
					string tag(join(parts.begin() + 1, parts.end(), CFG_SEPARATOR));
					split(parts, value, ",", true);

					if (parts.size() > 0) {
						if (parts[0] == CFG_LEVEL_TRACE) {
							levels_[tag] = LOGLVL_TRACE;
						}
						else if (parts[0] == CFG_LEVEL_DEBUG) {
							levels_[tag] = LOGLVL_DEBUG;
						}
						else if (parts[0] == CFG_LEVEL_INFO) {
							levels_[tag] = LOGLVL_INFO;
						}
						else if (parts[0] == CFG_LEVEL_WARN) {
							levels_[tag] = LOGLVL_WARN;
						}
						else if (parts[0] == CFG_LEVEL_ERROR) {
							levels_[tag] = LOGLVL_ERROR;
						}
						else if (parts[0] == CFG_LEVEL_FATAL) {
							levels_[tag] = LOGLVL_FATAL;
						}
						else {
							rootLog_ << "configure(): ignoring level \"" << value << "\" for \"" << tag << "\"" << endl;
						}
					}
					if (parts.size() > 1) {
						if (numTargets_ == targets_.size()) {
							rootLog_ << "configure(): ignoring target \"" << parts [1] << "\" for \"" << tag << "\". Limit of " << numTargets_ << "reached." << endl;
						}
						else {
							size_t index = numTargets_++;
							targets_[index].open(parts[1], ios_base::out | ios_base::app);
							logTargets_[tag] = index;
						}
					}
				}
			}
		}
		configDone_ = true;
	}
}

static void num(ostream& s, int i)
{
	if (i >= 100) {
		s << char('0' + ((i / 1000) % 10)) << char('0' + ((i / 100) % 10)) << char('0' + ((i / 10) % 10)) << char('0' + (i % 10));
	}
	else {
		s << char('0' + ((i / 10) % 10)) << char('0' + (i % 10));
	}
}

void Logger::startLine(ostream& s, Level level)
{
	time_t tt(chrono::system_clock::to_time_t(chrono::system_clock::now()));
	tm ti;

	localtime_s(&ti, &tt);
	
	num(s, ti.tm_year + 1900); s << '/'; num(s, ti.tm_mon + 1); s << '/'; num(s, ti.tm_mday); s << "T";
	num(s, ti.tm_hour); s << ':'; num(s, ti.tm_min); s << ':'; num(s, ti.tm_sec);
	s << " [" << LOGLVL_NAME [level] << "] " << name_ << " ";
}
