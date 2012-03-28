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
    internal class EntityModule : ContentControl, IEntityWindow, IWorkspaceWindow
    {
        private ControlResult _control;

        public EntityModule(ControlResult control, string moduleTitle)
        {
            this._control = control;
            this.Content = control.Control;
            this.Title = moduleTitle;
        }

        public ObjectView View
        {
            get { return this._control.MainView; }
        }

        /// <summary>
        /// 直接取根对象的名字作为Title
        /// </summary>
        public string Title { get; private set; }
        //get { return this._control.MainView.EntityViewInfo.Label; }
    }
}
