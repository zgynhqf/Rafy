/*******************************************************
 * 
 * 作者：http://avalondock.codeplex.com/
 * 创建日期：20130521
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 codeplex 20130521 17:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Rafy.WPF.Controls
{
    public class Resizer : Thumb
    {
        static Resizer()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Resizer), new FrameworkPropertyMetadata(typeof(Resizer)));

            MinWidthProperty.OverrideMetadata(typeof(Resizer), new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
            MinHeightProperty.OverrideMetadata(typeof(Resizer), new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(Resizer), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
            VerticalAlignmentProperty.OverrideMetadata(typeof(Resizer), new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        }
    }
}