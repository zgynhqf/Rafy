/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.Domain;
using Rafy.MetaModel.View;

namespace Rafy.WPF
{
    /// <summary>
    /// 用户自定义界面时，需要继承这个类型。
    /// 
    /// <remarks>
    /// 本类型在基类的基础上，默认不支持 Current 属性，同时开放了一些接口。
    /// </remarks>
    /// </summary>
    public abstract class CustomLogicalView : LogicalView
    {
        public CustomLogicalView(WPFEntityViewMeta meta) : base(meta) { }

        /// <summary>
        /// 子类调用此方法来设置当前视图关联的控件。
        /// </summary>
        /// <returns></returns>
        protected new void SetControl(FrameworkElement control)
        {
            base.SetControl(control);
        }

        /// <summary>
        /// 默认不支持 Current 属性。
        /// </summary>
        public override Entity Current
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 默认不支持此操作。
        /// </summary>
        protected override void RefreshCurrentEntityCore() { }
    }
}