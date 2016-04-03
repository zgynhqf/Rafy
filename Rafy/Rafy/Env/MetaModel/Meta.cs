using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 元数据
    /// </summary>
    [DebuggerDisplay("Name:{Name}")]
    public class Meta : MetaBase
    {
        private string _Name;

        /// <summary>
        /// 名字
        /// </summary>
        public virtual string Name
        {
            get { return this._Name; }
            set { this.SetValue(ref this._Name, value); }
        }
    }

    /// <summary>
    /// 视图元数据
    /// </summary>
    [DebuggerDisplay("Name:{Name} Label:{Label}")]
    public class ViewMeta : Meta
    {
        private string _label;
        /// <summary>
        /// 显示的标题
        /// </summary>
        public virtual string Label
        {
            get { return this._label; }
            set { this.SetValue(ref this._label, value); }
        }

        private bool _IsVisible = true;
        /// <summary>
        /// 指示这个界面元数据，当前是否可见。
        /// 可以简单地设置这个属性为 false，来达到不生成这个界面元素的功能。
        /// </summary>
        public virtual bool IsVisible
        {
            get { return this._IsVisible; }
            set { this.SetValue(ref this._IsVisible, value); }
        }
    }
}