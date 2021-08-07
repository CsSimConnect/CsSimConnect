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
    public class FrequencyTextBox : NumberTextBox
    {
        private string freqStyle = "NAV";

        public string FreqStyle
        {
            get => freqStyle;
            set
            {
                string upperValue = value.ToUpper();
                if (upperValue.Equals("NAV"))
                {
                    NumDigits = 6;
                }
                else if (upperValue.Equals("ADF"))
                {
                    NumDigits = 5;
                }
                freqStyle = upperValue;
            }
        }

        public FrequencyTextBox()
        {
            FreqStyle = "NAV";

            TextChanged += new TextChangedEventHandler(MaskedTextBox_TextChanged);
        }

        private void MaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is FrequencyTextBox tbEntry && tbEntry.Text.Length > 0)
            {
                tbEntry.Text = FormatNumber(tbEntry.Text);
                CaretIndex = tbEntry.Text.Length;
            }
        }

        override public string FormatNumber(string fieldText)
        {
            StringBuilder sb = new();

            if (fieldText != null)
            {
                foreach (char c in fieldText)
                {
                    if (char.IsDigit(c))
                    {
                        if (sb.Length == 3)
                        {
                            sb.Append('.');
                        }
                        sb.Append(c);
                    }
                    if (sb.Length == NumDigits)
                    {
                        break;
                    }
                }
            }
            return sb.ToString();
        }

    }
}
