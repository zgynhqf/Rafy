using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace Rafy.WPF
{
    public class TipTextBox : TextBox
    {
        private string _emptyValue;

        public string EmptyValue
        {
            get
            {
                return _emptyValue;
            }
            set
            {
                _emptyValue = value;
                ProcessDefaultValue();
            }
        }

        public TipTextBox()
        {
            _emptyValue = string.Empty;
            this.Foreground = Brushes.Gray;
        }

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            if (this.Text == _emptyValue)
            {
                this.Text = string.Empty;
            }

            Foreground = Brushes.Black;
        }

        protected override void OnLostKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            ProcessDefaultValue();

            Foreground = Brushes.Gray;
        }

        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            SelectAll();
        }

        private void ProcessDefaultValue()
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                this.Text = this._emptyValue;
            }
        }
    }
}