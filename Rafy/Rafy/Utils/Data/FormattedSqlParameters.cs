/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130928
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130928 18:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// FormattedSql 中的参数列表封装
    /// </summary>
    public class FormattedSqlParameters
    {
        private List<object> _parameters = new List<object>();

        /// <summary>
        /// 添加一个参数，并返回该参数应该使用的索引号
        /// 
        /// 当在 Sql 中直接写入 {0} 时，可以使用本方法直接添加一个参数到参数列表中。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(object value)
        {
            this._parameters.Add(value);
            return this._parameters.Count - 1;
        }

        /// <summary>
        /// 添加一个参数，并在 SQL 中添加相应的索引号
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal void WriteParameter(StringBuilder sql, object value)
        {
            this._parameters.Add(value);

            var offset = this._parameters.Count - 1;
            sql.Append('{').Append(offset).Append('}');
        }

        /// <summary>
        /// 添加一个参数，并在 SQL 中添加相应的索引号
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal void WriteParameter(TextWriter sql, object value)
        {
            this._parameters.Add(value);

            var offset = this._parameters.Count - 1;
            sql.Write('{');
            sql.Write(offset);
            sql.Write('}');
        }

        /// <summary>
        /// 获取指定位置的参数值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index]
        {
            get { return _parameters[index]; }
        }

        /// <summary>
        /// 当前参数的个数
        /// </summary>
        public int Count
        {
            get { return this._parameters.Count; }
        }

        /// <summary>
        /// 按照添加时的索引，返回所有的参数值数组。
        /// 此数组可以直接使用在 DBAccesser 方法中。
        /// </summary>
        /// <returns></returns>
        public object[] ToArray()
        {
            return this._parameters.ToArray();
        }

        /// <summary>
        /// 隐式操作符，使得本类的对象可以直接当作 object[] 使用。方便 DBA 类型的操作。
        /// </summary>
        /// <returns></returns>
        public static implicit operator object[](FormattedSqlParameters value)
        {
            return value._parameters.ToArray();
        }

        public override string ToString()
        {
            return string.Join(",", this._parameters);
        }
    }
}