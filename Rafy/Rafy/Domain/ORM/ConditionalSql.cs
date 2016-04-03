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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rafy.Data;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// Sql 构造器
    /// 
    /// 调用本类对象的 Write 打头的一系统方法来构造条件，本类型会自动添加 Where、And 等连接符。
    /// 相关配置可以查看属性 AutoConcat 及 HasWhere。
    /// 
    /// 如果想直接面向 Sql 字符串进行操作，可以使用 Append 打头的方法，或者使用 InnerStringBuider 属性获取内部的 StringBuider 后再进行操作。
    /// </summary>
    public class ConditionalSql : FormattedSql
    {
        //#region 构造器

        //public ConditionalSql()  { }

        //public ConditionalSql(TextWriter sql) : base(sql) { }

        //#endregion

        #region 添加条件If

        /// <summary>
        /// 如果值不为空，则添加自定义 Where 语句 Sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        public void WriteWhereSqlIf(string sql, object parameter)
        {
            if (IsNotEmpty(parameter)) this.WriteWhereSql(sql, parameter);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性与某值相等的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteEqualIf(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteEqual(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性与某值不相等的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteNotEqualIf(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteNotEqual(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性比某值大的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteGreaterIf(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteGreater(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性 >= 某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteGreaterEqualIf(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteGreaterEqual(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性 <![CDATA[<]]> 某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteLessIf(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteLess(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性 <![CDATA[<=]]> 某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteLessEqualIf(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteLessEqual(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性包含某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteContainsIf(IManagedProperty property, string value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteContains(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性以某字符串值开头的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteStartWithIf(IManagedProperty property, string value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteStartWith(property, value, propertyOwner);
        }

        /// <summary>
        /// 如果值不为空，则添加某属性以某字符串值为模糊匹配的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">将会对该值进行是否为空的检测。值中需要带有通配符。如 '%'</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteLikeIf(IManagedProperty property, string value, Type propertyOwner = null)
        {
            if (IsNotEmpty(value)) this.WriteLike(property, value, propertyOwner);
        }

        /// <summary>
        /// 判断某个值是否非空。
        /// 
        /// 如果是字符串，则检测它是否为非空字符。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(object value)
        {
            return DomainHelper.IsNotEmpty(value);
        }

        #endregion

        #region 添加条件

        private Dictionary<Type, RdbTable> _tablesCache = new Dictionary<Type, RdbTable>();

        private bool _hasWhere = false;

        private SqlConditionAutoConcat _autoConcat = SqlConditionAutoConcat.And;

        /// <summary>
        /// 是否当前 SQL 中已经存在了 Where 关键字。
        /// 
        /// 如果没有存在 Where，则会在再次添加条件时，自动添加 Where 关键字。
        /// </summary>
        public bool HasWhere
        {
            get { return this._hasWhere; }
            set { this._hasWhere = value; }
        }

        /// <summary>
        /// 自动添加的连接符。
        /// 默认是 AND
        /// </summary>
        public SqlConditionAutoConcat AutoConcat
        {
            get { return this._autoConcat; }
            set { this._autoConcat = value; }
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
                    index = this.Parameters.Add(value);
                    return "{" + index + "}";
                });
            }

            this.Append(sql);

            //写完一个列，说明本次条件完毕。
            this._hasWhere = true;
        }

        /// <summary>
        /// 添加某属性与某值相等的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteEqual(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (value == null || value == DBNull.Value)
            {
                WriteIsNull(property, true, propertyOwner);
            }
            else
            {
                this.WriteConstrain(property, "=", value, propertyOwner);
            }
        }

        /// <summary>
        /// 添加某属性与某值不相等的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteNotEqual(IManagedProperty property, object value, Type propertyOwner = null)
        {
            if (value == null || value == DBNull.Value)
            {
                WriteIsNull(property, false, propertyOwner);
            }
            else
            {
                this.WriteConstrain(property, "!=", value, propertyOwner);
            }
        }

        /// <summary>
        /// 添加某属性比某值大的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteGreater(IManagedProperty property, object value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, ">", value, propertyOwner);
        }

        /// <summary>
        /// 添加某属性 >= 某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteGreaterEqual(IManagedProperty property, object value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, ">=", value, propertyOwner);
        }

        /// <summary>
        /// 添加某属性 <![CDATA[<]]> 某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteLess(IManagedProperty property, object value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, "<", value, propertyOwner);
        }

        /// <summary>
        /// 添加某属性 <![CDATA[<=]]> 某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteLessEqual(IManagedProperty property, object value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, "<=", value, propertyOwner);
        }

        /// <summary>
        /// 添加某属性包含某值的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteContains(IManagedProperty property, string value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, "LIKE", SqlGenerator.WILDCARD_ALL + value + SqlGenerator.WILDCARD_ALL, propertyOwner);
        }

        /// <summary>
        /// 添加某属性以某字符串值开头的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteStartWith(IManagedProperty property, string value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, "LIKE", value + SqlGenerator.WILDCARD_ALL, propertyOwner);
        }

        /// <summary>
        /// 添加某属性以某字符串值为模糊匹配的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="value">作为比较的值。值中需要带有通配符。如 '%'</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteLike(IManagedProperty property, string value, Type propertyOwner = null)
        {
            this.WriteConstrain(property, "LIKE", value, propertyOwner);
        }

        /// <summary>
        /// 添加某属性在某个序列中的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="values">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteIn(IManagedProperty property, IList values, Type propertyOwner = null)
        {
            this.WriteInCore(property, values, true, propertyOwner);
        }

        /// <summary>
        /// 添加某属性不在某个序列中的条件。
        /// </summary>
        /// <param name="property">作为条件判断的属性。</param>
        /// <param name="values">作为比较的值。</param>
        /// <param name="propertyOwner">
        /// 如果该属性的声明者没有映射具体的表（例如该属性为抽象基类的属性），则需要指定 propertyOwner 为具体的子类。
        /// </param>
        public void WriteNotIn(IManagedProperty property, IList values, Type propertyOwner = null)
        {
            this.WriteInCore(property, values, false, propertyOwner);
        }

        private void WriteInCore(IManagedProperty property, IList values, bool isIn, Type propertyOwner)
        {
            if (values == null) throw new ArgumentNullException("value");

            this.WriteQuoteColumn(property, propertyOwner);

            if (!isIn) { this.Append("NOT "); }
            this.Append("IN (");

            if (values.Count > 0)
            {
                var first = true;
                foreach (var value in values)
                {
                    if (!first) this.Append(',');
                    this.AppendParameter(value);
                    first = false;
                }
            }
            else
            {
                this.Append("NULL");
            }

            this.Append(")");
        }

        private void WriteConstrain(IManagedProperty property, string op, object value, Type propertyOwner)
        {
            this.WriteQuoteColumn(property, propertyOwner);
            this.Append(op);
            this.Append(" {");
            var offset = this.Parameters.Add(value);
            this.Append(offset);
            this.Append('}');
        }

        private void WriteIsNull(IManagedProperty property, bool isNull, Type propertyOwner)
        {
            this.WriteQuoteColumn(property, propertyOwner);
            this.Append("IS ");
            if (!isNull) { this.Append("NOT "); }
            this.Append("NULL");
        }

        private void WriteQuoteColumn(IManagedProperty property, Type propertyOwner)
        {
            this.BeginConstrain();

            var table = this.GetDbTable(property, propertyOwner);
            var columnName = table.Translate(property);

            this.AppendQuoteName(table).Append('.').AppendQuote(table, columnName).Append(' ');

            //写完一个列，说明本次条件完毕。
            this._hasWhere = true;
        }

        /// <summary>
        /// 找到指定托管属性对应的 ORM 表元数据。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="propertyOwner">The property owner.</param>
        /// <returns></returns>
        private RdbTable GetDbTable(IManagedProperty property, Type propertyOwner)
        {
            return GetPropertyTable(property, propertyOwner, this._tablesCache);
        }

        private void BeginConstrain()
        {
            //如果之前已经添加了别的条件，则应该添加一个连接符。
            if (this._hasWhere)
            {
                switch (this._autoConcat)
                {
                    case SqlConditionAutoConcat.And:
                        this.AppendAnd();
                        break;
                    case SqlConditionAutoConcat.Or:
                        this.AppendOr();
                        break;
                    case SqlConditionAutoConcat.None:
                    default:
                        break;
                }
            }
            else
            {
                //如果当前还没有任何条件，说明现在是第一个条件，直接添加 “WHERE”。
                this.AppendLine().AppendLine("WHERE");
            }
        }

        #endregion

        #region 操作符重载、方便使用的方法

        public static ConditionalSql operator +(ConditionalSql writer, string value)
        {
            writer.Append(value);
            return writer;
        }

        public static implicit operator ConditionalSql(string value)
        {
            var writer = new ConditionalSql();
            writer.Append(value);
            return writer;
        }

        #endregion

        /// <summary>
        /// 通过托管属性及其对应的实体类型，并通过可用的缓存列表，查找对应的 DbTable。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyOwner"></param>
        /// <param name="tablesCache"></param>
        /// <returns></returns>
        internal static RdbTable GetPropertyTable(IManagedProperty property, Type propertyOwner, Dictionary<Type, RdbTable> tablesCache)
        {
            RdbTable result = null;

            if (propertyOwner == null) propertyOwner = property.OwnerType;

            if (!tablesCache.TryGetValue(propertyOwner, out result))
            {
                if (!propertyOwner.IsAbstract)
                {
                    result = RdbTableFinder.TableFor(propertyOwner);
                }

                if (result == null)
                {
                    ORMHelper.ThrowBasePropertyNotMappedException(property.Name, propertyOwner);
                }

                tablesCache.Add(propertyOwner, result);
            }

            return result;
        }

    }

    /// <summary>
    /// 自动添加的连接符。
    /// 默认是 AND
    /// </summary>
    public enum SqlConditionAutoConcat
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