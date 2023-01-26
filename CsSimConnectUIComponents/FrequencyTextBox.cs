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

using Rakis.Logging;

namespace CsSimConnect.UIComponents
{
    public class FrequencyTextBox : NumberTextBox
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(FrequencyTextBox));

        public int FracDigits { get; set; }
        private string freqStyle = "NAV";

        public string FreqStyle
        {
            get => freqStyle;
            set
            {
                string upperValue = value.ToUpper();
                if (upperValue.Equals("NAV"))
                {
                    NumDigits = 5;
                    FracDigits = 2;
                }
                else if (upperValue.Equals("ADF"))
                {
                    NumDigits = 4;
                    FracDigits = 1;
                }
                freqStyle = upperValue;
            }
        }

        public FrequencyTextBox()
        {
            FreqStyle = "NAV";
        }

        public string NormalizeFreq(string input)
        {
            var point = '.';
            if (input.Contains(','))
            {
                point = ',';
            }
            string[] freqParts = input.Split(point);
            if (freqParts == null || (freqParts.Length == 0)) return "0";

            if (freqParts[0].Length > 3) freqParts[0] = freqParts[0].Substring(0, 3);
            if (freqParts.Length > 1)
            {
                return freqParts[0] + "." + (freqParts[1] + "000").Substring(0, FracDigits);
            }
            return (FracDigits > 0) ? (freqParts[0] + ".000".Substring(0, FracDigits + 1)) : freqParts[0];
        }

        public override uint AsUInt()
        {
            uint result = 0;
            foreach (char c in Text)
            {
                if (char.IsDigit(c))
                {
                    result = (uint)((result << 4) + (c - '0'));
                }
            }
            if (FreqStyle.Equals("ADF"))
            {
                result <<= 12;
            }
            log.Debug?.Log($"Frequency '{Text}' encoded as {result:X}");
            return result;
        }

        public void FromBCD(int freq)
        {
            if (freq < 0x100)
            {
                Set("0.00");
            }
            string result = "";
            while (freq > 0)
            {
                result = ((char)('0' + (freq & 0xf))) + result;
                if (result.Length == 4)
                {
                    result = "." + result;
                }
                freq >>= 4;
            }
            Set(result.Substring(0, result.Length - 2));
        }

        public override string FormatNumber(string fieldText)
        {
            return NormalizeFreq(fieldText);
        }

        public override void Set(string value)
        {
            log.Debug?.Log($"Frequency set to '{value}', formatting as {freqStyle}.");
            base.Set(value);
        }
    }
}
