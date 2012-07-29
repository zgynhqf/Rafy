/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120713 12:50
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120713 12:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OEA.ORM
{
    /// <summary>
    /// FormatSql 中的参数列表封装
    /// </summary>
    public class FormatSqlParameter
    {
        private List<object> _parameters = new List<object>();

        /// <summary>
        /// 添加一个参数，并返回该参数应该使用的索引号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int AddParameter(object value)
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
        public void AddParameter(StringBuilder sql, object value)
        {
            this._parameters.Add(value);

            var offset = this._parameters.Count - 1;
            sql.Append('{').Append(offset).Append('}');
        }

        public void AddParameter(TextWriter sql, object value)
        {
            this._parameters.Add(value);

            var offset = this._parameters.Count - 1;
            sql.Write('{');
            sql.Write(offset);
            sql.Write('}');
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
    }
}