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

using SimScanner.AddOns;
using static SimScanner.AddOns.Util;
using System;
using SimScanner.Sim;

namespace CsSimConnect
{
    class ListAddOns
    {
        static void Main(string[] args)
        {
            foreach (AddOn addOn in FindAddOns(SimScanner.Sim.Util.GetPrepar3Dv4()))
            {
                Console.WriteLine($"Addon \"{addOn.Name}\"");
                Console.WriteLine($"- Installed at {addOn.Path}");
                Console.WriteLine($"- Active={addOn.IsActive}");
                Console.WriteLine($"- Required={addOn.IsRequired}");

                foreach (Component comp in addOn.Components)
                {
                    Console.WriteLine($"  * {comp.Category} component at {comp.Path}");
                    if (comp.Category == ComponentCategory.Scenery)
                    {
                        Console.WriteLine($"    Name=\"{comp.Name}\"");
                        Console.WriteLine($"    Layer={comp.Layer}");
                    }
                }
            }
        }
    }
}
