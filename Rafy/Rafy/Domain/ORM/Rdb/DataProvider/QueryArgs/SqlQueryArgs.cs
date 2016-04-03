/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150316
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150316 16:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 使用 Sql 查询的参数。
    /// </summary>
    public class SqlQueryArgs : EntityQueryArgsBase, ISqlSelectArgs
    {
        private object[] _parameters;
        internal Type EntityType;

        /// <summary>
        /// 空构造函数，配合属性使用。
        /// </summary>
        public SqlQueryArgs() { }

        /// <summary>
        /// 通过一个 FormatSql 来构造。
        /// </summary>
        /// <param name="sql"></param>
        public SqlQueryArgs(FormattedSql sql) : this(sql, sql.Parameters) { }

        /// <summary>
        /// 通过标准跨库 Sql 及参数值来构造。
        /// </summary>
        /// <param name="formattedSql"></param>
        /// <param name="parameters"></param>
        private SqlQueryArgs(string formattedSql, params object[] parameters)
        {
            this.FormattedSql = formattedSql;
            this.Parameters = parameters;
        }

        /// <summary>
        /// 格式化参数的标准 SQL。
        /// </summary>
        public string FormattedSql { get; set; }

        /// <summary>
        /// FormatSql 对应的参数值。
        /// </summary>
        public object[] Parameters
        {
            get
            {
                //这个返回值不会是 null。
                if (this._parameters == null)
                {
                    this._parameters = new object[0];
                }
                return this._parameters;
            }
            set { this._parameters = value; }
        }

        #region ISelectArgs 成员

        Type ISqlSelectArgs.EntityType
        {
            get { return this.EntityType; }
        }

        #endregion
    }
}
