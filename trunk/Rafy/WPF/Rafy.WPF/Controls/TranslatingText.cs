/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121108 16:16
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121108 16:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 一个可以自动翻译开发文本的文本显示控件
    /// </summary>
    [ContentProperty("Text")]
    public class TranslatingText : Control
    {
        /// <summary>
        /// 需要被自动翻译的属性列表。
        /// </summary>
        public static readonly List<DependencyProperty> TranslatingProperties;

        static TranslatingText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TranslatingText), new FrameworkPropertyMetadata(typeof(TranslatingText)));

            TranslatingProperties = new List<DependencyProperty>();
        }

        private bool _translatedTextInvalidated = true;

        #region Text DependencyProperty

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TranslatingText),
            new PropertyMetadata((d, e) => (d as TranslatingText).OnTextChanged(e))
            );

        /// <summary>
        /// 要显示的翻译前的文本。
        /// </summary>
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        private void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            this._translatedTextInvalidated = true;
            this.InvalidateMeasure();
        }

        #endregion

        #region TranslatedText DependencyProperty

        private static readonly DependencyPropertyKey TranslatedTextPropertyKey = DependencyProperty.RegisterReadOnly(
            "TranslatedText", typeof(string), typeof(TranslatingText), new PropertyMetadata());

        public static readonly DependencyProperty TranslatedTextProperty = TranslatedTextPropertyKey.DependencyProperty;

        /// <summary>
        /// 翻译后的文本（只读属性）
        /// </summary>
        public string TranslatedText
        {
            get { return (string)this.GetValue(TranslatedTextProperty); }
            private set { this.SetValue(TranslatedTextPropertyKey, value); }
        }

        #endregion

        protected override Size MeasureOverride(Size constraint)
        {
            if (this._translatedTextInvalidated)
            {
                this.TranslatedText = this.Text.Translate();

                this._translatedTextInvalidated = false;
            }

            return base.MeasureOverride(constraint);
        }

        #region AutoTranslate AttachedDependencyProperty

        /// <summary>
        /// 在第一次加载元素完成 (Loaded 事件) 时，尝试自动翻译一系列属性。
        /// 并在属性变更时，会再次翻译。
        /// 
        /// 目前自动翻译以下属性：
        /// FrameworkElement.ToolTipProperty
        /// ContentControl.ContentProperty
        /// TextBlock.TextProperty
        /// Window.TitleProperty
        /// 如果需要添加新的翻译属性，可以把属性添加到 TranslatingProperties 集合中。
        /// </summary>
        public static readonly DependencyProperty AutoTranslateProperty = DependencyProperty.RegisterAttached(
            "AutoTranslate", typeof(bool), typeof(TranslatingText),
            new PropertyMetadata(AutoTranslatePropertyChanged)
            );

        public static bool GetAutoTranslate(FrameworkElement element)
        {
            return (bool)element.GetValue(AutoTranslateProperty);
        }

        public static void SetAutoTranslate(FrameworkElement element, bool value)
        {
            element.SetValue(AutoTranslateProperty, value);
        }

        private static void AutoTranslatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element != null)
            {
                var value = (bool)e.NewValue;
                if (value)
                {
                    element.Loaded += TranslateElementOnLoaded;
                }
                else
                {
                    element.Loaded -= TranslateElementOnLoaded;
                }
            }
        }

        private static void TranslateElementOnLoaded(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                element.Loaded -= TranslateElementOnLoaded;

                Translate(element);
            }
        }

        #endregion

        /// <summary>
        /// 尝试翻译几种已经元素的特定几个属性。
        /// 
        /// FrameworkElement.ToolTipProperty
        /// ContentControl.ContentProperty
        /// TextBlock.TextProperty
        /// Window.TitleProperty
        /// </summary>
        /// <param name="element"></param>
        private static void Translate(FrameworkElement element)
        {
            //如果服务端还没有启动完成的话，翻译会出错，这些需要忽略这些错误。
            try
            {
                TryTranslate(element, FrameworkElement.ToolTipProperty);

                var win = element as Window;
                if (win != null)
                {
                    TryTranslate(win, Window.TitleProperty);
                }

                var cc = element as ContentControl;
                if (cc != null)
                {
                    TryTranslate(cc, ContentControl.ContentProperty);
                }

                var txt = element as TextBlock;
                if (txt != null)
                {
                    TryTranslate(txt, TextBlock.TextProperty);
                }

                if (TranslatingProperties.Count > 0)
                {
                    foreach (var property in TranslatingProperties)
                    {
                        TryTranslate(element, property);
                    }
                }
            }
            catch { }
        }

        private static void TryTranslate(DependencyObject element, DependencyProperty property)
        {
            if (element.IsLocalValue(property))
            {
                var value = element.GetValue(property) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Translate();
                    element.SetCurrentValue(property, value);
                }

                #region 无用的注释代码

                //添加属性变更再次翻译功能后，以下问题解决。
                ////目前，发现翻译失败的场景如下：
                ////当某页签中的元素的属性使用了绑定表达式，而且这个页签在第一次加载过程中，被弹出窗口阻止了被设置为当前选中的页签，
                ////而停止了绑定值的计算。这时，虽然发生了 Loaded 事件，但是由于没有计算绑定值，所以属性值为空，导致翻译无效。
                ////else
                ////{
                ////    //如果还没有值，并且已经使用了绑定。那么就等待绑定完成时，再计算该值。
                ////    var binding = BindingOperations.GetBinding(element, property);
                ////    if (binding != null && binding.NotifyOnTargetUpdated)
                ////    {
                ////        Binding.AddTargetUpdatedHandler(element, (o, e) =>
                ////        {
                ////            if (e.Property == property)
                ////            {
                ////                var valueBound = element.GetValue(property) as string;
                ////                if (!string.IsNullOrEmpty(valueBound))
                ////                {
                ////                    valueBound = valueBound.Translate();
                ////                    element.SetValue(property, valueBound);
                ////                }
                ////            }
                ////        });
                ////    }
                ////} 

                #endregion

                //属性变更时，也需要再次进行翻译。
                var descriptor = DependencyPropertyDescriptor.FromProperty(property, element.GetType());
                descriptor.AddValueChanged(element, (o, e) =>
                {
                    var valueChanged = element.GetValue(property) as string;
                    if (!string.IsNullOrEmpty(valueChanged))
                    {
                        valueChanged = valueChanged.Translate();
                        element.SetCurrentValue(property, valueChanged);
                    }
                });
            }
        }
    }
}