using System;
using System.Data;
using System.Collections.Generic;
using hxy.Common.Data;
using OEA.ManagedProperty;

namespace OEA.ORM
{
    /// <summary>
    /// 数据库条件表达式。
    /// 
    /// 例如 Name Like '%xxx%'
    /// </summary>
    internal class DbConstraint : IConstraint
    {
        private IManagedProperty _property;

        private string _operator;

        private object _value;

        private DbTable _mainTable;

        public DbConstraint(IManagedProperty property, string op, object val)
        {
            this._property = property;
            this._operator = op;
            this._value = val;
        }

        public IManagedProperty Property
        {
            get { return _property; }
        }

        public string Operator
        {
            get { return _operator; }
        }

        public object Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        /// <summary>
        /// 本条件正用于查询的表（被查的主表）。
        /// </summary>
        /// <param name="mainTable"></param>
        internal void SetMainTable(DbTable mainTable)
        {
            this._mainTable = mainTable;
        }

        /// <summary>
        /// 此条件中托管属性对应的表
        /// </summary>
        public DbTable PropertyTable
        {
            get
            {
                //如果属性就是定义在请表下的，直接返回主表。
                if (this._property.OwnerType.IsAssignableFrom(this._mainTable.Class))
                {
                    return this._mainTable;
                }

                //如果不能使用主表，则直接使用托管属性来查找相应的表。
                return DbTableHost.TableFor(this._property.OwnerType);
            }
        }

        public virtual string GetSql(FormatSqlParameters paramaters)
        {
            var table = this.PropertyTable;
            string name = table.Translate(this._property);
            var offset = paramaters.AddParameter(this._value);
            return table.QuoteName + '.' + table.Quote(name) + this._operator + '{' + offset + '}';
        }
    }
}