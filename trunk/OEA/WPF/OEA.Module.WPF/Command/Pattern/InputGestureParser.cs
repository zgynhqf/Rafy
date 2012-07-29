/*******************************************************
 * 
 * 作者：http://www.codeproject.com/Articles/25445/WPF-Command-Pattern-Applied
 * 创建时间：周金根 2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 重新整理 胡庆访 20120518
 * 
*******************************************************/

using System;
using System.Windows.Input;
using OEA.MetaModel.View;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 包装 WPFCommand 并解析 Gestures。
    /// </summary>
    internal class InputGestureParser
    {
        private static readonly KeyGestureConverter _keyGestureConverter = new KeyGestureConverter();

        private InputGestureCollection _gestures;

        private InputGestureParser() { }

        internal static InputGestureCollection Parse(string gestures)
        {
            var parser = new InputGestureParser();
            parser.SetupGestures(gestures, string.Empty);
            return parser._gestures;
        }

        private InputGestureCollection Gestures
        {
            get
            {
                if (this._gestures == null)
                {
                    this._gestures = new InputGestureCollection();
                }

                return this._gestures;
            }
        }

        private void SetupGestures(string keyGestures, string displayStrings)
        {
            if (string.IsNullOrEmpty(displayStrings))
            {
                displayStrings = keyGestures;
            }

            while (!string.IsNullOrEmpty(keyGestures))
            {
                string currentDisplay;
                string currentGesture;
                int index = keyGestures.IndexOf(";", StringComparison.Ordinal);
                if (index >= 0)
                {
                    currentGesture = keyGestures.Substring(0, index);
                    keyGestures = keyGestures.Substring(index + 1);
                }
                else
                {
                    currentGesture = keyGestures;
                    keyGestures = string.Empty;
                }

                index = displayStrings.IndexOf(";", StringComparison.Ordinal);
                if (index >= 0)
                {
                    currentDisplay = displayStrings.Substring(0, index);
                    displayStrings = displayStrings.Substring(index + 1);
                }
                else
                {
                    currentDisplay = displayStrings;
                    displayStrings = string.Empty;
                }

                KeyGesture inputGesture = CreateFromResourceStrings(currentGesture, currentDisplay);
                if (inputGesture != null)
                {
                    this.Gestures.Add(inputGesture);
                }
            }
        }

        private KeyGesture CreateFromResourceStrings(string keyGestureToken, string keyDisplayString)
        {
            if (!string.IsNullOrEmpty(keyDisplayString))
            {
                keyGestureToken = keyGestureToken + ',' + keyDisplayString;
            }
            return (_keyGestureConverter.ConvertFromInvariantString(keyGestureToken) as KeyGesture);
        }
    }
}