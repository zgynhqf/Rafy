/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130524
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130524 18:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 数据表查询的参数
    /// </summary>
    public class TableQueryArgs : QueryArgs, ITableQueryArgs
    {
        private object[] _parameters;
        private PagingInfo _pagingInfo = PagingInfo.Empty;
        internal Type EntityType;
        private LiteDataTable _resultTable;

        /// <summary>
        /// 空构造函数，配合属性使用。
        /// </summary>
        public TableQueryArgs() { }

        /// <summary>
        /// 通过一个 ConditionalSql 来构造。
        /// </summary>
        /// <param name="sql"></param>
        public TableQueryArgs(ConditionalSql sql) : this(sql, sql.Parameters) { }

        /// <summary>
        /// 通过一个 FormattedSql 来构造。
        /// </summary>
        /// <param name="sql"></param>
        public TableQueryArgs(FormattedSql sql) : this(sql, sql.Parameters) { }

        /// <summary>
        /// 通过 FormattedSql 及分页信息来构造。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="pagingInfo">The paging information.</param>
        public TableQueryArgs(FormattedSql sql, PagingInfo pagingInfo)
            : this(sql, sql.Parameters)
        {
            this.PagingInfo = pagingInfo;
        }

        /// <summary>
        /// 通过标准跨库 Sql 及参数值来构造。
        /// </summary>
        /// <param name="formattedSql"></param>
        /// <param name="parameters"></param>
        public TableQueryArgs(string formattedSql, params object[] parameters)
        {
            this.FormattedSql = formattedSql;
            this.Parameters = parameters;
        }

        /// <summary>
        /// 获取数据的类型。
        /// 返回 <see cref="Rafy.Domain.RepositoryQueryType.Table" />
        /// </summary>
        public override RepositoryQueryType QueryType
        {
            get { return RepositoryQueryType.Table; }
        }

        /// <summary>
        /// 结果数据表
        /// </summary>
        public LiteDataTable ResultTable
        {
            get
            {
                if (_resultTable == null)
                {
                    _resultTable = new LiteDataTable();
                }
                return _resultTable;
            }
            set { _resultTable = value; }
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

        /// <summary>
        /// 要对结果进行分页的分页信息。
        /// 默认为 PagingInfo.Empty。
        /// </summary>
        public PagingInfo PagingInfo
        {
            get { return _pagingInfo; }
            set { _pagingInfo = value; }
        }

        #region ISelectArgs 成员

        Type ITableQueryArgs.EntityType
        {
            get { return this.EntityType; }
        }

        #endregion
    }
}
