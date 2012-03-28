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
using OEA.Library;
using OEA.MetaModel.View;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 用户自定义界面时，需要继承这个
    /// </summary>
    public abstract class CustomObjectView : WPFObjectView
    {
        public CustomObjectView(EntityViewMeta meta) : base(meta) { }

        public override Entity Current
        {
            get { return null; }
            set { }
        }

        public override void RefreshCurrentEntity() { }
    }
}