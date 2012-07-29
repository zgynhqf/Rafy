using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    public class TableMeta : Freezable
    {
        public TableMeta(string tableName)
        {
            this._TableName = tableName;
        }

        private string _TableName;
        /// <summary>
        /// 映射数据库中的字段名
        /// </summary>
        public string TableName
        {
            get { return this._TableName; }
        }

        private string _ViewSql;
        /// <summary>
        /// 如果是映射视图，则需要指定此属性为视图对应的 SQL
        /// </summary>
        public string ViewSql
        {
            get { return this._ViewSql; }
            set { this.SetValue(ref this._ViewSql, value); }
        }

        /// <summary>
        /// 是否存在映射视图
        /// </summary>
        public bool IsMappingView
        {
            get { return !string.IsNullOrEmpty(this._ViewSql); }
        }
    }
}
