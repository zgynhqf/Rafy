/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130903
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130903 17:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.View
{
    public class WebEntityPropertyViewMeta : EntityPropertyViewMeta
    {
        private int? _WidthFlex;
        /// <summary>
        /// 用于初始化表格控件的宽度属性
        /// </summary>
        public int? WidthFlex
        {
            get { return this._WidthFlex; }
            set { this.SetValue(ref this._WidthFlex, value); }
        }

        private bool _IsReadonly;
        /// <summary>
        /// Web 专用
        /// </summary>
        public bool IsReadonly
        {
            get { return this._IsReadonly; }
            set { this.SetValue(ref this._IsReadonly, value); }
        }
    }
}
