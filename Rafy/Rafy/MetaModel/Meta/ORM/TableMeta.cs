using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Utils;

namespace Rafy.MetaModel
{
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
        /// 映射数据库中的表名
        /// </summary>
        public string TableName
        {
            get { return this._TableName; }
        }

        private string _ViewSql;
        /// <summary>
        /// 如果是映射视图，则需要指定此属性为视图对应的 SQL。
        /// 可以是一个 Sql 视图名，也可以是一个能查询出表格的 Sql 语句。
        /// </summary>
        public string ViewSql
        {
            get { return this._ViewSql; }
            set
            {
                this.SetValue(ref this._ViewSql, value);
            }
        }

        /// <summary>
        /// 是否存在映射视图
        /// </summary>
        public bool IsMappingView
        {
            get { return string.IsNullOrEmpty(_TableName); }
        }
    }
}
