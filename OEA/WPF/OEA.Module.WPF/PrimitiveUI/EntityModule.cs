/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120226
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 一个最简单的实现 IEntityWindow 接口的类。
    /// </summary>
    internal class EntityModule : ContentControl, IEntityWindow
    {
        private ControlResult _ui;

        public EntityModule(ControlResult ui, bool autoLoad = true)
        {
            this._ui = ui;

            this.Content = ui.Control;

            if (autoLoad) { this.AsyncLoadListData(ui); }
        }

        /// <summary>
        /// 对应的窗口主要的 view
        /// </summary>
        public ObjectView View
        {
            get { return this._ui.MainView; }
        }

        private void AsyncLoadListData(ControlResult ui)
        {
            //如果是个列表，并且没有导航面板，则默认开始查询数据
            var listView = ui.MainView as ListObjectView;
            if (listView != null &&
                listView.ConditionQueryView == null &&
                listView.NavigationQueryView == null
                )
            {
                listView.DataLoader.LoadDataAsync();
            }
        }
    }
}
