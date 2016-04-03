/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121219 14:47
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121219 14:47
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    internal class ORMHelper
    {
        //internal static void ThrowInConstraintException(string property, Type propertyOwner)
        //{
        //    throw new ORMException(string.Format(
        //        @"类型 {0} 中声明的属性 {1}，在使用查询时，IN、NOT IN 对应的数组参数的长度不能为 0。",
        //        propertyOwner.FullName, property
        //        ));
        //}

        internal static void ThrowBasePropertyNotMappedException(string property, Type propertyOwner)
        {
            throw new ORMException(string.Format(
                @"类型 {0} 中声明的属性 {1}，不能直接用于 ORM 查询。
这是因为该实体类没有映射数据库。如果该类是一个实体基类，请在查询方法中指定 propertyOwner 参数为具体的子实体类型。",
                propertyOwner.FullName, property
                ));
        }
        internal static void ThrowBasePropertyNotMappedException(Type propertyOwner)
        {
            throw new ORMException(string.Format(
                @"类型 {0} 不能直接用于 ORM 查询。
这是因为该实体类没有映射数据库。如果该类是一个实体基类，请在查询方法中指定 propertyOwner 参数为具体的子实体类型。",
                propertyOwner.FullName
                ));
        }
    }
}