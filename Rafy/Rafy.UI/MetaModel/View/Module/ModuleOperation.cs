/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 某个模块中定义的一些可用的功能操作。
    /// 
    /// 这些功能操作会被用来实现权限控制。
    /// </summary>
    public class ModuleOperation : Meta
    {
        /// <summary>
        /// Name 属性表示功能的名称，这个名称在模块中应该是唯一的，该值会被存储到数据层中。
        /// </summary>
        public override string Name
        {
            get { return base.Name ?? this._Label; }
            set { base.Name = value; }
        }

        private string _Label;
        /// <summary>
        /// 表示界面中功能用于显示的名称。
        /// </summary>
        public string Label
        {
            get { return this._Label; }
            set { this.SetValue(ref this._Label, value); }
        }
    }
}