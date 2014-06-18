/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：20130330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130330
 * 
*******************************************************/

using System.Windows;
using System.Windows.Controls;

namespace DesignerEngine
{
    public class Toolbox : ItemsControl
    {
        static Toolbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Toolbox), new FrameworkPropertyMetadata(typeof(Toolbox)));
        }

        // Defines the ItemHeight and ItemWidth properties of
        // the WrapPanel used for this Toolbox
        public Size ItemSize
        {
            get { return itemSize; }
            set { itemSize = value; }
        }
        private Size itemSize = new Size(50, 50);

        // Creates or identifies the element that is used to display the given item.        
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolboxItem();
        }

        // Determines if the specified item is (or is eligible to be) its own container.        
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is ToolboxItem);
        }
    }
}