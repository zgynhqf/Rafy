/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System.Windows;

namespace OEA.Module.WPF.Editors
{
    public interface IWPFPropertyEditor : IPropertyEditor
    {
        new FrameworkElement Control { get; }

        new FrameworkElement LabelControl { get; }

        /// <summary>
        /// 重新给当前的编辑控件（DataGrid 在每次创建单元格时都需要执行此代码。原因不详。）
        /// </summary>
        void RebindEditingControl();

        /// <summary>
        /// 把生成的某个编辑控件准备好，马上开始编辑。
        /// </summary>
        /// <param name="editingElement"></param>
        /// <param name="editingEventArgs"></param>
        void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs);
    }
}