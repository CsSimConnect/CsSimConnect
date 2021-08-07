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

using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace CsSimConnect.UIComponents
{

    public class NumberTextBox : TextBox
    {
        private static readonly Logger log = Logger.GetLogger(typeof(NumberTextBox));


        public int NumDigits { get; set; }
        public bool Positive { get; set; }
        public bool AddTicks { get; set; }

        public NumberTextBox()
        {
            NumDigits = 3;
            Positive = true;
            AddTicks = false;

            TextChanged += new TextChangedEventHandler(MaskedTextBox_TextChanged);
        }

        void MaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is FrequencyTextBox tbEntry && tbEntry.Text.Length > 0)
            {
                tbEntry.Text = FormatNumber(tbEntry.Text);
            }
        }

        public string Normalize(string s)
        {
            if (s == null) return "0";

            StringBuilder bld = new();
            uint digitCount = 0;
            foreach (char c in s)
            {
                if (!Positive && ((bld.Length == 0) && (c == '-')))
                {
                    bld.Append(c);
                    continue;
                }
                else if (char.IsDigit(c) && (digitCount < NumDigits))
                {
                    bld.Append(c);
                }
                else
                {
                    log.Warn?.Log($"Dropping character '{c}' from field value \"{s}\".");
                }
            }
            return bld.ToString();
        }

        public virtual string FormatNumber(string fieldText)
        {
            if (fieldText == null) return "0";
            if (!AddTicks) return fieldText;

            char[] s = Normalize(fieldText).ToCharArray();
            Array.Reverse(s);

            StringBuilder sb = new();
            uint digits = 3;

            foreach (char c in s)
            {
                if ((digits == 0) && char.IsDigit(c))
                {
                    sb.Insert(0, ',');
                    digits = 3;
                }
                sb.Insert(0, c);
            }
            return sb.ToString();
        }

        public void Set(string value)
        {
            Text = FormatNumber(value);
        }

        public void Set(int? value) => Set(value?.ToString());

        public void ReFormat()
        {
            Set(Text);
        }

        public string Normalized() => Normalize(Text);

        public int AsInt()
        {
            try
            {
                return int.Parse(Normalize(Text));
            }
            catch (Exception e)
            {
                log.Error?.Log($"Exception while parsing number: {e.Message}");
            }
            return 0;
        }

        public uint AsUInt()
        {
            try
            {
                return uint.Parse(Normalize(Text));
            }
            catch (Exception e)
            {
                log.Error?.Log($"Exception while parsing number: {e.Message}");
            }
            return 0;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (SelectionLength == 0)
            {
                SelectAll();
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsKeyboardFocusWithin)
            {
                Focus();
                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseLeftButtonDown(e);
            }
        }
    }
}
