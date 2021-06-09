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

using SimScanner.Bgl;
using System;

namespace BglReader
{
    class BglReader
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"BglReader called with {args.Length} arguments, #0 is '{args[0]}'");
            var file = new BglFile(args[0]);
            var status = file.Valid ? "VALID" : "NOT VALID";
            Console.WriteLine($"BglFile(\"{args[0]}\") is {status}");
            Console.WriteLine($"Magic1 = 0x{file.Header.Magic1:x}, Magic2 = 0x{file.Header.Magic2:x}");
            Console.WriteLine($"FileTime = {file.FileTime}");
            Console.WriteLine($"Number of sections = {file.Header.SectionCount}");

            foreach (BglSection section in file.Sections)
            {
                Console.WriteLine($"Section {section.Index}: {section.Header.Type} ({section.Header.SubSectionCount} subsection(s) of {section.Header.SubSectionSize} byte(s) each)");
                if (section.IsAirport)
                {
                    Console.WriteLine($"  ==> ICAO code {section.Airport.ICAO}, region ident {section.Airport.RegionCode}.");
                }
            }
        }
    }
}
