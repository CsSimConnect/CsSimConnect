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

using System.Text;
using System.Windows.Controls;

namespace CsSimConnect.UIComponents
{
    public class NumberTextBox : TextBox
    {

        public int NumDigits { get; set; }
        public bool Positive { get; set; }
        public bool AddTicks { get; set; }

        public NumberTextBox()
        {
            NumDigits = 3;
            TextChanged += new TextChangedEventHandler(MaskedTextBox_TextChanged);
        }

        void MaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is FrequencyTextBox tbEntry && tbEntry.Text.Length > 0)
            {
                tbEntry.Text = FormatNumber(tbEntry.Text);
            }
        }

        public static string Normalize(string s)
        {
            return s?.Replace(",", "").Trim() ?? "0";
        }

        public string FormatNumber(string FieldText)
        {
            int digitCount = 0;
            StringBuilder sb = new();

            if (FieldText != null)
            {
                foreach (char c in FieldText)
                {
                    if (!Positive && (c == '-'))
                    {
                        sb.Append(c);
                    }
                    else if (char.IsDigit(c))
                    {
                        sb.Append(c);
                        if (++digitCount == NumDigits)
                        {
                            break;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public string Normalized() => Normalize(Text);
        public int AsInt() => int.Parse(Normalize(Text));
        public uint AsUInt() => uint.Parse(Normalize(Text));

    }
}
