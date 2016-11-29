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
using System.Data;
using System.Linq;
using System.Text;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 属性对应的列的元数据
    /// </summary>
    public class ColumnMeta : Freezable
    {
        private bool _HasFKConstraint;
        /// <summary>
        /// 如果这是一个引用属性的列，则这个属性表示数据库中是否有对应的外键存在（引用属性也可以不映射外键）。
        /// </summary>
        public bool HasFKConstraint
        {
            get { return this._HasFKConstraint; }
            set { this.SetValue(ref this._HasFKConstraint, value); }
        }

        private bool _IsIdentity;
        /// <summary>
        /// 是否自增长列
        /// </summary>
        public bool IsIdentity
        {
            get { return this._IsIdentity; }
            set { this.SetValue(ref this._IsIdentity, value); }
        }

        private bool _IsPrimaryKey;
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return this._IsPrimaryKey; }
            set
            {
                this.SetValue(ref this._IsPrimaryKey, value);

                //是主键，则肯定是必须的
                if (value)
                {
                    this.IsRequired = true;
                }
            }
        }

        private bool? _IsRequired;
        /// <summary>
        /// 是否必须的，如果没有赋值，则按照默认的类型计算方法来计算该值。
        /// </summary>
        public bool? IsRequired
        {
            get { return this._IsRequired; }
            set
            {
                this.SetValue(ref this._IsRequired, value);

                //如果不是必须的，则肯定不是主键
                if (value.HasValue && !value.Value)
                {
                    this.IsPrimaryKey = false;
                }
            }
        }

        private string _ColumnName;
        /// <summary>
        /// 映射数据库中的字段名
        /// </summary>
        public string ColumnName
        {
            get { return this._ColumnName; }
            set { this.SetValue(ref this._ColumnName, value); }
        }

        private DbType? _DataType;
        /// <summary>
        /// 映射数据库中的字段的类型。
        /// 如果没有设置，则使用默认的映射规则。
        /// </summary>
        public DbType? DataType
        {
            get { return this._DataType; }
            set { this.SetValue(ref this._DataType, value); }
        }

        private string _DataTypeLength;
        /// <summary>
        /// 映射数据库中的字段的长度、精度等信息。
        /// 可以是数字，也可以是 MAX 等字符串。
        /// 如果是空，则表示使用默认的长度。
        /// </summary>
        public string DataTypeLength
        {
            get { return this._DataTypeLength; }
            set { this.SetValue(ref this._DataTypeLength, value); }
        }
    }
}
