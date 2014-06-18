/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using System.Data;
using System.Diagnostics;

namespace Rafy.DbMigration.Model
{
    [DebuggerDisplay("Name: {Name}")]
    public class Column : Extendable
    {
        private bool _isRequired;

        private bool _isPrimaryKey;

        private ForeignConstraint _foreignConstraint;

        public Column(DbType dataType, string length, string name, Table table)
        {
            if (table == null) throw new ArgumentNullException("DataTable");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            this.DataType = dataType;
            this.Length = length;
            this.Name = name;
            this.Table = table;
        }

        /// <summary>
        /// 所属表
        /// </summary>
        public Table Table { get; private set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DbType DataType { get; private set; }

        /// <summary>
        /// 可指定列的长度
        /// 可以指定数字，或者 MAX。
        /// 如果是空，则使用默认长度。
        /// </summary>
        public string Length { get; private set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 是否必须的
        /// </summary>
        public bool IsRequired
        {
            get { return this._isRequired; }
            set
            {
                this._isRequired = value;

                //如果不是必须的，则肯定不是主键
                if (!value) { this.IsPrimaryKey = false; }
            }
        }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return this._isPrimaryKey; }
            set
            {
                this._isPrimaryKey = value;

                //是主键，则肯定是必须的
                if (value)
                {
                    this.IsRequired = true;
                }
            }
        }

        /// <summary>
        /// 获取是否外键
        /// </summary>
        public bool IsForeignKey
        {
            get { return this.ForeignConstraint != null; }
        }

        /// <summary>
        /// 表示这个主键列是否为自增列。
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 如果是外键，这表示外键表
        /// </summary>
        public ForeignConstraint ForeignConstraint
        {
            get { return this._foreignConstraint; }
            set
            {
                this._foreignConstraint = value;

                if (value != null) { value.Init(this); }
            }
        }
    }
}