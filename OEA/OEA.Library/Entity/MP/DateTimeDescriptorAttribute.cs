///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120420
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120420
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using OEA.ManagedProperty;
//using OEA.MetaModel;
//using OEA.MetaModel.Attributes;
//using OEA.ORM;
//using OEA.Reflection;

//namespace OEA.Library
//{
//    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
//    public sealed class DateTimeDescriptorAttribute : Attribute
//    {
//        private readonly DateTimeType _type;

//        public DateTimeDescriptorAttribute(DateTimeType type)
//        {
//            this._type = type;
//        }

//        public DateTimeType Type
//        {
//            get { return this._type; }
//        }
//    }

//    public enum DateTimeType
//    {
//        Date, DateTime
//    }
//}