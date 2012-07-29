/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 所有WPF逻辑视图都继承此类
    /// </summary>
    public abstract class WPFObjectView : ObjectView
    {
        private ItemsControl _commandsContainer;

        public WPFObjectView(EntityViewMeta meta)
            : base(meta)
        {
            base.DataLoader = new ViewDataLoader(this);
            this.UICommands = new List<UICommand>();
        }

        public new FrameworkElement Control
        {
            get { return base.Control as FrameworkElement; }
        }

        internal new ViewDataLoader DataLoader
        {
            get { return base.DataLoader as ViewDataLoader; }
        }

        /// <summary>
        /// AutoUI 最终布局完成的控件。
        /// 在这个布局控件中，本 View 是它的 MainView。
        /// </summary>
        public FrameworkElement LayoutControl { get; internal set; }

        /// <summary>
        /// 工具栏
        /// </summary>
        public ItemsControl CommandsContainer
        {
            get { return this._commandsContainer; }
            set
            {
                if (value == null) throw new ArgumentNullException("commandsContainer");
                if (this._commandsContainer != null) throw new InvalidOperationException("只能设置一次！");

                this._commandsContainer = value;

                value.SetServicedControl(this.Control);
            }
        }

        /// <summary>
        /// 本 View 中所有可用的 UICommand。
        /// 这里只是把它们缓存起来，为了在界面中统一加入快捷键
        /// </summary>
        internal List<UICommand> UICommands { get; private set; }
    }
}