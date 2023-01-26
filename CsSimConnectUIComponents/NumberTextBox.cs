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

using CsSimConnect.Events;
using Rakis.Logging;
using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace CsSimConnect.UIComponents
{

    public class NumberTextBox : TextBox
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(NumberTextBox));


        public int NumDigits { get; set; }
        public bool Positive { get; set; }
        public bool AddTicks { get; set; }

        private string wheelUpEventName;
        private ClientEvent wheelUpEvent;
        public string WheelUpEvent {
            get => wheelUpEventName;
            set {
                wheelUpEventName = value;
                wheelUpEvent = EventManager.GetEvent(wheelUpEventName);
            }
        }

        private string wheelDownEventName;
        private ClientEvent wheelDownEvent;
        public string WheelDownEvent
        {
            get => wheelDownEventName;
            set
            {
                wheelDownEventName = value;
                wheelDownEvent = EventManager.GetEvent(wheelDownEventName);
            }
        }

        private string setValueEventName;
        protected ClientEvent setValueEvent;
        public string SetValueEvent
        {
            get => setValueEventName;
            set
            {
                setValueEventName = value;
                setValueEvent = EventManager.GetEvent(setValueEventName);
            }
        }

        private string setSignedValueEventName;
        protected ClientEvent setSignedValueEvent;
        public string SetSignedValueEvent
        {
            get => setSignedValueEventName;
            set
            {
                setSignedValueEventName = value;
                setSignedValueEvent = EventManager.GetEvent(setSignedValueEventName);
            }
        }

        public NumberTextBox()
        {
            NumDigits = 3;
            Positive = true;
            AddTicks = false;
        }

        public string Normalize(string s)
        {
            if (s == null)
            {
                return "0";
            }

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
                digits--;
            }
            return sb.ToString();
        }

        public virtual void Set(string value)
        {
            Text = FormatNumber(value);
        }

        public void Set(int? value) => Set(value?.ToString());

        public void ReFormat()
        {
            Set(Text);
        }

        public string Normalized() => Normalize(Text);

        public virtual int AsInt()
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

        public virtual uint AsUInt()
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReFormat();

                setValueEvent?.Send(data: AsUInt());
                setSignedValueEvent?.SendSigned(data: AsInt());

                Keyboard.ClearFocus();
            }
            base.OnKeyDown(e);
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

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                wheelUpEvent?.Send();
            }
            else if (e.Delta < 0)
            {
                wheelDownEvent?.Send();
            }
            base.OnMouseWheel(e);
        }
    }
}
