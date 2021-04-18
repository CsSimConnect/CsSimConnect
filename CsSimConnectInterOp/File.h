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

 
/**
 * nl::rakis::File
 */

#include <sys/stat.h>
#include <string>
#include <locale>
#include <utility>
#include <codecvt>

namespace nl {
	namespace rakis {

		inline std::wstring str2wstr(const std::string& s) { return std::wstring(s.begin(), s.end()); }
        inline std::string wstr2str(const std::wstring& s) {
			std::string result; result.reserve(s.size()); for (auto c: s) { int b(wctob(c)); if (b != EOF) { result.push_back(char(b)); } } return result;
//				std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> conv;
//				return conv.to_bytes(s);
		};

		class File {
		public:  // Types and constants
			static const wchar_t      separatorChar;
			static const char         separatorChar1;
			static const std::wstring separator;
			static const std::string  separator1;

		private: // Hidden members
			std::wstring name_;
			std::wstring path_;
			struct _stat stat_;
			bool         statValid_;
			bool         statDone_;
			int          statResult_;

			void init();

            static const std::wstring defaultPrefix;
            static const std::wstring defaultExt;

        public:  // Public Members
			File();
			File(const char* name);
			File(const std::string& name);
			File(const wchar_t* name);
			File(const std::wstring& name);
            File(File const& file);
			~File();

            File tempFile(std::wstring const& prefix, std::wstring const& ext) const;
            File tempFile(std::string const& prefix,  std::wstring const& ext) const { return tempFile(str2wstr(prefix), ext); };
            File tempFile(std::wstring const& prefix, std::string const& ext)  const { return tempFile(prefix, str2wstr(ext)); };
            File tempFile(std::string const& prefix, std::string const& ext)   const { return tempFile(str2wstr(prefix), str2wstr(ext)); };
            File tempFile(std::wstring const& ext)                             const { return tempFile(defaultPrefix, ext); };
            File tempFile(std::string const& ext)                              const { return tempFile(defaultPrefix, str2wstr(ext)); };
            File tempFile(const char* ext)                                     const { return tempFile(defaultPrefix, str2wstr(ext)); };
            File tempFile()                                                    const { return tempFile(defaultPrefix, defaultExt); };

            bool canExecute()             const { return statValid_ && ((stat_.st_mode&_S_IEXEC) != 0); }
			bool canRead()                const { return statValid_ && ((stat_.st_mode&_S_IREAD) != 0); }
			bool canWrite()               const { return statValid_ && ((stat_.st_mode&_S_IWRITE) != 0); }
			bool exists()                 const { return statValid_; }

            bool remove();

            std::wstring const& getPath() const { return path_; }
            std::string  getPath1()       const { return wstr2str(path_); };
			std::wstring const& getName() const { return name_; }
            std::string  getName1()       const { return wstr2str(name_); };
			std::wstring getParent()      const;
            std::string  getParent1()     const;
            std::wstring getExt()         const;
            std::string  getExt1()        const;

            File getParentFile()          const { return File(getParent()); }
            File getChild(std::wstring const& name) const;
            File getChild(std::string const& name) const;

            bool isAbsolute() const;
			bool isDirectory()            const { return statValid_ && ((stat_.st_mode&S_IFDIR) != 0); }
			bool isFile()                 const { return statValid_ && ((stat_.st_mode&S_IFREG) != 0); }
			size_t length()               const { return (statValid_ ? stat_.st_size : -1); }

			operator const wchar_t*()     const { return path_.c_str(); }
            File& operator= (File const& file);

		private: // Blocked implementations
		};

	}
}
