/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110421
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110421
 * 
*******************************************************/

using System.Windows;

namespace OEA.Module.WPF
{
    /// <summary>
    /// OEA 框架在皮肤中用到的所有约定样式
    /// </summary>
    public static class OEAStyles
    {
        public static Style GroupContainerStyle
        {
            get { return FindStyle("OEA_GroupContainerStyle"); }
        }

        public static Style CommandsContainer
        {
            get { return FindStyle("OEA_CommandsContainer"); }
        }

        public static Style TabControlHeaderHide
        {
            get { return FindStyle("OEA_TabControlHeaderHide"); }
        }

        public static Style StringPropertyEditor_TextBox
        {
            get { return FindStyle("OEA_StringPropertyEditor_TextBox"); }
        }

        public static Style TreeColumn_TextBlock
        {
            get { return FindStyle("OEA_TreeColumn_TextBlock"); }
        }

        public static Style TreeColumn_TextBlock_Number
        {
            get { return FindStyle("OEA_TreeColumn_TextBlock_Number"); }
        }

        private static Style FindStyle(string key)
        {
            var app = Application.Current;
            return app != null ? app.TryFindResource(key) as Style : null;
        }
    }
}
