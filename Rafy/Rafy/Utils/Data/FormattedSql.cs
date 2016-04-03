/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130928
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130928 18:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rafy.Data
{
    /// <summary>
    /// 格式化 Sql 构造器
    /// 
    /// 如果想直接面向 Sql 字符串进行操作，可以使用 Append 打头的方法，或者使用 InnerWriter 属性获取内部的 TextWriter 后再进行操作。
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class FormattedSql
    {
        /// <summary>
        /// 最终生成的 Sql 字符串的 TextWriter
        /// </summary>
        private TextWriter _sql;

        /// <summary>
        /// 内部使用的 TextWriter，可能被外部使用属性 InnerWriter 进行替换。
        /// </summary>
        private TextWriter _writer;

        #region 构造器

        public FormattedSql()
        {
            _sql = new StringWriter();
            _writer = _sql;
        }

        //public FormattedSql(TextWriter sql)
        //{
        //    this._sql = sql;
        //}

        #endregion

        #region 参数

        private FormattedSqlParameters _parameters = new FormattedSqlParameters();

        /// <summary>
        /// 当前可用的参数
        /// </summary>
        public FormattedSqlParameters Parameters
        {
            get { return this._parameters; }
        }

        /// <summary>
        /// 写入一个参数值。
        /// </summary>
        /// <param name="value"></param>
        public FormattedSql AppendParameter(object value)
        {
            this._parameters.WriteParameter(this._writer, value);
            return this;
        }

        #endregion

        #region 直接添加原始 SQL 语句的方法

        /// <summary>
        /// 获取内部的 TextWriter，用于直接面向字符串进行文本输出。
        /// 同时，也可以使用新的 TextWriter 来装饰当前的 TextWriter。
        /// </summary>
        public TextWriter InnerWriter
        {
            get { return _writer; }
            set { _writer = value; }
        }

        /// <summary>
        /// 直接添加 " AND "。
        /// </summary>
        /// <returns></returns>
        public FormattedSql AppendAnd()
        {
            this._writer.Write(" AND ");
            return this;
        }

        /// <summary>
        /// 直接添加 " OR "。
        /// </summary>
        /// <returns></returns>
        public FormattedSql AppendOr()
        {
            this._writer.Write(" OR ");
            return this;
        }

        /// <summary>
        /// 直接添加指定的字符串。
        /// </summary>
        /// <returns></returns>
        public FormattedSql Append(string value)
        {
            this._writer.Write(value);
            return this;
        }

        /// <summary>
        /// 直接添加指定的 char 值。
        /// </summary>
        /// <returns></returns>
        public FormattedSql Append(char value)
        {
            this._writer.Write(value);
            return this;
        }

        /// <summary>
        /// 直接添加指定的 int 值。
        /// </summary>
        /// <returns></returns>
        public FormattedSql Append(int value)
        {
            this._writer.Write(value);
            return this;
        }

        /// <summary>
        /// 直接添加指定的 double 值。
        /// </summary>
        /// <returns></returns>
        public FormattedSql Append(double value)
        {
            this._writer.Write(value);
            return this;
        }

        /// <summary>
        /// 直接添加指定的 object 值。
        /// </summary>
        /// <returns></returns>
        public FormattedSql Append(object value)
        {
            this._writer.Write(value);
            return this;
        }

        /// <summary>
        /// 直接添加指定的 bool 值。
        /// </summary>
        /// <returns></returns>
        public FormattedSql Append(bool value)
        {
            this._writer.Write(value);
            return this;
        }

        /// <summary>
        /// 直接添加指定的字符串值，并添加回车。
        /// </summary>
        /// <returns></returns>
        public FormattedSql AppendLine(string value)
        {
            this._writer.WriteLine(value);
            return this;
        }

        /// <summary>
        /// 添加回车
        /// </summary>
        /// <returns></returns>
        public FormattedSql AppendLine()
        {
            this._writer.WriteLine();
            return this;
        }

        #endregion

        #region 操作符重载、方便使用的方法

        public static FormattedSql operator +(FormattedSql writer, string value)
        {
            writer._writer.Write(value);
            return writer;
        }

        public static implicit operator string(FormattedSql value)
        {
            return value._sql.ToString();
        }

        public static implicit operator FormattedSql(string value)
        {
            var writer = new FormattedSql();
            writer._writer.Write(value);
            return writer;
        }

        public override string ToString()
        {
            return _sql.ToString();
        }

        private string DebuggerDisplay
        {
            get
            {
                return _sql.ToString() + @"

Parameters: " + this._parameters.ToString();
            }
        }

        #endregion
    }
}
