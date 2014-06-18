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
using System;

namespace Rafy.WPF
{
    /// <summary>
    /// Rafy 框架在皮肤中用到的所有约定样式
    /// </summary>
    public static class RafyResources
    {
        #region 内置的样式

        public static Style GroupContainerStyle
        {
            get { return FindStyle("Rafy_GroupContainerStyle"); }
        }

        public static Style CommandsContainer
        {
            get { return FindStyle("Rafy_CommandsContainer"); }
        }

        public static Style TabControlHeaderHide
        {
            get { return FindStyle("Rafy_TabControlHeaderHide"); }
        }

        public static Style StringPropertyEditor_TextBox
        {
            get { return FindStyle("Rafy_StringPropertyEditor_TextBox"); }
        }

        public static Style TreeColumn_TextBlock
        {
            get { return FindStyle("Rafy_TreeColumn_TextBlock"); }
        }

        public static Style TreeColumn_TextBlock_Number
        {
            get { return FindStyle("Rafy_TreeColumn_TextBlock_Number"); }
        }

        public static DataTemplate Rafy_MTTG_SelectionColumnTemplate
        {
            get { return FindResource("Rafy_MTTG_SelectionColumnTemplate") as DataTemplate; }
        }

        #endregion

        /// <summary>
        /// 查找某个键对应的样式。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Style FindStyle(object key)
        {
            return FindResource(key) as Style;
        }

        /// <summary>
        /// 查找某个键对应的对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object FindResource(object key)
        {
            var app = Application.Current;
            return app != null ? app.TryFindResource(key) : null;
        }

        /// <summary>
        /// 把指定的 Resouce 加入到应用程序中
        /// 
        /// 使用方法：
        /// RafyResources.AddResource(typeof(WPFModule), "Resources/ThemeBasic.xaml");
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="path"></param>
        public static void AddResource(Type assemblyType, params string[] pathes)
        {
            var app = Application.Current;
            if (app != null)
            {
                foreach (var path in pathes)
                {
                    var uri = Helper.GetPackUri(assemblyType.Assembly, path);
                    var resouceDic = Application.LoadComponent(uri) as ResourceDictionary;
                    app.Resources.MergedDictionaries.Add(resouceDic);
                }
            }
        }

        /// <summary>
        /// 把指定的Resouce加入到应用程序中
        /// 
        /// 使用方法：
        /// RafyResources.AddResource("Rafy.WPF;component/Resources/ComboListControl.xaml");
        /// </summary>
        /// <param name="packUri"></param>
        public static void AddResource(string packUri)
        {
            var app = Application.Current;
            if (app != null)
            {
                var uri = new Uri(packUri, UriKind.RelativeOrAbsolute);
                var resouceDic = Application.LoadComponent(uri) as ResourceDictionary;
                app.Resources.MergedDictionaries.Add(resouceDic);
            }
        }
    }
}