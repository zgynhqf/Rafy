/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// 属性对应的列的元数据
    /// </summary>
    public class ColumnMeta : Freezable
    {
        private bool _IsPK;

        private bool? _IsRequired;

        private string _ColumnName;

        /// <summary>
        /// 是否 ID 主键
        /// </summary>
        public bool IsPKID
        {
            get { return this._IsPK; }
            set { this.SetValue(ref this._IsPK, value); }
        }

        /// <summary>
        /// 是否必须的，如果没有赋值，则按照默认的类型计算方法来计算该值。
        /// </summary>
        public bool? IsRequired
        {
            get { return this._IsRequired; }
            set { this.SetValue(ref this._IsRequired, value); }
        }

        /// <summary>
        /// 映射数据库中的字段名
        /// </summary>
        public string ColumnName
        {
            get { return this._ColumnName; }
            set { this.SetValue(ref this._ColumnName, value); }
        }
    }
}
