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

namespace AutoPilotController
{
    public class HeadingTextBox : TextBox
    {

        public HeadingTextBox()
        {
            TextChanged += new TextChangedEventHandler(MaskedTextBox_TextChanged);
        }

        void MaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is FrequencyTextBox tbEntry && tbEntry.Text.Length > 0)
            {
                tbEntry.Text = formatNumber(tbEntry.Text);
            }
        }

        public static string formatNumber(string FieldText)
        {
            int maxLen = 3;
            StringBuilder sb = new StringBuilder();

            if (FieldText != null)
            {
                foreach (char c in FieldText)
                {
                    if (char.IsDigit(c))
                    {
                        sb.Append(c);
                    }
                    if (sb.Length == maxLen)
                    {
                        break;
                    }
                }
            }
            return sb.ToString();
        }
    }
}
