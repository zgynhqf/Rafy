/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120719 15:42
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120719 15:42
 * 
*******************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OEA.ManagedProperty;

namespace OEA.ORM
{
    /// <summary>
    /// Sql 构造器
    /// </summary>
    public class SqlWriter : StringWriter
    {
        #region 构造器

        public SqlWriter() { }

        public SqlWriter(StringBuilder sb) : base(sb) { }

        #endregion

        #region 条件

        private Dictionary<Type, DbTable> _tables = new Dictionary<Type, DbTable>();

        private bool _hasWhere = false;

        /// <summary>
        /// 是否当前 SQL 中已经存在了 Where 关键字。
        /// 
        /// 如果没有存在 Where，则会在再次添加条件时，自动添加 Where 关键字。
        /// </summary>
        public bool HasWhere
        {
            get { return _hasWhere; }
            set { _hasWhere = value; }
        }

        private SqlStringAutoConcat _autoConcat = SqlStringAutoConcat.And;

        /// <summary>
        /// 自动添加的连接符。
        /// 默认是 AND
        /// </summary>
        public SqlStringAutoConcat AutoConcat
        {
            get { return this._autoConcat; }
            set { this._autoConcat = value; }
        }

        public void AddTables(params Type[] entityTypes)
        {
            foreach (var type in entityTypes)
            {
                var table = DbTableHost.TableFor(type);
                this._tables[type] = table;
            }
        }

        public SqlWriter WriteAnd()
        {
            this.Write(" AND ");
            return this;
        }

        public SqlWriter WriteOr()
        {
            this.Write(" OR ");
            return this;
        }

        public void WriteEqual(IManagedProperty property, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                WriteIsNull(property, true);
            }
            else
            {
                this.WriteConstrain(property, "=", value);
            }
        }

        public void WriteNotEqual(IManagedProperty property, object value)
        {
            if (value == null || value == DBNull.Value)
            {
                WriteIsNull(property, false);
            }
            else
            {
                this.WriteConstrain(property, "!=", value);
            }
        }

        public void WriteGreater(IManagedProperty property, object value)
        {
            this.WriteConstrain(property, ">", value);
        }

        public void WriteGreaterEqual(IManagedProperty property, object value)
        {
            this.WriteConstrain(property, ">=", value);
        }

        public void WriteLess(IManagedProperty property, object value)
        {
            this.WriteConstrain(property, "<", value);
        }

        public void WriteLessEqual(IManagedProperty property, object value)
        {
            this.WriteConstrain(property, "<=", value);
        }

        public void WriteContains(IManagedProperty property, string value)
        {
            this.WriteConstrain(property, "like", '%' + value + '%');
        }

        public void WriteStartWith(IManagedProperty property, string value)
        {
            this.WriteConstrain(property, "like", value + '%');
        }

        public void WriteLike(IManagedProperty property, string value)
        {
            this.WriteConstrain(property, "like", value);
        }

        public void WriteIn(IManagedProperty property, IList values)
        {
            this.WriteInCore(property, values, true);
        }

        public void WriteNotIn(IManagedProperty property, IList values)
        {
            this.WriteInCore(property, values, false);
        }

        public void WriteQuoteColumn(IManagedProperty property)
        {
            this.BeginConstrain();

            var table = this.FindDbTable(property);
            var columnName = table.Translate(property);

            this.Write(table.QuoteName);
            this.Write('.');
            this.Write(table.Quote(columnName));
            this.Write(' ');

            //写完一个列，说明本次条件完毕。
            this._hasWhere = true;
        }

        /// <summary>
        /// 添加自定义 Where 语句 Sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void WriteWhereSql(string sql, params object[] parameters)
        {
            this.BeginConstrain();

            if (parameters != null && parameters.Length > 0)
            {
                sql = Regex.Replace(sql, @"\{(?<index>\d+)\}", m =>
                {
                    var index = Convert.ToInt32(m.Groups["index"].Value);
                    var value = parameters[index];
                    index = this._parameters.AddParameter(value);
                    return "{" + index + "}";
                });
            }

            this.Write(sql);

            //写完一个列，说明本次条件完毕。
            this._hasWhere = true;
        }

        private void WriteInCore(IManagedProperty property, IList values, bool isIn)
        {
            if (values == null) throw new ArgumentNullException("value");

            this.WriteQuoteColumn(property);

            if (!isIn) { this.Write("NOT "); }
            this.Write("IN (");

            if (values.Count > 0)
            {
                var first = true;
                foreach (var value in values)
                {
                    if (!first) this.Write(',');
                    this.WriteParameter(value);
                    first = false;
                }
            }
            else
            {
                this.Write("NULL");
            }

            this.Write(")");
        }

        private void WriteConstrain(IManagedProperty property, string op, object value)
        {
            this.WriteQuoteColumn(property);
            this.Write(op);
            this.Write(" {");
            var offset = this._parameters.AddParameter(value);
            this.Write(offset);
            this.Write('}');
        }

        private void WriteIsNull(IManagedProperty property, bool isNull)
        {
            this.WriteQuoteColumn(property);
            this.Write("IS ");
            if (!isNull) { this.Write("NOT "); }
            this.Write("NULL");
        }

        private void BeginConstrain()
        {
            if (this._hasWhere)
            {
                switch (this._autoConcat)
                {
                    case SqlStringAutoConcat.And:
                        this.WriteAnd();
                        break;
                    case SqlStringAutoConcat.Or:
                        this.WriteOr();
                        break;
                    case SqlStringAutoConcat.None:
                    default:
                        break;
                }
            }
            else
            {
                this.WriteLine();
                this.WriteLine("WHERE");
            }
        }

        private DbTable FindDbTable(IManagedProperty property)
        {
            DbTable result = null;

            var ownerType = property.OwnerType;
            if (!this._tables.TryGetValue(ownerType, out result))
            {
                foreach (var kv in this._tables)
                {
                    var type = kv.Key;
                    if (ownerType.IsAssignableFrom(type))
                    {
                        result = kv.Value;
                        break;
                    }
                }

                if (result == null)
                {
                    result = DbTableHost.TableFor(ownerType);
                    this._tables.Add(ownerType, result);
                }
            }

            return result;
        }

        #endregion

        #region 参数

        private FormatSqlParameters _parameters = new FormatSqlParameters();

        /// <summary>
        /// 当前可用的参数
        /// </summary>
        public FormatSqlParameters Parameters
        {
            get { return this._parameters; }
        }

        /// <summary>
        /// 写入一个参数值。
        /// </summary>
        /// <param name="value"></param>
        public void WriteParameter(object value)
        {
            this._parameters.AddParameter(this, value);
        }

        #endregion

        /// <summary>
        /// 判断某个值是否非空。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsNotNull(object value)
        {
            bool notNull = false;

            if (value is string)
            {
                notNull = !string.IsNullOrEmpty(value as string);
            }
            else if (value != null)
            {
                notNull = true;
            }

            return notNull;
        }

        #region 方便使用的方法、操作符重载等

        public static SqlWriter operator +(SqlWriter a, string b)
        {
            a.Write(b);
            return a;
        }

        public static implicit operator string(SqlWriter value)
        {
            return value.ToString();
        }

        public static implicit operator SqlWriter(string value)
        {
            var writer = new SqlWriter();
            writer.Write(value);
            return writer;
        }

        public static implicit operator SqlWriter(StringBuilder value)
        {
            return new SqlWriter(value);
        }

        public SqlWriter Append(string value)
        {
            this.Write(value);
            return this;
        }

        public SqlWriter Append(char value)
        {
            this.Write(value);
            return this;
        }

        public SqlWriter Append(int value)
        {
            this.Write(value);
            return this;
        }

        public SqlWriter Append(double value)
        {
            this.Write(value);
            return this;
        }

        public SqlWriter Append(object value)
        {
            this.Write(value);
            return this;
        }

        public SqlWriter Append(bool value)
        {
            this.Write(value);
            return this;
        }

        /// <summary>
        /// 写入一个参数值。
        /// </summary>
        /// <param name="value"></param>
        public SqlWriter AppendParameter(object value)
        {
            this.WriteParameter(value);
            return this;
        }

        #endregion
    }

    public static class SqlWriterExtension
    {
        public static void WriteEqualIf(this SqlWriter sql, IManagedProperty property, object value)
        {
            if (SqlWriter.IsNotNull(value)) sql.WriteEqual(property, value);
        }

        public static void WriteGreaterEqualIf(this SqlWriter sql, IManagedProperty property, object value)
        {
            if (SqlWriter.IsNotNull(value)) sql.WriteGreaterEqual(property, value);
        }

        public static void WriteLessEqualIf(this SqlWriter sql, IManagedProperty property, object value)
        {
            if (SqlWriter.IsNotNull(value)) sql.WriteLessEqual(property, value);
        }

        public static void WriteContainsIf(this SqlWriter sql, IManagedProperty property, string value)
        {
            if (SqlWriter.IsNotNull(value)) sql.WriteContains(property, value);
        }
    }

    /// <summary>
    /// 自动添加的连接符。
    /// 默认是 AND
    /// </summary>
    public enum SqlStringAutoConcat
    {
        /// <summary>
        /// 自动用 And 连接两个 Where 语句
        /// </summary>
        And,
        /// <summary>
        /// 自动用 Or 连接两个 Where 语句
        /// </summary>
        Or,
        /// <summary>
        /// 关闭自动连接
        /// </summary>
        None
    }
}