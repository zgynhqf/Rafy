using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace OEA.MetaModel
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
            set { this.SetValue(ref this._Name, value, "Name"); }
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
            set { this.SetValue(ref this._label, value, "Label"); }
        }

        private bool _IsVisible = true;
        public virtual bool IsVisible
        {
            get { return this._IsVisible; }
            set { this.SetValue(ref this._IsVisible, value); }
        }
    }
}