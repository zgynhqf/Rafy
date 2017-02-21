using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 实体映射表的元数据。
    /// </summary>
    public class TableMeta : Freezable
    {
        /// <summary>
        /// 构造一个映射指定表名的元数据对象
        /// </summary>
        /// <param name="tableName"></param>
        public TableMeta(string tableName)
        {
            this._TableName = tableName;
        }

        /// <summary>
        /// 构造一个映射视图的元数据对象
        /// </summary>
        public TableMeta() { }

        private string _TableName;
        /// <summary>
        /// 映射数据库中的表名。
        /// 如果此属性为 null，表示正在映射某个虚拟的视图。见：<see cref="IsMappingView"/>。
        /// </summary>
        public string TableName
        {
            get { return this._TableName; }
            set { this.SetValue(ref this._TableName, value); }
        }

        //private string _ViewSql;
        ///// <summary>
        ///// 如果是映射视图，则需要指定此属性为视图对应的 SQL。
        ///// 可以是一个 Sql 视图名，也可以是一个能查询出表格的 Sql 语句。
        ///// </summary>
        //public string ViewSql
        //{
        //    get { return this._ViewSql; }
        //    set { this.SetValue(ref this._ViewSql, value); }
        //}

        /// <summary>
        /// 是否正在映射某个虚拟的视图。
        /// 当映射视图时，不会生成数据库表，仓库中也需要在所有的查询中都编写自定义查询。
        /// </summary>
        public bool IsMappingView
        {
            get { return string.IsNullOrEmpty(_TableName); }
        }
    }
}
