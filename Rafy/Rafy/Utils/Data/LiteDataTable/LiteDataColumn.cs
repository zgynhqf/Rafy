/*******************************************************
 * 
 * 作者：Steven
 * 创建日期：2006-04-15
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130524 09:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.IO.Compression;

namespace Rafy.Data
{
    /// <summary>
    /// 列定义
    /// <remarks>
    /// 定义列的属性和名称
    /// </remarks>
    /// </summary>
    /// Author: Steven
    /// Version: 1.0
    /// History:
    ///     2006-04-15 Steven [创建] 
    [DataContract(Name = "Column")]
    [Serializable]
    [DebuggerDisplay("{Type} {ColumnName}")]
    public class LiteDataColumn
    {
        private string _columnName;
        private string _typeName;
        private Type _type;

        public LiteDataColumn() { }

        public LiteDataColumn(string columnName, Type type)
        {
            this._columnName = columnName;
            this.Type = type;
        }

        public LiteDataColumn(string columnName, string typeString)
        {
            this._columnName = columnName;
            TypeName = typeString;
        }

        /// <summary>
        /// 列名
        /// </summary>
        [DataMember]
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>
        /// 类型名(字符型)
        /// </summary>
        [DataMember]
        private string TypeName
        {
            get { return _typeName; }
            set
            {
                _typeName = value;
                _type = null;
            }
        }

        /// <summary>
        /// 列的类型。
        /// <remarks>只能是基础数据类型。（mscorlib 程序集中定义的类型）</remarks>
        /// </summary>
        public Type Type
        {
            get
            {
                if (_type == null)
                {
                    //mscorlib
                    _type = typeof(int).Assembly.GetType(_typeName);
                }
                return _type;
            }
            set
            {
                _type = value;
                _typeName = _type.FullName;
            }
        }
    }
}