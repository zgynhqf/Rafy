/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120429
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120429
 * 
*******************************************************/


using System;
using System.Reflection;
using OEA.ManagedProperty;
using OEA.Library;
using OEA.Reflection;

namespace OEA.ORM.Oracle
{
    /// <summary>
    /// 从 IManagedProperty 到 IDataBridge
    /// </summary>
    public class ManagedPropertyBridge : ORM.ManagedPropertyBridge
    {
        public ManagedPropertyBridge(IManagedProperty field) : base(field) { }

        protected override object OnValueWriting(object value)
        {
            var type = this.DataType;
            if (type == typeof(bool))
            {
                return value.ToString() == "1" ? true : false;
            }

            //null 转换为空字符串
            if (value == null && type == typeof(string))
            {
                return string.Empty;
            }

            return base.OnValueWriting(value);
        }
    }
}