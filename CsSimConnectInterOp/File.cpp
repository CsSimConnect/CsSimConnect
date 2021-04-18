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


#include <iostream>
#include <sstream>
#include <vector>

#include "File.h"

using namespace std;

using namespace nl::rakis;


const char    File::separatorChar1('\\');
const wchar_t File::separatorChar('\\');
const string  File::separator1("\\");
const wstring File::separator(L"\\");

File::File()
	: statValid_(false), statDone_(false), statResult_(-1)
{
}

File::File(const char* name)
	: path_(str2wstr(name)), statValid_(false), statDone_(false), statResult_(-1)
{
	init();
}

File::File(const string& name)
	: path_(str2wstr(name)), statValid_(false), statDone_(false), statResult_(-1)
{
	init();
}

File::File(const wchar_t* name)
	: path_(name), statValid_(false), statDone_(false), statResult_(-1)
{
	init();
}

File::File(const wstring& name)
	: path_(name), statValid_(false), statDone_(false), statResult_(-1)
{
	init();
}

File::File(File const& file)
    : path_(file.path_), statValid_(false), statDone_(false), statResult_(-1)
{
	init();
}


File::~File()
{
}

File& File::operator=(File const& file)
{
    path_ = file.path_;
    statValid_ = false;
    statDone_ = false;
    statResult_ = -1;

    init();

    return *this;
}

wstring File::getParent() const
{
	wstring::const_iterator it(path_.end());

	while ((it != path_.begin()) && (*--it != separatorChar)) /* DONOTHING*/;

	return wstring(path_.begin(), it);
}

string File::getParent1() const
{
	wstring::const_iterator it(path_.end());

	while ((it != path_.begin()) && (*--it != separatorChar)) /* DONOTHING*/;

	return wstr2str(wstring(path_.begin(), it));
}

bool File::isAbsolute() const
{
	return !path_.empty() && ((path_ [0] == separatorChar) || ((path_.size() >= 2) && isalpha(path_ [0]) && (path_ [1] == ':')));
}

void File::init()
{
    if (path_.empty() || (path_ [path_.size()-1] == separatorChar)) {
        path_.append(L".");
    }
	statResult_ = _wstat(path_.c_str(), &stat_);
	statDone_ = true;
	statValid_ = (statResult_ == 0);

	wstring::const_iterator it(path_.end());
	while ((it != path_.begin()) && (*--it != separatorChar)) /* DONOTHING*/;
	if ((it != path_.end()) && (*it == separatorChar)) {
		it++;
	}
	name_.assign(it, (wstring::const_iterator)path_.end());
}

const wstring File::defaultPrefix(L"temp_");
const wstring File::defaultExt(L"tmp");

File File::getChild(wstring const& name) const
{
    wstring childPath(path_);
    childPath.append(separator).append(name);
    return childPath;
}

File File::getChild(string const& name) const
{
    wstring childPath(path_);
    childPath.append(separator).append(name.begin(), name.end());
    return childPath;
}

wstring File::getExt() const
{
    wstring::size_type idx = name_.rfind('.');
    if ((idx > 0) && (idx < name_.size())) {
        return name_.substr(idx+1);
    }
    return L"";
}

string File::getExt1() const
{
    wstring::const_iterator it(name_.end());
    while (it != name_.begin()) {
        if (*--it == '.') {
            return wstr2str(wstring(++it, name_.end()));
        }
    }
    return "";
}

bool File::remove()
{
    bool result(!exists());

    if (!result) {
        result = _unlink(getPath1().c_str()) == 0;
        if (result) {
            statValid_ = false;
            statDone_ = false;
            statResult_ = -1;

            init();
        }
    }
    return result;
}